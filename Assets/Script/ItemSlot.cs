using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ItemSlot : MonoBehaviour
{
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    private Button button;
    private InventoryManager manager;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) button = gameObject.AddComponent<Button>();

        // 直接获取 Manager 引用
        manager = FindObjectOfType<InventoryManager>();
        button.onClick.AddListener(() =>
        {
            manager?.OnSlotClicked(this);
        });
    }

    // 往格子里放入新物品（必须是 public）
    public void AddItem(string itemName, int quantity, Sprite itemSprite)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.itemSprite = itemSprite;
        isFull = true;
        UpdateUI();
    }

    public void AddQuantity(int addCount)
    {
        quantity += addCount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        itemImage.sprite = itemSprite;
        itemImage.enabled = quantity > 0;
        quantityText.text = quantity.ToString();
        quantityText.enabled = quantity > 0;
        isFull = quantity > 0;
    }

    public void ClearSlot()
    {
        itemName = "";
        quantity = 0;
        itemSprite = null;
        isFull = false;
        itemImage.enabled = false;
        quantityText.enabled = false;
    }
}