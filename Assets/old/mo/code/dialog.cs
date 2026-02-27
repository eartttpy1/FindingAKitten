using UnityEngine;

public class dialog : MonoBehaviour
{    
    public Canvas FCanvas;
    public Canvas DialogCanvas;
    private bool isPlayer = false;
        void Start()
        {
            FCanvas.enabled = false;
            DialogCanvas.enabled = false;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && isPlayer == true)
            {
                // สลับสถานะ: ถ้าเปิดอยู่ให้ปิด ถ้าปิดอยู่ให้เปิด
                DialogCanvas.enabled = !DialogCanvas.enabled;
                
                // ถ้าเปิด Dialog อยู่ ก็ให้ซ่อนปุ่ม F ไปเลย
                FCanvas.enabled = !DialogCanvas.enabled;
            }
        }
    
        void OnTriggerEnter(Collider other) 
        {
            if (other.gameObject.tag == "box") 
            {
                FCanvas.enabled = true;
                isPlayer = true;
            }
        }
        void OnTriggerExit(Collider other) 
        {
            if (other.gameObject.tag == "box") 
            {
                FCanvas.enabled = false;
                DialogCanvas.enabled = false;
                isPlayer = false;
            }
        }
}
