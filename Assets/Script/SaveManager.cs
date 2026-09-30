using UnityEngine;

/// <summary>单槽位存档管理器：覆盖式存档，只有一个存档</summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    //存档key名字，不要修改
    private const string SAVE_KEY = "MySupermarketSave";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); //跨场景保留，菜单↔游戏场景都能用
    }

    /// <summary>检查本地是否存在存档</summary>
    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    /// <summary>执行保存：覆盖旧存档</summary>
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.hasSaveFile = true;

        // =========把需要保存的数值写入data=========
        data.playerMoney = GameManager.Instance.money;
        data.hiredEmployeeCount = EmployeeHirePanel.Instance.hiredEmployeeCount;

        //序列化为Json存入PlayerPrefs
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save(); //立刻写入磁盘

        Debug.Log("✅游戏保存完成，覆盖当前存档");
        GameTip.Instance.ShowTip("保存成功！");
    }

    /// <summary>读取存档数据</summary>
    public SaveData LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("没有存档文件");
            return null;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("✅读取存档完成");
        return data;
    }

    /// <summary>删除存档（可选功能）</summary>
    public void DeleteSave()
    {
        if (HasSave())
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("存档已删除");
        }
    }
}
