using UnityEngine;
using Supermarket;

[CreateAssetMenu(fileName = "CustomerData", menuName = "Game/CustomerData")]
public class CustomerData : ScriptableObject
{
    [Header("顾客基础信息")]
    public string customerName;

    [Header("头像素材")]
    public Sprite customerPortrait;      //背包显示大头头像

    /// <summary>候选食物池，顾客会从这里随机选食物</summary>
    public FoodData[] candidateFoods;

    [Header("随机数量范围")]
    public int minRequireCount = 1;
    public int maxRequireCount = 3;
}
