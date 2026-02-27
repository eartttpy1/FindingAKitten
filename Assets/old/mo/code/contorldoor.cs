using UnityEngine;
using UnityEngine.Playables;

public class controldoor : MonoBehaviour
{
    public PlayableDirector timeline1;
    public PlayableDirector timeline2;
    public GameObject crystal1;
    public GameObject crystal2;
    

    private bool isForward = true;
    private bool isPlayerInside = false; // ตัวแปรเช็คว่าผู้เล่นอยู่ในเขตไหม

    void Update()
    {
        // ถ้าผู้เล่นอยู่ในเขต และ กดปุ่ม F
        if (isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            TogglePlatforms();
        }
    }

    void TogglePlatforms()
    {
        if (timeline1 == null && timeline2 == null) return;
        timeline1.Stop();
        timeline2.Stop();

        if (isForward)
        {
            timeline1.time = 0; // เริ่มที่ต้น
            timeline2.time = timeline2.duration;
        // บังคับให้ timeline1 อัปเดตตำแหน่งที่เวลา 0 ทันที
            timeline1.Evaluate();
            timeline2.Evaluate();
            timeline1.playableGraph.GetRootPlayable(0).SetSpeed(1);
            timeline2.playableGraph.GetRootPlayable(0).SetSpeed(-1);
            crystal1.SetActive(true);
            crystal2.SetActive(false);
        }
        else
        {
            timeline1.time = timeline1.duration; // เริ่มที่จุดจบ
            timeline2.time = 0;
            // บังคับให้ timeline1 อัปเดตตำแหน่งที่จุดจบก่อนเปลี่ยน Speed
            timeline1.Evaluate(); 
            timeline2.Evaluate();
            timeline1.playableGraph.GetRootPlayable(0).SetSpeed(-1);
            timeline2.playableGraph.GetRootPlayable(0).SetSpeed(1);
            crystal1.SetActive(false);
            crystal2.SetActive(true);
        }
        timeline1.Play();
        timeline2.Play();
        isForward = !isForward;
    }

    // เมื่อเดินเข้ามาในเขต
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // เช็คว่าสิ่งที่เข้ามาคือ Player (ต้องตั้ง Tag ที่ตัวละครด้วย)
        {
            isPlayerInside = true;
            Debug.Log("Player entered range");
        }
    }

    // เมื่อเดินออกจากเขต
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("Player left range");
        }
    }
}
