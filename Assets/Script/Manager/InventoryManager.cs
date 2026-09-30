using UnityEngine;
using Supermarket;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public static InventoryManager Instance;
    public ItemSlot[] itemSlot;
    public DeliveryPanel deliveryPanel;

    [Header("音效设置")]
    public AudioClip inventoryOpenSound;
    [Range(0f, 1f)] public float soundVolume = 0.6f;

    private bool menuActivated = false;
    private AudioSource audioSource;

    private Dictionary<string, InventoryItem> itemDictionary = new Dictionary<string, InventoryItem>();
    private Dictionary<string, FoodData> foodDataMap = new Dictionary<string, FoodData>();

    void Awake()
    {
        //修复单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetButtonDown("book"))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isOpen = InventoryMenu.activeSelf;
        bool newState = !isOpen;
        InventoryMenu.SetActive(newState);
        PlayInventorySound();

        // ✅修复：同步menuActivated状态
        menuActivated = newState;

        //打开背包 → 暂停游戏；关闭背包 → 恢复游戏
        if (newState)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
            //关闭的时候清空头像，防止残留
            CustomerPortraitUI ui = InventoryMenu.GetComponentInChildren<CustomerPortraitUI>();
            if (ui != null) ui.ClearPortrait();
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    void PlayInventorySound()
    {
        if (inventoryOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(inventoryOpenSound, soundVolume);
    }

    /// <summary>
    /// 添加物品进背包
    /// </summary>
    public void AddItem(string itemName, int quantity, Sprite icon, FoodData foodData)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        if (itemDictionary.ContainsKey(itemName))
            itemDictionary[itemName].count += quantity;
        else
            itemDictionary.Add(itemName, new InventoryItem { count = quantity, icon = icon });

        if (foodData != null && !foodDataMap.ContainsKey(itemName))
            foodDataMap.Add(itemName, foodData);

        RefreshInventoryUI();
    }

    /// <summary>
    /// 消耗背包物品
    /// </summary>
    public void ConsumeItem(string itemName, int quantity)
    {
        if (!itemDictionary.ContainsKey(itemName)) return;

        itemDictionary[itemName].count -= quantity;
        if (itemDictionary[itemName].count <= 0)
            itemDictionary.Remove(itemName);

        RefreshInventoryUI();
    }

    /// <summary>
    /// 根据物品名称获取FoodData（给DeliveryPanel调用）
    /// </summary>
    public FoodData GetFoodDataByName(string name)
    {
        foodDataMap.TryGetValue(name, out FoodData fd);
        return fd;
    }

    /// <summary>
    /// 刷新所有背包格子显示
    /// </summary>
    public void RefreshInventoryUI()
    {
        foreach (var slot in itemSlot)
            slot.ClearSlot();

        int slotIndex = 0;
        foreach (var item in itemDictionary)
        {
            if (slotIndex >= itemSlot.Length) break;

            itemSlot[slotIndex].AddItem(item.Key, item.Value.count, item.Value.icon);
            slotIndex++;
        }
    }

    /// <summary>
    /// 背包格子点击事件
    /// </summary>
    public void OnSlotClicked(ItemSlot slot)
    {
        if (!menuActivated)
        {
            Debug.Log("OnSlotClicked 被忽略：背包未打开");
            return;
        }
        if (deliveryPanel == null)
        {
            Debug.LogError("OnSlotClicked 被忽略：InventoryManager 未拖 DeliveryPanel");
            return;
        }
        if (!deliveryPanel.gameObject.activeSelf)
        {
            Debug.Log("OnSlotClicked 被忽略：交付面板未打开（先点已接单顾客再点食物）");
            return;
        }

        string itemName = slot.itemName;
        if (string.IsNullOrEmpty(itemName)) return;

        if (!itemDictionary.TryGetValue(itemName, out InventoryItem item) || item.count <= 0)
            return;

        // ✅修复：先调用交付面板尝试接收，接收成功后再扣物品，防止物品丢失
        deliveryPanel.AddItemFromInventory(itemName, item.icon, 1);
        // 只有交付面板接收成功才扣背包；
        // 注意：AddItemFromInventory内部如果条件不满足直接return，不会修改任何数据；
        // 所以不能在这里Consume；
        // 👉 把【ConsumeItem(itemName,1)】移动到 DeliveryPanel.AddItemFromInventory 内部真正接收物品之后！！
    }
}

[System.Serializable]
public class InventoryItem
{
    public int count;
    public Sprite icon;
}
