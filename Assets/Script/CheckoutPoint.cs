using UnityEngine;

/// <summary>
/// 收银点位组件：顾客点位占用标记 + 员工在岗自动收银逻辑
/// 挂载在每个收银台物体上
/// </summary>
public class CheckoutPoint : MonoBehaviour
{
    [Tooltip("是否已经被顾客占用")]
    public bool isOccupied = false;

    [Header("员工自动收银配置")]
    [Tooltip("检测员工的球体半径")]
    public float employeeDetectRadius = 4f;
    [Tooltip("自动收银判断间隔(秒)")]
    public float autoCheckoutInterval = 3f;
    [Tooltip("员工在岗每次自动收银获得金钱")]
    public int autoCheckoutIncome = 15;

    private float _autoCheckoutTimer;

    void Update()
    {
        //员工自动收银计时逻辑
        _autoCheckoutTimer += Time.deltaTime;
        if (_autoCheckoutTimer >= autoCheckoutInterval)
        {
            _autoCheckoutTimer = 0f;
            CheckHasEmployeeNearby();
        }
    }

    /// <summary>释放点位，顾客离开/销毁时调用</summary>
    public void FreePoint()
    {
        Debug.Log($"【收银点释放】 {gameObject.name}");
        isOccupied = false;
    }

    /// <summary>占用点位，顾客前来收银调用</summary>
    public void OccupyPoint()
    {
        Debug.Log($"【收银点占用】 {gameObject.name}");
        isOccupied = true;
    }

    /// <summary>获取寻路目标世界坐标</summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// <summary>检测范围内是否存在标签【员工1】</summary>
    void CheckHasEmployeeNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, employeeDetectRadius);
        bool hasEmployee = false;
        foreach (var col in hits)
        {
            if (col.CompareTag("Employee_01"))
            {
                hasEmployee = true;
                break;
            }
        }

        if (hasEmployee && GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(autoCheckoutIncome);
            Debug.Log($"员工在岗，自动收银，获得{autoCheckoutIncome}金钱");
        }
    }

    /// <summary>
    /// Gizmos绘制
    /// 小球：红色=被顾客占用，绿色=空闲；青色线框=员工检测范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        //点位占用状态小球
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);

        //员工检测球体线框
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, employeeDetectRadius);
    }
}
