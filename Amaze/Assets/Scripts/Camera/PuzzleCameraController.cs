using UnityEngine;
using Cinemachine;

[ExecuteAlways] // Enable in Edit mode
public class PuzzleCameraCinemachineController : MonoBehaviour
{
    public static PuzzleCameraCinemachineController Instance;

    public CinemachineVirtualCamera virtualCamera;

    [SerializeField] private float targetWidth = 10f;
    [SerializeField] private float targetHeight = 16f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        AdjustCamera();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            AdjustCamera();
        }
    }
#endif

    public void SetCameraSize(float width, float height)
    {
        targetWidth = width;
        targetHeight = height;
        AdjustCamera();
    }

    void AdjustCamera()
    {
        if (virtualCamera == null) return;

        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = targetWidth / targetHeight;

        float orthographicSize;

        if (screenAspect < targetAspect)
        {
            orthographicSize = (targetWidth / screenAspect) / 2f;
        }
        else
        {
            orthographicSize = targetHeight / 2f;
        }

        virtualCamera.m_Lens.OrthographicSize = orthographicSize;
    }
}
