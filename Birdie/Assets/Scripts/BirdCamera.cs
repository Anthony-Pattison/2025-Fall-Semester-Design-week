using UnityEngine;

public class BirdCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform bird;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -8f);

    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool lookAtBird = true;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0f, 0f);

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (bird == null) return;

        // Calculate desired position based on bird's rotation
        Vector3 desiredPosition = bird.position + bird.rotation * offset;

        // Smoothly move camera to desired position using SmoothDamp for physics-synced movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);

        // Smoothly rotate camera to match bird's yaw (left/right rotation)
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, bird.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Look at the bird
        if (lookAtBird)
        {
            Vector3 lookTarget = bird.position + lookAtOffset;
            transform.LookAt(lookTarget);
        }
    }
}