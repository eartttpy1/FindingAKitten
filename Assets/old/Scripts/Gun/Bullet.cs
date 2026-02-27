using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. ถ้าชนศัตรู
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // ทำลายกระสุน
        }
        // 2. ถ้าชนศัตรู
        if (other.CompareTag("EnemyShooter"))
        {
            EnemyShooterAI enemy = other.GetComponent<EnemyShooterAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // ทำลายกระสุน
        }

        if (other.CompareTag("EnemyAss"))
        {
            EnemyMachineGunAI enemy = other.GetComponent<EnemyMachineGunAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // ทำลายกระสุน
        }

        if (other.CompareTag("EnemyShot"))
        {
            EnemyShotgunAI enemy = other.GetComponent<EnemyShotgunAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // ทำลายกระสุน
        }

        if (other.CompareTag("ZombieBoss"))
        {
            EnemyBossLaserAI enemy = other.GetComponent<EnemyBossLaserAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // ทำลายกระสุน
        }

        // 2. ถ้าชนกำแพง หรือ พื้น (เพิ่มส่วนนี้)
        else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            Destroy(gameObject); // ทำลายกระสุนทันทีเมื่อชนกำแพง
        }
    }
}