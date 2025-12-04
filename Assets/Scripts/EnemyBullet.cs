using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    //Rigidbody componentini al, yerçekimini devredışı bırak, hızını ileri yönde değiştir
    public float speed = 30f;
    public float lifeTime = 3f;
    public int bulletDamage=1;

    void Start()
    {
        //Rigidbody componentini al, yerçekimini devredışı bırak, hızını ileri yönde değiştir
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false; 
        rb.linearVelocity = transform.forward * speed;
        //Süre dolunca nesneyi yok et
        Destroy(gameObject, lifeTime);
    }
    void OnTriggerEnter(Collider other)
    {
        //Temas edilen nesne oyuncu ise:
        if (other.gameObject.CompareTag("Player"))
        {
            //Oyuncudaki Player scriptini al
            Player player =other.GetComponent<Player>();
            //takeDamage fonksiyonunu çağır
            player.takeDamage(bulletDamage);
            //Mermiyi yok et
            Destroy(gameObject);
        }
    }
}