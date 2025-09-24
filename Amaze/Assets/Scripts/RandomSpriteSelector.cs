using UnityEngine;

/// <summary>
/// This script manages a list of sprites and assigns a random one
/// to the GameObject's SpriteRenderer component when the game starts.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteSelector : MonoBehaviour
{
    // The array to hold all the sprites you want to choose from.
    // Drag and drop your sprites into this array in the Unity Inspector.
    public Sprite[] sprites;

    // A private reference to the SpriteRenderer component.
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// We get a reference to the SpriteRenderer here.
    /// </summary>
    void Awake()
    {
        // Get the SpriteRenderer component attached to this GameObject.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// We select and apply a random sprite here.
    /// </summary>
    void Start()
    {
        // Call the public method to assign a random sprite.
        AssignRandomSprite();
    }

    /// <summary>
    /// Assigns a random sprite from the list to the SpriteRenderer.
    /// This method is public so it can be called from other scripts.
    /// </summary>
    public void AssignRandomSprite()
    {
        // Check if the SpriteRenderer component exists and the sprites array is not empty.
        if (spriteRenderer != null && sprites.Length > 0)
        {
            // Select a random index from the sprites array.
            int randomIndex = Random.Range(0, sprites.Length);

            // Assign the randomly selected sprite to the SpriteRenderer.
            spriteRenderer.sprite = sprites[randomIndex];
        }
        else
        {
            // Log a warning if something is missing, which helps with debugging.
            if (spriteRenderer == null)
            {
                Debug.LogWarning("RandomSpriteSelector requires a SpriteRenderer component on the same GameObject.");
            }
            if (sprites.Length == 0)
            {
                Debug.LogWarning("RandomSpriteSelector has no sprites assigned to its list. Please add sprites in the Inspector.");
            }
        }
    }
}
