using UnityEngine;
using UnityEngine.SceneManagement;

public class Playable : MonoBehaviour
{
    public string nextSceneName;
    private float timer;

    void Start()
    {
        timer = 0f; // รีเซ็ตเวลาใหม่ทุกครั้งที่เริ่มฉากนี้
    }

    void Update()
    {
        // นับเวลาเพิ่มขึ้นเรื่อยๆ ตามเวลาจริงที่ผ่านไป
        timer += Time.deltaTime;
    }

    public void Play() 
    {
        // บันทึกเวลาที่นับได้ลงใน TimeKeeper ก่อนเปลี่ยนฉาก
        TimeKeeper.ElapsedTime = timer;
        SceneManager.LoadScene(nextSceneName);
    }
}
