using UnityEngine;
using Supermarket;
public class Shelf : MonoBehaviour
{
    [Header("当前货架物品")]
    public string itemName;
    public int addCount = 1;
    public Sprite itemIcon;
    public FoodData foodData; // 必须拖入对应的 ScriptableObject
    [Header("交互设置")]
    public float interactRange = 2.5f;
    public InventoryManager inventoryManager;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void OnMouseDown()
    {
        if (player == null || inventoryManager == null || foodData == null) return;
        if (Vector3.Distance(transform.position, player.position) > interactRange) return;

        // ✅ 修复 CS1501：补全第 4 个参数 foodData
        inventoryManager.AddItem(itemName, addCount, itemIcon, foodData);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}