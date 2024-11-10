using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CarController : MonoBehaviour
{

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;

    private bool isBreaking;
    private float currentBreakForce;

    private float currentSteerAngle;

    [SerializeField] private float maxSteeringAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandelSteering();
        UpdateWheels();

    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);

        isBreaking = Input.GetKey(KeyCode.Space);

    }
    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        currentBreakForce = isBreaking ? breakForce : 0f;

        if (isBreaking)
        {
            ApplyBreaking();
        }
        if (!isBreaking)
        {
            frontRightWheelCollider.brakeTorque = frontLeftWheelCollider.brakeTorque =
                rearRightWheelCollider.brakeTorque =
                rearLeftWheelCollider.brakeTorque = 0f;
        }


    }
    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;
    } 
    private void HandelSteering()
    {
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

    }
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheel);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheel);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheel);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheel);
    }
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }   

}
