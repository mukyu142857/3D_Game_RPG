using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    public float moveSpeed = 5f;
    public float jumpForce = 4.5f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;

        if (controller.isGrounded)
        {
            velocity.y = -0.5f;
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 totalMove = moveDir * moveSpeed + velocity;
        controller.Move(totalMove * Time.deltaTime);
    }
}