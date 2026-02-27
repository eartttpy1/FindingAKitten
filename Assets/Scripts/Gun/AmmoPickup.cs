using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    public int weaponIndexToRefill; // 1=พก, 2=กล, 3=ลูกซอง
    public int ammoAmount = 20;     // จำนวนกระสุนที่จะให้

    [Header("Effects")]
    public AudioClip pickupSound;   // เสียงตอนเก็บ
    public GameObject pickupEffect; // Effect วิบวับตอนเก็บ (ถ้ามี)

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าคนที่ชนใช่ Player ไหม (ต้องตั้ง Tag Player ที่ตัวละครด้วย)
        if (other.CompareTag("Player"))
        {
            WeaponControl weaponControl = other.GetComponent<WeaponControl>();

            if (weaponControl != null)
            {
                // สั่งเติมกระสุนเข้า "กระสุนสำรอง"
                weaponControl.AddAmmo(weaponIndexToRefill, ammoAmount);

                // เล่นเสียงแบบ 2D
                if (pickupSound != null)
                {
                    // สร้าง GameObject เปล่าขึ้นมาชั่วคราวเพื่อเล่นเสียง
                    GameObject audioObj = new GameObject("2D Ammo Pickup Sound");
                    AudioSource audioSrc = audioObj.AddComponent<AudioSource>();

                    audioSrc.clip = pickupSound;
                    audioSrc.spatialBlend = 0f; // *** ตั้งค่าเป็น 0 เพื่อให้เป็นเสียง 2D ***
                    audioSrc.Play();

                    // สั่งทำลาย GameObject ทิ้งเมื่อเสียงเล่นจบพอดี
                    Destroy(audioObj, pickupSound.length);
                }

                // เล่น Effect (ถ้ามี)
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // ลบกล่องทิ้ง
                Destroy(gameObject);
            }
        }
    }
}