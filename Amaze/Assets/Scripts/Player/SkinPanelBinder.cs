using UnityEngine;
using UnityEngine.UI;

public class SkinPanelBinder : MonoBehaviour
{
    public Button[] skinButtons;
    public Image[] lockOverlays; // same order as skinButtons

    private void Start()
    {
        RefreshUI();

        for (int i = 0; i < skinButtons.Length; i++)
        {
            int index = i;
            skinButtons[i].onClick.AddListener(() =>
            {
                PlayerSkinManager.instance.SelectSkin(index);
                RefreshUI();
            });
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < skinButtons.Length; i++)
        {
            bool unlocked = PlayerSkinManager.instance.IsUnlocked(i);
            if (i < lockOverlays.Length)
            {
                lockOverlays[i].gameObject.SetActive(!unlocked);
            }
        }
    }
}
