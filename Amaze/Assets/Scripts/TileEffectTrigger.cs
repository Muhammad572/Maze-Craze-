using UnityEngine;

public class TileEffectTrigger : MonoBehaviour
{
    public GameObject effectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{gameObject.name} triggered by {other.name}");
        if (other.CompareTag("Player") && effectPrefab != null)
        {
            Color playerColor = Color.white; // Default to white

            // Try to get the player's color from the PlayerColor component
            PlayerColor pc = other.GetComponent<PlayerColor>();
            if (pc != null)
            {
                playerColor = pc.playerPrimaryColor;
            }
            // Optional: If SwipeSlidePlayer directly has the color, you could get it here
            // else
            // {
            //     SwipeSlidePlayer slidePlayer = other.GetComponent<SwipeSlidePlayer>();
            //     if (slidePlayer != null)
            //     {
            //         // If SwipeSlidePlayer has a 'playerColor' field
            //         // playerColor = slidePlayer.playerColor;
            //     }
            // }


            // Optional: use player move direction
            Vector2 moveDir = SwipeSlidePlayer.Instance.LastMoveDirection;
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject fx = Instantiate(effectPrefab, transform.position, rotation);

            // Force play & auto destroy
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Apply the player's color to the particle system's start color
                var mainModule = ps.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(playerColor);

                ps.Play();
            }
            Destroy(fx, 2f);
        }
    }
}