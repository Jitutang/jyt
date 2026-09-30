using System;

/// <summary>单存档保存的数据结构</summary>
[Serializable]
public class SaveData
{
    // 是否存在有效存档
    public bool hasSaveFile;

    // 需要保存的数据，在这里添加
    public int playerMoney;         //玩家金钱
    public int hiredEmployeeCount;  //当前雇佣员工1的数量（最多3）
}
