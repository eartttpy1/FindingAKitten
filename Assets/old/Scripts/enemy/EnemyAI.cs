using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float timeBetweenAttacks = 1.5f;
    private float nextAttackTime;

    [Header("Senses")]
    public float sightRange = 15f;
    public float fieldOfView = 120f;
    public Transform player;

    [Header("Drops")]
    public GameObject ammoPrefab; // ลาก Prefab กล่องกระสุนมาใส่ช่องนี้
    [Range(0f, 100f)]
    public float dropChance = 100f; // โอกาสดรอป (0-100) ค่าเริ่มต้นคือ 100%

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spotSound;
    public AudioClip hitSound;
    public AudioClip dieSound;
    public AudioClip attackSound;

    // [ADDED] ตัวแปรสำหรับคุม Animation
    private Animator anim;
    private NavMeshAgent agent;
    private bool isDead = false;
    private bool hasSpottedPlayer = false;

    private void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        // [ADDED] ดึง Animator (ใช้ GetComponentInChildren เผื่อใส่ไว้ในลูก)
        anim = GetComponentInChildren<Animator>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

        if (player == null && GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (isDead) return;

        // [ADDED] สั่ง Animation เดิน/ยืน
        // เช็คว่า Agent กำลังขยับอยู่ไหม โดยดูจากความเร็ว
        if (anim != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            anim.SetBool("isMoving", isMoving);
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer(distanceToPlayer))
        {
            EngagePlayer(distanceToPlayer);
        }
        else if (hasSpottedPlayer)
        {
            EngagePlayer(distanceToPlayer);
        }
    }

    // ... (ฟังก์ชัน CanSeePlayer เหมือนเดิม) ...
    private bool CanSeePlayer(float distance)
    {
        if (distance > sightRange) return false;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleBetween < fieldOfView / 2f)
        {
            RaycastHit hit;
            // ยก Raycast ขึ้นนิดหน่อย (+Vector3.up) จะได้ไม่ชนพื้น
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, sightRange))
            {
                if (hit.transform.CompareTag("Player")) return true;
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
        }

        agent.SetDestination(player.position);

        if (distance <= attackRange)
        {
            // หันหน้าหา Player (Logic 3D) แต่ Sprite จะหันหากล้องเองจาก script Billboard
            Vector3 lookPos = player.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + timeBetweenAttacks;
            }
        }
    }

    private void Attack()
    {
        // [ADDED] สั่ง Animation โจมตี
        if (anim != null) anim.SetTrigger("Attack");

        PlaySound(attackSound);

        Debug.Log("Enemy Punches Player!");

        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
        
        // ถ้ามี Script อยู่จริง ให้สั่งลดเลือด
        if (pHealth != null) 
        {
            pHealth.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hasSpottedPlayer = true;
        currentHealth -= amount;
        PlaySound(hitSound);

        // [ADDED] ถ้ามี Animation เจ็บ (Hurt) ใส่ตรงนี้ได้

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.ResetPath(); // ยกเลิกเส้นทางเดิน
        PlaySound(dieSound);
        Destroy(gameObject, 10f);

        GetComponent<Collider>().enabled = false;

        // [ADDED] สั่ง Animation ตาย
        if (anim != null) anim.SetTrigger("Die");

        DropAmmo();
    }

   // [ADDED] ฟังก์ชันจัดการการดรอปของแบบแนบติดพื้น
    private void DropAmmo()
    {
        if (ammoPrefab != null)
        {
            if (Random.Range(0f, 100f) <= dropChance)
            {
                // ตั้งค่าจุดดรอปเริ่มต้นไว้ที่ตัวศัตรูก่อน
                Vector3 dropPosition = transform.position;

                // ยิง Raycast ลงไปข้างล่าง (Vector3.down) เพื่อหาพื้นดิน
                RaycastHit hit;
                // ยิงจากกลางตัวศัตรู ลงไป 5 เมตร
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 5f))
                {
                    // ถ้าเลเซอร์ชนอะไรสักอย่าง (พื้น) ให้เปลี่ยนจุดดรอปเป็นจุดที่ชน
                    // บวกความสูงขึ้นมานิดเดียว (0.1f) เพื่อไม่ให้โมเดลจมพื้น
                    dropPosition = hit.point + (Vector3.up * 0.1f); 
                }

                // เสกไอเทมขึ้นมาในฉากที่ตำแหน่งพื้นพอดี
                Instantiate(ammoPrefab, dropPosition, Quaternion.identity);
                Debug.Log("ดรอปกระสุนที่พื้นแล้ว!");
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


    private void OnDrawGizmosSelected()
    {
        // 1. วาดวงกลมระยะโจมตี (Attack Range) - สีแดง
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. วาดวงกลมระยะมองเห็น (Sight Range) - สีเหลือง
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // 3. วาดเส้นขอบเขตมุมมอง (Field of View)
        Vector3 viewAngleA = DirFromAngle(-fieldOfView / 2, false);
        Vector3 viewAngleB = DirFromAngle(fieldOfView / 2, false);

        Gizmos.color = Color.yellow;
        // วาดเส้นซ้ายและขวาของมุมมอง
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * sightRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * sightRange);
    }

    // ฟังก์ชันช่วยคำนวณทิศทางจากมุมองศา
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}