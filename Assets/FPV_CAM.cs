using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV_CAM : MonoBehaviour
{
    WebCamTexture webcamTexture;
    //RenderTexture renderTexture;
    public Camera cam;
    public Material mat;
    //public Shader shader;
    Texture2D distortMap;
    //float _CX = 315.46f;
    //float _CY = 240.96f;
    //float _FX = 246.88f;
    //float _FY = 249.75f;
    float _K1 = 0.21874f;
    float _K2 = -0.24239f;
    float _P1 = -0.00089613f;
    float _P2 = 0.00064407f;
    float _K3 = 0.063342f;

    // Start is called before the first frame update
    void Start()
    {
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
        int sample_mutply = 2;
        for (int i = 0; i < height * sample_mutply; i++)
        {
            for (int j = 0; j < width * sample_mutply; j++)
            {
                double x = 1.0 * j / (width * sample_mutply);
                double y = 1.0 * i / (height * sample_mutply);
                double r2 = x * x + y * y;
                double distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                double x_distort = x * distort;
                double y_distort = y * distort;
                x_distort += (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort += (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                //Debug.Log(x_distort + "," + y_distort);
                int idxU = (int)Math.Round(x_distort * width);
                int idxV = (int)Math.Round(y_distort * height);
                int mapIdx = idxV * width * 2 + idxU * 2;
                //Debug.Log(mapIdx);
                if (mapIdx < width * height * 2)
                {
                    distortData[mapIdx] = (float)x;
                    distortData[mapIdx + 1] = (float)y;
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

    void OnPreRender()
    {
        //cam.targetTexture = renderTexture;
        //cam.forceIntoRenderTexture = true;
        //Graphics.Blit(webcamTexture, null as RenderTexture);
    }

    private void OnDestroy()
    {
        //renderTexture.Release();
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
