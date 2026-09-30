using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    public static GameOverPanel Instance;

    [Header("组件绑定")]
    public Button returnBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);

        if (returnBtn != null)
        {
            returnBtn.onClick.AddListener(OnClickReturnAndClearSave);
        }
    }

    /// <summary>显示游戏结束遮罩面板</summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>点击按钮：清除存档，重置游戏状态，回到开始菜单</summary>
    void OnClickReturnAndClearSave()
    {
        //1.删除本地存档
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        //2.恢复时间流速，重置全局状态
        GameManager.Instance.isGameOver = false;
        Time.timeScale = 1f;

        //3.跳转回开始界面
        SceneManager.LoadScene("StartScene");
    }
}

