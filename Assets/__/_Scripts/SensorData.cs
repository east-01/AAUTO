using UnityEngine;
using System;
using System.Net.Sockets;
using Unity.MLAgents.Sensors;
using System.IO;

public class SensorData : MonoBehaviour
{
    [SerializeField] Camera cameraCenterFront;
    [SerializeField] Camera cameraLeftFront;
    [SerializeField] Camera cameraRightFront;
    [SerializeField] RayPerceptionSensor raySensorTop;
    [SerializeField] int camWidth = 1920;
    [SerializeField] int camHeight = 1080;


    public string pythonIP = "python-sensor";
    public int pythonPort = 5005;

    private TcpClient client;
    private NetworkStream stream;

    private Texture2D texFront;
    private Texture2D texLeft;
    private Texture2D texRight;

    void Start()
    {
        try
        {
            client = new TcpClient(pythonIP, pythonPort);
            stream = client.GetStream();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to Python server: {e}");
        }

        SetupCamera(cameraCenterFront, ref texFront);
        SetupCamera(cameraLeftFront, ref texLeft);
        SetupCamera(cameraRightFront, ref texRight);
    }

    void LateUpdate()
    {
        // Capture and send cameras
        SendCamera(cameraCenterFront, texFront);
        SendCamera(cameraLeftFront, texLeft);
        SendCamera(cameraRightFront, texRight);

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
