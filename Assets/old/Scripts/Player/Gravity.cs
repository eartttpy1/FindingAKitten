using UnityEngine;

public class Gravity : MonoBehaviour
{
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset downward velocity if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}