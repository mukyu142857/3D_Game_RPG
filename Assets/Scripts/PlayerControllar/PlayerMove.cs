using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    public float moveSpeed = 5f;
    public float jumpForce = 4.5f;
    public float gravity = -9.81f;
    public float rotateSmooth = 10f;
    private Vector3 velocity;

    [Header("翻滚参数")]
    public float rollSpeed = 50;          // 翻滚速度
    public float rollDuration = 0.4f;      // 翻滚持续时间
    public float rollCooldown = 0.4f;      // 翻滚冷却时间

    private bool isRolling = false;
    private float rollTimer = 0f;
    private float rollCooldownTimer = 0f;
    private Vector3 rollDirection;

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

        // 冷却计时
        if (rollCooldownTimer > 0)
        {
            rollCooldownTimer -= Time.deltaTime;
        }

        // 翻滚逻辑
        if (isRolling)
        {
            // 翻滚过程中持续向目标方向移动
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0)
            {
                // 翻滚结束
                isRolling = false;
                rollCooldownTimer = rollCooldown;
            }
            else
            {
                // 翻滚移动（忽略重力，贴地移动）
                Vector3 rollMove = rollDirection * rollSpeed * Time.deltaTime;
                // 保持贴地
                if (controller.isGrounded)
                {
                    velocity.y = -0.5f;
                }
                else
                {
                    velocity.y += gravity * Time.deltaTime;
                }
                controller.Move(rollMove + velocity * Time.deltaTime);
                return; // 翻滚期间跳过普通移动和旋转
            }
        }

        // 普通状态下的旋转
        if(followCamera != null)
        {
            Quaternion targetRot = Quaternion.Euler(0, followCamera.yaw, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }

        // 翻滚输入检测
        if (Input.GetButtonDown("Jump") && controller.isGrounded && !isRolling && rollCooldownTimer <= 0)
        {
            StartRoll(h, v, camRight, camForward);
        }

        // 跳跃重力逻辑
        if (controller.isGrounded)
        {
            velocity.y = -0.5f;
            // 原跳跃已被翻滚替代，如需保留跳跃功能，可改为其他按键
            // if (Input.GetButtonDown("Jump"))
            // {
            //     velocity.y = jumpForce;
            // }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(moveDir * moveSpeed * Time.deltaTime + velocity * Time.deltaTime);
    }

    /// <summary>
    /// 开始翻滚
    /// </summary>
    void StartRoll(float h, float v, Vector3 camRight, Vector3 camForward)
    {
        // 优先使用WASD输入方向
        Vector3 inputDir = (camRight * h + camForward * v).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            // 有输入：朝输入方向翻滚
            rollDirection = inputDir;
        }
        else
        {
            // 无输入：朝角色当前朝向翻滚
            rollDirection = transform.forward;
        }

        isRolling = true;
        rollTimer = rollDuration;
    }
}