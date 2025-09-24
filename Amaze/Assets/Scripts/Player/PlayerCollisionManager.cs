using UnityEngine;
using System.Collections; // Required for IEnumerator and Coroutines

public class PlayerCollisionManager : MonoBehaviour
{
    public float deactivateDelay = 0.5f; // Renamed for clarity to reflect deactivation

    void OnTriggerEnter2D(Collider2D collision)
    {
        PathObject pathObj = collision.gameObject.GetComponent<PathObject>();

        if (pathObj != null)
        {
            // Start the coroutine to deactivate the object after a delay
            StartCoroutine(DeactivateAfterDelay(collision.gameObject, deactivateDelay));
        }
    }

    // New Coroutine method to deactivate an object after a specified delay
    private IEnumerator DeactivateAfterDelay(GameObject objectToDeactivate, float delay)
    {
        // Wait for the specified amount of seconds
        yield return new WaitForSeconds(delay);

        // After the delay, deactivate the GameObject
        if (objectToDeactivate != null) // Check if the object still exists (not destroyed by something else)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}
