using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections; // จำเป็นต้องมีบรรทัดนี้สำหรับ Coroutine

public class WeaponControl : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator; // ตัวคุม Animation ปืน (UI)
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform cameraTransform;

    [Header("UI - HUD")]
    [SerializeField] private RectTransform weaponHolder; // **ลากตัว WeaponHolder (ตัวแม่ของรูปปืน) มาใส่ตรงนี้**
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Text ammoText;

    [Header("Settings")]
    [SerializeField] private float punchRange = 2.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Reload Animation")]
    [SerializeField] private float reloadDropAmount = 150f; // ระยะที่ปืนจะมุดลง
    [SerializeField] private float reloadTime = 1.0f;       // เวลารีโหลด

    [Header("Sound Clips")]
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip punchHitSound;
    [SerializeField] private AudioClip pistolSound;      // เปลี่ยนชื่อเป็น Pistol
    [SerializeField] private AudioClip machineGunSound;  // **เพิ่มเสียงปืนกล**
    [SerializeField] private AudioClip shotgunSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip dryFireSound;

    [System.Serializable]
    public class Weapon
    {
        public string name;
        public int weaponID; // ID สำหรับส่งให้ Animator (0,1,2,3)
        public Sprite crosshairSprite;
        public Sprite weaponIcon;
        public bool isAutomatic; // ติ๊กถูก = กดค้างยิงรัว

        [Header("Ammo System")]
        public int clipSize;
        public int currentClip;
        public int maxReserve;
        public int currentReserve;

        [Header("Shooting")]
        public int bulletsPerShot;
        public float fireRate;
        public float spread;
        public GameObject bulletPrefab;
        public float bulletSpeed;
    }

    [Header("Weapon Settings")]
    public List<Weapon> weapons;
    // Element 0: หมัด
    // Element 1: ปืนพก
    // Element 2: ปืนกล
    // Element 3: ลูกซอง

    private int currentWeaponIndex = 0;
    private float nextFireTime = 0f;
    private bool isReloading = false;        // เช็คว่ากำลังรีโหลดอยู่ไหม
    private Vector2 originalHolderPosition;  // จำตำแหน่งเดิมของปืน

    private void Start()
    {
        // จำตำแหน่งเริ่มต้นของ WeaponHolder ไว้ (เพื่อเวลามุดลงจะได้เด้งกลับมาถูกที่)
        if (weaponHolder != null) originalHolderPosition = weaponHolder.anchoredPosition;

        foreach (var w in weapons)
        {
            w.currentClip = w.clipSize;
            w.currentReserve = w.maxReserve / 2;
        }

        
    }

    private void Update()
    {
        // ถ้ารีโหลดอยู่ ห้ามทำอะไรทั้งนั้น
        if (isReloading) return;

        HandleWeaponSwitching();
        HandleShooting();
        HandleReload();
    }

    private void HandleWeaponSwitching()
    {
        // เพิ่มปุ่มเลข 4
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchWeapon(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchWeapon(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchWeapon(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchWeapon(3);
    }

    private void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        currentWeaponIndex = index;

        // ส่ง ID ไปบอก Animator เพื่อเปลี่ยนรูปปืนตอนถือ (Idle)
        animator.SetInteger("WeaponID", weapons[index].weaponID);

        // สั่งให้ Animator รู้ว่า "ฉันกดเปลี่ยนปืนแล้วนะ!" (ทำแค่ครั้งเดียว)
        animator.SetTrigger("Switch");

        UpdateHUD();
    }

    private void HandleShooting()
    {
        Weapon currentWep = weapons[currentWeaponIndex];
        bool isShooting = false;

        // เช็คการกดค้าง หรือ กดทีละนัด
        if (currentWep.isAutomatic)
        {
            if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime) isShooting = true;
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime) isShooting = true;
        }

        if (isShooting)
        {
            // เช็คกระสุนหมด (ยกเว้นหมัด)
            if (currentWeaponIndex != 0 && currentWep.currentClip <= 0)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame && dryFireSound)
                {
                    audioSource.PlayOneShot(dryFireSound);
                    // TryReload(); // เปิดบรรทัดนี้ถ้าอยากให้ Auto Reload
                }
                return;
            }

            if (currentWeaponIndex == 0) PunchAttack();
            else ShootGun(currentWep);

            UpdateHUD();
        }
    }

    private void PunchAttack()
    {
        nextFireTime = Time.time + weapons[0].fireRate;

        // สั่ง Animator เล่นท่ายิง/ต่อย
        animator.SetTrigger("Shoot");

        if (punchSound) audioSource.PlayOneShot(punchSound);

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, punchRange, enemyLayer))
        {
            if (punchHitSound) audioSource.PlayOneShot(punchHitSound);
            // hit.collider.GetComponent<EnemyHealth>().TakeDamage(10);
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(20); // ต่อยแรง 20
            }
        }
    }

    private void ShootGun(Weapon weapon)
    {
        nextFireTime = Time.time + weapon.fireRate;
        weapon.currentClip--;

        // สั่ง Animator เล่นท่ายิง (เปลี่ยนรูปปืนเป็นท่ายิง)
        animator.SetTrigger("Shoot");

        PlayWeaponSound();

        // คำนวณจุดเล็ง
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(75);

        // Loop สร้างกระสุน (รองรับลูกซองที่ออกหลายนัด)
        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            Vector3 direction = targetPoint - firePoint.position;

            // คำนวณแรงดีด/กระจาย
            if (weapon.spread > 0)
            {
                float xSpread = Random.Range(-weapon.spread, weapon.spread);
                float ySpread = Random.Range(-weapon.spread, weapon.spread);
                direction += new Vector3(xSpread, ySpread, 0);
            }

            // สร้างลูกกระสุน (Prefab)
            GameObject currentBullet = Instantiate(weapon.bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = direction.normalized * weapon.bulletSpeed; 
        }
    }

    // ---------------------------------------------------------
    //  ระบบรีโหลด (Reload Logic) - ใช้ Coroutine ทำ Animation
    // ---------------------------------------------------------
    private void HandleReload()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }
    }

    public void TryReload()
    {
        Weapon w = weapons[currentWeaponIndex];

        // 1. เช็คเงื่อนไข: หมัดห้ามโหลด / กระสุนเต็ม / ไม่มีสำรอง
        if (currentWeaponIndex == 0 || w.currentClip >= w.clipSize || w.currentReserve <= 0) return;

        // 2. เริ่ม Animation
        StartCoroutine(ReloadRoutine(w));
    }

    private IEnumerator ReloadRoutine(Weapon w)
    {
        isReloading = true; // ล็อคปืน

        if (reloadSound) audioSource.PlayOneShot(reloadSound);

        // --- Animation: เลื่อนปืนลง ---
        Vector2 startPos = originalHolderPosition;
        Vector2 downPos = startPos - new Vector2(0, reloadDropAmount);

        float halfDuration = reloadTime / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            if (weaponHolder != null)
                weaponHolder.anchoredPosition = Vector2.Lerp(startPos, downPos, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (weaponHolder != null) weaponHolder.anchoredPosition = downPos;

        // --- เติมกระสุน (จังหวะที่ปืนอยู่ข้างล่างสุด) ---
        int amountNeeded = w.clipSize - w.currentClip;
        int amountToReload = Mathf.Min(amountNeeded, w.currentReserve);
        w.currentClip += amountToReload;
        w.currentReserve -= amountToReload;
        UpdateHUD();

        // --- Animation: เลื่อนปืนกลับขึ้นมา ---
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            if (weaponHolder != null)
                weaponHolder.anchoredPosition = Vector2.Lerp(downPos, startPos, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (weaponHolder != null) weaponHolder.anchoredPosition = startPos;

        isReloading = false; // ปลดล็อค
    }

    public void AddAmmo(int weaponIndex, int amount)
    {
        weapons[weaponIndex].currentReserve += amount;
        if (weapons[weaponIndex].currentReserve > weapons[weaponIndex].maxReserve)
        {
            weapons[weaponIndex].currentReserve = weapons[weaponIndex].maxReserve;
        }
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        Weapon currentWep = weapons[currentWeaponIndex];

        if (crosshairImage != null && currentWep.crosshairSprite != null) crosshairImage.sprite = currentWep.crosshairSprite;
        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = currentWep.weaponIcon;
            weaponIconImage.enabled = (currentWep.weaponIcon != null);
        }

        if (ammoText != null)
        {
            if (currentWeaponIndex == 0) ammoText.text = "∞";
            else
            {
                ammoText.text = $"{currentWep.currentClip} / {currentWep.currentReserve}";
                ammoText.color = (currentWep.currentClip <= currentWep.clipSize * 0.25f) ? Color.red : Color.white;
            }
        }
    }

    private void PlayWeaponSound()
    {
        switch (currentWeaponIndex)
        {
            // Case 0 คือหมัด (เล่นเสียงในฟังก์ชัน PunchAttack แล้ว)
            case 1: if (pistolSound) audioSource.PlayOneShot(pistolSound); break;       // ปืนพก
            case 2: if (machineGunSound) audioSource.PlayOneShot(machineGunSound); break; // ปืนกล
            case 3: if (shotgunSound) audioSource.PlayOneShot(shotgunSound); break;     // ลูกซอง
        }
    }
}