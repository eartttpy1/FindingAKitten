using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public int currentItems = 0;
    public int requiredItems = 4;

    public Text questText;

    // [จุดเปลี่ยนสำคัญ] เปลี่ยนมารับเป็น GameObject ของประตูแทน
    public GameObject doorObject;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 1. ซ่อนประตูตั้งแต่เริ่มเกม (Deactivate)
        if (doorObject != null)
        {
            doorObject.SetActive(false);
        }

        UpdateUI();
    }

    public void CollectItem()
    {
        currentItems++;
        UpdateUI();

        // 2. เช็คว่าของครบไหม
        if (currentItems >= requiredItems)
        {
            // 3. ของครบปุ๊บ สั่งเปิดประตูให้โผล่มา (Activate)
            if (doorObject != null)
            {
                doorObject.SetActive(true);
            }
        }
    }

    private void UpdateUI()
    {
        if (questText != null)
        {
            // เช็คว่าเก็บหนูครบหรือยัง
            if (currentItems >= requiredItems)
            {
                // ถ้าครบแล้ว เปลี่ยนข้อความชี้เป้าหมายต่อไป
                questText.text = "กลับไปที่จุดเริ่มต้น";

                // (เสริม) ถ้าอยากให้ตัวหนังสือเปลี่ยนสีตอนเควสเสร็จด้วย ใช้คำสั่งนี้ได้ครับ (เอา // ออก)
                // questText.color = Color.green; 
            }
            else
            {
                // ถ้ายังไม่ครบ ให้แสดงจำนวนแบบเดิม
                questText.text = "ตามหาหนู: " + currentItems + " / " + requiredItems;
            }
        }
    }
}