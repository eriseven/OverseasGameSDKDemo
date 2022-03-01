using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;


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


public class SignInManager
{
    static SignInManager()
    {
        RegisterInterface(GoogleSignInImp.Platform, new GoogleSignInImp("259113062157-c9efto68ne73jplnvi6cav8au1ss8j5j.apps.googleusercontent.com"));
        RegisterInterface(FacebookSignInImp.Platform, new FacebookSignInImp());
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
        Reset();

        if (interfaces.TryGetValue(signInPlatform, out var currSignIn))
        {
            currSignIn.SignIn(finished);
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
