using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // 新增！TMP需要这个

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("背景音乐")]
    public AudioClip bgmClip;
    public float bgmVolume = 0.5f;
    public bool playOnStart = true;

    [Header("金钱系统")]
    public int money = 0;
    public int penaltyMoney = 10; //顾客生气扣钱

    // =========把你的金钱UI文本拖到这里=========
    public Text moneyText;

    [Header("飘字配置")]
    public GameObject floatTextPrefab;
    public float floatSpeed = 1.2f;
    public float floatDuration = 1.2f;

    public bool IsPaused { get; private set; }
    public bool isGameOver = false;

    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
    }

    void Start()
    {
        if (playOnStart && bgmClip != null)
            PlayBGM(bgmClip);
        UpdateMoneyUI(); //游戏启动，初始化金钱UI
    }

    #region 金钱逻辑
    public void AddMoney(int amount)
    {
        if (isGameOver) return;
        money += amount;
        UpdateMoneyUI(); //加钱刷新UI
    }

    public void SubMoney(int amount)
    {
        if (isGameOver) return;
        money -= amount;
        money = Mathf.Max(money, int.MinValue);
        UpdateMoneyUI(); //扣钱也刷新UI
        if (money < 0)
        {
            GameOver();
        }
    }

    /// <summary>刷新右上角金钱UI显示</summary>
    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money:{money}";
        }
    }
    #endregion

    #region 飘字
    public void SpawnFloatText(Vector3 worldPos, int amount)
    {
        if (floatTextPrefab == null || isGameOver) return;
        GameObject textObj = Instantiate(floatTextPrefab, worldPos, Quaternion.identity);
        Text text = textObj.GetComponent<Text>();
        if (text == null)
        {
            Destroy(textObj);
            return;
        }
        text.text = amount > 0 ? $"+{amount}" : $"{amount}";
        text.color = amount > 0 ? Color.green : Color.red;
        Destroy(textObj, floatDuration);
        StartCoroutine(FloatTextMove(textObj.transform));
    }
    private IEnumerator FloatTextMove(Transform trans)
    {
        float timer = 0f;
        while (timer < floatDuration)
        {
            if (trans == null) yield break;
            trans.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;
            timer += Time.unscaledDeltaTime;
        }
    }
    #endregion

    #region 游戏暂停系统
    public void PauseGame()
    {
        if (isGameOver || IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        if (isGameOver || !IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
    }
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }
    #endregion

    #region 游戏结束
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("游戏失败！金钱不足");
        Time.timeScale = 0f;

        //弹出游戏结束提示，设置5秒时长
        GameTip.Instance.ShowTip("GameOver", 5f);

        if (GameOverPanel.Instance != null)
        {
            GameOverPanel.Instance.Show();
        }
    }
    #endregion

    #region BGM控制
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }
    public void PauseBGM() => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }
    #endregion
}
