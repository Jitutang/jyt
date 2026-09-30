using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Supermarket
{
    /// <summary>
    /// 单条任务 UI
    /// 结构建议：
    ///   TaskItem (Image 背景, 挂本脚本)
    ///     ├─ Portrait (Image)   —— 顾客大头
    ///     ├─ FoodIcon (Image)   —— 所需食物图标
    ///     └─ CountText (TextMeshPro‑Text)   —— 需求数量 x3
    /// </summary>
    public class TaskItemUI : MonoBehaviour
    {
        public Image portraitImage;
        public Image foodImage;
        public TMP_Text countText;

        /// <summary>
        /// 由 TaskPanel 调用，用顾客脚本和食物数据初始化一条任务
        /// </summary>
        public void Setup(CustomerAI customer, FoodData food, int count)
        {
            // ===== 头像 =====
            if (customer != null && portraitImage != null)
            {
                if (customer.customerPortrait != null)
                {
                    portraitImage.sprite = customer.customerPortrait;
                    portraitImage.preserveAspect = true;
                    portraitImage.enabled = true;
                }
                else
                {
                    portraitImage.enabled = false;
                }
            }

            // ===== 食物图标 =====
            if (food != null && foodImage != null)
            {
                if (food.icon != null)
                {
                    foodImage.sprite = food.icon;
                    foodImage.preserveAspect = true;
                    foodImage.enabled = true;
                }
                else
                {
                    foodImage.enabled = false;
                }
            }

            // ===== 数量文本 =====
            if (countText != null)
            {
                countText.text = count.ToString();
            }

            // ===== 强制刷新布局 =====
            var innerContent = transform.Find("InnerContent");
            if (innerContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(innerContent.GetComponent<RectTransform>());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}