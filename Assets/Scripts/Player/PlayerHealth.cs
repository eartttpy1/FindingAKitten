using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ใช้รีเซ็ตฉากตอนตาย

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Text healthText;       // ตัวเลขเลือด (เช่น 100%)
    public Image damageOverlay;   // รูปสีแดงเต็มจอ (Red Vignette)

    [Header("Settings")]
    public float redScreenThreshold = 40f; // เลือดต่ำกว่านี้จอจะเริ่มแดง

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Player Took Damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. อัปเดตตัวเลข %
        if (healthText != null)
        {
            // คำนวณเป็นเปอร์เซ็นต์ (0-100)
            float percentage = (currentHealth / maxHealth) * 100f;
            healthText.text = Mathf.CeilToInt(percentage) + "%";

            // เปลี่ยนสีตัวเลขตามความวิกฤต
            if (percentage < 30) healthText.color = Color.red;
            else healthText.color = Color.green;
        }

        // 2. อัปเดตจอแดง (Vignette)
        if (damageOverlay != null)
        {
            float healthPercent = currentHealth / maxHealth; // 0 ถึง 1

            if (currentHealth < redScreenThreshold)
            {
                // สูตรคำนวณความเข้ม (ยิ่งเลือดน้อย ยิ่งเข้ม)
                // 1 - (เลือดปัจจุบัน / เลือดที่เริ่มแดง)
                float alpha = 1f - (currentHealth / redScreenThreshold);

                // ปรับสี Alpha ของภาพ
                Color c = damageOverlay.color;
                c.a = Mathf.Clamp(alpha, 0f, 0.8f); // ไม่ให้เข้มเกิน 0.8 เดี๋ยวมองไม่เห็นทาง
                damageOverlay.color = c;
            }
            else
            {
                // เลือดยังเยอะอยู่ ปิดจอแดง
                Color c = damageOverlay.color;
                c.a = 0f;
                damageOverlay.color = c;
            }
        }
    }

    void Die()
    {
        Debug.Log("Game Over!");
        // รีโหลดฉากเดิม (ตายแล้วเริ่มใหม่)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}