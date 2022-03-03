using Facebook.Unity;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

using UnityEngine;

internal static class FacebookSignInExtension
{
    public static string ToJson(this AccessToken token) { return JsonMapper.ToJson(token); }
}


public class FacebookSignInImp : ISignInInterface
{
    public static string Platform => "Facebook";
    public string SignInPlatform => Platform;
    
    private FirebaseAuth auth => FirebaseAuth.DefaultInstance;

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
            
            authwithfirebase(token.TokenString, finished);
            // SetPlatformFlag();
            // finished?.Invoke(new SignInResult()
            // {
            //     OpenID = token.UserId,
            //     SignInPlatform = this.SignInPlatform,
            //     Token = token.ToJson(),
            // });
        }
        else
        {
            var permission = new List<string>() { "public_profile", "email" };
            FB.LogInWithReadPermissions(permission, r =>
            {
                if (FB.IsLoggedIn)
                {
                    var token = Facebook.Unity.AccessToken.CurrentAccessToken;

                    authwithfirebase(token.TokenString, finished);
                    
                    // SetPlatformFlag();
                    // finished?.Invoke(new SignInResult()
                    // {
                    //     OpenID = token.UserId,
                    //     SignInPlatform = "Fackebook",
                    //     Token = token.ToJson(),
                    // });
                }
                else
                {
                    foreach (var kvp in r.ErrorDictionary)
                    {
                        Debug.LogError($"Error[{kvp.Key}:{kvp.Value}]");
                    }
                    SetPlatformFlag(true);
                    finished?.Invoke(new SignInResult()
                    {
                        SignInPlatform = this.SignInPlatform,
                        Error = string.IsNullOrEmpty(r.Error) ? "SignIn Failed!" : r.Error,
                    });
                }
            });
        }

    }

    private void authwithfirebase(string accesstoken, Action<SignInResult> finished)
    {
        FirebaseUser newuser = null;
        var credential = FacebookAuthProvider.GetCredential(accesstoken);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("singin encountered error" + task.Exception);
                return Task.FromException<string>(task.Exception);
            }
            else
            {
                newuser = task.Result;
                Debug.Log(newuser.DisplayName);
                return newuser.TokenAsync(false);
            }
        }).Unwrap().ContinueWith(task =>
        {
            SetPlatformFlag(true);
            if (task.IsFaulted)
            {
                finished?.Invoke(new SignInResult()
                {
                    Error = task.Exception.Message,
                    SignInPlatform = SignInPlatform,
                });
            }
            else
            {
                SetPlatformFlag();
                finished?.Invoke(new SignInResult()
                {
                    SignInPlatform = SignInPlatform,
                    OpenID = newuser.UserId,
                    Token = task.Result,
                });
            }
        });
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
                        SignInPlatform = SignInPlatform,
                        Error = "Initialize Failed!",
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
                        SignInPlatform = SignInPlatform,
                        Error = "Initialize Failed!",
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
