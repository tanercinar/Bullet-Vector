using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    //Gerekli değişkenlerin tanımlanması
    public int health = 3;
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public int damage = 1;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    public Material defaultMat;
    public Material shieldMat;
    private Renderer rend;
    private bool isShielded = false;
    private Rigidbody rb;

    public float stopDistance = 5f;
    public float fireRate = 1.5f;
    private Transform playerTarget;
    private float nextFireTime;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        //Varsayılan materyal ve renderer boş değil ise:
        if (defaultMat == null && rend != null)
        {
            //defaultMat değişkenini o andaki materyal olarak ayarla
            defaultMat = rend.material;
        }

        /*
        //Oyuncuyu bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        */
    }

    /*
    void Update()
    {
        if (playerTarget != null)
        {
            //Oyuncuya Dön
            Vector3 direction = playerTarget.position - transform.position;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            float distance = Vector3.Distance(transform.position, playerTarget.position);

            //Oyuncuyla aradaki mesafe durulması istenen mesafeden büyükse oyuncuya doğru yürü
            if (distance > stopDistance)
            {
                //Hareket vektörünü hesaplama
                Vector3 moveDir = transform.forward * moveSpeed;
                //y eksenindeki zıplama veya düşme anındaki hızı koru
                moveDir.y = rb.linearVelocity.y;
                //Rigidbody componetine hızı uygula
                rb.linearVelocity = moveDir;
            }
            else
            {
                //Dur
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }

            if (Time.time > nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    */

    //Düşmanın hasar almasını sağlayan fonksiyon
    public void takeDamage(int amount)
    {
        //Eğer kalkan açıksa:
        if (isShielded)
        {
            //Kalkanı kapat ve materyali eski haline getir
            isShielded = false;
            if (rend != null) rend.material = defaultMat;
        }
        //Kalkan kapalıysa düşmanın canını hasar değeri kadar azalt
        else
        {
            health -= amount;
        }

        if (health <= 0)
        {
            die();
        }
    }
    //Nesneyi yok eden fonksiyon
    public void die()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Eğer çarpışılan nesne Oyuncu ise:
        if (collision.gameObject.CompareTag("Player"))
        {
            //Oyuncudan Player scriptini al
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                //Oyuncunun hasar almasını sağlayan fonksiyonu çağır
                player.takeDamage(damage);
            }
            die();
        }
    }

    //Hız güçlendirmesi fonksiyonu
    public void ActivateBoost()
    {
        StartCoroutine(BoostRoutine());
    }

    //Kalkan güçlendirmesi fonksiyonu
    public void ActivateShield()
    {
        //Kalkanı aktive et ve materyali değiştir
        isShielded = true;
        if (rend != null) rend.material = shieldMat;
    }

    //Hasar arttırma güçlendirmesi fonksiyonu
    public void IncreaseDamage()
    {
        //Mermiye aktarılan hasar değerini bir arttır
        damage += 1;
    }

    IEnumerator BoostRoutine()
    {
        //Hız ve zıplama gücünü iki katına çıkart
        moveSpeed *= 2;
        jumpForce *= 2;
        yield return new WaitForSeconds(5f);
        //Süre bitince tekrar ikiye böl
        moveSpeed /= 2;
        jumpForce /= 2;
    }

    public void Dash()
    {
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        if (rb != null)
        {
            //Rigidbodye atılma etkisini uygula
            rb.linearVelocity = transform.forward * dashSpeed;
            //Atılma süresince bekle
            yield return new WaitForSeconds(dashDuration);
        }
    }

    //Ateş etme fonksiyonu
    public void Shoot()
    {
        //Mermi prefabi ve ateş edilecek nokta değişkenleri boş değil ise:
        if (enemyBulletPrefab != null && firePoint != null)
        {
            //firePoint noktasında bir Bullet prefab'i oluştur
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
            //Oluşturulan prefabin hassar değerini düşmandaki değer olarak değiştir(Hasar arttırma güçlendirmesinin çalışması için gerekli)
            bullet.GetComponent<EnemyBullet>().bulletDamage = damage;
        }
    }
}