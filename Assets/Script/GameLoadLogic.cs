using UnityEngine;

public class GameLoadLogic : MonoBehaviour
{
    void Start()
    {
        //如果有存档，就读取并且把数据赋值给游戏各个系统
        if (SaveManager.Instance.HasSave())
        {
            SaveData save = SaveManager.Instance.LoadGame();
            if (save == null) return;

            //恢复金钱
            GameManager.Instance.money = save.playerMoney;
            GameManager.Instance.UpdateMoneyUI();

            //恢复雇佣员工数量，并且生成对应数量员工
            EmployeeHirePanel.Instance.hiredEmployeeCount = save.hiredEmployeeCount;
            for (int i = 0; i < save.hiredEmployeeCount; i++)
            {
                Transform spawnPoint = EmployeeHirePanel.Instance.employeeSpawnPoints[i];
                Instantiate(EmployeeHirePanel.Instance.employeePrefab, spawnPoint.position, spawnPoint.rotation);
            }
            //刷新雇佣按钮状态，如果已经3人，按钮置灰
            EmployeeHirePanel.Instance.RefreshHireButton();

            GameTip.Instance.ShowTip("已加载存档！", 2f);
        }
        else
        {
            //没有存档：全部使用游戏默认初始值
            Debug.Log("本次为新游戏，无存档数据");
        }
    }
}
