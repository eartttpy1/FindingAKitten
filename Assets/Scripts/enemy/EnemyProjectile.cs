using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifeTime = 5f; // กันกระสุนลอยรกฉาก

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // ยิงไปข้างหน้า (ตามทิศที่ศัตรูหันมา)
        rb.linearVelocity = transform.forward * speed; // Unity 6 ใช้ linearVelocity, เก่ากว่าใช้ velocity
        
        Destroy(gameObject, lifeTime); // ทำลายตัวเองเมื่อหมดเวลา
    }

    void OnTriggerEnter(Collider other)
    {
        // ถ้าชน Player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // ลดเลือดเรา
            }
            Destroy(gameObject); // กระสุนหายไป
        }
        // ถ้าชนกำแพง (ที่ไม่ใช่ศัตรูด้วยกันเอง)
        else if (!other.CompareTag("Enemy") && !other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}