using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("鼠标皮肤PNG")]
    public Texture2D cursorTexture;

    [Header("光标热点（点击生效位置，一般是箭头尖端）")]
    public Vector2 cursorHotSpot = new Vector2(0, 0);

    [Header("鼠标点击音效")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Awake()
    {
        // 添加音频组件（自动生成，不用手动挂）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        SetCustomCursor();
    }

    /// <summary>
    /// 设置自定义鼠标样式
    /// </summary>
    void SetCustomCursor()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, cursorHotSpot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("请拖拽鼠标png图片到 cursorTexture！");
        }
    }

    void Update()
    {
        // 左键按下播放点击音效
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
        }
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // 切换回系统默认鼠标（备用方法，其他脚本可调用）
    public void ResetDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // 切换回自定义鼠标（备用）
    public void RestoreCustomCursor()
    {
        SetCustomCursor();
    }
}