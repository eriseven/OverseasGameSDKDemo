using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LitJson;
using UnityEngine;

using Firebase;
using Firebase.Auth;
using Debug = UnityEngine.Debug;


public class SignInResult
{
    public string Error;
    public string SignInPlatform;
    public string OpenID;
    public string Token;
}

internal interface ISignInInterface
{
    string SignInPlatform { get; }
    void TryQuickSignIn(Action<SignInResult> finished);
    void SignIn(Action<SignInResult> finished);
    void SignOut();
}

internal static class SignInManagerExtension
{
    public static void InvokeMainThread<T>(this Action<T> func, T args)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => { func?.Invoke(args); });
    }
}

public class SignInManager
{
    static SignInManager()
    {

    }

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        RegisterInterface(GoogleSignInImp.Platform,
            new GoogleSignInImp("259113062157-c9efto68ne73jplnvi6cav8au1ss8j5j.apps.googleusercontent.com"));
        RegisterInterface(FacebookSignInImp.Platform, new FacebookSignInImp());
#if UNITY_IOS
        RegisterInterface(AppleSignIn.Platform, AppleSignIn.Get());
#endif
        
        CheckFirebaseDependencies();
    }

    private static FirebaseAuth auth;
    
    [Conditional("UNITY_ANDROID")]
    private static void CheckFirebaseDependencies()
    {
        Debug.Log($"[SignInManager] CheckFirebaseDependencies");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log("CheckAndFixDependenciesAsync succeed!");
                    var app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                }
                else
                    Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                Debug.LogError("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }
    
    static Dictionary<string, ISignInInterface> interfaces = new Dictionary<string, ISignInInterface>();

    static void RegisterInterface(string platform, ISignInInterface imp)
    {
        interfaces[platform] = imp;
    }
    
    static string GetCurrSignIn()
    {
        return PlayerPrefs.GetString("LastSignInPlatform", "");
    }
    
    public static void TryQuickSignIn(Action<string> callback)
    {
        TryQuickSignIn((SignInResult r) =>
        {
            callback?.Invoke(JsonMapper.ToJson(r));
        });
    }
   
    [XLua.BlackList]
    public static void TryQuickSignIn(Action<SignInResult> finished)
    {
        try
        {
            if (currSignIn == null)
            {
                var currPaltform = GetCurrSignIn();
                if (!string.IsNullOrEmpty(currPaltform))
                {
                    interfaces.TryGetValue(currPaltform, out currSignIn);
                }
            }

            if (currSignIn != null)
            {
                currSignIn.TryQuickSignIn(finished);
            }
            else
            {
                finished?.Invoke(new SignInResult()
                {
                    Error = "Quick SignIn Failed!"
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    static ISignInInterface currSignIn;
    

    static void Reset()
    {
        PlayerPrefs.DeleteKey("LastSignInPlatform");
        currSignIn = null;
    }



    public static void SignIn(string signInPlatform, Action<string> finished)
    {
        SignIn(signInPlatform, (SignInResult r) =>
        {
            finished?.Invoke(JsonMapper.ToJson(r));
        });
    }
    
    [XLua.BlackList]
    public static void SignIn(string signInPlatform, Action<SignInResult> finished)
    {

        try
        {
            Reset();

            if (interfaces.TryGetValue(signInPlatform, out var currSignIn))
            {
                currSignIn.SignIn(finished);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static void SignOut()
    {
        if (currSignIn != null)
        {
            currSignIn.SignOut();
        }
        else
        {
             var currPaltform = GetCurrSignIn();
             if (!string.IsNullOrEmpty(currPaltform))
             {
                 interfaces.TryGetValue(currPaltform, out currSignIn);
             }

             currSignIn?.SignOut();
        }
        
        Reset();
    }
}
