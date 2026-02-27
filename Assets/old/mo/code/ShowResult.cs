using UnityEngine;
using TMPro;

public class ShowResult : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI bestTimeText; // เพิ่มช่องสำหรับแสดงเวลาที่ดีที่สุด

    void Start()
    {
        float currentTime = TimeKeeper.ElapsedTime;
        
        // 1. แสดงเวลาปัจจุบัน
        resultText.text = "Current Time: " + FormatTime(currentTime);

        // 2. ตรวจสอบและบันทึก Best Time
        // ใช้ชื่อ Key ว่า "BestTime" และกำหนดค่าเริ่มต้นเป็น 999999 (เพื่อให้เวลาแรกที่เล่นชนะเสมอ)
        float bestTime = PlayerPrefs.GetFloat("BestTime", 999999f);

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save(); // บันทึกลงเครื่องทันที
            bestTimeText.text = "New Record! Best: " + FormatTime(bestTime);
        }
        else
        {
            bestTimeText.text = "Best Time: " + FormatTime(bestTime);
        }
    }

    // Helper Function สำหรับจัดรูปแบบเวลา 00:00:00
    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
