using UnityEngine;
using TMPro;

public class ShowResult : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI bestTimeText;

    void Start()
    {
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.None;

        float currentTime = TimeKeeper.ElapsedTime;
        
        // 1. แสดงเวลาปัจจุบัน (MM:SS)
        resultText.text = "Current Time: " + FormatTime(currentTime);

        // 2. จัดการ Best Time
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
            bestTimeText.text = "New Record! Best: " + FormatTime(bestTime);
        }
        else
        {
            // ถ้าเป็นครั้งแรกที่เล่น (ยังไม่มีสถิติ) ให้แสดงเวลาปัจจุบันเป็น Best ไปก่อน
            float displayBest = (bestTime == float.MaxValue) ? currentTime : bestTime;
            bestTimeText.text = "Best Time: " + FormatTime(displayBest);
        }
    }

    // Helper Function สำหรับจัดรูปแบบเวลาแค่ 00:00 (นาที:วินาที)
    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        
        // {0:00} คือหลักนาที, {1:00} คือหลักวินาที
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // เพิ่มฟังก์ชันนี้ลงใน class ShowResult
    public void ResetBestTime()
    {
        PlayerPrefs.DeleteKey("BestTime"); // ลบค่าสถิติเฉพาะ Key นี้
        PlayerPrefs.Save();
        
        // อัปเดต UI ทันทีหลังจากกด Reset (ให้แสดงเป็น --:--)
        bestTimeText.text = "Best Time: --:--";
        Debug.Log("Best Time has been reset!");
    }
}