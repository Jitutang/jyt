using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 交付面板子物品条目UI，挂载在交付物品预制体上
/// 负责展示待交付物品图标、数量，归还按钮回调
/// </summary>
public class DeliveryItemUI : MonoBehaviour
{
    [Header("物品数据")]
    public string itemName;
    public Sprite icon;

    [Header("UI组件绑定")]
    [SerializeField] private Image foodIcon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button reduceButton;

    private int _count;
    private DeliveryPanel _parentPanel;

    /// <summary>当前物品数量，禁止小于0</summary>
    public int Count
    {
        get => _count;
        private set
        {
            _count = Mathf.Max(0, value);
            RefreshView();
        }
    }

    /// <summary>初始化条目数据，绑定归还按钮</summary>
    public void Setup(string name, Sprite itemIcon, int initCount, DeliveryPanel panel)
    {
        // 参数合法性校验
        if (string.IsNullOrEmpty(name) || itemIcon == null || panel == null)
        {
            Debug.LogError($"{nameof(DeliveryItemUI)} Setup传入参数不能为空！", this);
            return;
        }

        itemName = name;
        icon = itemIcon;
        _parentPanel = panel;
        Count = initCount;

        if (reduceButton != null)
        {
            reduceButton.onClick.RemoveAllListeners();
            reduceButton.onClick.AddListener(OnReduceBtnClick);
        }

        RefreshView();
    }

    /// <summary>增加物品数量</summary>
    public void AddCount(int add)
    {
        if (add <= 0) return;
        Count += add;
    }

    /// <summary>减少1个物品</summary>
    public void RemoveOne()
    {
        Count -= 1;
    }

    /// <summary>批量扣除指定数量</summary>
    public void RemoveCount(int amount)
    {
        if (amount <= 0) return;
        Count -= amount;
    }

    /// <summary>刷新图标、数量文本，count=0自动隐藏UI</summary>
    private void RefreshView()
    {
        if (foodIcon != null)
        {
            foodIcon.sprite = icon;
            foodIcon.enabled = _count > 0;
        }

        if (countText != null)
        {
            countText.text = $"x{_count}";
            countText.enabled = _count > 0;
        }

        // 数量为0，归还按钮置灰
        if (reduceButton != null)
        {
            reduceButton.interactable = _count > 0;
        }
    }

    /// <summary>点击减号归还按钮，通知父面板执行归还逻辑</summary>
    private void OnReduceBtnClick()
    {
        if (_parentPanel != null)
        {
            _parentPanel.ReturnOneToInventory(this);
        }
    }

    /// <summary>销毁时移除按钮监听，防止内存泄漏</summary>
    private void OnDestroy()
    {
        if (reduceButton != null)
        {
            reduceButton.onClick.RemoveAllListeners();
        }
    }
}
