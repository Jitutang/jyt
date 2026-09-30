using UnityEngine;
using TMPro;

/// <summary>屏幕全局提示文字：金钱不足、游戏结束等消息提示</summary>
public class GameTip : MonoBehaviour
{
    public static GameTip Instance;

    [Header("提示UI")]
    public TextMeshProUGUI tipText;
    [Tooltip("提示文字显示持续多少秒")]
    public float showDuration = 2f;

    private float _hideTimer;
    private bool _isShowTip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        tipText.text = "";
    }

    void Update()
    {
        if (_isShowTip)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0)
            {
                HideTip();
            }
        }
    }

    /// <summary>外部调用：显示提示消息</summary>
    /// <param name="msg">提示文字内容</param>
    /// <param name="keepTime">显示时长，不传使用默认时间</param>
    public void ShowTip(string msg, float? keepTime = null)
    {
        if (tipText == null) return;

        tipText.text = msg;
        _isShowTip = true;
        _hideTimer = keepTime ?? showDuration;
    }

    /// <summary>隐藏提示文字</summary>
    public void HideTip()
    {
        tipText.text = "";
        _isShowTip = false;
    }
}
