using UnityEngine;

public class TUTORIAL : MonoBehaviour
{
    public Canvas DoorCanvas;

    void Start()
    {
        // 1. เปิด Canvas ทันทีที่เริ่ม
        DoorCanvas.enabled = true;

        // 2. สั่งให้เรียกฟังก์ชัน "CloseCanvas" หลังจากผ่านไป 5 วินาที
        Invoke("CloseCanvas", 5f);
    }

    void CloseCanvas()
    {
        // 3. ปิด Canvas
        DoorCanvas.enabled = false;
    }
}
