using UnityEngine;

public class PlayerRigidbodyMove : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;

    private Rigidbody rb;
    private Animator anim;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // 冻结X/Z轴旋转，从根源防止角色歪倒乱飞
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        // 开启插值，移动更丝滑无抖动
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // 采集WASD输入（固定视角，直接对应世界坐标前后左右）
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0, v).normalized;

        // 同步动画Speed参数，自动切换待机/走路
        anim.SetFloat("Speed", moveInput.magnitude);
    }

    // 物理逻辑统一放FixedUpdate，避免抖动、穿透
    void FixedUpdate()
    {
        // 只控制水平速度，保留y轴重力，角色自动贴地不飘
        Vector3 targetVelocity = moveInput * moveSpeed;
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;

        // 移动时平滑转向面朝方向
        if (moveInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }
}