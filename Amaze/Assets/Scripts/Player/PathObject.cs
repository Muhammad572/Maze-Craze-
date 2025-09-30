using UnityEngine;

public class PathObject : MonoBehaviour
{
    private bool wasBroken = false;
    public GameObject breakEffectPrefab; // assign in Inspector

    public void RegisterWithLevelManager()
    {
        LevelManager.instance?.RegisterPathObject();
    }

    public void Break()
    {
        if (wasBroken) return; // 🚫 already broken
        wasBroken = true;

        gameObject.SetActive(false);

        // Spawn particle effect with swipe rotation
        if (breakEffectPrefab != null)
        {
            Vector2 moveDir = SwipeSlidePlayer.Instance.LastMoveDirection;
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Instantiate(breakEffectPrefab, transform.position, rotation);
        }

        // Deregister
        if (LevelManager.instance != null)
            LevelManager.instance.DeregisterPathObject(this);

        // 🔊 Play tile break sound via AudioManager
        if (AudioManager.instance != null)
            AudioManager.instance.PlayTileBreakSound(transform.position);
    }

    private void OnDisable()
    {
        // Don't double-deregister
        if (!wasBroken && LevelManager.instance != null)
        {
            LevelManager.instance.DeregisterPathObject(this);
        }
    }

    public void ResetState()
    {
        wasBroken = false;
        gameObject.SetActive(true);
    }
}
