using UnityEngine;

public class CloudFogEffect : MonoBehaviour
{
    [Header("Fog Settings")]
    [SerializeField] private Color fogColor = new Color(0.9f, 0.9f, 0.95f, 1f);
    [SerializeField] private float fogDensity = 0.08f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float visibilityDistance = 20f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip cloudEnterSound;
    [SerializeField] private AudioSource audioSource;

    private Camera mainCamera;
    private float originalFarClipPlane;
    private static int playersInClouds = 0; // Track how many clouds player is in
    private static float originalFogDensity;
    private static Color originalFogColor;
    private static bool fogInitialized = false;
    private bool playerInThisCloud = false;

    void Start()
    {
        mainCamera = Camera.main;

        // Store original settings only once
        if (!fogInitialized)
        {
            originalFogDensity = RenderSettings.fogDensity;
            originalFogColor = RenderSettings.fogColor;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            fogInitialized = true;
        }

        if (mainCamera != null)
        {
            originalFarClipPlane = mainCamera.farClipPlane;
        }
    }

    void Update()
    {
        // Debug: Show current fog state
        if (playerInThisCloud)
        {
            Debug.Log($"Player in cloud! Players in clouds: {playersInClouds}, Current fog density: {RenderSettings.fogDensity}");
        }

        // Update fog only if at least one cloud is active
        if (playersInClouds > 0)
        {
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, fogDensity, Time.deltaTime * transitionSpeed);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColor, Time.deltaTime * transitionSpeed);

            if (mainCamera != null)
            {
                mainCamera.farClipPlane = Mathf.Lerp(mainCamera.farClipPlane, visibilityDistance, Time.deltaTime * transitionSpeed);
            }
        }
        else
        {
            // Return to normal when not in any cloud
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, originalFogDensity, Time.deltaTime * transitionSpeed);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, originalFogColor, Time.deltaTime * transitionSpeed);

            if (mainCamera != null)
            {
                mainCamera.farClipPlane = Mathf.Lerp(mainCamera.farClipPlane, originalFarClipPlane, Time.deltaTime * transitionSpeed);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Cloud collision detected with: {other.gameObject.name}, Tag: {other.tag}");

        if (other.CompareTag("Player") && !playerInThisCloud)
        {
            Debug.Log("Player ENTERED cloud - Applying fog effect");
            playerInThisCloud = true;
            playersInClouds++;

            if (audioSource != null && cloudEnterSound != null)
            {
                audioSource.PlayOneShot(cloudEnterSound);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Cloud exit detected with: {other.gameObject.name}");

        if (other.CompareTag("Player") && playerInThisCloud)
        {
            Debug.Log("Player EXITED cloud - Removing fog effect");
            playerInThisCloud = false;
            playersInClouds = Mathf.Max(0, playersInClouds - 1);
        }
    }

    void OnDestroy()
    {
        // Clean up if this cloud is destroyed while player is inside
        if (playerInThisCloud)
        {
            playersInClouds = Mathf.Max(0, playersInClouds - 1);
        }
    }
}