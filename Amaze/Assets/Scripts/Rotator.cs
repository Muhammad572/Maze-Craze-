// Rotator.cs
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The minimum rotation speed in degrees per second (when player is stopped).")]
    public float minRotationSpeed = 50f;

    [Tooltip("The maximum rotation speed in degrees per second (when player is moving).")]
    public float maxRotationSpeed = 400f; // Adjusted default for more impact

    [Tooltip("How quickly the rotation speeds up when the player starts moving.")]
    public float accelerationRate = 5f; // Adjust this for faster/slower acceleration

    [Tooltip("How quickly the rotation slows down when the player stops moving.")]
    public float decelerationRate = 3f; // Adjust this for faster/slower deceleration

    // Public enum and variable for direction mode (from previous revision)
    public enum RotationDirectionMode
    {
        SwipeBased,
        AlwaysClockwise,
        AlwaysCounterClockwise
    }
    [Tooltip("Choose how the rotation direction is determined.")]
    public RotationDirectionMode directionMode = RotationDirectionMode.SwipeBased;

    [Header("Start Settings")]
    [Tooltip("If 'Direction Mode' is 'Swipe Based', this is the initial direction the spinner will use until the first swipe.")]
    public Vector2 defaultStartSpinDirection = Vector2.right;

    private Vector2 lastSwipeDirection = Vector2.zero;
    private bool isSpinning = false;
    private float currentCalculatedRotationSpeed; // The actual speed used for rotation
    private bool playerIsMoving = false; // New: Tracks player's movement state

    void Start()
    {
        // Initialize current speed to min, so it starts slow
        currentCalculatedRotationSpeed = minRotationSpeed;
        isSpinning = true; // Start spinning immediately

        if (directionMode == RotationDirectionMode.SwipeBased && lastSwipeDirection == Vector2.zero)
        {
            lastSwipeDirection = defaultStartSpinDirection.normalized;
        }
    }

    // New: Public method to inform Rotator about player's movement state
    public void SetPlayerMovingState(bool moving)
    {
        playerIsMoving = moving;
    }

    // Public method to set the swipe direction and trigger spin
    public void SetSpinDirection(Vector2 swipeDir)
    {
        lastSwipeDirection = swipeDir.normalized;
        isSpinning = true; // Ensure spinning is active
    }

    // Method to stop the spin (if ever needed manually)
    public void StopSpin()
    {
        isSpinning = false;
        // Optionally, reset speed to min or 0 when stopped
        currentCalculatedRotationSpeed = minRotationSpeed;
    }

    void Update()
    {
        // Smoothly adjust rotation speed based on player movement state
        if (playerIsMoving)
        {
            currentCalculatedRotationSpeed = Mathf.Lerp(currentCalculatedRotationSpeed, maxRotationSpeed, Time.deltaTime * accelerationRate);
        }
        else
        {
            currentCalculatedRotationSpeed = Mathf.Lerp(currentCalculatedRotationSpeed, minRotationSpeed, Time.deltaTime * decelerationRate);
        }
        // Clamp to ensure the speed stays within defined min/max bounds
        currentCalculatedRotationSpeed = Mathf.Clamp(currentCalculatedRotationSpeed, minRotationSpeed, maxRotationSpeed);


        if (isSpinning)
        {
            float zRotation = 0f;

            switch (directionMode)
            {
                case RotationDirectionMode.SwipeBased:
                    if (lastSwipeDirection == Vector2.zero)
                    {
                        return;
                    }
                    if (Mathf.Abs(lastSwipeDirection.x) > Mathf.Abs(lastSwipeDirection.y))
                    {
                        zRotation = lastSwipeDirection.x > 0 ? -currentCalculatedRotationSpeed : currentCalculatedRotationSpeed;
                    }
                    else
                    {
                        zRotation = lastSwipeDirection.y > 0 ? -currentCalculatedRotationSpeed : currentCalculatedRotationSpeed;
                    }
                    break;

                case RotationDirectionMode.AlwaysClockwise:
                    zRotation = -currentCalculatedRotationSpeed;
                    break;

                case RotationDirectionMode.AlwaysCounterClockwise:
                    zRotation = currentCalculatedRotationSpeed;
                    break;
            }

            transform.Rotate(Vector3.forward, zRotation * Time.deltaTime);
        }
    }
}