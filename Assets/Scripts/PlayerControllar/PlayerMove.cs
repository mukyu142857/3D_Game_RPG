using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    public float moveSpeed = 1.4f;
    public float jumpForce = 4.5f;
    public float gravity = -9.81f;
    public float rotateSmooth = 10f;
    private Vector3 velocity;

    [Header("翻滚参数")]
    public float rollDistance = 3f;                  // 翻滚距离 (m)
    public float rollDuration = 0.7f;                // 翻滚持续时间
    public float rollCooldown = 3f;                  // 翻滚冷却时间
    public float rollInvincibleDuration = 0.35f;     // 翻滚无敌时间

    [Header("翻滚高亮")]
    public Color highlightColor = Color.yellow;      // 高亮颜色
    [Range(0f, 2f)] public float highlightIntensity = 0.5f; // 高亮发光强度
    public float highlightDuration = 0.3f;           // 高亮持续时间

    private bool isRolling = false;
    private float rollTimer = 0f;
    private float rollCooldownTimer = 0f;
    private float currentRollSpeed = 0f;
    private Vector3 rollDirection;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private bool isHighlighted = false;
    private float highlightTimer = 0f;
    private Renderer[] meshRenderers;

    [Header("疾跑参数")]
    public float sprintSpeed = 3.5f;            // 疾跑速度 (m/s)
    public float doubleTapInterval = 0.32f;     // 双击W判定间隔
    public float sprintMaintainWindow = 0.1f;   // 松开W后维持疾跑的窗口
    public float sprintCooldownTime = 0.4f;     // 疾跑冷却时间（从松开W算起）

    private bool isSprinting = false;
    private float lastWPressTime = -999f;       // 上次按下W的时间
    private float sprintReleaseTime = -999f;    // 疾跑中松开W的时间
    private bool wPressedLastFrame = false;

    [Header("拖拽赋值MainCamera身上的FollowCamera脚本")]
    public FollowCamera followCamera;
    private Transform mainCam;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main.transform;
        meshRenderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// 是否处于翻滚无敌状态（供外部伤害系统查询）
    /// </summary>
    public bool IsInvincible => isInvincible;

    /// <summary>
    /// 是否处于冷却完成高亮状态
    /// </summary>
    public bool IsHighlighted => isHighlighted;

    /// <summary>
    /// 开始高亮效果
    /// </summary>
    void StartHighlight()
    {
        isHighlighted = true;
        highlightTimer = highlightDuration;
        SetEmission(true);
    }

    /// <summary>
    /// 结束高亮效果
    /// </summary>
    void EndHighlight()
    {
        isHighlighted = false;
        highlightTimer = 0f;
        SetEmission(false);
    }

    /// <summary>
    /// 设置所有网格渲染器的自发光
    /// </summary>
    void SetEmission(bool enabled)
    {
        if (meshRenderers == null) return;

        foreach (var r in meshRenderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                if (enabled)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                }
            }
        }
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

        // 冷却计时（冷却结束触发高亮）
        if (rollCooldownTimer > 0)
        {
            rollCooldownTimer -= Time.deltaTime;
            if (rollCooldownTimer <= 0)
            {
                rollCooldownTimer = 0;
                StartHighlight();
            }
        }

        // 高亮计时
        if (isHighlighted)
        {
            highlightTimer -= Time.deltaTime;
            if (highlightTimer <= 0)
            {
                EndHighlight();
            }
        }

        // 无敌计时
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }

        // 翻滚逻辑
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0)
            {
                // 翻滚结束，无硬直，开始冷却
                isRolling = false;
                rollCooldownTimer = rollCooldown;
            }
            else
            {
                // 翻滚移动（距离=速度×时间, 速度=距离/时间）
                Vector3 rollMove = rollDirection * currentRollSpeed * Time.deltaTime;
                if (controller.isGrounded)
                {
                    velocity.y = -0.5f;
                }
                else
                {
                    velocity.y += gravity * Time.deltaTime;
                }
                controller.Move(rollMove + velocity * Time.deltaTime);
                return; // 翻滚期间跳过其他动作
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

        // ========== 疾跑逻辑 ==========
        bool wPressed = Input.GetKey(KeyCode.W) || v > 0.1f;

        // S键取消疾跑（不进入冷却）
        if (Input.GetKey(KeyCode.S) || v < -0.1f)
        {
            if (isSprinting)
            {
                isSprinting = false;
                sprintReleaseTime = -999f;
            }
        }

        // W键刚按下（上升沿）
        if (wPressed && !wPressedLastFrame)
        {
            if (isSprinting)
            {
                // 疾跑中松开W后在维持窗口内重按 → 维持疾跑
                if (sprintReleaseTime > 0 && Time.time - sprintReleaseTime <= sprintMaintainWindow)
                {
                    sprintReleaseTime = -999f;
                }
            }
            else
            {
                // 检查是否在冷却期
                float timeSinceRelease = sprintReleaseTime > 0 ? Time.time - sprintReleaseTime : 999f;
                if (timeSinceRelease > sprintCooldownTime || sprintReleaseTime < 0)
                {
                    // 双击判定：两次W按下间隔在判定窗口内
                    float interval = Time.time - lastWPressTime;
                    if (interval <= doubleTapInterval && interval > 0.05f)
                    {
                        isSprinting = true;
                        sprintReleaseTime = -999f;
                    }
                }
            }
            lastWPressTime = Time.time;
        }

        // W键刚松开（下降沿）
        if (!wPressed && wPressedLastFrame)
        {
            if (isSprinting)
            {
                sprintReleaseTime = Time.time;
            }
        }

        // 疾跑中松开W超过维持窗口 → 停止疾跑，进入冷却
        if (isSprinting && !wPressed && sprintReleaseTime > 0)
        {
            if (Time.time - sprintReleaseTime > sprintMaintainWindow)
            {
                isSprinting = false;
                // sprintReleaseTime 保留，用于冷却判定
            }
        }

        wPressedLastFrame = wPressed;

        // 当前移动速度
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

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
        controller.Move(moveDir * currentSpeed * Time.deltaTime + velocity * Time.deltaTime);
    }

    /// <summary>
    /// 开始翻滚
    /// </summary>
    void StartRoll(float h, float v, Vector3 camRight, Vector3 camForward)
    {
        // 中断当前动作（疾跑等）
        isSprinting = false;

        // 清除高亮（如果正在显示）
        if (isHighlighted)
        {
            EndHighlight();
        }

        // 翻滚方向：优先WASD输入方向，否则角色前方
        Vector3 inputDir = (camRight * h + camForward * v).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            rollDirection = inputDir;
        }
        else
        {
            rollDirection = transform.forward;
        }

        // 根据距离和时间计算翻滚速度 (v = d / t)
        currentRollSpeed = rollDistance / rollDuration;

        // 启动翻滚
        isRolling = true;
        rollTimer = rollDuration;

        // 启动无敌帧
        isInvincible = true;
        invincibleTimer = rollInvincibleDuration;
    }
}