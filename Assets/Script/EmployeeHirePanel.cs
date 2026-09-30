using UnityEngine;
using UnityEngine.UI;

public class EmployeeHirePanel : MonoBehaviour
{
    public static EmployeeHirePanel Instance;

    [Header("员工设置")]
    public GameObject employeePrefab;    //员工1预制体
    public Transform[] employeeSpawnPoints; //收银台旁边3个员工生成点
    public int hireCost = 100;           //雇佣花费100金钱
    public int maxEmployeeCount = 3;     //最多雇佣3个

    [Header("UI")]
    public Button hireBtn;

    public int hiredEmployeeCount = 0;   //当前已雇佣员工数量

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (hireBtn != null)
            hireBtn.onClick.AddListener(HireEmployee);
        RefreshHireButton();
    }

    /// <summary>雇佣员工按钮点击事件</summary>
    void HireEmployee()
    {
        //判断是否达到上限
        if (hiredEmployeeCount >= maxEmployeeCount)
        {
            GameTip.Instance.ShowTip("员工数量已达上限！最多3名");
            return;
        }
        //判断金钱
        if (GameManager.Instance.money < hireCost)
        {
            GameTip.Instance.ShowTip("Not enough money");
            return;
        }

        //扣钱
        GameManager.Instance.AddMoney(-hireCost);

        //在对应出生点实例化员工
        Transform spawnPoint = employeeSpawnPoints[hiredEmployeeCount];
        Instantiate(employeePrefab, spawnPoint.position, spawnPoint.rotation);

        hiredEmployeeCount++;
        RefreshHireButton();
        GameTip.Instance.ShowTip("雇佣员工成功！");
    }

    /// <summary>刷新按钮：满3人就置灰</summary>
    public void RefreshHireButton()
    {
        if (hireBtn == null) return;
        hireBtn.interactable = hiredEmployeeCount < maxEmployeeCount;
    }
}
