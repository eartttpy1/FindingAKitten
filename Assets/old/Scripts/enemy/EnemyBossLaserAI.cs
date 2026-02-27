using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBossLaserAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 500f; 
    private float currentHealth;
    public float moveSpeed = 2.0f; 

    [Header("Combat (Laser)")]
    public float attackRange = 20f;       
    public float chargeTime = 1.5f;       
    public float laserDuration = 2.0f;    
    public float laserDamagePerTick = 10f;
    public float damageTickRate = 0.2f;   
    public float attackCooldown = 3.0f;   

    private float nextDamageTick;
    private bool isAttacking = false;     

    [Header("Laser Setup")]
    public LineRenderer laserLine;        
    public Transform firePoint;

    [Header("Phase 2 (Summoning)")]
    public GameObject minionPrefab;       
    public float summonCooldown = 15f;    
    public float summonSpawnRadius = 3f;  
    public ParticleSystem summonEffect;   
    // ---------------------------------------------------------
    // [เพิ่มใหม่] ตัวแปรเสียงตอนเสกลูกน้อง
    // ---------------------------------------------------------
    public AudioClip summonSound;         

    private bool isPhase2 = false;
    private float nextSummonTime;

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

    [Header("Senses")]
    public float sightRange = 30f;
    public float fieldOfView = 120f;
    public Transform player;
    public Collider playerCollider;

    [Header("Audio & Voice Lines")]
    public AudioSource audioSource;
    public AudioClip spotSound;
    public AudioClip chargeLaserSound; 
    public AudioClip fireLaserSound;   
    public AudioClip hitSound;

    [Header("Boss Voice Lines")]
    public AudioClip voiceLine90; 
    public AudioClip voiceLine50; 
    public AudioClip dieSound;    

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
            laserLine.enabled = false; 
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

        if (isPhase2 && Time.time >= nextSummonTime)
        {
            SummonMinions();
        }

        bool isMoving = agent.velocity.magnitude > 0.1f;
        if (anim != null) anim.SetBool("isMoving", isMoving);

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
            StartCoroutine(LaserAttackRoutine());
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

        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void ClearAnimationTriggers()
    {
        if (anim != null)
        {
            anim.ResetTrigger("ChargeLaser");
            anim.ResetTrigger("FireLaser");
            anim.SetBool("isMoving", false); 
        }
    }

    private IEnumerator LaserAttackRoutine()
    {
        isAttacking = true;

        ClearAnimationTriggers(); 

        if (anim != null) anim.SetTrigger("ChargeLaser");
        PlaySound(chargeLaserSound);

        yield return new WaitForSeconds(chargeTime);

        ClearAnimationTriggers(); 

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
                laserLine.SetPosition(0, firePoint.position);

                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, attackRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    laserLine.SetPosition(1, hit.point);
                    bool isHitPlayer = hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player");

                    if (isHitPlayer)
                    {
                        if (Time.time >= nextDamageTick)
                        {
                            PlayerHealth pHealth = hit.transform.GetComponent<PlayerHealth>();
                            if (pHealth == null) pHealth = hit.transform.root.GetComponent<PlayerHealth>();

                            if (pHealth != null)
                            {
                                pHealth.TakeDamage(laserDamagePerTick);
                                nextDamageTick = Time.time + damageTickRate; 
                            }
                        }
                    }
                    else if (hit.transform.gameObject == this.gameObject)
                    {
                        laserLine.SetPosition(1, firePoint.position + firePoint.forward * attackRange);
                    }
                }
                else
                {
                    laserLine.SetPosition(1, firePoint.position + firePoint.forward * attackRange);
                }
            }
            yield return null; 
        }

        if (laserLine != null) laserLine.enabled = false;

        ClearAnimationTriggers();

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

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
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= 0.9f && healthPercent > 0.5f && !hasPlayed90)
        {
            PlaySound(voiceLine90);
            hasPlayed90 = true;
        }
        else if (healthPercent <= 0.5f && healthPercent > 0f && !hasPlayed50)
        {
            PlaySound(voiceLine50);
            hasPlayed50 = true;

            if (!isPhase2)
            {
                isPhase2 = true;
                nextSummonTime = Time.time; 
                Debug.Log("Boss entered Phase 2! Summoning minions...");
            }
        }
    }

    private void SummonMinions()
    {
        if (minionPrefab == null) return;

        Vector3 leftPos1 = transform.position - transform.right * summonSpawnRadius;
        Vector3 leftPos2 = transform.position - transform.right * (summonSpawnRadius + 2f);
        Vector3 rightPos1 = transform.position + transform.right * summonSpawnRadius;
        Vector3 rightPos2 = transform.position + transform.right * (summonSpawnRadius + 2f);

        if (summonEffect != null) summonEffect.Play();

        // ---------------------------------------------------------
        // [เพิ่มใหม่] เล่นเสียงตอนที่เสกลูกน้องสำเร็จ
        // ---------------------------------------------------------
        PlaySound(summonSound);

        Instantiate(minionPrefab, leftPos1, transform.rotation);
        Instantiate(minionPrefab, leftPos2, transform.rotation);
        Instantiate(minionPrefab, rightPos1, transform.rotation);
        Instantiate(minionPrefab, rightPos2, transform.rotation);

        nextSummonTime = Time.time + summonCooldown;
    }

    private void Die()
    {
        if (isDead) return; 
        isDead = true;
        agent.isStopped = true;
        GetComponent<Collider>().enabled = false;

        StopAllCoroutines();
        if (laserLine != null) laserLine.enabled = false; 

        ClearAnimationTriggers();
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