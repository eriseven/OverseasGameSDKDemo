using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XLua;


[LuaCallCSharp]
public class SDKWrapper : MonoBehaviour
{
    
    private static StringBuilder logStringBuilder = new StringBuilder();

    private void OnDestroy()
    {
        OnUnity3dSendMessage = delegate(string s) {  };
    }

    [CSharpCallLua]
    public static event Action<string> OnUnity3dSendMessage = delegate(string s) {  };
    
    public void Unity3dSendMessage(string json)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() => OnUnity3dSendMessage?.Invoke(json));
	}
    
    #region Android

#if UNITY_ANDROID
    private static void logAndroidAPI(string className, string apiName, params object[] args)
    {
        logStringBuilder.Clear();
        logStringBuilder.AppendFormat("SDKWrapper[{0}, {0}]:", className, apiName);
        foreach (var arg in args)
        {
            logStringBuilder.AppendFormat(" {0}", arg);
        }

        Debug.Log(logStringBuilder.ToString());
        // LogWrapper.LogDebug(logStringBuilder.ToString());
    }

    public static void callSdkApi(string className, string apiName, params object[] args)
    {
        #if UNITY_EDITOR
        logAndroidAPI(className, apiName, args);
        GameObject.Find("GameCore").GetComponent<SDKWrapper>().Unity3dSendMessage(args[0] as string);
#else
        using (var cls = new AndroidJavaClass(className))
        {
            logAndroidAPI(className, apiName, args);
            cls.CallStatic(apiName, args);
        }
#endif
    }

#endif
    
    #endregion
    

    #region iOS
#if UNITY_IPHONE
	// [DllImport("__Internal")]
	// public static extern void __initIosSDK(string gameId, string cpId, string serverId, string appkey, string sandboxKey);
#endif
    #endregion
}
