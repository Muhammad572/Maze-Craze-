using UnityEngine;

public class SkinButton : MonoBehaviour
{
    public int skinIndex; // 0..23

    public void OnSkinButtonClicked()
    {
        PlayerSkinManager.instance?.SelectSkin(skinIndex);
    }
}
