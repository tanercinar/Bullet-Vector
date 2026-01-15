using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public int health = 3;
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public int damage = 1;
    public bool isBulletNearby = false;
    public bool isDashing = false;

    [Header("Yetenek Ayarları")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    [Header("Görsel Ayarlar")]
    public Material defaultMat;
    public Material shieldMat;
    private Renderer rend;
    private bool isShielded = false;
    private Rigidbody rb;
    private EnemyAI brain;

    private bool shouldMove = false;
    private Vector3 currentTargetPos;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        brain = GetComponent<EnemyAI>();

        if (defaultMat == null && rend != null) 
            defaultMat = rend.material;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void FixedUpdate()
    {
        if (shouldMove && !isDashing)
        {
            Vector3 moveDir = transform.forward * moveSpeed;
            moveDir.y = rb.linearVelocity.y;
            rb.linearVelocity = moveDir;
        }
        else if (!isDashing)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public void SetMoveTarget(Vector3 target)
    {
        currentTargetPos = target;
        shouldMove = true;
    }

    public void StopMoving(Vector3 targetToLookAt)
    {
        currentTargetPos = targetToLookAt;
        shouldMove = false;
    }

    public void Jump()
    {
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void Shoot(Quaternion? overrideRotation = null)
    {
        if (enemyBulletPrefab != null && firePoint != null)
        {
            Quaternion bulletRot = overrideRotation.HasValue ? overrideRotation.Value : firePoint.rotation;
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, bulletRot);
            EnemyBullet bScript = bullet.GetComponent<EnemyBullet>();
            if (bScript != null)
            {
                bScript.bulletDamage = damage;
                bScript.ownerAI = brain;
            }
        }
    }

    public void takeDamage(int amount)
    {
        if (brain != null) 
            brain.OnTakeDamage();

        if (isShielded)
        {
            isShielded = false;
            if (rend != null) rend.material = defaultMat;
        }
        else
        {
            health -= amount;
        }

        if (health <= 0) die();
    }

    public void die()
    {
        if (brain != null) 
            brain.OnDie();
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.takeDamage(damage);
                if (brain != null) 
                    brain.OnKillTarget();
            }
            die();
        }
    }

    public void ActivateBoost() 
    { 
        StartCoroutine(BoostRoutine()); 
    }
    
    public void ActivateShield() 
    { 
        isShielded = true; 
        if (rend != null) rend.material = shieldMat; 
    }
    
    public void IncreaseDamage() 
    { 
        damage += 1; 
    }

    IEnumerator BoostRoutine()
    {
        moveSpeed *= 2; 
        jumpForce *= 2;
        yield return new WaitForSeconds(5f);
        moveSpeed /= 2; 
        jumpForce /= 2;
    }

    public void Dash(Vector3 direction)
    {
        if (!isDashing)
        {
            StartCoroutine(DashRoutine(direction));
        }
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        
        Vector3 finalDir = direction.sqrMagnitude > 0.1f ? direction : transform.forward;
        
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            Vector3 v = finalDir.normalized * dashSpeed;
            v.y = rb.linearVelocity.y; 
            rb.linearVelocity = v;
            
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        isDashing = false;
    }
}