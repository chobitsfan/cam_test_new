using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV_CAM : MonoBehaviour
{
    [DllImport("vplayerUnity.dll")]
    public static extern IntPtr NPlayer_Init();
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_Connect(IntPtr pPlayer, string url, int mode);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_GetWidth(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_GetHeight(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_Uninit(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_ReadFrame(IntPtr pPlayer, IntPtr buffer, out UInt64 timestamp);

    WebCamTexture webcamTexture;
    //RenderTexture renderTexture;
    public Camera cam;
    public Material mat;
    //public Shader shader;
    Texture2D distortMap;
    double _CX = 6.395 * 100;
    double _CY = 3.595 * 100;
    double _FX = 1.2936588953959019 * 1000;
    double _FY = 1.2936588953959019 * 1000;
    double _K1 = 3.9125784966932795 * 0.01;
    double _K2 = 7.6818881727080013 * 0.1;
    double _P1 = 0;
    double _P2 = 0;
    double _K3 = -3.238587127227778;

    protected IntPtr ptr;
    protected int w, h;
    protected int frameLen;
    public byte[] buffer;
    protected IntPtr unmanagedBuffer;
    protected bool bStart;
    Texture2D texY;
    Texture2D texU;
    Texture2D texV;

    // Start is called before the first frame update
    void Start()
    {
        ptr = IntPtr.Zero;
        ptr = NPlayer_Init();
        NPlayer_Connect(ptr, "rtsp://192.168.50.92/v1/", 1);
        bStart = false;
        
        int camWidth = 1280;
        int camHeight = 720;
        cam.fieldOfView = (float)(Math.Atan(camHeight / 2.0 / _FY) * 2 / Math.PI * 180);
        Debug.Log(Screen.width + "x" + Screen.height + ":" + SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        int width = Screen.width;
        int height = Screen.height;
        distortMap = new Texture2D(width, height, TextureFormat.RGFloat, false, true);
        distortMap.filterMode = FilterMode.Point;
        distortMap.anisoLevel = 1;
        distortMap.wrapMode = TextureWrapMode.Clamp;
        float[] distortData = new float[width * height * 2];
        for (int i = 0; i < distortData.Length; i++)
        {
            distortData[i] = -1;
        }
        for (double i = 0; i < camHeight; i+=0.5)
        {
            for (double j = 0; j < camWidth; j+=0.5)
            {
                double x = (j - _CX) / _FX;
                double y = (i - _CY) / _FY;
                double r2 = x * x + y * y;
                double distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                double x_distort = x * distort;
                double y_distort = y * distort;
                x_distort += (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort += (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                x_distort = x_distort * _FX + _CX;
                y_distort = y_distort * _FY + _CY;
                //Debug.Log(x_distort + "," + y_distort);
                int idxU = (int)Math.Round(x_distort / camWidth * width);
                int idxV = (int)Math.Round(y_distort / camHeight * height);
                if (idxU >=0 && idxV>=0 && idxU < width && idxV < height)
                {
                    int mapIdx = idxV * width * 2 + idxU * 2;
                    //Debug.Log(mapIdx);
                    distortData[mapIdx] = (float)j / camWidth;
                    distortData[mapIdx + 1] = (float)i / camHeight;
                }
            }
        }
        /*for (int i = 0; i < distortData.Length; i++)
        {
            if (distortData[i] < 0)
            {
                distortData[i] = distortData[i - 1];
            }
        }*/
        distortMap.SetPixelData(distortData, 0);
        distortMap.Apply(false);
        mat.SetTexture("_DistortTex", distortMap);

        //renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        //cam.targetTexture = renderTexture;
        //cam.forceIntoRenderTexture = true;

        //cam.SetReplacementShader(shader, "");
        WebCamDevice[] webCams = WebCamTexture.devices;
        foreach (WebCamDevice webCam in webCams)
        {
            if (webCam.name.StartsWith("USB2.0"))
            {
                Debug.Log("background camera:" + webCam.name);
                webcamTexture = new WebCamTexture(webCam.name);
                webcamTexture.Play();
                mat.SetTexture("_CamTex", webcamTexture);
                break;
            }
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    //void OnPreRender()
    //{
    //cam.targetTexture = renderTexture;
    //cam.forceIntoRenderTexture = true;
    //Graphics.Blit(webcamTexture, null as RenderTexture);
    //}

    void initVideoFrameBuffer()
    {
        w = NPlayer_GetWidth(ptr);
        h = NPlayer_GetHeight(ptr);        
        if (w != 0 && h != 0)
        {
            Debug.Log("width = " + w + ", height = " + h);
            frameLen = w * h * 3;
            Debug.Log("frameLen = " + frameLen);
            buffer = new byte[frameLen];
            unmanagedBuffer = Marshal.AllocHGlobal(frameLen);

            bStart = true;

            texY = new Texture2D(w, h, TextureFormat.Alpha8, false);
            //U分量和V分量分別存放在兩張貼圖中
            texU = new Texture2D(w >> 1, h >> 1, TextureFormat.Alpha8, false);
            texV = new Texture2D(w >> 1, h >> 1, TextureFormat.Alpha8, false);
            mat.SetTexture("_YTex", texY);
            mat.SetTexture("_UTex", texU);
            mat.SetTexture("_VTex", texV);
        }
    }

    void releaseVideoFrameBuffer()
    {
        if (unmanagedBuffer == IntPtr.Zero)
            Marshal.FreeHGlobal(unmanagedBuffer);
    }

    void getVideoFameBuffer()
    {
        UInt64 timestamp;
        frameLen = NPlayer_ReadFrame(ptr, unmanagedBuffer, out timestamp);
        Marshal.Copy(unmanagedBuffer, buffer, 0, frameLen);

        int Ycount = w * h;
        int UVcount = w * (h >> 2);
        texY.SetPixelData(buffer, 0, 0);
        texY.Apply();
        texU.SetPixelData(buffer, 0, Ycount);
        texU.Apply();
        texV.SetPixelData(buffer, 0, Ycount + UVcount);
        texV.Apply();
    }

    void Update()
    {
        if (!bStart)
        {
            //Debug.Log("initVideoFrameBuffer");
            initVideoFrameBuffer();
        }
        else
        {
            //Debug.Log("getVideoFameBuffer");
            getVideoFameBuffer();
        }
    }

    private void OnDestroy()
    {
        //renderTexture.Release();
        Debug.Log("VplayerUnityframeReader OnDestroy");
        NPlayer_Uninit(ptr);
        ptr = IntPtr.Zero;
        releaseVideoFrameBuffer();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Debug.Log(source.format+""+destination.format);
        //Debug.Log("OnRenderImage");
        //Graphics.Blit(webcamTexture, null as RenderTexture);
        Graphics.Blit(source, destination, mat);
        //Graphics.Blit(source, destination);
        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), source, mat, -1);
        //cam.targetTexture = null;
        //Graphics.Blit(destination, null as RenderTexture);
    }

    //void OnPostRender()
    //{
        //Debug.Log("OnPostRender");
        //Graphics.Blit(webcamTexture, renderTexture);
        //Graphics.DrawTexture(new Rect(10, 10, 100, 100), webcamTexture);
        //cam.targetTexture = null;
        //Debug.Log("haha");
    //}
}
