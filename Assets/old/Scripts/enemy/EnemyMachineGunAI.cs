using UnityEngine;
using UnityEngine.AI;
using System.Collections; // ** ต้องมีอันนี้เพื่อใช้ Coroutine (ระบบยิงรัว) **

public class EnemyMachineGunAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 80f; // เลือดอาจจะเยอะกว่าปืนพกนิดนึง
    private float currentHealth;
    public float moveSpeed = 2.5f; // เดินช้ากว่านิดนึงเพราะถือปืนหนัก

    [Header("Combat (Machine Gun)")]
    public float attackRange = 15f;      // ระยะเริ่มยิง (ไกลกว่าปืนพก)
    public int burstCount = 5;           // ยิงชุดละกี่นัด (เช่น 5 นัดรัวๆ)
    public float fireRate = 0.1f;        // ความเร็วระหว่างนัดใน 1 ชุด (0.1 = รัวมาก)
    public float burstCooldown = 2.0f;   // ระยะเวลาพักเพื่อรีโหลดก่อนยิงชุดต่อไป
    public float bulletSpread = 1.0f;    // ความกระจายของกระสุน (จะได้หลบได้บ้าง)

    private float nextBurstTime;
    private bool isFiringBurst = false;  // เช็คว่ากำลังยิงรัวอยู่ไหม จะได้ไม่เดิน

    [Header("Reaction")]
    public float reactionTime = 1.0f;
    private float spotTime;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Senses")]
    public float sightRange = 25f;
    public float fieldOfView = 120f;
    public Transform player;

    // ---------------------------------------------------------
    // [ADDED] ระบบดรอปไอเทม 3 ช่อง
    // ---------------------------------------------------------
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
            playerCollider = player.GetComponent<Collider>();
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

        // หันหน้าหาผู้เล่นเสมอ
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // ถ้ากำลังยิงรัวอยู่ ให้หยุดเดินและไม่ต้องทำคำสั่งอื่น
        if (isFiringBurst)
        {
            agent.isStopped = true;
            return;
        }

        if (distance <= attackRange)
        {
            agent.isStopped = true;

            if (Time.time < spotTime + reactionTime) return;

            if (Time.time >= nextBurstTime)
            {
                // สั่งยิงรัวเป็นชุด
                StartCoroutine(FireBurstRoutine());
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    // ระบบ Coroutine สำหรับยิงรัวทีละนัด
    private IEnumerator FireBurstRoutine()
    {
        isFiringBurst = true;

        if (anim != null) anim.SetTrigger("Attack");

        for (int i = 0; i < burstCount; i++)
        {
            if (isDead) break; // ถ้าตายระหว่างยิง ให้หยุดยิงทันที

            PlaySound(shootSound);

            if (projectilePrefab != null && firePoint != null)
            {
                Vector3 targetPosition;
                if (playerCollider != null) targetPosition = playerCollider.bounds.center;
                else targetPosition = player.position + Vector3.up;

                // คำนวณทิศทาง + ใส่ความกระจายของกระสุน (Spread)
                Vector3 aimDir = (targetPosition - firePoint.position).normalized;

                float xOffset = Random.Range(-bulletSpread, bulletSpread);
                float yOffset = Random.Range(-bulletSpread, bulletSpread);
                Vector3 spread = new Vector3(xOffset, yOffset, 0).normalized * 0.1f; // ปรับค่าน้อยๆ จะได้ไม่เบี้ยวเกิน

                aimDir += spread;

                GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(aimDir));

                Collider bulletCollider = bullet.GetComponent<Collider>();
                if (bulletCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(bulletCollider, myCollider);
                }
            }

            // รอเวลาเสี้ยววินาทีก่อนยิงนัดถัดไป
            yield return new WaitForSeconds(fireRate);
        }

        // ยิงครบชุดแล้ว ตั้งเวลาดีเลย์ก่อนยิงชุดใหม่
        isFiringBurst = false;
        nextBurstTime = Time.time + burstCooldown;
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

        // หยุดการยิงรัวทันทีเมื่อตาย
        StopAllCoroutines();
        isFiringBurst = false;

        if (anim != null) anim.SetTrigger("Die");
        PlaySound(dieSound);
        Destroy(gameObject, 10f);

        // ---------------------------------------------------------
        // [ADDED] เรียกฟังก์ชันดรอปของทั้ง 3 ชิ้น
        // ---------------------------------------------------------
        TryDropItem(ammoPrefab, dropChance);
        TryDropItem(item2Prefab, item2DropChance);
        TryDropItem(item3Prefab, item3DropChance);
    }

    // ---------------------------------------------------------
    // [ADDED] ฟังก์ชันจัดการการดรอปของแบบแนบติดพื้น + สุ่มตำแหน่งกระจายตัว
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