using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Supermarket
{
    public class DeliverySlot : MonoBehaviour
    {
        public Image foodImage;
        public TMP_Text countText;

        public FoodData foodData;
        public int count;

        public void SetData(FoodData data, int cnt)
        {
            foodData = data;
            count = cnt;
            if (foodImage != null)
            {
                foodImage.sprite = data.icon;
                foodImage.enabled = true;
            }
            if (countText != null)
            {
                countText.text = "x" + count;
                countText.enabled = true;
            }
        }

        public void AddCount(int add)
        {
            count += add;
            if (countText != null) countText.text = "x" + count;
        }

        public void Clear()
        {
            foodData = null;
            count = 0;
            if (foodImage != null) foodImage.enabled = false;
            if (countText != null) countText.enabled = false;
        }
    }
}