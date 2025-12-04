using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Sırasıyla merminin: hızının, yok edilmeden önceki süresinin, hasarının tanımlanması
    public float speed = 100f;
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
        //Temas edilen nesne düşman ise:
        if (other.gameObject.CompareTag("Enemy"))
        {
            //Düşmandaki Enemy scriptini al
            Enemy enemy=other.GetComponent<Enemy>();
            //takeDamage fonksiyonunu çağır
            enemy.takeDamage(bulletDamage);
            //Mermiyi yok et
            Destroy(gameObject);
        }
    }
}