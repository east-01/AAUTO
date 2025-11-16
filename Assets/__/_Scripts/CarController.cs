using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class CarController : MonoBehaviour
{

    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    private float currentBreakForce;
    private float currentSteerAngle;
    private float currentThrottle = 0f;
    private float currentCarSpeedMPH;

    private Rigidbody rb;
    private TextMeshProUGUI textMPH;

    [Header("Car Settings")]
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float motorForce = 4000f;
    [SerializeField] private float breakForce = 5000f;
    [SerializeField] private float maxSpeedMPH = 120f;
    [SerializeField] private float throttleAcceleration = 6f;
    [SerializeField] private float throttleDeceleration = 8f;
    [SerializeField] private bool isFrontWheelDrive = true;

    [Header("Wheel Colliders")]
    [SerializeField] private List<WheelCollider> FrontWheelColliders;
    [SerializeField] private List<WheelCollider> BackWheelColliders;

    [Header("Wheel Visuals")]
    [SerializeField] private List<Transform> FrontWheelTransforms;
    [SerializeField] private List<Transform> BackWheelTransforms;

    [Header("Test Settings")]
    [SerializeField] private bool useKeyboardInput = false; // DISABLE FOR AGENT USE

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject textObject = GameObject.Find("TextMPH");
        if (textObject != null)
        {
            textMPH = textObject.GetComponent<TextMeshProUGUI>();
        }
    }
    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBreaking();
        UpdateWheels();
        ApplySteeringStabilizer();
    }
    private void Update()
    {
        if (useKeyboardInput)
            GetKeyboardInput();
        PrintMPH();
    }

    // Rrevent Fishtailing
    private void ApplySteeringStabilizer()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);

        float angle = Vector3.SignedAngle(flatVelocity, forward, Vector3.up);

        rb.AddTorque(Vector3.up * -angle * 5f);
    }

    // Set Inputs for either Keyboard or Agent
    public void SetInput(float vertical, float horizontal, bool breaking)
    {
        verticalInput = vertical;
        horizontalInput = horizontal;
        isBreaking = breaking;
    }
    
    // For Motion Testing
    private void GetKeyboardInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    // Display Speed in MPH
    private void PrintMPH()
    {
        float speed = rb.linearVelocity.magnitude;
        currentCarSpeedMPH = speed * 2.23694f;
        if (textMPH != null)
        {
            textMPH.text = Mathf.RoundToInt(currentCarSpeedMPH) + " MPH";
        }
    }

    // Handle Motor Torque and Throttle
    private void HandleMotor()
    {
        // Fade Acceleration as you approach top speed
        float torqueFactor = 1f / (1f + Mathf.Exp(((currentCarSpeedMPH / maxSpeedMPH) - 0.85f) * 12f));
        float effectiveMotorForce = motorForce * torqueFactor;

        if (verticalInput > 0f)
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, verticalInput, throttleAcceleration * Time.fixedDeltaTime);
        }
        else if (verticalInput < 0f && Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward)) < 1.5f)
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, verticalInput, throttleDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentThrottle = 0f;
        }

        // Apply Motor Torque to either Front or Back Wheels
        List<WheelCollider> driveWheels = isFrontWheelDrive ? FrontWheelColliders : BackWheelColliders;
        foreach (WheelCollider wheel in driveWheels)
        {
            wheel.motorTorque = currentThrottle * effectiveMotorForce;
        }
    }

    // Handle Braking
    private void HandleBreaking()
    {
        currentBreakForce = isBreaking ? breakForce : 0f;

        foreach (WheelCollider wheel in FrontWheelColliders)
        {
            wheel.brakeTorque = currentBreakForce;
        }

        foreach (WheelCollider wheel in BackWheelColliders)
        {
            wheel.brakeTorque = currentBreakForce;
        }
    }


    // Handle Steering
    private void HandleSteering()
    {
        float targetSteerAngle = maxSteeringAngle * horizontalInput;

        float steerSpeed = 5f;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerSpeed * Time.fixedDeltaTime);

        foreach (WheelCollider wheel in FrontWheelColliders)
        {
            wheel.steerAngle = currentSteerAngle;
        }
    }

    // Match Wheel Visuals to Wheel Colliders
    private void UpdateWheels()
    {
        Vector3 pos;
        Quaternion rot;

        for (int i = 0; i < BackWheelColliders.Count; i++)
        {
            BackWheelColliders[i].GetWorldPose(out pos, out rot);
            BackWheelTransforms[i].position = pos;
            BackWheelTransforms[i].rotation = rot;
        }

        for (int i = 0; i < FrontWheelColliders.Count; i++)
        {
            FrontWheelColliders[i].GetWorldPose(out pos, out rot);
            FrontWheelTransforms[i].position = pos;
            FrontWheelTransforms[i].rotation = rot;
        }
    }   

}