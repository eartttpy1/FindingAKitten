using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Settings")]
    public float healAmount = 25f;  // จำนวนเลือดที่ต้องการฟื้นฟู (เช่น กล่องเล็ก 25, กล่องใหญ่ 50)

    [Header("Effects")]
    public AudioClip pickupSound;   // เสียงตอนเก็บไอเทม (เสียงดื่มน้ำ/เสียงวิ้งๆ)
    public GameObject pickupEffect; // Effect ตอนเก็บ (ถ้ามี)

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าวัตถุที่มาชนคือ Player หรือไม่
        if (other.CompareTag("Player"))
        {
            // ดึงสคริปต์ PlayerHealth จากตัว Player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // ** เช็คว่าเลือดเต็มหรือยัง? ** // ถ้าเลือดยังไม่เต็ม ถึงจะยอมให้เก็บไอเทมได้
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    // 1. สั่งเพิ่มเลือด (ฟังก์ชันนี้เราเขียนไว้แล้วใน PlayerHealth.cs)
                    playerHealth.Heal(healAmount);

                    // 2. เล่นเสียงแบบ 2D
                    if (pickupSound != null)
                    {
                        // สร้าง GameObject เปล่าขึ้นมาชั่วคราวเพื่อเล่นเสียง
                        GameObject audioObj = new GameObject("2D Pickup Sound");
                        AudioSource audioSrc = audioObj.AddComponent<AudioSource>();

                        audioSrc.clip = pickupSound;
                        audioSrc.spatialBlend = 0f; // *** ตั้งค่าเป็น 0 เพื่อให้เป็นเสียง 2D ***
                        audioSrc.Play();

                        // สั่งทำลาย GameObject ทิ้งเมื่อเสียงเล่นจบพอดี
                        Destroy(audioObj, pickupSound.length);
                    }

                    // 3. สร้าง Effect (ถ้ามี)
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }

                    // 4. ทำลายไอเทมทิ้ง
                    Destroy(gameObject);
                }
                else
                {
                    // ถ้าเลือดเต็มแล้ว เดินชนก็จะไม่เกิดอะไรขึ้น (เก็บไว้กินตอนเลือดลดได้)
                    Debug.Log("เลือดเต็มแล้ว เก็บกล่องยาไม่ได้!");
                }
            }
        }
    }
}