using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction lookAction;

    public bool YRotOnly;

    // แนะนำให้ปรับค่านี้ลงใน Inspector (เช่น 0.1 หรือ 0.5) เพราะเราเอา Time.deltaTime ออกแล้ว
    public float sensX = 0.5f; 
    public float sensY = 0.5f;

    private float xRotation;
    private float yRotation;

    public float playerYRotation;

    private void Awake()
    {
        lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        // เราเอา Event performed/canceled ออกไปเลยครับ
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() { lookAction.Enable(); }
    private void OnDisable() { lookAction.Disable(); }

    private void LateUpdate()
    {
        // 1. อ่านค่าตรงๆ ในลูป Update แบบนี้ ค่าจะไม่ค้างแน่นอน
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // 2. เอา Time.deltaTime ออก เพื่อไม่ให้เกิดการคูณเบิ้ลตอนเฟรมเรตตก
        float mouseX = lookInput.x * sensX; 
        float mouseY = lookInput.y * sensY;

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