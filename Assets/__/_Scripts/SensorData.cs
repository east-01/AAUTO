using UnityEngine;
using System;
using System.Net.Sockets;
using Unity.MLAgents.Sensors;
using System.IO;


// Requires a Python server to be running to send the data
public class SensorData : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField] Camera cameraFront;
    [SerializeField] Camera cameraFrontLeft;
    [SerializeField] Camera cameraFrontRight;
    [SerializeField] Camera cameraSideLeft;
    [SerializeField] Camera cameraSideRight;

    [Header("Camera Resolution")]
    [SerializeField] int camWidth = 1920;
    [SerializeField] int camHeight = 1080;

    [Header("Ray Sensor Setup")]
    [SerializeField] RayPerceptionSensor raySensorTop;

    [Header("Python Server Connection")]
    [SerializeField] public string pythonIP = "127.0.0.1";// For local use "127.0.0.1"; For cluster use "python-sensor"?
    [SerializeField] public int pythonPort = 5005;

    private TcpClient client;
    private NetworkStream stream;

    private Texture2D texFront;
    private Texture2D texFrontLeft;
    private Texture2D texFrontRight;
    private Texture2D texSideLeft;
    private Texture2D texSideRight;

    void Start()
    {
        try
        {
            client = new TcpClient(pythonIP, pythonPort);
            stream = client.GetStream();
            Debug.Log($"Successfully connected to Python server at {pythonIP}:{pythonPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to Python server: {e}");
        }

        SetupCamera(cameraFront, ref texFront);
        SetupCamera(cameraFrontLeft, ref texFrontLeft);
        SetupCamera(cameraFrontRight, ref texFrontRight);
        SetupCamera(cameraSideLeft, ref texSideLeft);
        SetupCamera(cameraSideRight, ref texSideRight);
    }

    void LateUpdate()
    {
        // Capture and send cameras
        SendCamera(cameraFront, texFront);
        SendCamera(cameraFrontLeft, texFrontLeft);
        SendCamera(cameraFrontRight, texFrontRight);
        SendCamera(cameraSideLeft, texSideLeft);
        SendCamera(cameraSideRight, texSideRight);

        // Capture and send RayPerceptionSensor
        //SendRaySensor(raySensorTop);
    }

    void SetupCamera(Camera cam, ref Texture2D tex)
    {
        cam.targetTexture = new RenderTexture(camWidth, camHeight, 24);
        tex = new Texture2D(camWidth, camHeight, TextureFormat.RGB24, false);
    }

    void SendCamera(Camera cam, Texture2D tex)
    {
        if (cam == null)
        {
            Debug.LogWarning("Camera is null");
        }
        if (tex == null)
        {
            Debug.LogWarning("Texture is null");
        }

        // Render camera to texture
        RenderTexture.active = cam.targetTexture;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        // Convert to bytes
        byte[] bytes = tex.GetRawTextureData();

        // Send length + bytes
        byte[] lenBytes = BitConverter.GetBytes(bytes.Length);
        stream.Write(lenBytes, 0, lenBytes.Length);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
    }

    void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
