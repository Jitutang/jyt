using UnityEngine;

namespace Supermarket
{
    [CreateAssetMenu(fileName = "FoodData", menuName = "Supermarket/Food Data", order = 1)]
    public class FoodData : ScriptableObject
    {
        [Tooltip("食物图标（显示在顾客头顶和任务栏）")]
        public Sprite icon;

        [Tooltip("食物售价（完成任务时获得的奖励）")]
        public int price = 15;

        [Tooltip("食物名称（可选，用于调试）")]
        public string foodName;
    }
}