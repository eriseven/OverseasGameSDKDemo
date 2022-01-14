package com.unity3d.player;

import android.util.Log;

public class SDKWrapper {
    private static final String TAG = "SDKWrapper";

    public static final String TargetGameObject = "GameCore";

    public static final String ONGSCSDKCALLBACK = "Unity3dSendMessage";

    public static void unity3dSendMessage(String json) {
        Log.d(TAG, "send message to Unity3D, message data =" + json);
        UnityPlayer.UnitySendMessage(TargetGameObject, ONGSCSDKCALLBACK, json);
    }

    public static void Initialize(String json)
    {
        String result = "{\"code\": 0, \"msg\": \"SDKWrapper Initialized!\"}";
        unity3dSendMessage(result);
    }
}
