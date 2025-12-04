using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    //ESC'ye basıldığında görünecek olan panel
    public GameObject pauseMenuUI;
    //Ana menü sahnesinin adı
    public string mainMenuSceneName = "MainMenu";
    //Oyunun durup durmadığının kontrol eden değişken
    public static bool GameIsPaused = false;

    void Update()
    {
        //Esc tuşuna basıldığında:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Oyun durdurulmuş durumda ise devam ettiren fonksiyonu çağır
            if (GameIsPaused)
            {
                Resume();
            }
            //Devam ediyor ise durduran fonksiyonu çağır
            else
            {
                Pause();
            }
        }
    }
    void Pause()
    {
        //Durdurma menüsünü aktive et
        pauseMenuUI.SetActive(true); 
        //Zamanı durdur 
        Time.timeScale = 0f;
        //Zamanı durdurma durumunu kontrol eden değişkenini değiştir
        GameIsPaused = true;
        //Oyun başladığında kitlenmiş ve görünmez olan(Player scriptinde Start() fonksiyounda) fare imlecini görünür ve hareket edebilir hale getir
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Resume()
    {
        //Durdurma menüsünü deaktive et
        pauseMenuUI.SetActive(false);
        //Zamanı nomal akış hızına
        Time.timeScale = 1f;   
        //Zamanı durdurma durumunu kontrol eden değişkenini değiştir       
        GameIsPaused = false;
        //Mouse imlecini tekrar kilitle ve görünmez yap
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadMainMenu()
    {
        //Zaman akışını normale döndür
        Time.timeScale = 1f;
        GameIsPaused = false;
        //Ana menü sahnesini yükle
        SceneManager.LoadScene(mainMenuSceneName);
    }

}