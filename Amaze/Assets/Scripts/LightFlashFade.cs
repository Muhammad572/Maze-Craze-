using UnityEngine;

public class LightFlashFade : MonoBehaviour
{
    public float fadeDuration = 0.3f;   // how quickly the light fades
    private Light flashLight;           // 3D light
#if USING_URP
    private UnityEngine.Rendering.Universal.Light2D flashLight2D; // if using 2D lights
#endif
    private float startIntensity;
    private float t = 0f;

    void Awake()
    {
        flashLight = GetComponent<Light>();
#if USING_URP
        flashLight2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
#endif
    }

    void OnEnable()
    {
        if (flashLight != null)
            startIntensity = flashLight.intensity;
#if USING_URP
        if (flashLight2D != null)
            startIntensity = flashLight2D.intensity;
#endif
        t = 0f;
    }

    void Update()
    {
        t += Time.deltaTime / fadeDuration;

        if (flashLight != null)
            flashLight.intensity = Mathf.Lerp(startIntensity, 0f, t);

#if USING_URP
        if (flashLight2D != null)
            flashLight2D.intensity = Mathf.Lerp(startIntensity, 0f, t);
#endif

        if (t >= 1f)
            Destroy(gameObject); // remove light when done
    }
}
