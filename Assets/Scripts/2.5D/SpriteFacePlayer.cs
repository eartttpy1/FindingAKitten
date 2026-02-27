using UnityEngine;

public class SpriteFacePlayer : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera; // Main camera object

    private float yRotation; // Will get the rotation of the player/player camera

    private void Awake()
    {
        mainCamera = GameObject.Find("Main Camera"); // Finds the player camera on Awake
    }

    private void Update()
    {
        yRotation = mainCamera.GetComponent<CameraController>().playerYRotation; // Finds the Yrotation of the player/player camera

        transform.rotation = Quaternion.Euler(0, yRotation, 0); // Makes the rotation of the object equal to the player's rotation.
    }
}