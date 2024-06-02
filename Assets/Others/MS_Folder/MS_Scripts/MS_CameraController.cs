using DG.Tweening;
using UnityEngine;

public class MS_CameraController : MonoBehaviour
{
    public Transform playerTransform;
    public Camera mainCamera;
    public float margin = 10f; // Margin value for all sides
    public float followSpeed = 5f; // Speed at which the camera follows the player

    void Update()
    {
        // Ensure playerTransform and mainCamera are assigned
        if (playerTransform == null || mainCamera == null)
        {
            Debug.LogWarning("Assign player transform and main camera in the inspector!");
            return;
        }

        // Convert player position to viewport space
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(playerTransform.position);

        // Add margins
        float leftMargin = margin;
        float rightMargin = 1 - margin;
        float bottomMargin = margin / 2;
        float topMargin = 1 - margin;

        // Check if player is outside camera view including margins
        if (viewportPos.x < leftMargin || viewportPos.x > rightMargin ||
            viewportPos.y < bottomMargin || viewportPos.y > topMargin ||
            viewportPos.z < 0)
        {
            // Smoothly interpolate the camera towards the player's position
            Vector3 targetPosition = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}
