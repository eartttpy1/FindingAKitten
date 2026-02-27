using UnityEngine;
using UnityEngine.SceneManagement;

public class warp : MonoBehaviour
{
    public GameObject Door ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Door") 
        {
            SceneManager.LoadScene("ending");
        }
    }

}