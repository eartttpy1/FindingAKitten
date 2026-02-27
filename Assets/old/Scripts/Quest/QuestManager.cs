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
            questText.text = "ตามหาหนู: " + currentItems + " / " + requiredItems;
        }
    }
}