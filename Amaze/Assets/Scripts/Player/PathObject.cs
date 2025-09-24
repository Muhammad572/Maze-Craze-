using UnityEngine;

public class PathObject : MonoBehaviour
{
    private bool wasBroken = false;
    public GameObject breakEffectPrefab; // Assign in Inspector
    

    public void RegisterWithLevelManager()
    {
        LevelManager.instance?.RegisterPathObject();
    }

    public void Break()
    {
        if (wasBroken) return; // 🚫 Already broken

        wasBroken = true;
        gameObject.SetActive(false);


        if (breakEffectPrefab != null)
        {
            Vector2 moveDir = SwipeSlidePlayer.Instance.LastMoveDirection;

            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Instantiate(breakEffectPrefab, transform.position, rotation);
        }

        if (LevelManager.instance != null)
        {
            LevelManager.instance.DeregisterPathObject(this);
        }
        // // 🔊 Play move sound ONCE per swipe
        if (AudioManager.instance != null)
                // AudioManager.instance.PlaySound("End");
                AudioManager.instance.PlayOneShot(AudioManager.instance.tilebreakSound,0.1f);
        //         AudioManager.instance.playMe();
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
