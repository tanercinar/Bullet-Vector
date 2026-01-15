using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    private Enemy enemy;
    private QBrain brain;
    
    [Header("Decision Settings")]
    public float decisionInterval = 0.2f;
    private float decisionTimer = 0f;
    
    [Header("Sensing")]
    public float visionRange = 50f;
    public float visionAngle = 120f;
    public int maxTargetCheck = 20;
    private Transform target;
    private bool canSeeTarget = false;
    
    [Header("Movement")]
    public float turnSpeed = 180f; // Saniyede dönüş açısı (derece)
    public float turnAmount = 30f; // Her karar başına dönüş miktarı
    
    [Header("State Variables")]
    private string currentState = "";
    private string previousState = "";
    
    // Action indices
    private const int ACTION_MOVE_FORWARD = 0;
    private const int ACTION_TURN_LEFT = 1;
    private const int ACTION_TURN_RIGHT = 2;
    private const int ACTION_SHOOT = 3;
    private const int ACTION_DASH_LEFT = 4;
    private const int ACTION_DASH_RIGHT = 5;
    private const int ACTION_JUMP = 6;
    private const int ACTION_WAIT = 7;
    
    // Tracking
    private float lastDistanceToTarget = 999f;
    private float lastAngleToTarget = 180f;
    private float searchTimer = 0f;
    private int searchDirection = 1;
    
    void Start()
    {
        enemy = GetComponent<Enemy>();
        brain = GetComponent<QBrain>();
        
        if (brain == null)
        {
            Debug.LogError($"[EnemyAI] {gameObject.name} has no QBrain component!");
            return;
        }
        
        // Register all actions
        brain.RegisterAction("MoveForward");
        brain.RegisterAction("TurnLeft");
        brain.RegisterAction("TurnRight");
        brain.RegisterAction("Shoot");
        brain.RegisterAction("DashLeft");
        brain.RegisterAction("DashRight");
        brain.RegisterAction("Jump");
        brain.RegisterAction("Wait");
        
        searchDirection = Random.value > 0.5f ? 1 : -1;
    }
    
    void Update()
    {
        if (brain == null) return;
        
        decisionTimer += Time.deltaTime;
        
        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            MakeDecision();
        }
    }
    
    void MakeDecision()
    {
        previousState = currentState;
        
        UpdateSensing();
        
        currentState = BuildState();
        
        // Hedef görünce ödül
        if (canSeeTarget && !previousState.StartsWith("1_"))
        {
            brain.GiveReward(currentState, 3f);
        }
        
        int actionIndex = brain.ChooseAction(currentState);
        
        // Hedef görünüyorsa ve iyi hizalanmışsa ama ateş etmiyorsa cezalandır
        if (canSeeTarget && target != null)
        {
            Vector3 dirToTarget = target.position - transform.position;
            dirToTarget.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angle < 10f && actionIndex != ACTION_SHOOT)
            {
                brain.GivePunishment(currentState, 2f);
            }
        }
        
        ExecuteAction(actionIndex);
    }
    
    void UpdateSensing()
    {
        canSeeTarget = false;
        target = null;
        
        // ============ SADECE OYUNCUYU HEDEFLE ============
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            return;
        }
        
        Vector3 directionToTarget = player.transform.position - transform.position;
        float distance = directionToTarget.magnitude;
        
        // Menzil kontrolü
        if (distance > visionRange)
        {
            return;
        }
        
        // Görüş açısı kontrolü
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        
        if (angle > visionAngle)
        {
            return;
        }
        
        // Raycast ile görüş kontrolü
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayDir = (player.transform.position + Vector3.up * 0.5f) - rayStart;
        
        if (Physics.Raycast(rayStart, rayDir.normalized, out hit, distance + 1f))
        {
            if (hit.collider.gameObject == player)
            {
                canSeeTarget = true;
                target = player.transform;
                searchTimer = 0f;
            }
        }
        else
        {
            // Raycast hiçbir şeye çarpmadı - doğrudan görüş var
            canSeeTarget = true;
            target = player.transform;
            searchTimer = 0f;
        }
    }
    
    string BuildState()
    {
        int targetVisible = canSeeTarget ? 1 : 0;
        int distanceBucket = 0;
        int angleBucket = 0;
        int angleDirection = 0;
        int healthBucket = Mathf.Clamp(enemy.health, 0, 3);
        int bulletNearby = enemy.isBulletNearby ? 1 : 0;
        
        if (canSeeTarget && target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            distanceBucket = Mathf.Clamp((int)(distance / 10f), 0, 5);
            
            Vector3 dirToTarget = target.position - transform.position;
            dirToTarget.y = 0;
            
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angle < 5f) angleBucket = 0;
            else if (angle < 15f) angleBucket = 1;
            else if (angle < 30f) angleBucket = 2;
            else if (angle < 60f) angleBucket = 3;
            else angleBucket = 4;
            
            Vector3 cross = Vector3.Cross(transform.forward, dirToTarget);
            if (cross.y > 0.1f) angleDirection = 1;
            else if (cross.y < -0.1f) angleDirection = -1;
            else angleDirection = 0;
            
            lastDistanceToTarget = distance;
            lastAngleToTarget = angle;
        }
        
        return $"{targetVisible}_{distanceBucket}_{angleBucket}_{angleDirection}_{healthBucket}_{bulletNearby}";
    }
    
    void ExecuteAction(int actionIndex)
    {
        switch (actionIndex)
        {
            case ACTION_MOVE_FORWARD:
                MoveForward();
                break;
            case ACTION_TURN_LEFT:
                TurnLeft();
                break;
            case ACTION_TURN_RIGHT:
                TurnRight();
                break;
            case ACTION_SHOOT:
                Shoot();
                break;
            case ACTION_DASH_LEFT:
                DashLeft();
                break;
            case ACTION_DASH_RIGHT:
                DashRight();
                break;
            case ACTION_JUMP:
                Jump();
                break;
            case ACTION_WAIT:
                Wait();
                break;
        }
    }

        
    void MoveForward()
    {
        // Hedef görünüyorsa ona doğru yönel
        if (canSeeTarget && target != null)
        {
            Vector3 dirToTarget = target.position - transform.position;
            dirToTarget.y = 0;
            
            if (dirToTarget.sqrMagnitude > 0.1f)
            {
                // Hedefe doğru yumuşak dönüş
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * decisionInterval);
            }
        }
        
        Vector3 targetPos = transform.position + transform.forward * 5f;
        enemy.SetMoveTarget(targetPos);
        
        if (canSeeTarget && target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance > 20f)
            {
                brain.GiveReward(currentState, 1f);
            }
            else if (distance < 8f)
            {
                brain.GivePunishment(currentState, 2f);
            }
        }
        else
        {
            brain.GiveReward(currentState, 0.1f);
        }
    }
    
    void TurnLeft()
    {
        transform.Rotate(Vector3.up, -turnAmount);
        enemy.StopMoving(transform.position);
        
        EvaluateTurnQuality(-1);
    }
    
    void TurnRight()
    {
        transform.Rotate(Vector3.up, turnAmount);
        enemy.StopMoving(transform.position);
        
        EvaluateTurnQuality(1);
    }
    
    void EvaluateTurnQuality(int turnDirection)
    {
        if (!canSeeTarget || target == null)
        {
            searchTimer += decisionInterval;
            brain.GiveReward(currentState, 0.2f);
            return;
        }
        
        Vector3 dirToTarget = target.position - transform.position;
        dirToTarget.y = 0;
        float currentAngle = Vector3.Angle(transform.forward, dirToTarget);
        
        Vector3 cross = Vector3.Cross(transform.forward, dirToTarget);
        int targetDirection = 0;
        if (cross.y > 0.1f) targetDirection = 1;
        else if (cross.y < -0.1f) targetDirection = -1;
        
        if (turnDirection == targetDirection)
        {
            brain.GiveReward(currentState, 10f);
            
            if (currentAngle < lastAngleToTarget - 3f)
            {
                brain.GiveReward(currentState, 5f);
            }
        }
        else if (turnDirection == -targetDirection)
        {
            brain.GivePunishment(currentState, 12f);
        }
        else if (currentAngle < 10f)
        {
            brain.GivePunishment(currentState, 5f);
        }
        
        lastAngleToTarget = currentAngle;
    }
    
    void Shoot()
    {
        enemy.StopMoving(transform.position);
        
        Quaternion? finalBulletRotation = null;
        
        if (canSeeTarget && target != null)
        {
            Vector3 dirToTarget = target.position - transform.position;
            dirToTarget.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            
            // Hedef Alma desteği
            // Açı çok kötüyse önce dön, ateş etme
            if (angle > 10f)
            {
                // Hedefe doğru hızlı dönüş
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnAmount);
                brain.GivePunishment(currentState, 5f);
                return; // Ateş etme, sadece dön
            }
            
            // Küçük açı düzeltmesi (10 derece altı)
            if (angle > 0f && angle <= 10f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                // RotateTowards yerine direkt atama yaparak "boşluk" sorununu çözüyoruz
                transform.rotation = targetRotation;
                
                // Merminin tam hedefe gitmesi için firePoint'ten hesapla
                if (enemy.firePoint != null)
                {
                    // Hedefin merkezine (veya biraz yukarısına) nişan al
                    Vector3 aimDirection = target.position - enemy.firePoint.position;
                    finalBulletRotation = Quaternion.LookRotation(aimDirection);
                }
            }
            
            // ŞİMDİ ATEŞ ET
            enemy.Shoot(finalBulletRotation);
            
            dirToTarget = target.position - transform.position;
            dirToTarget.y = 0;
            angle = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angle < 5f)
            {
                brain.GiveReward(currentState, 8f);
            }
            else if (angle < 15f)
            {
                brain.GiveReward(currentState, 4f);
            }
            else if (angle < 30f)
            {
                brain.GiveReward(currentState, 1f);
            }
        }
        else
        {
            enemy.Shoot();
            brain.GivePunishment(currentState, 15f);
        }
    }
    
    void DashLeft()
    {
        Vector3 direction = -transform.right;
        enemy.Dash(direction);
        
        if (enemy.isBulletNearby)
        {
            brain.GiveReward(currentState, 10f);
        }
        else
        {
            brain.GivePunishment(currentState, 3f);
        }
    }
    
    void DashRight()
    {
        Vector3 direction = transform.right;
        enemy.Dash(direction);
        
        if (enemy.isBulletNearby)
        {
            brain.GiveReward(currentState, 10f);
        }
        else
        {
            brain.GivePunishment(currentState, 3f);
        }
    }
    
    void Jump()
    {
        enemy.Jump();
        
        if (enemy.isBulletNearby)
        {
            brain.GiveReward(currentState, 5f);
        }
    }
    
    void Wait()
    {
        enemy.StopMoving(transform.position);
        
        if (canSeeTarget)
        {
            brain.GivePunishment(currentState, 5f);
        }
        else
        {
            brain.GivePunishment(currentState, 0.5f);
        }
    }
    
    // ============ REWARD CALLBACKS ============
    
    public void OnHitTarget()
    {
        brain.GiveReward(currentState, 200f);
        Debug.Log($"[AI] ★★ HIT! +200 | ε:{brain.GetExplorationRate():F4}");
    }
    
    public void OnTakeDamage()
    {
        brain.GivePunishment(currentState, 50f);
    }
    
    public void OnDie()
    {
        brain.GivePunishment(currentState, 300f);
        Debug.Log($"[AI] ✗✗ DIED -300 | ε:{brain.GetExplorationRate():F4}");
    }
    
    public void OnKillTarget()
    {
        brain.GiveReward(currentState, 1000f);
        Debug.Log($"[AI] ★★★ KILL! +1000 | ε:{brain.GetExplorationRate():F4} | States:{brain.GetStateCount()}");
    }
    
    public void OnDodgeBullet()
    {
        brain.GiveReward(currentState, 30f);
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || brain == null) return;
        
        if (canSeeTarget && target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, target.position + Vector3.up);
            
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            
            if (angle < 5f) Gizmos.color = Color.green;
            else if (angle < 15f) Gizmos.color = Color.yellow;
            else if (angle < 30f) Gizmos.color = new Color(1f, 0.5f, 0f);
            else Gizmos.color = Color.red;
            
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 5f);
        }
        
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Vector3 forward = transform.forward * visionRange;
        Vector3 left = Quaternion.Euler(0, -visionAngle, 0) * forward;
        Vector3 right = Quaternion.Euler(0, visionAngle, 0) * forward;
        
        Gizmos.DrawRay(transform.position + Vector3.up, left);
        Gizmos.DrawRay(transform.position + Vector3.up, right);
    }
}