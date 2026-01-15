using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;
    public int bulletDamage = 1;
    public EnemyAI ownerAI;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null) 
        {
            rb.useGravity = false; 
            rb.linearVelocity = transform.forward * speed;
        }
        
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Sahibine çarpma
        if (ownerAI != null && other.gameObject == ownerAI.gameObject) 
            return;
        
        // Sensor trigger'larında yok olma
        if (other.isTrigger) 
            return;
        
        // Diğer düşmanlara çarpma (düşman mermisi düşmana hasar vermesin)
        if (other.CompareTag("Enemy"))
            return;

        // Oyuncuya çarp
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if(player != null)
            {
                player.takeDamage(bulletDamage);
                
                // AI'ı ödüllendir
                if (ownerAI != null)
                    ownerAI.OnHitTarget();
                
                Destroy(gameObject);
                return;
            }
        }

        // Duvar veya zemine çarptı - mermiyi yok et
        Destroy(gameObject);
    }
}