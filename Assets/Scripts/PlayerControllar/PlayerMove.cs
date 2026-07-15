using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    public float moveSpeed = 5f;
    public float jumpForce = 4.5f;
    public float gravity = -9.81f;
    public float rotateSmooth = 10f;
    private Vector3 velocity;
    [Header("拖拽赋值MainCamera身上的FollowCamera脚本")]
    public FollowCamera followCamera;
    private Transform mainCam;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //获取相机前后方向，消除Y轴影响
        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camRight * h + camForward * v).normalized;
        if(followCamera != null)
        {
            Quaternion targetRot = Quaternion.Euler(0, followCamera.yaw, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }

        //跳跃重力逻辑
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
        controller.Move(moveDir * moveSpeed * Time.deltaTime + velocity * Time.deltaTime);
    }
}