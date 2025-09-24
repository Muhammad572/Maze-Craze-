using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cinemachine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private Coroutine rewindRoutine;

    // --- UI & Level Management ---
    [Header("UI & Level Management")]
    public List<Vector2> cameraTargetSizes;
    [Tooltip("Manually assign levels or leave empty to auto-populate")]
    public List<GameObject> levels;
    public TextMeshProUGUI levelTitleText;
    [Tooltip("Optional: Drag the parent object containing levels here")]
    public GameObject levelsContainer;
    [Tooltip("Sprites to randomly assign to the TileBack object in each level.")]
    public Sprite[] tileBackSprites;
    [SerializeField] private GameObject[] uiElementsToToggle;
    [SerializeField] private Button skipButton;

    // --- Level Effects ---
    [Header("Level Effects")]
    public GameObject levelSuccessEffectPrefab;
    public Transform effectSpawnPoint;
    public CinemachineImpulseSource impulseSource; // For camera shake

    // --- Rewind Settings ---
    [Header("Rewind Settings")]
    [Tooltip("How long the camera zoom in/out takes.")]
    [SerializeField] private float zoomDuration = 1.0f;
    [Tooltip("How fast the player rewinds the path.")]
    [SerializeField] private float rewindSpeed = 0.3f;
    [Tooltip("Camera zoom size when rewinding.")]
    [SerializeField] private float rewindZoomSize = 3f;
    [Tooltip("Factor to speed up rewind when skipping. 1 is normal speed.")]
    [SerializeField] private float skipSpeedMultiplier = 6f;

    // --- Audio Settings ---
    [Header("Audio")]
    [Tooltip("The name of the rewind sound effect in the AudioManager.")]
    public string rewindSoundName = "RewindWhoosh";
    [Tooltip("Pitch multiplier for the rewind sound when skipping.")]
    public float skipSoundPitch = 1.5f;
    private int levelsSinceLastAd = 0;

    // --- Private Fields ---
    private int currentLevelIndex = -1;
    private int _remainingPathObjectsInCurrentLevel = 0;
    private bool usingAutoPopulatedLevels = false;
    private GameObject player;
    private float originalCameraSize;
    private CinemachineVirtualCamera playerCinemachineCam;
    private Transform currentTileBack;
    private Coroutine fadeRoutine;
    private AudioSource rewindAudioSource;
    private bool skipRewindRequested = false;

    // --- Unity Lifecycle Methods ---
    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        playerCinemachineCam = FindFirstObjectByType<CinemachineVirtualCamera>();
        if (skipButton != null)
            skipButton.onClick.AddListener(RequestSkipRewind);

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    void Start()
    {
        // ❌ FIXED: Removed the call to ResetProgress() which was deleting the saved level.
        ResetProgress();
        if (levels == null || levels.Count == 0)
        {
            usingAutoPopulatedLevels = true;
            PopulateLevelsFromHierarchy();
        }

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: No levels found!");
            return;
        }

        if (usingAutoPopulatedLevels)
            Debug.Log($"Auto-populated {levels.Count} levels from hierarchy");
        else
            Debug.Log($"Using {levels.Count} manually assigned levels");

        DeactivateAllLevels();
        int savedLevel = PlayerPrefs.GetInt("LastLevelIndex", 0);
        ActivateLevel(savedLevel);
    }

    void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(RequestSkipRewind);
        StopAllCoroutines();
    }

    // --- Public Methods ---
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastLevelIndex");
    }

    public void NotifyPlayerReplaced(GameObject newPlayer)
    {
        player = newPlayer;
        if (player.TryGetComponent<SwipeSlidePlayer>(out var slide))
            slide.ResetPlayerState(newPlayer.transform.position);

        Debug.Log("🔄 Player replaced in LevelManager.");
    }

    public void RegisterPathObject()
    {
        _remainingPathObjectsInCurrentLevel++;
    }

    public void DeregisterPathObject(PathObject po)
    {
        if (_remainingPathObjectsInCurrentLevel <= 0) return;

        _remainingPathObjectsInCurrentLevel--;
        if (_remainingPathObjectsInCurrentLevel <= 0)
            LevelCompleted();
    }

    public void RequestSkipRewind()
    {
        skipRewindRequested = true;
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    // --- Private Routines ---
    private void PopulateLevelsFromHierarchy()
    {
        if (levelsContainer == null)
        {
            levelsContainer = GameObject.Find("Levels");
            if (levelsContainer == null)
                levelsContainer = GameObject.Find("Grid");
        }

        if (levelsContainer == null)
        {
            Debug.LogError("Auto-population failed: no levels container found.");
            levels = new List<GameObject>();
            return;
        }

        Debug.Log($"Found levels container: {levelsContainer.name}");
        levels = new List<GameObject>();

        Transform nestedLevels = levelsContainer.transform.Find("Levels");
        if (nestedLevels != null)
            levelsContainer = nestedLevels.gameObject;

        for (int i = 0; i < levelsContainer.transform.childCount; i++)
        {
            GameObject child = levelsContainer.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Level_"))
                levels.Add(child);
        }

        if (levels.Count == 0)
            FindLevelsRecursive(levelsContainer.transform);
    }

    private void FindLevelsRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Level_"))
                levels.Add(child.gameObject);

            if (child.childCount > 0)
                FindLevelsRecursive(child);
        }
    }

    private void DeactivateAllLevels()
    {
        foreach (GameObject level in levels)
        {
            if (level != null) level.SetActive(false);
        }
    }

    private void ActivateLevel(int index)
    {
        if (player != null)
        {
            Destroy(player);
            player = null;
        }

        if (index < 0 || index >= levels.Count)
        {
            Debug.LogError($"Invalid level index: {index}");
            return;
        }

        if (currentLevelIndex != -1 && currentLevelIndex < levels.Count && levels[currentLevelIndex] != null)
            levels[currentLevelIndex].SetActive(false);

        _remainingPathObjectsInCurrentLevel = 0;

        GameObject newLevel = levels[index];
        newLevel.SetActive(true);
        currentLevelIndex = index;

        if (levelTitleText != null)
            levelTitleText.text = $"Level {currentLevelIndex + 1}";

        Transform startPoint = FindStartTagInChildren(newLevel);
        if (startPoint != null && PlayerSkinManager.instance != null)
        {
            player = PlayerSkinManager.instance.SpawnSelectedSkinAt(startPoint.position);
            if (player.TryGetComponent<SwipeSlidePlayer>(out var playerScript))
            {
                playerScript.ResetPlayerState(startPoint.position);
                playerScript.DeactivateTrailForLevelChange();
                playerScript.ActivateTrailAfterLevelLoad();
            }
        }
        else
        {
            Debug.LogWarning($"❌ No Start tag found in {newLevel.name}");
        }

        if (playerCinemachineCam != null)
            originalCameraSize = playerCinemachineCam.m_Lens.OrthographicSize;

        if (PuzzleCameraCinemachineController.Instance != null && index < cameraTargetSizes.Count)
        {
            Vector2 size = cameraTargetSizes[index];
            PuzzleCameraCinemachineController.Instance.SetCameraSize(size.x, size.y);
        }

        Transform tileBack = newLevel.transform.Find("Env/TileBack");
        if (tileBack != null)
        {
            currentTileBack = tileBack;
            if (tileBack.TryGetComponent<SpriteRenderer>(out var tileBackRenderer) && tileBackSprites.Length > 0)
            {
                int randomIndex = Random.Range(0, tileBackSprites.Length);
                tileBackRenderer.sprite = tileBackSprites[randomIndex];
            }
        }

        PathObject[] pathObjects = newLevel.GetComponentsInChildren<PathObject>(true);
        foreach (PathObject po in pathObjects)
        {
            po.gameObject.SetActive(true);
            po.ResetState();
            po.RegisterWithLevelManager();
        }

        if (_remainingPathObjectsInCurrentLevel == 0)
            LevelCompleted();
    }

    private Transform FindStartTagInChildren(GameObject levelRoot)
    {
        foreach (Transform t in levelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("Start"))
                return t;
        }
        return null;
    }

    private void LevelCompleted()
    {
        StartCoroutine(LevelCompleteRoutine());
    }

    private IEnumerator LevelCompleteRoutine()
    {
        if (!this || !gameObject.scene.isLoaded) yield break;

        float playerMoveFinishWait = 0.35f;
        if (player != null && player.TryGetComponent<SwipeSlidePlayer>(out var slidePlayer))
            playerMoveFinishWait = slidePlayer.moveDuration + 0.2f;

        yield return new WaitForSeconds(playerMoveFinishWait);

        if (player != null && player.TryGetComponent<SwipeSlidePlayer>(out slidePlayer) && playerCinemachineCam != null)
        {
            yield return StartCoroutine(RewindPlayerPathAndZoom(slidePlayer));
        }
        else
        {
            Debug.LogWarning("Rewind effect skipped: Player or Cinemachine Camera not found/assigned.");
            yield return new WaitForSeconds(zoomDuration);
            ActivateNextLevel();
        }
    }

    private void ActivateNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        if (nextIndex >= levels.Count)
            nextIndex = 0;

        // ✅ Save progress
        PlayerPrefs.SetInt("LastLevelIndex", nextIndex);
        PlayerPrefs.Save();

        // ✅ Count levels for ads
        levelsSinceLastAd++;
        if (levelsSinceLastAd >= 2)
        {
            levelsSinceLastAd = 0; // reset
            if (Interstitial.instance != null)
            {
                Interstitial.instance.ShowInterstitialAd();
            }
            else
            {
                Debug.LogWarning("Interstitial instance not found.");
            }
        }

        ActivateLevel(nextIndex);
    }

    private IEnumerator RewindPlayerPathAndZoom(SwipeSlidePlayer playerScript)
    {
        rewindRoutine = StartCoroutine(RunRewind(playerScript));
        yield return rewindRoutine;
    }

    private IEnumerator RunRewind(SwipeSlidePlayer playerScript)
    {
        // 5. UI animations: Fade out
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCanvasGroup(true, 0f, 0.2f));

        yield return new WaitForSeconds(0.2f);

        if (skipButton != null) skipButton.gameObject.SetActive(true);
        skipRewindRequested = false;

        List<Vector2> path = playerScript.GetPathHistory();
        if (path == null || path.Count < 2)
        {
            EndRewindRoutine();
            yield break;
        }

        // --- Camera Setup ---
        var tempTarget = new GameObject("RewindTarget");
        float targetZoomSize = rewindZoomSize;
        if (currentTileBack != null && currentTileBack.TryGetComponent<SpriteRenderer>(out var sr))
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            targetZoomSize = CalculateSizeToFitTileBack(sr, aspectRatio);
            tempTarget.transform.position = currentTileBack.position;
        }
        else
        {
            Vector3 pathCenter = Vector3.zero;
            foreach (Vector2 pos in path) pathCenter += new Vector3(pos.x, pos.y, playerScript.transform.position.z);
            pathCenter /= path.Count;
            tempTarget.transform.position = pathCenter;
        }

        var originalTarget = playerCinemachineCam.Follow;
        playerCinemachineCam.Follow = tempTarget.transform;
        yield return StartCoroutine(ZoomCamera(playerCinemachineCam, originalCameraSize, targetZoomSize, zoomDuration));

        playerScript.ActivateTrailAfterLevelLoad();

        // 3. Rewind sound effect + pitch up
        if (AudioManager.instance != null)
        {
            rewindAudioSource = AudioManager.instance.GetComponent<AudioSource>();
            if (rewindAudioSource == null) rewindAudioSource = AudioManager.instance.gameObject.AddComponent<AudioSource>();
            
            AudioManager.instance.PlaySound(rewindSoundName);
            rewindAudioSource.loop = true;
            if (skipRewindRequested)
                rewindAudioSource.pitch = skipSoundPitch;
        }

        // --- 1. Speed-up effect when skipping ---
        float currentRewindSpeed = skipRewindRequested ? rewindSpeed / skipSpeedMultiplier : rewindSpeed;
        
        // Rewind movement
        for (int i = path.Count - 1; i >= 0; i--)
        {
            if (skipRewindRequested || playerScript == null) break;

            Vector3 targetPos = new Vector3(path[i].x, path[i].y, playerScript.transform.position.z);
            Vector3 startPos = playerScript.transform.position;

            float t = 0f;
            while (t < 1f)
            {
                if (skipRewindRequested || playerScript == null) break;
                t += Time.deltaTime / currentRewindSpeed;
                playerScript.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            if (skipRewindRequested || playerScript == null) break;
            playerScript.transform.position = targetPos;
        }

        if (playerScript != null && path.Count > 0)
        {
            Vector2 lastPos = path[0];
            playerScript.transform.position = new Vector3(lastPos.x, lastPos.y, playerScript.transform.position.z);
        }

        // Stop rewind sound
        if (rewindAudioSource != null && rewindAudioSource.isPlaying)
        {
            rewindAudioSource.Stop();
        }

        if (playerScript != null)
        {
            playerScript.DeactivateTrailForLevelChange();
            Destroy(playerScript.gameObject);
        }

        // --- Cleanup & Success Effects ---
        yield return StartCoroutine(ZoomCamera(playerCinemachineCam, rewindZoomSize, originalCameraSize, zoomDuration));
        playerCinemachineCam.Follow = originalTarget;
        Destroy(tempTarget);

        EndRewindRoutine();
    }

    private void EndRewindRoutine()
    {
        // 5. UI animations: Fade back in
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCanvasGroup(false, 1f, 0.2f));

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // 2. Screen flash or particle burst at the end
        if (levelSuccessEffectPrefab != null && effectSpawnPoint != null)
        {
            Instantiate(levelSuccessEffectPrefab, effectSpawnPoint.position, Quaternion.identity);
            // Call your ScreenFlash script here:
            // ScreenFlash.Flash(Color.white, 0.2f);
        }
        
        // 4. Camera shake at success
        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        // 3. Play success sound & activate next level
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundWithCallback("LevelSuccess", () =>
            {
                if (this && gameObject.scene.isLoaded)
                    ActivateNextLevel();
            });
        }
        else
        {
            ActivateNextLevel();
        }

        rewindRoutine = null;
    }

    private IEnumerator ZoomCamera(CinemachineVirtualCamera cam, float fromSize, float toSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cam.m_Lens.OrthographicSize = Mathf.Lerp(fromSize, toSize, t);
            yield return null;
        }
        cam.m_Lens.OrthographicSize = toSize;
    }

    private IEnumerator FadeCanvasGroup(bool isFadingOut, float targetAlpha, float duration)
    {
        foreach (var element in uiElementsToToggle)
        {
            if (element.TryGetComponent<CanvasGroup>(out var group))
            {
                float startAlpha = group.alpha;
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                    yield return null;
                }
                group.alpha = targetAlpha;
                group.interactable = !isFadingOut;
                group.blocksRaycasts = !isFadingOut;
            }
            else
            {
                element.SetActive(targetAlpha > 0);
            }
        }
    }

    private float CalculateSizeToFitTileBack(SpriteRenderer sr, float aspectRatio)
    {
        Bounds bounds = sr.bounds;
        float tileHeight = bounds.size.y;
        float tileWidth = bounds.size.x;

        float sizeByHeight = tileHeight / 2f;
        float sizeByWidth = (tileWidth / 2f) / aspectRatio;
        
        return Mathf.Max(sizeByHeight, sizeByWidth);
    }
}