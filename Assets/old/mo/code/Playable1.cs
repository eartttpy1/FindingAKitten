using UnityEngine;
using UnityEngine.SceneManagement;

public class Playable1 : MonoBehaviour
{
    public string nextSceneName;

    public void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Play() 
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
