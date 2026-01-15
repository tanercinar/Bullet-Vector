using UnityEngine;

public class DodgeSensor : MonoBehaviour
{
    private Enemy enemyScript;
    private EnemyAI aiScript;
    private bool wasNearby = false;

    void Start()
    {
        enemyScript = GetComponentInParent<Enemy>();
        aiScript = GetComponentInParent<EnemyAI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet")) 
        {
            enemyScript.isBulletNearby = true;
            wasNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBullet")) 
        {
            enemyScript.isBulletNearby = false;
            
            if (wasNearby && aiScript != null)
            {
                aiScript.OnDodgeBullet();
                wasNearby = false;
            }
        }
    }
}