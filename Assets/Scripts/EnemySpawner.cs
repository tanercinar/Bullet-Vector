using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //Düşman nesnesinin prefabi
    public GameObject enemyPrefab;
    //Oluşturulacak güçlendirme nesnelerinin dizisi
    public GameObject[] powerUpPrefabs;
    //Düşman ve güçlendirmelerin çağırma bekleme süresi
    public float spawnCooldown = 2f;
    public float powerUpCooldown = 10f;
    //Koordinat değişkenleri
    public float y = 1f;
    public float xMin = -10f;
    public float xMax = 10f;
    public float zMin = -10f;
    public float zMax = 10f;

    void Start()
    {
        //Düşman ve Güçlendirme rutinlerini başlat
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        //Sonsuz Döngü
        while (true)
        {
            //Bekleme süresi kadar bekle
            yield return new WaitForSeconds(spawnCooldown);
            //Belirlenen koordinatlar içinde rastgele bir posizyon ayarla
            float randomX = Random.Range(xMin, xMax);
            float randomZ = Random.Range(zMin, zMax);
            Vector3 position = new Vector3(randomX, y, randomZ);
            //Düşmanı belirlenen pozisyonda çağır
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, position, Quaternion.identity);
            }
        }
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        //Sonsuz Döngü
        while (true)
        {
            //Bekleme süresi kadar bekle
            yield return new WaitForSeconds(powerUpCooldown);
            //Belirlenen koordinatlar içinde rastgele bir posizyon ayarla
            float randomX = Random.Range(xMin, xMax);
            float randomZ = Random.Range(zMin, zMax);
            Vector3 position = new Vector3(randomX, y, randomZ);
            //Prefab dizisinden rastgele bir indexi belirlenen pozisyonda çağır
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
            {
                int index = Random.Range(0, powerUpPrefabs.Length);
                Instantiate(powerUpPrefabs[index], position, Quaternion.identity);
            }
        }
    }

}