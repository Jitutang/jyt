using UnityEngine;
using UnityEngine.UI;

public class CustomerPortraitUI : MonoBehaviour
{
    public Image portraitImage;

    public void SetPortrait(Sprite sprite)
    {
        if (portraitImage == null) return;
        portraitImage.sprite = sprite;
        portraitImage.enabled = sprite != null;
    }

    public void ClearPortrait()
    {
        if (portraitImage == null) return;
        portraitImage.sprite = null;
        portraitImage.enabled = false;
    }
}
