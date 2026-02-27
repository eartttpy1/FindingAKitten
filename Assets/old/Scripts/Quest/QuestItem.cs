using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ส่งสัญญาณไปบอกว่าเก็บได้ 1 ชิ้น
            QuestManager.instance.CollectItem();

            // เล่นเสียงแบบ 2D
            if (pickupSound != null)
            {
                // สร้าง GameObject เปล่าขึ้นมาชั่วคราวเพื่อเล่นเสียง
                GameObject audioObj = new GameObject("2D Quest Pickup Sound");
                AudioSource audioSrc = audioObj.AddComponent<AudioSource>();

                audioSrc.clip = pickupSound;
                audioSrc.spatialBlend = 0f; // *** ตั้งค่าเป็น 0 เพื่อให้เป็นเสียง 2D ***
                audioSrc.Play();

                // สั่งทำลาย GameObject ทิ้งเมื่อเสียงเล่นจบ
                Destroy(audioObj, pickupSound.length);
            }

            // สร้าง Effect (ถ้ามี)
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // ทำลายไอเทมทิ้ง
            Destroy(gameObject);
        }
    }
}