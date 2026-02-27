using UnityEngine;
using UnityEngine.SceneManagement;

public class Playable1 : MonoBehaviour
{
    public string nextSceneName;


    public void Play() 
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
