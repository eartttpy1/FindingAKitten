using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBossLaserAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 500f; // เลือดบอสต้องเยอะ!
    private float currentHealth;
    public float moveSpeed = 2.0f; // บอสมักจะเดินช้าแต่น่าเกรงขาม

    [Header("Combat (Laser)")]
    public float attackRange = 20f;       // ระยะทำการเลเซอร์ (ไกลมาก)
    public float chargeTime = 1.5f;       // เวลาชาร์จพลังก่อนยิง (ให้ผู้เล่นมีเวลาหลบ)
    public float laserDuration = 2.0f;    // ระยะเวลายิงเลเซอร์ค้างไว้
    public float laserDamagePerTick = 10f;// ดาเมจต่อครั้งที่โดนเลเซอร์
    public float damageTickRate = 0.2f;   // โดนดาเมจทุกๆ 0.2 วินาที (ถ้าแช่ในเลเซอร์)
    public float attackCooldown = 3.0f;   // คูลดาวน์หลังยิงเสร็จ

    private float nextDamageTick;
    private bool isAttacking = false;     // กำลังชาร์จหรือยิงเลเซอร์อยู่ไหม

    [Header("Laser Setup")]
    public LineRenderer laserLine;        // ** เส้นเลเซอร์ **
    public Transform firePoint;

    [Header("Senses")]
    public float sightRange = 30f;
    public float fieldOfView = 120f;
    public Transform player;
    public Collider playerCollider;

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

    [Header("Audio & Voice Lines")]
    public AudioSource audioSource;
    public AudioClip spotSound;
    public AudioClip chargeLaserSound; // เสียงตอนกำลังชาร์จ (ถ้ามี)
    public AudioClip fireLaserSound;   // เสียงตอนยิงเลเซอร์
    public AudioClip hitSound;

    [Header("Boss Voice Lines")]
    public AudioClip voiceLine90; // เสียงตอนเลือดลดไป 10% (เหลือ 90%)
    public AudioClip voiceLine50; // เสียงตอนเลือดเหลือ 50%
    public AudioClip dieSound;    // เสียงตอนตาย (0%)

    // เช็คว่าเล่นเสียงไปหรือยัง จะได้ไม่เล่นซ้ำรัวๆ
    private bool hasPlayed90 = false;
    private bool hasPlayed50 = false;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isDead = false;
    private bool hasSpottedPlayer = false;

    private void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange - 2f;

        if (laserLine != null)
        {
            laserLine.enabled = false; // ปิดเลเซอร์ไว้ก่อนตอนเริ่ม
            laserLine.useWorldSpace = true;
        }

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

        // ถ้ากำลังโจมตี (ชาร์จ/ยิงเลเซอร์) ไม่ต้องเดิน ให้ยืนนิ่งๆ และหันหน้าหาผู้เล่น
        if (isAttacking)
        {
            agent.isStopped = true;
            FacePlayer();
            return;
        }

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
                    return true;
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

        FacePlayer();

        if (distance <= attackRange)
        {
            agent.isStopped = true;
            StartCoroutine(LaserAttackRoutine()); // เริ่มกระบวนการยิงเลเซอร์
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    private void FacePlayer()
    {
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;

        // ค่อยๆ หันหน้าไปหาผู้เล่น (ให้เลเซอร์ส่ายตามได้นิดหน่อย)
        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // ---------------------------------------------------------
    // ระบบยิงเลเซอร์ (ชาร์จ -> ยิงค้าง -> พัก)
    // ---------------------------------------------------------
    private IEnumerator LaserAttackRoutine()
    {
        isAttacking = true;

        // 1. ช่วงชาร์จพลัง
        if (anim != null) anim.SetTrigger("ChargeLaser");
        PlaySound(chargeLaserSound);
        // (คุณสามารถใส่ Particle System ชาร์จพลังตรง FirePoint ได้ที่นี่)

        yield return new WaitForSeconds(chargeTime);

        // 2. ช่วงยิงเลเซอร์
        if (anim != null) anim.SetTrigger("FireLaser");
        PlaySound(fireLaserSound);

        if (laserLine != null) laserLine.enabled = true;

        float timer = 0f;
        while (timer < laserDuration)
        {
            if (isDead) break;

            timer += Time.deltaTime;

            if (laserLine != null && firePoint != null)
            {
                // จุดเริ่มต้นเลเซอร์คือปากกระบอก
                laserLine.SetPosition(0, firePoint.position);

                // ยิง Raycast ไปข้างหน้า เพื่อหาจุดสิ้นสุดเลเซอร์ (ชนอะไรไหม?)
                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, attackRange))
                {
                    // เลเซอร์หยุดตรงที่ชน
                    laserLine.SetPosition(1, hit.point);

                    // ถ้าชน Player ให้ทำดาเมจเป็นรอบๆ (Tick)
                    if (hit.transform.CompareTag("Player") && Time.time >= nextDamageTick)
                    {
                        PlayerHealth pHealth = hit.transform.GetComponent<PlayerHealth>();
                        if (pHealth == null) pHealth = hit.transform.root.GetComponent<PlayerHealth>();

                        if (pHealth != null)
                        {
                            pHealth.TakeDamage(laserDamagePerTick);
                            nextDamageTick = Time.time + damageTickRate; // เซ็ตเวลาโดนดาเมจครั้งต่อไป
                        }
                    }
                }
                else
                {
                    // ถ้าไม่ชนอะไรเลย ให้ทะลุไปจนสุดระยะ
                    laserLine.SetPosition(1, firePoint.position + firePoint.forward * attackRange);
                }
            }
            yield return null; // รอเฟรมถัดไป
        }

        // 3. ปิดเลเซอร์ และเข้าสู่ช่วง Cooldown
        if (laserLine != null) laserLine.enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // ---------------------------------------------------------
    // ระบบโดนโจมตี & เสียงบอส
    // ---------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hasSpottedPlayer = true;

        currentHealth -= amount;
        PlaySound(hitSound);

        CheckBossVoiceLines();

        if (currentHealth <= 0) Die();
    }

    private void CheckBossVoiceLines()
    {
        // คำนวณเปอร์เซ็นต์เลือด (0.0 ถึง 1.0)
        float healthPercent = currentHealth / maxHealth;

        // ถ้าเลือดต่ำกว่าหรือเท่ากับ 90% (ลด 10%) และยังไม่เคยพูดประโยคนี้
        if (healthPercent <= 0.9f && healthPercent > 0.5f && !hasPlayed90)
        {
            PlaySound(voiceLine90);
            hasPlayed90 = true;
        }
        // ถ้าเลือดต่ำกว่าหรือเท่ากับ 50% และยังไม่เคยพูดประโยคนี้
        else if (healthPercent <= 0.5f && healthPercent > 0f && !hasPlayed50)
        {
            PlaySound(voiceLine50);
            hasPlayed50 = true;
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        GetComponent<Collider>().enabled = false;

        StopAllCoroutines();
        if (laserLine != null) laserLine.enabled = false; // ปิดเลเซอร์ตอนตาย

        if (anim != null) anim.SetTrigger("Die");

        // เสียงตอนเลือด 0% (ตาย)
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