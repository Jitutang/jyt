using UnityEngine;
public class SaveBtnCall : MonoBehaviour
{
    public void SaveClick()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("SaveManager实例不存在！");
        }
    }
}
