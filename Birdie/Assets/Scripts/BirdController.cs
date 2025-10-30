using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float flapForce = 8f;
    [SerializeField] private float flapCooldown = 0.5f;
    [SerializeField] private float flapForwardBoost = 3f;
    [SerializeField] private float turnSpeed = 80f;
    [SerializeField] private float baseForwardSpeed = 10f;
    [SerializeField] private float maxForwardSpeed = 30f;
    [SerializeField] private float minForwardSpeed = 8f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Diving Settings")]
    [SerializeField] private float diveForce = 15f;
    [SerializeField] private float diveSpeedGain = 5f;
    [SerializeField] private float diveGravityMultiplier = 2f;

    [Header("Gliding Settings")]
    [SerializeField] private float glideGravityMultiplier = 0.3f;
    [SerializeField] private float speedDecayRate = 2f;
    [SerializeField] private float glideSpeedBonus = 1.5f;
    [SerializeField] private float glideLevelOutSpeed = 2f;
    [SerializeField] private float glidePullUpForce = 8f;
    [SerializeField] private float momentumConversionRate = 0.7f;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 30f;
    [SerializeField] private float tiltSpeed = 3f;

    [Header("Animation Settings")]
    [SerializeField] private Animator birdAnimator;
    [SerializeField] private string flapTriggerName = "Flap";

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flapSound;
    [SerializeField] private AudioClip diveSound;
    [SerializeField] private AudioClip glideSound;
    [SerializeField] private float flapVolume = 1f;
    [SerializeField] private float diveVolume = 0.7f;
    [SerializeField] private float glideVolume = 0.6f;

    private Rigidbody rb;
    private float currentVerticalVelocity;
    private float currentForwardSpeed;
    private bool isDiving;
    private bool isGliding;
    private bool wasGlidingLastFrame;
    private float lastFlapTime = -999f;
    private bool wasDivingLastFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentForwardSpeed = baseForwardSpeed;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        HandleInput();
        ApplyTilt();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleInput()
    {
        // Dive - Hold Spacebar
        isDiving = Input.GetKey(KeyCode.Space);

        // Play dive sound when starting to dive
        if (isDiving && !wasDivingLastFrame)
        {
            PlayDiveSound();
        }
        wasDivingLastFrame = isDiving;

        // Flap - Left Mouse Button only (with cooldown)
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastFlap = Time.time - lastFlapTime;
            if (timeSinceLastFlap >= flapCooldown)
            {
                Flap();
            }
        }

        // Glide - Right Mouse Button
        bool currentlyGliding = Input.GetMouseButton(1);

        // Play glide sound when starting to glide
        if (currentlyGliding && !isGliding)
        {
            PlayGlideSound();
        }

        isGliding = currentlyGliding;
    }

    void Flap()
    {
        currentVerticalVelocity = flapForce;
        currentForwardSpeed += flapForwardBoost;
        lastFlapTime = Time.time;

        // Play flap sound
        PlayFlapSound();

        // Trigger flap animation
        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger(flapTriggerName);
        }
    }

    void PlayFlapSound()
    {
        if (audioSource != null && flapSound != null)
        {
            audioSource.PlayOneShot(flapSound, flapVolume);
        }
    }

    void PlayDiveSound()
    {
        if (audioSource != null && diveSound != null)
        {
            audioSource.PlayOneShot(diveSound, diveVolume);
        }
    }

    void PlayGlideSound()
    {
        if (audioSource != null && glideSound != null)
        {
            audioSource.PlayOneShot(glideSound, glideVolume);
        }
    }

    void ApplyMovement()
    {
        // Get horizontal input for turning
        float horizontal = Input.GetAxis("Horizontal"); // A = -1, D = 1

        // Rotate the bird (steering)
        float turnAmount = horizontal * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, turnAmount, 0f);

        // Handle diving mechanics
        if (isDiving)
        {
            // Push down and gain speed
            currentVerticalVelocity -= diveForce * Time.fixedDeltaTime;
            currentForwardSpeed += diveSpeedGain * Time.fixedDeltaTime;

            // Apply stronger gravity while diving
            currentVerticalVelocity -= gravity * diveGravityMultiplier * Time.fixedDeltaTime;
        }
        else if (isGliding)
        {
            // When entering glide from a dive, convert downward momentum to forward speed
            if (!wasGlidingLastFrame && currentVerticalVelocity < 0)
            {
                float downwardMomentum = Mathf.Abs(currentVerticalVelocity);
                float speedBoost = downwardMomentum * momentumConversionRate;
                currentForwardSpeed += speedBoost;
            }

            // Continuously pull up while gliding - creates the curved trajectory
            if (currentVerticalVelocity < 0)
            {
                // Still pulling up from dive
                currentVerticalVelocity += glidePullUpForce * Time.fixedDeltaTime;
                // Don't let it go above 0 too fast during the pull-up
                if (currentVerticalVelocity > 0)
                {
                    currentVerticalVelocity *= 0.5f;
                }
            }
            else
            {
                // Once leveled out, apply minimal gravity
                currentVerticalVelocity -= gravity * glideGravityMultiplier * Time.fixedDeltaTime;
            }

            // Slowly decay speed back to base, but keep bonus from diving
            float targetSpeed = baseForwardSpeed * glideSpeedBonus;
            if (currentForwardSpeed > targetSpeed)
            {
                currentForwardSpeed -= speedDecayRate * Time.fixedDeltaTime;
                currentForwardSpeed = Mathf.Max(currentForwardSpeed, targetSpeed);
            }
        }
        else
        {
            // Normal gravity when neither diving nor gliding
            currentVerticalVelocity -= gravity * Time.fixedDeltaTime;

            // Return to base speed
            if (currentForwardSpeed > baseForwardSpeed)
            {
                currentForwardSpeed -= speedDecayRate * 1.5f * Time.fixedDeltaTime;
                currentForwardSpeed = Mathf.Max(currentForwardSpeed, baseForwardSpeed);
            }
            else if (currentForwardSpeed < baseForwardSpeed)
            {
                currentForwardSpeed += speedDecayRate * Time.fixedDeltaTime;
                currentForwardSpeed = Mathf.Min(currentForwardSpeed, baseForwardSpeed);
            }
        }

        // Clamp speeds
        currentVerticalVelocity = Mathf.Max(currentVerticalVelocity, -maxFallSpeed);
        currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, minForwardSpeed, maxForwardSpeed);

        // Forward movement (uses current speed)
        Vector3 forwardMovement = transform.forward * currentForwardSpeed;
        Vector3 verticalMovement = Vector3.up * currentVerticalVelocity;

        // Combine movements
        Vector3 finalVelocity = forwardMovement + verticalMovement;
        rb.linearVelocity = finalVelocity;

        // Track gliding state for next frame
        wasGlidingLastFrame = isGliding;
    }

    void ApplyTilt()
    {
        // Calculate target tilt based on horizontal input
        float horizontal = Input.GetAxis("Horizontal");
        float targetZRotation = -horizontal * tiltAmount;

        // Calculate pitch based on state
        float targetXRotation;
        if (isDiving)
        {
            // Pitch forward more dramatically when diving
            targetXRotation = Mathf.Clamp(-currentVerticalVelocity * 4f, -60f, 45f);
        }
        else if (isGliding)
        {
            // Smoothly level out during glide - bird follows the curved trajectory
            float currentPitch = transform.eulerAngles.x > 180 ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
            targetXRotation = Mathf.Lerp(currentPitch, 0f, Time.deltaTime * glideLevelOutSpeed);
        }
        else
        {
            // Normal pitch based on vertical velocity
            targetXRotation = Mathf.Clamp(-currentVerticalVelocity * 3f, -45f, 45f);
        }

        // Smoothly rotate towards target
        Quaternion targetRotation = Quaternion.Euler(targetXRotation, transform.eulerAngles.y, targetZRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}