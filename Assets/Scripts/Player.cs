using UnityEngine;
using System.Collections;
using TMPro;
public class Player : MonoBehaviour
{
    //Gerekli değişkenlerin tanımlanması
    public int health=10;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 200f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public AudioSource gunAudioSource;
    public AudioClip fireSound;
    public float fireDelay = 0.6f;
    public Camera playerCamera;
    private Rigidbody rb;
    private float nextFireTime = 0f;
    private float xRotation = 0f;
    public GameObject weapon;
    public float recoilForce = 9f;
    public float recoilRecovery = 7f;
    private float currentRecoilX = 0f;
    private bool isShielded = false;
    public int playerBulletDamage = 1;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float nextDashTime = 0f;
    private bool isDashing = false;
    public GameObject shieldText;
    public float slowDuration = 2f;
    private bool isTimeSlowed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //Fare imlecinin oyun esnasında kilitlenip görünmez hale getirilmesi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //Space tuşuna basıldığında oyuncunun zıplaması
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        //Mouse imlecinin girdisinin alınması
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //Oyuncunun aşağı yukarı bakışını ayarla
        xRotation -= mouseY;
        //Bakış açısını -90 ile 90 derece arasında sabitle
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //Kafa hareketi(Y eksenindeki hareket)'e göre kamerayı döndür
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //Yatay eksende gövdeyi döndür
        transform.Rotate(Vector3.up * mouseX);

        //Silahın geri tepmesi   
        if (weapon != null)
        {
            currentRecoilX = Mathf.Lerp(currentRecoilX, 0f, recoilRecovery * Time.deltaTime);
            weapon.transform.localRotation = Quaternion.Euler(-currentRecoilX, 0f, 0f);
        }
        //Sol tıka basılır ve ateş etme bekleme süresinde değilse ve oyun durdurulmamış ise ateş etme fonksiyonunu çağır ve yeniden bekleme süresine sok
        if (Input.GetMouseButton(0) && Time.time > nextFireTime && Time.timeScale != 0)
        {
            Shoot();
            nextFireTime = Time.time + fireDelay;
        }
        //Sağ tıka basılırsa ve dash(atılma) bekleme süresinde değil ise atılma rutinini başlat
        if (Input.GetMouseButtonDown(1) && Time.time > nextDashTime)
        {
            Debug.Log("Dash");
            StartCoroutine(DashRoutine());
        }
        //Q tuşuna basılır ve zaman yavaşlatılmış durumda değil ise zamanı yavaşlatma rutinini başlat
        if (Input.GetKeyDown(KeyCode.Q) && !isTimeSlowed)
        {
        StartCoroutine(SlowMotionRoutine());
        }
    }

    void FixedUpdate()
    {
        //Eğer dash hareketi yapıyorsa buradan sonraki hareket kodlarını  çalıştırma
        if (isDashing)
        {
            return;
        } 
        //Klavyeden x ve z eksenlerinin girdisini al
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        //Hareket vektörünü hesaplama
        Vector3 move = transform.right * x + transform.forward * z;
        //Çapraz giderken hızlanmayı engellemek için normalize ederek çarpım yap
        move = move.normalized * moveSpeed;
        //y eksenindeki zıplama veya düşme anındaki hızı koru
        move.y = rb.linearVelocity.y;
        //Rigidbody componetine hızı uygula
        rb.linearVelocity = move;
    }

    //Ateş etme fonksiyonu
    void Shoot()
    {
        //Mermi prefabi ve ateş edilecek nokta değişkenleri boş değil ise:
        if (bulletPrefab && firePoint)
        {
            //firePoint noktasında bir Bullet prefab'i oluştur
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            //Oluşturulan prefabin hassar değerini oyuncudaki değer olarak değiştir(Hasar arttırma güçlendirmesinin çalışması için gerekli)
            bullet.GetComponent<Bullet>().bulletDamage = playerBulletDamage;
        }
        //Ateş sesi ve ses kaynağı varsa sesi oynat
        if (gunAudioSource && fireSound)
        {
            gunAudioSource.PlayOneShot(fireSound);
        }
        //Silahı yukarı kaldırarak geri tepme uygula
        currentRecoilX += recoilForce;
    }
    //Oyuncunun hasar almasını sağlayan fonksiyon
    public void takeDamage(int damage)
    {
        //Eğer kalkan açıksa:
        if (isShielded)
        {
            //Kalkanı kapat ve Kalkan textini deaktive et
            isShielded=false;
            shieldText.SetActive(false);
        }
        //Kalkan kapalıysa oyuncunun canını hasar değeri kadar azalt
        else
        {
            health-=damage;
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
        //Kalkanı ve kalkan textini aktive et
        isShielded = true;
        shieldText.SetActive(true);
    }
    //Hasar arttırma güçlendirmesi fonksiyonu    
    public void IncreaseDamage()
    {
        //Mermiye aktarılan hasar değerini bir arttır
        playerBulletDamage+=1;
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
    IEnumerator DashRoutine()
    {
        Debug.Log("Dash Routine");
        //FixedUpdate'in çalışmasını engelle
        isDashing = true;
        //Bekleme süresini başlat
        nextDashTime = Time.time + dashCooldown;
        //Klavyeden x ve z ekseni yön girdilerini al
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        //Atılma yönünü belirle
        Vector3 dashDir = transform.right * x + transform.forward * z;
        //Oyuncu herhangi bir yöne gitmiyorsa ileri yönde atılsın
        if (dashDir.sqrMagnitude == 0)
            dashDir = transform.forward;
        //Rigidbodye atılma etkisini uygula
        rb.linearVelocity = dashDir.normalized * dashSpeed;
        //Atılma süresince bekle
        yield return new WaitForSeconds(dashDuration);
        //FixedUpdate'i tekrar çalışır hale getir
        isDashing = false;
    }
    //Zamanı yavaşlatma
    IEnumerator SlowMotionRoutine()
    {
        isTimeSlowed = true;
        //Zaman akış hızını yarıya düşür
        Time.timeScale = 0.5f;
        //Fizik hesaplamalarını buna senkronize et
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        //Zaman yavaşladığında oyuncu etkilenmemesi için hareket hızını geçici olarak iki katına çıkart
        moveSpeed *= 2;
        yield return new WaitForSecondsRealtime(slowDuration);
        //Süre bitince her şeyi normal haline getir
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        moveSpeed/=2;
        isTimeSlowed = false;
    }

}