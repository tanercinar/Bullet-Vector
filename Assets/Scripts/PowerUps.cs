using UnityEngine;

public class PowerUps : MonoBehaviour
{
    //Hangi güçlendirme olduğunun editör üzerinden ayarını sağlayan değişken
    public int powerUpType;

    void Update()
    {
        //Güçlendirme objelerini kendi etraflarında döndür
        transform.Rotate(new Vector3(0, 50, 0) * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
    {
        //Etkileşime girilen nesnenin oyuncu mu düşman mı olduğunun kontrolü
        if (other.CompareTag("Player"))
        {
            //Oyuncu ise Player script'ini al
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                //Değişkenin değerine göre oyuncudan gerekli fonksiyonun çağırılması
                if (powerUpType == 0) player.ActivateBoost();
                if (powerUpType == 1) player.ActivateShield();
                if(powerUpType == 2) player.IncreaseDamage();
                //Nesnenin kendini yok etmesi
                Destroy(gameObject);
        }
        }
        if(other.CompareTag("Enemy"))
        {
            //Düşmansa nesneden Enemy scriptini al
            Enemy enemy = other.GetComponent<Enemy>();
            EnemyAI ai = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                //Değişkenin değerine göre düşmandan gerekli fonksiyonu çağır
                if (powerUpType == 0) enemy.ActivateBoost();
                if (powerUpType == 1) enemy.ActivateShield();
                if(powerUpType == 2) enemy.IncreaseDamage();
                //Nesnenin kendini yok etmesi
                Destroy(gameObject);
            }
        }
    }
}