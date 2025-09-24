using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SwipeSlidePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDuration = 0.5f;
    public float moveSpeed = 5f;
    public LayerMask obstacleLayer;
    private Vector2 moveDirection;
    private bool isMoving = false;
    private bool hasPlayedMoveSound = false;

    private Rigidbody2D rb;
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    public float minSwipeDistance = 50f;

    private Vector2 targetPosition;
    private CircleCollider2D circleCollider;

    [Header("Burst Movement")]
    public float burstSpeedMultiplier = 2.0f;
    public float burstDuration = 0.15f;
    private float currentMoveTime;

    private Rotator rotator;

    // A reference to the TrailRenderer component
    private TrailRenderer trailRenderer;

    public static SwipeSlidePlayer Instance;
    public Vector2 LastMoveDirection { get; private set; }

    private bool isPaused = false;

    // --- NEW: Path History Variables ---
    List<Vector2> pathHistory = new List<Vector2>();
    private LevelManager levelManager; // Reference to LevelManager (for potentially future use)
    // --- END NEW ---

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (trailRenderer == null)
        {
            Debug.LogWarning("SwipeSlidePlayer: No TrailRenderer found in this GameObject's children.");
        }
        else
        {
            trailRenderer.enabled = true; // Set to true here, but LevelManager controls actual activation for levels.
        }

        if (circleCollider == null)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        levelManager = LevelManager.instance; // Get instance of LevelManager
    }

    void Update()
    {
        if (isPaused) return;
        if (Time.timeScale == 0f) return;

        if (!isMoving)
        {
            DetectSwipe();
        }
    }


    void FixedUpdate()
    {
        if (isPaused) return;
        if (Time.timeScale == 0f) return;

        if (isMoving)
        {
            currentMoveTime += Time.fixedDeltaTime;
            float currentEffectiveSpeed = moveSpeed;

            if (currentMoveTime < burstDuration)
            {
                currentEffectiveSpeed = moveSpeed * burstSpeedMultiplier;
            }

            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, currentEffectiveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            BreakOverlappingTilesDuringMove(); // Check for tiles to break as we move

            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                StopMoving();
            }
        }
    }

    private void DetectSwipe()
    {
        if (Time.timeScale == 0f) return;
    #if UNITY_EDITOR || UNITY_STANDALONE
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            touchEndPos = Input.mousePosition;
            ProcessSwipe();
        }

    #else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
                touchStartPos = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                ProcessSwipe();
            }
        }
    #endif
    }


    private void ProcessSwipe()
    {
         if (isPaused) return;
         if (touchStartPos == Vector2.zero || touchEndPos == Vector2.zero) return;
        Debug.Log("Swipe detected!");
        Vector2 swipeVector = touchEndPos - touchStartPos;
        if (swipeVector.magnitude < minSwipeDistance)
        {
            Debug.Log("Swipe too short!");
            return;
        }

        swipeVector.Normalize();

        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            moveDirection = swipeVector.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            moveDirection = swipeVector.y > 0 ? Vector2.up : Vector2.down;
        }
        LastMoveDirection = moveDirection;
        float currentRadius = circleCollider.radius * transform.localScale.x;

        RaycastHit2D hit = Physics2D.CircleCast(
            rb.position,
            currentRadius,
            moveDirection,
            Mathf.Infinity,
            obstacleLayer
        );

        Vector2 finalStopPoint;
        if (hit.collider != null)
        {
            finalStopPoint = hit.point - moveDirection * currentRadius;
        }
        else
        {
            // If no obstacle, move far enough to cover the whole board
            finalStopPoint = (Vector2)rb.position + moveDirection * 50f;
        }

        // Snap to grid
        float snappedX = Mathf.Round(finalStopPoint.x * 2) / 2f;
        float snappedY = Mathf.Round(finalStopPoint.y * 2) / 2f;

        if (moveDirection == Vector2.right || moveDirection == Vector2.left)
        {
            targetPosition = new Vector2(snappedX, Mathf.Round(rb.position.y * 2) / 2f);
        }
        else
        {
            targetPosition = new Vector2(Mathf.Round(rb.position.x * 2) / 2f, snappedY);
        }

        // Snap current pos too for precision
        rb.position = new Vector2(Mathf.Round(rb.position.x * 2) / 2f, Mathf.Round(rb.position.y * 2) / 2f);

        if (targetPosition != rb.position && !isMoving)
        {
            Debug.Log($"Starting new movement to {targetPosition}!");
            isMoving = true;
            currentMoveTime = 0f;

            if (AudioManager.instance != null && !hasPlayedMoveSound)
             if (AudioManager.instance.playerMoveSound != null)
            {
                AudioManager.instance.PlayOneShot(AudioManager.instance.playerMoveSound);
            } // Assuming playMe is the correct method now
                hasPlayedMoveSound = true;

            // --- NEW: Record target position at the start of a new movement ---
            // We only record if the player actually *moves* to a new unique position
            // The actual recording is done in StopMoving to ensure player reached destination
            // For now, we'll keep recording in StopMoving for reliability.
            // A path should probably only record the actual tiles visited, not intermediate points.
            // So, `pathHistory.Add(targetPosition);` would go in StopMoving().
            // But if pathHistory is used for rewind and includes currentPosition, we need to adapt.
            // Let's ensure the initial position is added in ResetPlayerState.
            // The current position of the player before the move starts should already be in history.
            // So, simply add the new target position when the move completes.
        }
    }

    private void StopMoving()
    {
        if (isPaused) return;

        rb.MovePosition(targetPosition);
        isMoving = false;
        moveDirection = Vector2.zero;
        currentMoveTime = 0f;

        hasPlayedMoveSound = false;

        // --- NEW: Record the final position after a move completes ---
        // Ensure we don't add duplicate points if player attempts to move to same spot

        // Convert targetPosition (Vector3) to Vector2 for comparison and storage
         Vector2 currentTargetPositionAsVector2 = (Vector2)targetPosition;

        if (pathHistory.Count == 0 || pathHistory[pathHistory.Count - 1] != currentTargetPositionAsVector2)
        {
            pathHistory.Add(currentTargetPositionAsVector2); // Add the Vector2 version
            Debug.Log($"Path recorded: {currentTargetPositionAsVector2}. History count: {pathHistory.Count}");
        }
        // --- END NEW ---
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Path"))
        {
            PathObject pathObj = other.GetComponent<PathObject>();
            if (pathObj != null)
            {
                pathObj.gameObject.SetActive(false); // This effectively "breaks" the tile by deactivating it
            }
        }
    }

    public void ResetPlayerState(Vector2 position)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        if (trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (rb == null || circleCollider == null)
        {
            Debug.LogError("SwipeSlidePlayer: Missing Rigidbody2D or CircleCollider2D on reset.");
            return;
        }

        rb.position = position;
        targetPosition = position;
        isMoving = false;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        currentMoveTime = 0f;

        // --- NEW: Reset path history ---
        ClearPathHistory(position);
        // --- END NEW ---
    }


    private void BreakOverlappingTilesDuringMove()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(rb.position, 0.1f);

        foreach (Collider2D collider in colliders)
        {
            PathObject pathObject = collider.GetComponent<PathObject>();
            if (pathObject != null)
            {
                // Assuming PathObject.Break() correctly handles deactivation and deregistration
                pathObject.Break();
            }
        }
    }

    public void DeactivateTrailForLevelChange()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            trailRenderer.Clear();
        }
    }

    public void ActivateTrailAfterLevelLoad()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.Clear(); // Clears any residual segments
            trailRenderer.emitting = true; // Ensure it starts emitting
        }
    }

    private void OnEnable()
    {
        PanelManager.OnPauseStateChanged += HandlePause;
    }

    private void OnDisable()
    {
        PanelManager.OnPauseStateChanged -= HandlePause;
    }

    private void HandlePause(bool paused)
    {
        isPaused = paused;

        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();

            isMoving = false;
            moveDirection = Vector2.zero;
            currentMoveTime = 0f;
            touchStartPos = Vector2.zero;
            touchEndPos = Vector2.zero;
            targetPosition = rb.position;

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false; // Stop emitting, but keep segments
            }
        }
        else
        {
            rb.WakeUp();
            if (trailRenderer != null)
            {
                trailRenderer.emitting = true; // Resume emitting
            }
        }
    }

    // --- NEW: Public methods for path history ---
    public void ClearPathHistory(Vector3 initialPosition)
    {
        pathHistory.Clear();
        // Add the initial position only if it's the very start.
        // The first actual *move* will add its target position in StopMoving.
        pathHistory.Add(initialPosition);
        Debug.Log($"Path history cleared. Initial position: {initialPosition}");
    }

    public List<Vector2> GetPathHistory()
    {
        return new List<Vector2>(pathHistory); // Return a copy
    }
    // --- END NEW ---
}