using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("菜单按钮")]
    public Button btnNewGame;
    public Button btnContinueGame; //继续游戏按钮，有存档才激活
    public Button btnQuitGame;

    void Start()
    {
        // 启动菜单的时候，检测是否存在存档，控制继续按钮是否可交互
        RefreshContinueButtonState();

        if (btnNewGame != null) btnNewGame.onClick.AddListener(StartNewGame);
        if (btnContinueGame != null) btnContinueGame.onClick.AddListener(ContinueGame);
        if (btnQuitGame != null) btnQuitGame.onClick.AddListener(QuitGame);
    }

    /// <summary>刷新继续游戏按钮状态：无存档置灰</summary>
    void RefreshContinueButtonState()
    {
        if (SaveManager.Instance != null)
        {
            btnContinueGame.interactable = SaveManager.Instance.HasSave();
        }
    }

    /// <summary>新游戏：直接进游戏，不读存档</summary>
    public void StartNewGame()
    {
        SceneManager.LoadScene("Scene_1");
    }

    /// <summary>继续游戏：读取存档，加载游戏场景</summary>
    public void ContinueGame()
    {
        if (!SaveManager.Instance.HasSave()) return;

        //先加载游戏场景，场景加载完毕再赋值存档数据
        SceneManager.LoadScene("Scene_1");
    }

    /// <summary>退出游戏</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
