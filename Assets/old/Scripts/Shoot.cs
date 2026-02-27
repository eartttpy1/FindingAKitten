using UnityEngine;
using UnityEngine.UI; // *สำคัญ* ต้องมีบรรทัดนี้เพื่อคุม UI
using System.Collections;

public class Shoot : MonoBehaviour
{
    [System.Serializable]
    public class WeaponStats
    {
        public string name;
        public Sprite icon;             // *ใหม่* รูปไอคอนปืนที่จะโชว์
        public bool isAutomatic;
        public float fireRate;
        public int pelletsPerShot;
        public float spreadAngle;
        public int maxAmmo;
        public int currentAmmo;
        public float bulletSpeed;
        public AudioClip shotSound;
    }

    [Header("UI References")] // ลากของจาก Canvas มาใส่ตรงนี้
    public Image weaponIconUI;
    public Text ammoTextUI;

    [Header("Weapon Setup")]
    public Transform shootingPos;
    public GameObject bulletPrefab;
    public AudioSource audioSource;
    public AudioClip reloadSound;

    public WeaponStats[] weapons;

    private int currentWeaponIndex = 0;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        // เติมกระสุนให้เต็มทุกปืนตอนเริ่ม
        foreach (var w in weapons)
        {
            w.currentAmmo = w.maxAmmo;
        }

        SelectWeapon(0); // เริ่มที่ปืนแรก
    }

    void Update()
    {
        // เลือกอาวุธ (กด 2, 3, 4)
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectWeapon(2);

        if (isReloading) return;

        WeaponStats currentW = weapons[currentWeaponIndex];

        // รีโหลด
        if (Input.GetKeyDown(KeyCode.R) && currentW.currentAmmo < currentW.maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // ยิง
        bool fireInput = currentW.isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (fireInput)
        {
            if (currentW.currentAmmo > 0 && Time.time >= nextFireTime)
            {
                Fire(currentW);
                nextFireTime = Time.time + currentW.fireRate;
            }
        }
    }

    void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        currentWeaponIndex = index;

        // *อัปเดต UI ทันทีที่เปลี่ยนปืน*
        UpdateUI();
    }

    void Fire(WeaponStats weapon)
    {
        weapon.currentAmmo--;

        // *อัปเดต UI ทันทีที่ยิง (กระสุนลด)*
        UpdateUI();

        if (audioSource && weapon.shotSound)
        {
            audioSource.PlayOneShot(weapon.shotSound);
        }

        for (int i = 0; i < weapon.pelletsPerShot; i++)
        {
            Quaternion spreadRotation = shootingPos.rotation;
            if (weapon.spreadAngle > 0)
            {
                float randomX = Random.Range(-weapon.spreadAngle, weapon.spreadAngle);
                float randomY = Random.Range(-weapon.spreadAngle, weapon.spreadAngle);
                spreadRotation = Quaternion.Euler(shootingPos.eulerAngles.x + randomX, shootingPos.eulerAngles.y + randomY, shootingPos.eulerAngles.z);
            }

            GameObject b = Instantiate(bulletPrefab, shootingPos.position, spreadRotation);
            Rigidbody rb = b.GetComponent<Rigidbody>();
            if (rb) rb.linearVelocity = b.transform.forward * weapon.bulletSpeed;
            Destroy(b, 3f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        ammoTextUI.text = "Reloading..."; // ขึ้นข้อความว่ากำลังรีโหลด

        if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(2f);

        weapons[currentWeaponIndex].currentAmmo = weapons[currentWeaponIndex].maxAmmo;

        isReloading = false;

        // *อัปเดต UI เมื่อรีโหลดเสร็จ*
        UpdateUI();
    }

    // ฟังก์ชันสำหรับอัปเดตหน้าจอ
    void UpdateUI()
    {
        WeaponStats currentW = weapons[currentWeaponIndex];

        // เปลี่ยนรูปปืน
        if (weaponIconUI != null && currentW.icon != null)
        {
            weaponIconUI.sprite = currentW.icon;
            weaponIconUI.enabled = true;
        }
        else if (currentW.icon == null)
        {
            // ถ้าไม่ได้ใส่รูปไว้ ให้ซ่อน Image ไปก่อนกันเป็นสีขาวๆ
            weaponIconUI.enabled = false;
        }

        // เปลี่ยนตัวเลขกระสุน (Format: 5 / 10)
        if (ammoTextUI != null)
        {
            ammoTextUI.text = currentW.currentAmmo + " / " + currentW.maxAmmo;
        }
    }
}