using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Supermarket;

/// <summary>
/// 物品交付面板：接收背包送来的物品，校验数量完成对顾客交付
/// 支持 DeliveryItemUI 子条目回调归还物品
/// </summary>
public class DeliveryPanel : MonoBehaviour
{
    [Header("UI 组件")]
    public Image foodIcon;
    public TextMeshProUGUI countText;
    public Button deliverButton;
    public TextMeshProUGUI statusText;

    [Header("关联引用")]
    public InventoryManager inventoryManager;

    private CustomerAI currentCustomer;
    private string currentItemName;
    private int currentItemCount;
    private Sprite currentItemSprite;

    public static DeliveryPanel Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 场景里 DeliveryPanel 默认就是关闭的。不要在 Start 里再 SetActive(false)：
        // 物体第一次被 Open() 激活时，Start 会在 Open 之后执行，把刚打开的面板立刻关掉。
        if (deliverButton != null)
            deliverButton.onClick.AddListener(TryDeliver);
    }

    /// <summary>打开交付面板，绑定目标顾客，清空旧数据</summary>
    public void Open(CustomerAI customer)
    {
        currentCustomer = customer;
        ClearDelivery();
        gameObject.SetActive(true);

        if (statusText != null) statusText.text = string.Empty;

        if (currentCustomer == null || currentCustomer.GetRequiredFoodData() == null)
        {
            if (statusText != null) statusText.text = "Customer task data abnormal";
            Debug.LogError("交付面板：顾客FoodData为空");
        }
        RefreshDeliverButtonState();
    }

    /// <summary>关闭面板，清空引用</summary>
    public void Close()
    {
        gameObject.SetActive(false);
        currentCustomer = null;
        ClearDelivery();
    }

    /// <summary>从背包添加物品到交付面板</summary>
    public void AddItemFromInventory(string name, Sprite icon, int addCount)
    {
        Debug.Log($"尝试添加物品：{name}，数量{addCount}");
        if (string.IsNullOrEmpty(name) || addCount <= 0)
        {
            Debug.LogWarning("AddItemFromInventory：物品名称为空或者数量<=0");
            return;
        }

        if (currentCustomer == null || currentCustomer.GetRequiredFoodData() == null)
        {
            if (statusText != null) statusText.text = "Customer data abnormal";
            Debug.Log("交付面板：顾客数据异常");
            return;
        }

        // 如果面板已经放了别的物品，禁止放入
        if (!string.IsNullOrEmpty(currentItemName) && currentItemName != name)
        {
            if (statusText != null) statusText.text = "Only one type of item can be delivered!";
            Debug.Log($"面板已有物品 {currentItemName}，不能放入 {name}");
            return;
        }

        string targetFood = currentCustomer.GetRequiredFoodData().foodName;
        if (name != targetFood)
        {
            if (statusText != null) statusText.text = "Not the food the customer ordered!";
            Debug.Log($"物品不匹配！顾客要：{targetFood}，你放入：{name}");
            return;
        }

        // ========== 修改顺序：先更新面板数据，校验通过后，再扣背包 ==========
        currentItemName = name;
        currentItemSprite = icon;
        currentItemCount += addCount;

        // 扣背包
        InventoryManager.Instance.ConsumeItem(name, addCount);

        RefreshPanelUI();
        RefreshDeliverButtonState();

        if (statusText != null) statusText.text = string.Empty;
        Debug.Log($"添加成功！当前物品：{currentItemName}，当前数量：{currentItemCount}");
    }

    /// <summary>由DeliveryItemUI子条目调用：归还一件物品回背包</summary>
    public void ReturnOneToInventory(DeliveryItemUI item)
    {
        if (item == null || item.Count <= 0 || inventoryManager == null)
            return;

        FoodData data = InventoryManager.Instance.GetFoodDataByName(item.itemName);
        inventoryManager.AddItem(item.itemName, 1, item.icon, data);
        item.RemoveOne();

        if (item.Count <= 0)
        {
            Destroy(item.gameObject);
        }

        RefreshDeliverButtonState();
    }

    /// <summary>尝试完成交付，条件全部满足通知顾客前往收银台</summary>
    void TryDeliver()
    {
        Debug.Log($"尝试交付：当前物品:{currentItemName}，数量:{currentItemCount}");
        if (currentCustomer == null)
        {
            if (statusText != null) statusText.text = "No target customer";
            Debug.Log("交付失败：顾客为空");
            return;
        }

        FoodData targetData = currentCustomer.GetRequiredFoodData();
        if (targetData == null)
        {
            if (statusText != null) statusText.text = "Customer food data null";
            Debug.Log("交付失败：顾客FoodData为空");
            return;
        }

        string needFood = targetData.foodName;
        int needNum = currentCustomer.GetRequiredFoodCount();
        Debug.Log($"顾客需要：{needFood} x{needNum}");

        if (currentItemName != needFood)
        {
            if (statusText != null) statusText.text = "Wrong food";
            Debug.Log("交付失败：物品不对");
            return;
        }
        if (currentItemCount < needNum)
        {
            if (statusText != null) statusText.text = $"Not enough, need {needNum}";
            Debug.Log($"交付失败：数量不足，当前{currentItemCount}，需要{needNum}");
            return;
        }

        //交付成功，顾客去收银台
        currentCustomer.OnFoodDelivered();
        Debug.Log("✅ 交付成功！");
        Close();
    }

    /// <summary>刷新面板图标、数量UI</summary>
    void RefreshPanelUI()
    {
        if (foodIcon != null)
        {
            foodIcon.sprite = currentItemSprite;
            foodIcon.enabled = currentItemCount > 0;
        }
        if (countText != null)
        {
            countText.text = $"x{currentItemCount}";
            countText.enabled = currentItemCount > 0;
        }
    }

    /// <summary>根据数量是否达标，控制交付按钮是否可点击</summary>
    void RefreshDeliverButtonState()
    {
        if (deliverButton == null || currentCustomer == null)
        {
            Debug.Log("RefreshDeliverButtonState：按钮或者顾客为空");
            return;
        }

        FoodData targetData = currentCustomer.GetRequiredFoodData();
        if (targetData == null)
        {
            deliverButton.interactable = false;
            Debug.Log("RefreshDeliverButtonState：顾客FoodData为空，按钮不可点");
            return;
        }

        int needNum = currentCustomer.GetRequiredFoodCount();
        bool canDeliver = currentItemCount >= needNum;
        deliverButton.interactable = canDeliver;
        Debug.Log($"按钮状态：{canDeliver}，当前数量{currentItemCount}，需要{needNum}");
    }

    /// <summary>清空交付面板所有数据</summary>
    void ClearDelivery()
    {
        currentItemName = string.Empty;
        currentItemCount = 0;
        currentItemSprite = null;
        RefreshPanelUI();
        RefreshDeliverButtonState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
