using UnityEngine;
using UnityEngine.AI;

public class EnemyShooterAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 50f;
    private float currentHealth;
    public float moveSpeed = 3.0f;

    [Header("Combat")]
    public float attackRange = 10f;
    public float timeBetweenAttacks = 2.0f;

    // ---------------------------------------------------------
    // [NEW] เพิ่มตัวแปร Reaction Time (เวลาดีเลย์ก่อนเริ่มยิงนัดแรก)
    // ---------------------------------------------------------
    public float reactionTime = 5.0f; // รอ 5 วินาทีก่อนยิง
    private float spotTime;           // ตัวแปรจำเวลาที่เจอตัว
    private float nextAttackTime;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Senses")]
    public float sightRange = 20f;
    public float fieldOfView = 120f;
    public Transform player;

    [Header("Drops")]
    public float dropYOffset = 0.5f; // <--- [เพิ่มใหม่] ปรับความสูงของไอเทมตอนดรอป (ถ้าจมดินให้เพิ่มเลขนี้)

    public GameObject ammoPrefab; 
    [Range(0f, 100f)] public float dropChance = 100f;

    [Space(10)]
    public GameObject item2Prefab; 
    [Range(0f, 100f)] public float item2DropChance = 50f;

    [Space(10)]
    public GameObject item3Prefab; 
    [Range(0f, 100f)] public float item3DropChance = 25f;

    [Header("Targeting")]
    public Collider playerCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spotSound;
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip dieSound;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isDead = false;
    private bool hasSpottedPlayer = false;
    private Collider myCollider;

    private void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        myCollider = GetComponent<Collider>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange - 2f;

        if (player == null && GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerCollider == null && player != null)
        {
            playerCollider = player.GetComponent<Collider>();
        }
    }

    private void Update()
    {
        if (isDead || player == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        if (anim != null) anim.SetBool("isMoving", isMoving);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer(distanceToPlayer) || hasSpottedPlayer)
        {
            EngagePlayer(distanceToPlayer);
        }
    }

    private bool CanSeePlayer(float distance)
    {
        if (distance > sightRange) return false;

        Vector3 targetCheckPos = (playerCollider != null) ? playerCollider.bounds.center : player.position + Vector3.up;
        Vector3 eyePos = transform.position + Vector3.up;
        Vector3 directionToPlayer = (targetCheckPos - eyePos).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(eyePos, directionToPlayer, out hit, sightRange))
            {
                if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void EngagePlayer(float distance)
    {
        if (!hasSpottedPlayer)
        {
            hasSpottedPlayer = true;
            PlaySound(spotSound);

            spotTime = Time.time;
        }

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (distance <= attackRange)
        {
            agent.isStopped = true;

            if (Time.time < spotTime + reactionTime)
            {
                return;
            }

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + timeBetweenAttacks;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    private void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        PlaySound(shootSound);

        if (projectilePrefab != null && firePoint != null)
        {
            Vector3 targetPosition;
            if (playerCollider != null) targetPosition = playerCollider.bounds.center;
            else targetPosition = player.position + Vector3.up;

            Vector3 aimDir = (targetPosition - firePoint.position).normalized;
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(aimDir));

            Collider bulletCollider = bullet.GetComponent<Collider>();
            if (bulletCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(bulletCollider, myCollider);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        if (!hasSpottedPlayer)
        {
            hasSpottedPlayer = true;
            spotTime = Time.time;
        }

        currentHealth -= amount;
        PlaySound(hitSound);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        GetComponent<Collider>().enabled = false;
        if (anim != null) anim.SetTrigger("Die");
        PlaySound(dieSound);
        Destroy(gameObject, 10f);

        // เปลี่ยนมาเรียกฟังก์ชันสุ่มดรอปไอเทมทั้ง 3 ชิ้น
        TryDropItem(ammoPrefab, dropChance);
        TryDropItem(item2Prefab, item2DropChance);
        TryDropItem(item3Prefab, item3DropChance);
    }

    // ---------------------------------------------------------
    // [อัปเดต] ยุบรวมระบบ Drop ให้เป็นฟังก์ชันเดียว รับค่า Prefab และโอกาสดรอป
    // ---------------------------------------------------------
    private void TryDropItem(GameObject itemPrefab, float chance)
    {
        if (itemPrefab == null) return; 

        if (Random.Range(0f, 100f) <= chance)
        {
            Vector3 dropPosition = transform.position;

            RaycastHit hit;
            // ยิง Raycast หาระดับพื้น
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10f))
            {
                // ใช้ค่า dropYOffset ยกลอยขึ้นมา เพื่อไม่ให้โมเดลจมดิน
                dropPosition = hit.point + (Vector3.up * dropYOffset); 
            }

            // สุ่มตำแหน่งกระจายตัวแนวราบ
            Vector2 randomSpread = Random.insideUnitCircle * 1.0f; // ขยายวงกระจายให้กว้างขึ้นนิดนึง
            dropPosition += new Vector3(randomSpread.x, 0, randomSpread.y);

            Instantiate(itemPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"ดรอป {itemPrefab.name} สำเร็จ!");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 viewAngleA = DirFromAngle(-fieldOfView / 2, false);
        Vector3 viewAngleB = DirFromAngle(fieldOfView / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * sightRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * sightRange);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}