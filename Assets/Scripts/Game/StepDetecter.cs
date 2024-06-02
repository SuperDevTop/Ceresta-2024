using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepDetector : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;
    public float threshold; // Threshold for peak detection default value 1
    public float peakDetectionWindow = 0.5f; // Window size for peak detection (in seconds)
    public float stepDetectionInterval; // Interval for step detection (in seconds) default value 1

    public float lastStepTime;
    public int stepCount;

    void Start()
    {
        if (threshold <= 0) threshold = 1.25f;
        if (stepDetectionInterval <= 0) stepDetectionInterval = 1;

        lastStepTime = Time.time;
        stepCount = 0;
    }

    void Update()
    {
        if (playerManager.movePhysically)
        {
            // Calculate elapsed time since last step detection
            float elapsedTime = Time.time - lastStepTime;

            // Check if it's time to perform step detection
            if (elapsedTime >= stepDetectionInterval)
            {
                // Get accelerometer input
                Vector3 acceleration = Input.acceleration;

                // Calculate magnitude of acceleration
                float accelerationMagnitude = acceleration.magnitude;

                // Detect peaks in acceleration data
                if (accelerationMagnitude > threshold)
                {
                    // Increment step count

                    if (playerManager.canMoveRight || playerManager.canMoveLeft || playerManager.canMoveUp || playerManager.canMoveDown)
                    {
                        //if (!playerManager.movingRight && !playerManager.movingLeft && !playerManager.movingUp && !playerManager.movingDown)
                        //{
                        if (playerManager.playerFace.localEulerAngles == new Vector3(0, 0, 270) && playerManager.canMoveRight)
                        {
                            stepCount++;
                        }
                        else if (playerManager.playerFace.localEulerAngles == new Vector3(0, 0, 90) && playerManager.canMoveLeft)
                        {
                            stepCount++;
                        }
                        else if (playerManager.playerFace.localEulerAngles == new Vector3(0, 0, 0) && playerManager.canMoveUp)
                        {
                            stepCount++;
                        }
                        else if (playerManager.playerFace.localEulerAngles == new Vector3(0, 0, 180) && playerManager.canMoveDown)
                        {
                            stepCount++;
                        }

                        // Update last step time
                        lastStepTime = Time.time;

                        Debug.Log("Steps : " + stepCount);
                        //}
                    }
                }
            }
        }
    }
}
