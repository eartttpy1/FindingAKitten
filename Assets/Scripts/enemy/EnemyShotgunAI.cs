using UnityEngine;
using UnityEngine.AI;

public class EnemyShotgunAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f; // เลือดเยอะหน่อย เพราะต้องเดินลุยเข้ามาใกล้
    private float currentHealth;
    public float moveSpeed = 3.5f; // เดินเร็วปานกลาง

    [Header("Combat (Shotgun)")]
    public float attackRange = 7f;         // ระยะยิง (ใกล้กว่าปืนพกและปืนกล)
    public float timeBetweenAttacks = 2.5f; // ยิงช้า (ต้องปั๊มลูกซอง)
    public int pelletCount = 5;             // ยิงออกไปกี่เม็ดต่อ 1 นัด (เช่น 5-8 เม็ด)
    public float spreadAngle = 15f;        // องศาความบานของกระสุน (ยิ่งเยอะยิ่งบาน)

    private float nextAttackTime;

    [Header("Reaction")]
    public float reactionTime = 0.5f; // อาจจะไวกว่าตัวอื่นนิดนึง เพราะอยู่ใกล้
    private float spotTime;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Senses")]
    public float sightRange = 15f; // ระยะมองเห็น ไม่ต้องไกลมาก
    public float fieldOfView = 120f;
    public Transform player;

    // ---------------------------------------------------------
    // [ADDED] ระบบดรอปไอเทม 3 ช่อง
    // ---------------------------------------------------------
    [Header("Drops")]
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
    public AudioClip shootSound; // ควรเป็นเสียงปัง! ดังๆ
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
        agent.stoppingDistance = attackRange - 1.5f; // หยุดก่อนถึงระยะนิดนึง

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

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (distance <= attackRange)
        {
            agent.isStopped = true;

            if (Time.time < spotTime + reactionTime) return;

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

            // ทิศทางหลักที่จะยิงไปหาผู้เล่น
            Vector3 aimDir = (targetPosition - firePoint.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(aimDir);

            // [NEW] ลูปสร้างกระสุนหลายลูก (ลูกซอง)
            for (int i = 0; i < pelletCount; i++)
            {
                // สุ่มองศาความบาน ทั้งแกน X (ซ้ายขวา) และ Y (บนล่าง)
                float spreadX = Random.Range(-spreadAngle, spreadAngle);
                float spreadY = Random.Range(-spreadAngle, spreadAngle);

                // เอาทิศทางหลัก มาหมุนเพิ่มตามองศาที่สุ่มไว้
                Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
                Quaternion finalRotation = baseRotation * spreadRotation;

                // สร้างกระสุน 1 เม็ด
                GameObject bullet = Instantiate(projectilePrefab, firePoint.position, finalRotation);

                Collider bulletCollider = bullet.GetComponent<Collider>();
                if (bulletCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(bulletCollider, myCollider);
                }
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
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 5f))
            {
                dropPosition = hit.point + (Vector3.up * 0.1f);
            }

            // สุ่มตำแหน่งกระจายตัวเล็กน้อย
            Vector2 randomSpread = Random.insideUnitCircle * 0.5f;
            dropPosition += new Vector3(randomSpread.x, 0, randomSpread.y);

            Instantiate(itemPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"ดรอป {itemPrefab.name} ที่พื้นแล้ว!");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}