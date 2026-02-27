using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeTravelManager : MonoBehaviour
{
    [Header("Time State")]
    public bool isPast = true; // เริ่มต้นที่อดีตหรือไม่?

    [Header("Environment Parents")]
    public GameObject pastParent;   // โยน Parent ของเฟอร์นิเจอร์/ศัตรูในอดีตมาใส่
    public GameObject futureParent; // โยน Parent ของเฟอร์นิเจอร์/ศัตรูในอนาคตมาใส่

    [Header("Visual & Tone (Post Processing)")]
    public GameObject pastVolume;   // GameObject ที่ใส่ Global Volume โทนสีอดีต
    public GameObject futureVolume; // GameObject ที่ใส่ Global Volume โทนสีอนาคต

    [Header("Flash Effect")]
    public Image whiteFlashUI;      // UI Image สีขาวที่จะให้วาบขึ้นมาบังจอ
    public float flashDuration = 0.5f; // ระยะเวลาที่แสงวาบค่อยๆ จางลง

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip timeTravelSound;

    private bool isTimeTraveling = false; // ตัวแปรกันผู้เล่นกดปุ่ม Q รัวๆ

    private void Start()
    {
        // อัปเดตสภาพแวดล้อมให้ตรงกับค่าเริ่มต้นตอนเริ่มเกม
        UpdateTimeState();
        
        // ทำให้จอใสก่อนตอนเริ่มเกมเผื่อลืมตั้งค่าใน UI
        if (whiteFlashUI != null)
        {
            Color c = whiteFlashUI.color;
            c.a = 0f;
            whiteFlashUI.color = c;
            whiteFlashUI.raycastTarget = false; // ป้องกัน UI บังการคลิกเมาส์
        }
    }

    private void Update()
    {
        // ถ้ากด Q และไม่ได้กำลังข้ามเวลาอยู่
        if (Input.GetKeyDown(KeyCode.Q) && !isTimeTraveling)
        {
            StartCoroutine(TimeTravelRoutine());
        }
    }

    private IEnumerator TimeTravelRoutine()
    {
        isTimeTraveling = true;

        // 1. เล่นเสียงข้ามเวลา
        if (timeTravelSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(timeTravelSound);
        }

        // 2. ทำให้จอขาววาบ 100% ทันที (Flash)
        if (whiteFlashUI != null)
        {
            Color c = whiteFlashUI.color;
            c.a = 1f;
            whiteFlashUI.color = c;
        }

        // 3. สลับสถานะเวลา
        isPast = !isPast;

        // 4. เปลี่ยนแมพและโทนสี "ตอนที่จอขาววาบอยู่" (เพื่อบังสายตาผู้เล่นตอนโหลดของ)
        UpdateTimeState();

        // 5. ค่อยๆ เฟดแสงสีขาวให้จางหายไป
        if (whiteFlashUI != null)
        {
            float timer = 0f;
            while (timer < flashDuration)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / flashDuration); // ค่อยๆ ลดค่า Alpha จาก 1 ไป 0
                
                Color c = whiteFlashUI.color;
                c.a = alpha;
                whiteFlashUI.color = c;
                
                yield return null;
            }
            
            // ทำให้แน่ใจว่าใส 100% ตอนจบ
            Color finalColor = whiteFlashUI.color;
            finalColor.a = 0f;
            whiteFlashUI.color = finalColor;
        }

        isTimeTraveling = false; // เปิดให้กดข้ามเวลาครั้งต่อไปได้
    }

    private void UpdateTimeState()
    {
        // สลับเปิด-ปิด Object ภายในฉาก
        if (pastParent != null) pastParent.SetActive(isPast);
        if (futureParent != null) futureParent.SetActive(!isPast);

        // สลับเปิด-ปิด โทนสี (Global Volume)
        if (pastVolume != null) pastVolume.SetActive(isPast);
        if (futureVolume != null) futureVolume.SetActive(!isPast);
    }
}