using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;

    private bool gyroEnabled;
    public Gyroscope gyro;

    public Quaternion initialRotation;//other

    void Start()
    {
        //gyroEnabled = EnableGyro();

        //initialRotation = transform.rotation;//other

        InitializeGyro();
    }

    bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            return true;
        }
        return false;
    }

    void Update()
    {
        if (playerManager && playerManager.movePhysically)
        {
            ThirdExample();
            RotatePlayerFace();
        }

        //SecondExample();

        //WorkingReverse();
    }

    void WorkingReverse()
    {
        if (gyroEnabled)
        {
            // Get the rotation rate around the z-axis
            //float zRotationRate = -gyro.rotationRateUnbiased.z;
            float zRotationRate = gyro.rotationRateUnbiased.z;

            // Adjust the rotation speed if needed
            float rotationSpeed = 100f;

            // Rotate the object around the z-axis based on the phone's rotation
            transform.Rotate(Vector3.forward, zRotationRate * rotationSpeed * Time.deltaTime);
        }
    }

    void SecondExample()
    {
        // Get gyroscope rotation rate
        Quaternion gyroRotationRate = gyro.attitude;

        // Apply rotation to object
        transform.rotation = Quaternion.Euler(0, 0, -gyroRotationRate.eulerAngles.z);
    }

    void InitializeGyro()
    {
        // Check if gyroscope is supported
        gyroEnabled = SystemInfo.supportsGyroscope;

        if (gyroEnabled)
        {
            // Enable the gyroscope
            gyro = Input.gyro;
            gyro.enabled = true;

            // Set the initial rotation
            initialRotation = Quaternion.Euler(0, 0, 0); // You can adjust the initial rotation as needed
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device.");
        }
    }

    void ThirdExample()
    {
        if (gyroEnabled)
        {
            // Rotate the object based on gyroscope data when in portrait mode
            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                // Get gyroscope rotation rate
                Quaternion gyroRotationRate = gyro.attitude;

                // Apply rotation to object
                transform.rotation = initialRotation * Quaternion.Euler(0, 0, gyroRotationRate.eulerAngles.z);

                //Debug.Log(transform.eulerAngles);
            }
        }
    }

    void RotatePlayerFace()
    {
        if (Application.isEditor) return;

        if (transform.eulerAngles.z >= 260f && transform.eulerAngles.z <= 280f)
        {
            //right
            playerManager.RotatePlayer(new Vector3(0, 0, 270));
            //playerManager.RotateCamera(new Vector3(0, 0, 270), playerManager.canMoveRight);
        }
        else if (transform.eulerAngles.z >= 80f && transform.eulerAngles.z <= 100f)
        {
            //left
            playerManager.RotatePlayer(new Vector3(0, 0, 90));
            //playerManager.RotateCamera(new Vector3(0, 0, 90), playerManager.canMoveLeft);
        }
        else if (transform.eulerAngles.z >= 350f || transform.eulerAngles.z <= 10f)
        {
            //up
            playerManager.RotatePlayer(new Vector3(0, 0, 0));
            //playerManager.RotateCamera(new Vector3(0, 0, 0), playerManager.canMoveUp);
        }
        else if (transform.eulerAngles.z >= 170f && transform.eulerAngles.z <= 190f)
        {
            //down
            playerManager.RotatePlayer(new Vector3(0, 0, 180));
            //playerManager.RotateCamera(new Vector3(0, 0, 180), playerManager.canMoveDown);
        }
    }
}
