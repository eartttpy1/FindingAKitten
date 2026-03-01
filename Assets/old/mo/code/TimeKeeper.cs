using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    // static เพื่อให้ ShowResult เรียกใช้ได้จากทุกที่โดยไม่ต้องลาก Object
    public static float ElapsedTime; 

    void Start()
    {
        ElapsedTime = 0f; // รีเซ็ตเวลาใหม่ทุกครั้งที่เริ่มเล่น
    }

    void Update()
    {
        // นับเวลาเพิ่มขึ้นเรื่อยๆ ตามเวลาจริงที่ผ่านไป
        ElapsedTime += Time.deltaTime;
    }
}