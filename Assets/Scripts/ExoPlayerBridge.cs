using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;        // required for DllImport
using System;
using System.IO;

public class ExoPlayerBridge : MonoBehaviour
{

    private AndroidJavaObject exoPlayer = null;
    private AndroidJavaObject activityContext = null;

    private IntPtr nativeTexturePtr = IntPtr.Zero;
    private int nativeTextureWidth = 0;
    private int nativeTextureHeight = 0;

    private bool startedVideo = false;
    private bool videoPaused = false;

    private Renderer mediaRenderer = null;

    public static int eventBase
    {
        get { return _eventBase; }
        set
        {
            _eventBase = value;

            OVR_Media_Surface_SetEventBase(_eventBase);

        }
    }
    private static int _eventBase = 0;

    private enum MediaSurfaceEventType
    {
        Initialize = 0,
        Shutdown = 1,
        Update = 2,
        Max_EventType
    };


    private static void IssuePluginEvent(MediaSurfaceEventType eventType)
    {
        GL.IssuePluginEvent((int)eventType + eventBase);
    }

    /// <summary>
    /// Initialization of the movie surface
    /// </summary>
    void Awake()
    {
        Debug.Log("MovieSample Awake");
        OVR_Media_Surface_Init();
        mediaRenderer = GetComponent<Renderer>();
        if (mediaRenderer.material == null || mediaRenderer.material.mainTexture == null)
        {
            Debug.LogError("Can't GetNativeTexturePtr() for movie surface");
        }

        nativeTexturePtr = mediaRenderer.material.mainTexture.GetNativeTexturePtr();
        nativeTextureWidth = mediaRenderer.material.mainTexture.width;
        nativeTextureHeight = mediaRenderer.material.mainTexture.height;
        Debug.Log("Movie Texture id: " + nativeTexturePtr);

        IssuePluginEvent(MediaSurfaceEventType.Initialize);

    }

    /// <summary>
    /// Auto-starts video playback
    /// </summary>
    IEnumerator DelayedStartVideo()
    {
        yield return null; // delay 1 frame to allow MediaSurfaceInit from the render thread.
        RetrieveStreamingAsset("");
        if (!startedVideo)
        {
            Debug.Log("Mediasurface DelayedStartVideo");
            exoPlayer = StartVideoPlayerOnTextureId(nativeTexturePtr, nativeTextureWidth, nativeTextureHeight);
        }

    }
    // Use this for initialization
    void Start()
    {

        Debug.Log("MovieSample Start");
        StartCoroutine(DelayedStartVideo());
    }

    private void OnDestroy()
    {

        Debug.Log("OnDestroy");

        nativeTexturePtr = IntPtr.Zero;

        IssuePluginEvent(MediaSurfaceEventType.Shutdown);

    }

    IEnumerator RetrieveStreamingAsset(string mediaFileName)
    {
        string streamingMediaPath = Application.streamingAssetsPath + "/" + mediaFileName;
        string persistentPath = Application.persistentDataPath + "/" + mediaFileName;
        if (!File.Exists(persistentPath))
        {
            WWW wwwReader = new WWW(streamingMediaPath);
            yield return wwwReader;

            if (wwwReader.error != null)
            {
                Debug.LogError("wwwReader error: " + wwwReader.error);
            }

            System.IO.File.WriteAllBytes(persistentPath, wwwReader.bytes);
        }


        Debug.Log("Movie FullPath: ");
    }

    void Update()
    {
        IssuePluginEvent(MediaSurfaceEventType.Update);
    }
    /// <summary>
    /// Set up the video player with the movie surface texture id.
    /// </summary>
    AndroidJavaObject StartVideoPlayerOnTextureId(IntPtr texId, int texWidth, int texHeight)
    {


        IntPtr androidSurface = OVR_Media_Surface(texId, texWidth, texHeight);
        Debug.Log("MoviePlayer: SetUpVideoPlayer after create surface");

        if (exoPlayer == null)
        {
            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
            Debug.Log("com.google.android.exoplayer.unity.bridge.ExoPlayerBridge");
            using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.google.android.exoplayer.unity.bridge.ExoPlayerBridge"))
            {
                if (pluginClass != null)
                {
                    exoPlayer = pluginClass.CallStatic<AndroidJavaObject>("instance");
                    exoPlayer.Call("setContext", activityContext);

                    activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                        // exoPlayer.Call("init", "https://storage.googleapis.com/wvmedia/clear/h264/tears/tears.mpd", 0, "wv:clearsd&hd(mp4,h264)", "");
                        exoPlayer.Call("init", "https://storage.googleapis.com/wvmedia/cenc/h264/tears/tears_sd.mpd", 0, "", "widevine_test");

                        IntPtr setSurfaceMethodId = AndroidJNI.GetMethodID(exoPlayer.GetRawClass(), "preparePlayer", "(Landroid/view/Surface;)V");
                        jvalue[] parms = new jvalue[1];
                        parms[0] = new jvalue();
                        parms[0].l = androidSurface;
                        AndroidJNI.CallVoidMethod(exoPlayer.GetRawObject(), setSurfaceMethodId, parms);
                    }));
                }
            }
        }
        return exoPlayer;
    }



    [DllImport("OculusMediaSurface")]
    private static extern void OVR_Media_Surface_Init();

    // This function returns an Android Surface object that is
    // bound to a SurfaceTexture object on an independent OpenGL texture id.
    // Each frame, before the TimeWarp processing, the SurfaceTexture is checked
    // for updates, and if one is present, the contents of the SurfaceTexture
    // will be copied over to the provided surfaceTexId and mipmaps will be 
    // generated so normal Unity rendering can use it.
    [DllImport("OculusMediaSurface")]
    private static extern IntPtr OVR_Media_Surface(IntPtr surfaceTexId, int surfaceWidth, int surfaceHeight);

    [DllImport("OculusMediaSurface")]
    private static extern void OVR_Media_Surface_SetEventBase(int eventBase);
}