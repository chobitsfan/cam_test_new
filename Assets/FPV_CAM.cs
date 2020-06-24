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
        distortMap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGFloat, false, true);
        float[] distortData = new float[Screen.width * Screen.height * 2];
        for (int i=0;i<Screen.height;i++)
        {
            for (int j=0;j<Screen.width;j++)
            {
                float x = 1.0f * j / Screen.width;
                float y = 1.0f * i / Screen.height;
                float r2 = x * x + y * y;
                float distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                float x_distort = x * distort;
                float y_distort = y * distort;
                x_distort = x_distort + (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort = y_distort + (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                //Debug.Log(x_distort + "," + y_distort);
                int idxU = (int)(x_distort * Screen.width);
                int idxV = (int)(y_distort * Screen.height);
                int mapIdx = idxV * Screen.width * 2+ idxU * 2;
                //Debug.Log(mapIdx);
                if (mapIdx < Screen.width * Screen.height * 2)
                {
                    distortData[mapIdx] = x;
                    distortData[mapIdx + 1] = y;
                }
                //distortData[i * Screen.width + j] = 1.0f * j / Screen.width;
                //distortData[i * Screen.width + j+1] = 1.0f * i / Screen.height;

                //distortData[i * Screen.width * 2 + j * 2] = 1.0f * j / Screen.width;
                //distortData[i * Screen.width *2  + j *2 + 1] = 1.0f * j / Screen.width;
            }
        }
        distortMap.filterMode = FilterMode.Point;
        distortMap.anisoLevel = 1;
        distortMap.wrapMode = TextureWrapMode.Clamp;
        distortMap.SetPixelData(distortData, 0);
        distortMap.Apply(false);
        mat.SetTexture("_DistortTex", distortMap);
        //renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
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

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Debug.Log(source.format+""+destination.format);
        //Debug.Log("OnRenderImage");
        //Graphics.Blit(webcamTexture, null as RenderTexture);
        Graphics.Blit(source, destination, mat);
        //Graphics.Blit(source, destination);
        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), source, mat, -1);
        //cam.targetTexture = null;
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
