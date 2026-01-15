using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrainQBrain : MonoBehaviour
{
    [Header("Öğrenme Parametreleri")]
    public float learningRate = 0.3f;
    public float discountFactor = 0.95f;
    public float explorationDecayRate = 0.00002f;
    public float minExploration = 0.05f;
    
    [Header("Kaydet/Yükle")]
    public string saveFileName = "QBrain.json";
    public string resourceFileName = "QBrain"; // Resources klasöründeki dosya adı (uzantısız)
    public float autoSaveInterval = 30f;
    public bool loadOnStart = false;
    
    // TÜM DÜŞMANLAR TARAFINDAN PAYLAŞILIR (STATİK)
    private static Dictionary<string, float[]> sharedQTable = new Dictionary<string, float[]>();
    private static float sharedExplorationRate = 1.0f;
    private static int totalLearningSteps = 0;
    private static bool isInitialized = false;
    private static string savePath;
    private static float lastSaveTime = 0f;
    private static object saveLock = new object();
    private static bool useTrainedModel = false; // Eğitilmiş model mi kullanılıyor?
    private static bool allowSaving = true; // Kaydetme izni var mı?
    
    // Örneğe Özgü (Her düşman için ayrı)
    private static List<string> sharedActions = new List<string>();
    private string previousState = null;
    private int previousAction = -1;
    
    void Awake()
    {
        if (!isInitialized)
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            sharedExplorationRate = 1.0f;
            sharedActions = new List<string>();
            sharedQTable = new Dictionary<string, float[]>();
            
            Debug.Log($"[QBrain] ★ ORTAK BEYİN BAŞLATILIYOR ★");
            Debug.Log($"[QBrain] Kayıt Yolu: {savePath}");
            
            // ============ PLAYERPREFS KONTROLÜ ============
            int pref = PlayerPrefs.GetInt("UseTrainedAI", 0);
            useTrainedModel = (pref == 1);
            
            Debug.Log($"[QBrain] UseTrainedAI tercihi: {pref} | Eğitilmiş model kullanılıyor mu: {useTrainedModel}");
            
            if (useTrainedModel)
            {
                // ============ RESOURCES'TAN YÜKLE ============
                Debug.Log($"[QBrain] Eğitilmiş model Resources/{resourceFileName} konumundan yükleniyor...");
                
                TextAsset aiData = Resources.Load<TextAsset>(resourceFileName);
                
                if (aiData != null)
                {
                    Debug.Log("[QBrain] ✓ Eğitilmiş model Resources klasöründe bulundu!");
                    LoadFromJSON(aiData.text);
                    
                    // Eğitilmiş model kullanılıyorsa keşif oranı düşük olmalı
                    if (sharedExplorationRate > 0.1f)
                    {
                        sharedExplorationRate = minExploration;
                        Debug.Log($"[QBrain] Keşif oranı minimuma ayarlandı: {sharedExplorationRate}");
                    }
                    
                    // Eğitilmiş modeli değiştirme - kaydetmeyi devre dışı bırak
                    allowSaving = false;
                    Debug.Log("[QBrain] Kayıt DEVRE DIŞI (Eğitilmiş model kullanılıyor)");
                }
                else
                {
                    Debug.LogError($"[QBrain] ✗ Resources/{resourceFileName}.json bulunamadı!");
                    Debug.LogError("[QBrain] Dosyanın Assets/Resources/ klasöründe olduğundan emin olun");
                    Debug.Log("[QBrain] Yeni eğitime (fallback) başlanıyor...");
                    
                    // Resources'ta bulunamadı, yeni eğitime başla
                    useTrainedModel = false;
                    allowSaving = true;
                    sharedExplorationRate = 1.0f;
                    CreateSaveDirectory();
                }
            }
            else
            {
                // ============ YENİ EĞİTİM VEYA MEVCUT DOSYADAN YÜKLE ============
                Debug.Log("[QBrain] Eğitim modu - ilerleme kaydedilecek");
                allowSaving = true;
                
                CreateSaveDirectory();
                
                if (loadOnStart && File.Exists(savePath))
                {
                    LoadBrain();
                    Debug.Log($"[QBrain] Mevcut beyin yüklendi. ε={sharedExplorationRate:F4}");
                }
                else
                {
                    Debug.Log("[QBrain] Sıfırdan yeni eğitim başlıyor!");
                    sharedExplorationRate = 1.0f;
                    SaveBrain();
                }
            }
            
            lastSaveTime = Time.time;
            isInitialized = true;
            
            Debug.Log($"[QBrain] ═══════════════════════════════════");
            Debug.Log($"[QBrain] Mod: {(useTrainedModel ? "EĞİTİLMİŞ MODEL" : "EĞİTİM")}");
            Debug.Log($"[QBrain] Keşif Oranı (ε): {sharedExplorationRate:F4}");
            Debug.Log($"[QBrain] Yüklenen Durumlar: {sharedQTable.Count}");
            Debug.Log($"[QBrain] Kaydetme Açık: {allowSaving}");
            Debug.Log($"[QBrain] ═══════════════════════════════════");
        }
        else
        {
            Debug.Log($"[QBrain] Düşman Bağlandı | ε: {sharedExplorationRate:F4} | Durumlar: {sharedQTable.Count}");
        }
    }
    
    void CreateSaveDirectory()
    {
        try
        {
            string directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"[QBrain] Klasör oluşturuldu: {directory}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[QBrain] Klasör oluşturulamadı: {e.Message}");
        }
    }
    
    void Update()
    {
        // Exploration decay sadece eğitim modunda
        if (allowSaving && sharedExplorationRate > minExploration)
        {
            sharedExplorationRate -= explorationDecayRate * Time.deltaTime;
            if (sharedExplorationRate < minExploration)
                sharedExplorationRate = minExploration;
        }
        
        // Auto-save sadece eğitim modunda
        if (allowSaving)
        {
            QBrain[] allBrains = FindObjectsByType<QBrain>(FindObjectsSortMode.None);
            bool isFirstBrain = (allBrains.Length > 0 && allBrains[0] == this);
            
            if (isFirstBrain && Time.time - lastSaveTime >= autoSaveInterval)
            {
                SaveBrain();
                lastSaveTime = Time.time;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (allowSaving)
        {
            QBrain[] allBrains = FindObjectsByType<QBrain>(FindObjectsSortMode.None);
            if (allBrains.Length > 0 && allBrains[0] == this)
            {
                SaveBrain();
            }
        }
    }
    
    public void RegisterAction(string actionName)
    {
        if (!sharedActions.Contains(actionName))
        {
            sharedActions.Add(actionName);
            Debug.Log($"[QBrain] Kaydedilen aksiyon: {actionName} (Toplam: {sharedActions.Count})");
        }
    }
    
    public int ChooseAction(string state)
    {
        if (sharedActions.Count == 0)
        {
            Debug.LogError("[QBrain] Hiçbir aksiyon kaydedilmedi!");
            return 0;
        }
        
        if (!sharedQTable.ContainsKey(state))
        {
            sharedQTable[state] = new float[sharedActions.Count];
        }
        
        int chosenAction;
        
        if (UnityEngine.Random.value < sharedExplorationRate)
        {
            chosenAction = UnityEngine.Random.Range(0, sharedActions.Count);
        }
        else
        {
            chosenAction = GetBestAction(state);
        }
        
        previousState = state;
        previousAction = chosenAction;
        
        return chosenAction;
    }
    
    private int GetBestAction(string state)
    {
        if (!sharedQTable.ContainsKey(state))
        {
            return UnityEngine.Random.Range(0, sharedActions.Count);
        }
        
        float[] qValues = sharedQTable[state];
        
        // Q-table'daki action sayısı ile mevcut action sayısı uyuşmazsa
        if (qValues.Length != sharedActions.Count)
        {
            Debug.LogWarning($"[QBrain] Q-değerleri uzunluk uyuşmazlığı! Q:{qValues.Length} Aksiyonlar:{sharedActions.Count}");
            return UnityEngine.Random.Range(0, sharedActions.Count);
        }
        
        int bestAction = 0;
        float bestValue = qValues[0];
        List<int> bestActions = new List<int> { 0 };
        
        for (int i = 1; i < qValues.Length; i++)
        {
            if (qValues[i] > bestValue)
            {
                bestValue = qValues[i];
                bestAction = i;
                bestActions.Clear();
                bestActions.Add(i);
            }
            else if (Mathf.Approximately(qValues[i], bestValue))
            {
                bestActions.Add(i);
            }
        }
        
        return bestActions[UnityEngine.Random.Range(0, bestActions.Count)];
    }
    
    public void Learn(string currentState, float reward)
    {
        // Eğitilmiş model kullanılıyorsa öğrenme yapma
        if (!allowSaving)
            return;
        
        if (previousState == null || previousAction == -1)
            return;
        
        if (sharedActions.Count == 0)
            return;
        
        if (!sharedQTable.ContainsKey(previousState))
            sharedQTable[previousState] = new float[sharedActions.Count];
        if (!sharedQTable.ContainsKey(currentState))
            sharedQTable[currentState] = new float[sharedActions.Count];
        
        float[] prevQValues = sharedQTable[previousState];
        float[] currQValues = sharedQTable[currentState];
        
        if (previousAction >= prevQValues.Length)
        {
            Debug.LogError($"[QBrain] Aksiyon indeksi {previousAction} sınır dışı");
            return;
        }
        
        float maxQ = currQValues[0];
        for (int i = 1; i < currQValues.Length; i++)
        {
            if (currQValues[i] > maxQ)
                maxQ = currQValues[i];
        }
        
        float oldQ = prevQValues[previousAction];
        float newQ = oldQ + learningRate * (reward + discountFactor * maxQ - oldQ);
        prevQValues[previousAction] = newQ;
        
        totalLearningSteps++;
        
        if (totalLearningSteps % 500 == 0)
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            Debug.Log($"[QBrain] Step {totalLearningSteps} | ε={sharedExplorationRate:F4} | States:{sharedQTable.Count} | Enemies:{enemyCount}");
        }
    }
    
    public void GiveReward(string currentState, float reward)
    {
        Learn(currentState, reward);
    }
    
    public void GivePunishment(string currentState, float punishment)
    {
        Learn(currentState, -Mathf.Abs(punishment));
    }
    
    public string GetActionName(int actionIndex)
    {
        if (actionIndex >= 0 && actionIndex < sharedActions.Count)
            return sharedActions[actionIndex];
        return "Unknown";
    }
    
    // ============ SERIALIZATION ============
    
    [Serializable]
    public class BrainData
    {
        public float explorationRate;
        public List<string> actions = new List<string>();
        public List<StateData> states = new List<StateData>();
    }
    
    [Serializable]
    public class StateData
    {
        public string state;
        public List<float> qValues = new List<float>();
    }
    
    // Resources'tan JSON string yükle
    private void LoadFromJSON(string json)
    {
        try
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[QBrain] JSON string is empty!");
                return;
            }
            
            BrainData data = JsonUtility.FromJson<BrainData>(json);
            
            if (data == null)
            {
                Debug.LogError("[QBrain] Failed to parse JSON!");
                return;
            }
            
            sharedExplorationRate = data.explorationRate;
            
            if (data.actions != null && data.actions.Count > 0)
            {
                sharedActions = new List<string>(data.actions);
            }
            
            sharedQTable.Clear();
            
            if (data.states != null)
            {
                foreach (var stateData in data.states)
                {
                    if (stateData.qValues != null)
                    {
                        sharedQTable[stateData.state] = stateData.qValues.ToArray();
                    }
                }
            }
            
            Debug.Log($"[QBrain] ✓ Loaded from JSON | States: {sharedQTable.Count} | Actions: {sharedActions.Count} | ε: {sharedExplorationRate:F4}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[QBrain] JSON parse error: {e.Message}");
        }
    }
    
    public void SaveBrain()
    {
        if (!allowSaving)
        {
            Debug.Log("[QBrain] Saving disabled (using trained model)");
            return;
        }
        
        lock (saveLock)
        {
            try
            {
                BrainData data = new BrainData();
                data.explorationRate = sharedExplorationRate;
                data.actions = new List<string>(sharedActions);
                
                foreach (var kvp in sharedQTable)
                {
                    StateData stateData = new StateData();
                    stateData.state = kvp.Key;
                    stateData.qValues = new List<float>(kvp.Value);
                    data.states.Add(stateData);
                }
                
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(savePath, json);
                
                FileInfo fi = new FileInfo(savePath);
                Debug.Log($"[QBrain] ✓ SAVED | States: {sharedQTable.Count} | Size: {fi.Length} bytes");
        }
            catch (Exception e)
            {
                Debug.LogError($"[QBrain] Save failed: {e.Message}");
            }
        }
    }
    
    public void LoadBrain()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.Log($"[QBrain] No save file at {savePath}");
                return;
            }
            
            string json = File.ReadAllText(savePath);
            LoadFromJSON(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[QBrain] Load failed: {e.Message}");
        }
    }
    
    public int GetStateCount()
    {
        return sharedQTable.Count;
    }
    
    public float GetExplorationRate()
    {
        return sharedExplorationRate;
    }
    
    public bool IsUsingTrainedModel()
    {
        return useTrainedModel;
    }
    
    [ContextMenu("Force Save Brain")]
    public void ForceSave()
    {
        if (!allowSaving)
        {
            Debug.LogWarning("[QBrain] Kayıt yapılamaz - eğitilmiş model kullanılıyor!");
            return;
        }
        SaveBrain();
    }
    
    [ContextMenu("Reset Brain")]
    public void ResetBrain()
    {
        sharedQTable.Clear();
        sharedExplorationRate = 1.0f;
        Debug.Log("[QBrain] Beyin sıfırlandı");
    }
    
    [ContextMenu("Show Status")]
    public void ShowStatus()
    {
        Debug.Log($"═══════════ QBRAIN DURUMU ═══════════");
        Debug.Log($"Mod: {(useTrainedModel ? "EĞİTİLMİŞ MODEL (Resources'tan)" : "EĞİTİM")}");
        Debug.Log($"Kayıt Yolu: {savePath}");
        Debug.Log($"Kaydetme Açık: {allowSaving}");
        Debug.Log($"Keşif Oranı (ε): {sharedExplorationRate:F4}");
        Debug.Log($"Durumlar: {sharedQTable.Count}");
        Debug.Log($"Aksiyonlar: {sharedActions.Count}");
        foreach (var action in sharedActions)
        {
            Debug.Log($"  - {action}");
        }
        Debug.Log($"═════════════════════════════════════");
    }
    
    // Editor'de Play Mode'dan çıkınca static'leri sıfırla
    public static void ResetStatics()
    {
        isInitialized = false;
        sharedQTable = new Dictionary<string, float[]>();
        sharedActions = new List<string>();
        sharedExplorationRate = 1.0f;
        totalLearningSteps = 0;
        useTrainedModel = false;
        allowSaving = true;
    }
    
    // Eğitimi bitirip Resources'a kopyalamak için yardımcı
    [ContextMenu("Export for Resources")]
    public void ExportForResources()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
            Debug.Log($"[QBrain] Resources klasörü oluşturuldu: {resourcesPath}");
        }
        
        string exportPath = Path.Combine(resourcesPath, saveFileName);
        
        BrainData data = new BrainData();
        data.explorationRate = minExploration; // Export'ta exploration minimum olsun
        data.actions = new List<string>(sharedActions);
        
        foreach (var kvp in sharedQTable)
        {
            StateData stateData = new StateData();
            stateData.state = kvp.Key;
            stateData.qValues = new List<float>(kvp.Value);
            data.states.Add(stateData);
        }
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(exportPath, json);
        
        Debug.Log($"[QBrain] ★ RESOURCES'A DIŞA AKTARILDI ★");
        Debug.Log($"[QBrain] Yol: {exportPath}");
        Debug.Log($"[QBrain] Durumlar: {sharedQTable.Count}");
        Debug.Log($"[QBrain] Dosyayı görmek için Unity'yi yenileyin (Ctrl+R)!");
    }
}