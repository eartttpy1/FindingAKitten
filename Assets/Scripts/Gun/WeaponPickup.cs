using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Index to Unlock")]
    [Tooltip("ใส่เลขปืนที่ต้องการให้ปลดล็อค (1=ปืนพก, 2=ปืนกล, 3=ลูกซอง)")]
    public int weaponIndexToUnlock = 1; 

    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ดึงสคริปต์ WeaponControl จากตัวผู้เล่น
            WeaponControl playerWeapon = other.GetComponentInChildren<WeaponControl>();
            
            if (playerWeapon != null)
            {
                // สั่งปลดล็อคปืนตามเลขที่ตั้งไว้
                playerWeapon.UnlockWeapon(weaponIndexToUnlock);

                // เล่นเสียงเก็บปืน (ถ้ามี)
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // ทำลายไอเทมทิ้งหลังจากเก็บเสร็จ
                Destroy(gameObject);
            }
        }
    }
}