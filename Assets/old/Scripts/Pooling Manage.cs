using UnityEngine;

public class PoolingManage : MonoBehaviour
{
    public GameObject bullet;
    public GameObject[] bulletArr; //ที่เก็บ / ถังกระสุน
    public int poolSize = 200; //ขนาด
    //ก่อนเริ่มเกม
    void Awake()
    {
        bulletArr = new GameObject[poolSize];

        //เริ่ม instantiate loop poolSize ครั้ง
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bullet, this.transform); //เก็บ instantiate ไว้ใน obj
            obj.SetActive(false);

            bulletArr[i] = obj;
        }
    }

    public GameObject GetPoolObject()
    {
        foreach (GameObject obj in bulletArr)
        {
            if (obj != null)
            {
                if (!obj.activeInHierarchy)
                {
                    return obj;
                }
            }
        }
        return null; // สถานการณ์คับขัน SOS maydaymayday
    }


}
