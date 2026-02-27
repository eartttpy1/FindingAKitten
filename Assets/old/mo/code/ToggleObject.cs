using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    // ลาก Object ที่ต้องการเปิด/ปิดมาใส่ในช่องนี้ที่หน้า Inspector
    public GameObject targetObject; 

    void Update()
    {
        // ตรวจสอบว่ามีการกดปุ่ม M หรือไม่
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (targetObject != null)
            {
                // อ่านค่าสถานะปัจจุบันแล้วกลับค่า (ถ้าเปิดอยู่จะปิด ถ้าปิดอยู่จะเปิด)
                bool isActive = targetObject.activeSelf;
                targetObject.SetActive(!isActive);
            }
        }
    }
}