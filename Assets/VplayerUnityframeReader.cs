using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

public class VplayerUnityframeReader : MonoBehaviour
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
	[DllImport("vplayerUnity.dll")]
	public static extern double getSysTick();
	[DllImport("winmm")]
	public static extern long timeGetTime();
	
	
	protected IntPtr ptr;
	protected int w, h;
	protected int frameLen;
	protected int mode; //0 for UDP, 1 for TCP
	protected UInt64 timestamp;
	private UInt64 pre_timestamp;
	public byte[] buffer;
	protected IntPtr unmanagedBuffer;
	protected bool bStart;
	// Use this for initialization
	DateTime start_time;

	double GetSysTick()
	{
		return getSysTick();
	}
	
	long GetSysTimeTick()
	{
		return timeGetTime();
	}
		
	long GetUTCTimeTick()
	{
		long unixTimestamp = DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks;
		return unixTimestamp/10000;
	}

	double GetTimeTickMSec()
	{
		long unixTimestamp = DateTime.Now.Ticks - start_time.Ticks;
		return unixTimestamp / (double)10000;
	}
	
	void Start()
    {
	    string url;
		
		ptr = IntPtr.Zero;
		ptr = NPlayer_Init();
		Debug.Log(ptr);
		//url = "rtsp://60.250.195.11/pcam0007/";
		//url = "rtsp://127.0.0.1/v1/";
		url = "rtsp://192.168.50.92/v1/";
		Debug.Log("Connect to "+url);
		mode = 1;
		NPlayer_Connect(ptr, url, mode);
		bStart = false;
		
		timestamp = 0;
	}
	
    // Update is called once per frame
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
	
    void OnDestroy()
    {
        Debug.Log("VplayerUnityframeReader OnDestroy");
		NPlayer_Uninit(ptr);
		ptr = IntPtr.Zero;
		releaseVideoFrameBuffer();
    }
	
	void initVideoFrameBuffer()
	{
		w = NPlayer_GetWidth(ptr);
		h = NPlayer_GetHeight(ptr);
		Debug.Log("width = "+w+", height = "+h);
		if (w != 0 && h != 0)
		{
			frameLen = w*h*3;
			Debug.Log("frameLen = "+frameLen);
			buffer = new byte[frameLen];
			unmanagedBuffer = Marshal.AllocHGlobal(frameLen);
			
			/*GameObject Img = GameObject.Find("ImageView");
			if (Img != null)
			{
				videoPlayer = Img.GetComponent<VideoView>();
			}
			*/
			
			start_time = DateTime.Now;
			bStart = true;
		}
	}
	
	void releaseVideoFrameBuffer()
	{
		if (unmanagedBuffer == IntPtr.Zero)
			Marshal.FreeHGlobal(unmanagedBuffer);
	}
	
	void getVideoFameBuffer()
	{
		//BinaryWriter bw;
		
		frameLen = NPlayer_ReadFrame(ptr, unmanagedBuffer, out timestamp);
		Marshal.Copy(unmanagedBuffer, buffer, 0, frameLen);
		//Debug.Log("NPlayer_ReadFrame ret = " + frameLen);
		//Debug.Log("first 4 byte = " + buffer[0] + " " + buffer[1] + " " + buffer[2] + " " + buffer[3]);
		
		//Debug.Log(DateTime.Now.Ticks);
		if (timestamp != pre_timestamp)
		{
			Debug.Log("timestamp = "+timestamp);			
			pre_timestamp = timestamp;
		}
		/*else{
			logWriter.WriteLine("skip "+timestamp);
		}*/
		
		//videoPlayer.SetRawData(buffer);
		//YUVPlayer.LoadYUV(buffer);
		
		/*
		bw = new BinaryWriter(new FileStream("mydataYUV", FileMode.Create));
		Debug.Log(bw);
		bw.Write(buffer);
		bw.Close();		*/
	}
}