using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("目标玩家")]

    public Transform playerTransform;

    [Header("鼠标灵敏度")]

    public float mouseSensitivity = 100f;

    [Header("上下俯仰角度限制")]

    public float minAngle = -30f;

    public float maxAngle = 60f;

    private Vector3 offset;

    [HideInInspector]

    public float yaw;

    private float pitch;

    void Awake()

    {

        offset = transform.position - playerTransform.position;

        // 手动设置镜头初始朝向

        yaw = 0f;

        pitch = -15f;

        Cursor.lockState = CursorLockMode.Locked;

    }

    void LateUpdate()

    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))

        {

            Cursor.lockState = CursorLockMode.Locked;

        }

        if (Input.GetKeyDown(KeyCode.Escape))

        {

            Cursor.lockState = CursorLockMode.None;

        }

        yaw += mouseX;

        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minAngle, maxAngle);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);

        transform.position = playerTransform.position + rot * offset;

        transform.LookAt(playerTransform.position + Vector3.up * 1.5f);

    }
}