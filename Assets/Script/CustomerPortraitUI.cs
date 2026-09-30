using UnityEngine;
using UnityEngine.UI;

namespace Supermarket
{
    public class CustomerPortraitUI : MonoBehaviour
    {
        public Image customerPortraitImage;

        public void SetPortrait(Sprite portrait)
        {
            if (portrait == null || customerPortraitImage == null) return;
            customerPortraitImage.sprite = portrait;
            customerPortraitImage.enabled = true;
        }

        public void ClearPortrait()
        {
            if (customerPortraitImage != null)
                customerPortraitImage.enabled = false;
        }
    }
}