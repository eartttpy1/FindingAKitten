using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction lookAction;
    private Vector2 lookInput;

    public bool YRotOnly;

    // ปรับลด Sensitivity ลงเพราะเราอาจจะไม่คูณ Time.deltaTime (ขึ้นอยู่กับความชอบ)
    public float sensX = 10f; 
    public float sensY = 10f;

    private float xRotation;
    private float yRotation;

    public float playerYRotation;

    private void Awake()
    {
        lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() { lookAction.Enable(); }
    private void OnDisable() { lookAction.Disable(); }

    // ใช้ LateUpdate เพื่อความลื่นไหลของกล้อง
    private void LateUpdate()
    {
        // การคูณ Time.deltaTime กับ Mouse Delta บางครั้งทำให้กระตุกถ้าเฟรมเรตไม่นิ่ง
        // ลองเทสดูว่าแบบไหนลื่นกว่าสำหรับโปรเจกต์คุณ (มี หรือ ไม่มี Time.deltaTime)
        // ถ้าเอา Time.deltaTime ออก ต้องปรับ Sensitivity ให้เหมาะสม
        float mouseX = lookInput.x * sensX * Time.deltaTime; 
        float mouseY = lookInput.y * sensY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        yRotation += mouseX;

        playerYRotation = yRotation;

        if (YRotOnly)
        {
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            playerObject.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            playerObject.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}