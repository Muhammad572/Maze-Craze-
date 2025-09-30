using UnityEngine;
using UnityEngine.UI;

public class RewardPanelManager : MonoBehaviour
{
    public static RewardPanelManager Instance;

    [Header("UI Elements")]
    public GameObject rewardPanelContainer;
    public Image rewardImage;
    public Button claimButton;

    private System.Action onClaimAction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rewardPanelContainer.SetActive(false);
    }

    public void ShowReward(Sprite sprite, System.Action claimAction, Color? tint = null)
    {
        Debug.Log("📌 ShowReward called, enabling panel");
        onClaimAction = claimAction;
        rewardImage.sprite = sprite;
        

        // ✅ Apply tint if given
        rewardImage.color = tint ?? Color.white;

        rewardPanelContainer.SetActive(true);

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            AudioManager.instance.PlayRandomRewardSound();
            
            onClaimAction?.Invoke();
            onClaimAction = null;
            rewardPanelContainer.SetActive(false);

            // reset color so skins stay normal
            rewardImage.color = Color.white;
        });
    }

}
