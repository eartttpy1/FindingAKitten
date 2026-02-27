using UnityEngine;
using UnityEngine.SceneManagement;
public class Ftohelp : MonoBehaviour
{    
    public Canvas helpCanvas;
    public GameObject Cage ;
    private bool isPlayer = false;
        void Start()
        {
            helpCanvas.enabled = false;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && isPlayer == true)
            {
                helpCanvas.enabled = false;
                SceneManager.LoadScene("endcust");
            }
        }
    
        void OnTriggerEnter(Collider other) 
        {
            if (other.gameObject.tag == "cage") 
            {
                helpCanvas.enabled = true;
                isPlayer = true;
            }
        }
        void OnTriggerExit(Collider other) 
        {
            if (other.gameObject.tag == "cage") 
            {
                helpCanvas.enabled = false;
                isPlayer = false;
            }
        }
}
