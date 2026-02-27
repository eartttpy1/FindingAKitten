using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class WeaponControl : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform cameraTransform;

    [Header("UI - HUD")]
    [SerializeField] private RectTransform weaponHolder;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Text ammoText;

    [Header("Settings")]
    [SerializeField] private float punchRange = 2.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Reload Animation")]
    [SerializeField] private float reloadDropAmount = 150f;
    [SerializeField] private float reloadTime = 1.0f;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip punchHitSound;
    [SerializeField] private AudioClip pistolSound;
    [SerializeField] private AudioClip machineGunSound;
    [SerializeField] private AudioClip shotgunSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip dryFireSound;

    [System.Serializable]
    public class Weapon
    {
        public string name;
        public int weaponID;
        public Sprite crosshairSprite;
        public Sprite weaponIcon;
        public bool isAutomatic;
        
        // [NEW] เพิ่มตัวแปรเช็คว่าปืนนี้เก็บมาหรือยัง
        public bool isUnlocked; 

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
    private bool isReloading = false;
    private Vector2 originalHolderPosition;

    private void Start()
    {
        if (weaponHolder != null) originalHolderPosition = weaponHolder.anchoredPosition;

        foreach (var w in weapons)
        {
            w.currentClip = w.clipSize;
            w.currentReserve = w.maxReserve / 2;
        }

        // [NEW] บังคับให้หมัด (Element 0) ปลดล็อคเสมอ และถือหมัดเป็นค่าเริ่มต้น
        if (weapons.Count > 0)
        {
            weapons[0].isUnlocked = true; 
            SwitchWeapon(0);
        }
    }

    private void Update()
    {
        if (isReloading) return;

        HandleWeaponSwitching();
        HandleShooting();
        HandleReload();
    }

    private void HandleWeaponSwitching()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchWeapon(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchWeapon(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchWeapon(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchWeapon(3);
    }

    private void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        // [NEW] เช็คว่าปืนนี้ปลดล็อค(เก็บมา)หรือยัง ถ้ายังให้หยุดคำสั่งเลย
        if (!weapons[index].isUnlocked)
        {
            Debug.Log($"ปืน {weapons[index].name} ยังไม่ได้ถูกเก็บ!");
            return; 
        }

        currentWeaponIndex = index;

        animator.SetInteger("WeaponID", weapons[index].weaponID);
        animator.SetTrigger("Switch");

        UpdateHUD();
    }

    private void HandleShooting()
    {
        Weapon currentWep = weapons[currentWeaponIndex];
        bool isShooting = false;

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
            if (currentWeaponIndex != 0 && currentWep.currentClip <= 0)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame && dryFireSound)
                {
                    audioSource.PlayOneShot(dryFireSound);
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
        animator.SetTrigger("Shoot");
        if (punchSound) audioSource.PlayOneShot(punchSound);

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, punchRange, enemyLayer))
        {
            if (punchHitSound) audioSource.PlayOneShot(punchHitSound);

            // เช็คทีละสคริปต์
            if (hit.collider.TryGetComponent(out EnemyAI e1)) e1.TakeDamage(20);
            else if (hit.collider.TryGetComponent(out EnemyShooterAI e2)) e2.TakeDamage(20);
            else if (hit.collider.TryGetComponent(out EnemyMachineGunAI e3)) e3.TakeDamage(20);
            else if (hit.collider.TryGetComponent(out EnemyShotgunAI e4)) e4.TakeDamage(20);
            else if (hit.collider.TryGetComponent(out EnemyBossLaserAI e5)) e5.TakeDamage(20);
        }
    }

    private void ShootGun(Weapon weapon)
    {
        nextFireTime = Time.time + weapon.fireRate;
        weapon.currentClip--;
        animator.SetTrigger("Shoot");
        PlayWeaponSound();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(75);

        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            Vector3 direction = targetPoint - firePoint.position;

            if (weapon.spread > 0)
            {
                float xSpread = Random.Range(-weapon.spread, weapon.spread);
                float ySpread = Random.Range(-weapon.spread, weapon.spread);
                direction += new Vector3(xSpread, ySpread, 0);
            }

            GameObject currentBullet = Instantiate(weapon.bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = direction.normalized * weapon.bulletSpeed; 
        }
    }

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
        if (currentWeaponIndex == 0 || w.currentClip >= w.clipSize || w.currentReserve <= 0) return;
        StartCoroutine(ReloadRoutine(w));
    }

    private IEnumerator ReloadRoutine(Weapon w)
    {
        isReloading = true; 
        if (reloadSound) audioSource.PlayOneShot(reloadSound);

        Vector2 startPos = originalHolderPosition;
        Vector2 downPos = startPos - new Vector2(0, reloadDropAmount);

        float halfDuration = reloadTime / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            if (weaponHolder != null) weaponHolder.anchoredPosition = Vector2.Lerp(startPos, downPos, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (weaponHolder != null) weaponHolder.anchoredPosition = downPos;

        int amountNeeded = w.clipSize - w.currentClip;
        int amountToReload = Mathf.Min(amountNeeded, w.currentReserve);
        w.currentClip += amountToReload;
        w.currentReserve -= amountToReload;
        UpdateHUD();

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            if (weaponHolder != null) weaponHolder.anchoredPosition = Vector2.Lerp(downPos, startPos, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (weaponHolder != null) weaponHolder.anchoredPosition = startPos;

        isReloading = false; 
    }

    // [NEW] ฟังก์ชันสำหรับให้ไอเทมปืนบนพื้น สั่งปลดล็อคปืน
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Count)
        {
            weapons[weaponIndex].isUnlocked = true;
            Debug.Log("เก็บปืนสำเร็จ! ปลดล็อคปืน: " + weapons[weaponIndex].name);

            // เก็บได้ปุ๊บ สลับไปถือปืนนั้นทันที
            SwitchWeapon(weaponIndex); 
        }
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
            case 1: if (pistolSound) audioSource.PlayOneShot(pistolSound); break;       
            case 2: if (machineGunSound) audioSource.PlayOneShot(machineGunSound); break; 
            case 3: if (shotgunSound) audioSource.PlayOneShot(shotgunSound); break;     
        }
    }
}