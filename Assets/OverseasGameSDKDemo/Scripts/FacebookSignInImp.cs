using Facebook.Unity;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class FacebookSignInExtension
{
    public static string ToJson(this AccessToken token) { return JsonMapper.ToJson(token); }
}


public class FacebookSignInImp : ISignInInterface
{
    public static string Platform => "Facebook";
    public string SignInPlatform => Platform;

    private void SetPlatformFlag(bool clear = false)
    {
        if (clear)
        {
            PlayerPrefs.SetString("LastSignInPlatform", SignInPlatform);
        }
        else
        {
            PlayerPrefs.DeleteKey("LastSignInPlatform");
        }
    }

    private void OnHideUnity(bool isgameshown)
    {
        if (!isgameshown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    private void InitCallBack()
    {
        Debug.Log(nameof(InitCallBack));
        if (FB.IsInitialized)
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("Facebook is logged in!");
            }
            FB.ActivateApp();
        }
        else
        {
            Debug.LogWarning("Failed to find facebook app");
        }
    }

    public void SignIn_Internal(Action<SignInResult> finished)
    {
        if (FB.IsLoggedIn)
        {
            var token = Facebook.Unity.AccessToken.CurrentAccessToken;
            SetPlatformFlag();
            finished?.Invoke(new SignInResult()
            {
                OpenID = token.UserId,
                SignInPlatform = this.SignInPlatform,
                Token = token.ToJson(),
            });
        }
        else
        {
            var permission = new List<string>() { "public_profile", "email" };
            FB.LogInWithReadPermissions(permission, r =>
            {
                if (FB.IsLoggedIn)
                {
                    var token = Facebook.Unity.AccessToken.CurrentAccessToken;
                    SetPlatformFlag();
                    finished?.Invoke(new SignInResult()
                    {
                        OpenID = token.UserId,
                        SignInPlatform = "Fackebook",
                        Token = token.ToJson(),
                    });
                }
                else
                {

                    SetPlatformFlag(true);
                    finished?.Invoke(new SignInResult()
                    {
                        Error = "",
                    });
                }
            });
        }

    }

    public void SignIn(Action<SignInResult> finished)
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                InitCallBack();
                if (FB.IsInitialized)
                {
                    SignIn_Internal(finished);
                }
                else
                {

                    SetPlatformFlag(true);
                    finished?.Invoke(new SignInResult()
                    {
                        Error = "",
                    });
                }
            }, OnHideUnity);
        }
        else
        {
            SignIn_Internal(finished);
        }
    }

    public void SignOut()
    {
        FB.LogOut();
    }

    public void TryQuickSignIn(Action<SignInResult> finished)
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                InitCallBack();
                if (FB.IsInitialized)
                {
                    SignIn_Internal(finished);
                }
                else
                {
                    SetPlatformFlag(true);
                    finished?.Invoke(new SignInResult()
                    {
                        Error = "",
                    });
                }
            }, OnHideUnity);

        }
        else
        {
            SignIn_Internal(finished);
        }

    }
}
