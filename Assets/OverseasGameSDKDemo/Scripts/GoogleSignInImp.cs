using Google;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LitJson;

internal static class GoogleSignInExtension
{
    public static string ToJson(this GoogleSignInUser user) { return JsonMapper.ToJson(user); }
}


internal class GoogleSignInImp : ISignInInterface
{
    private string webClientId = "<your client id here>";
    private GoogleSignInConfiguration configuration;

    void Log(string log)
    {
        Debug.Log($"[GoogleSignIn] {log}");
    }

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

    internal GoogleSignInImp(string webClientId)
    {
        this.webClientId = webClientId;
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

    }

    internal GoogleSignInImp()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };
    }

    public static string Platform => "Google";
    public string SignInPlatform => Platform;


    
    public void TryQuickSignIn(Action<SignInResult> finished)
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestAuthCode = true;
        Log("Calling SignIn Silently");

        onSignInFinished = finished;
        GoogleSignIn.DefaultInstance.SignInSilently()
              .ContinueWith(OnAuthenticationFinished);

    }


    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    Log("Got Error: " + error.Status + " " + error.Message);
                    SetPlatformFlag(true);
                    onSignInFinished?.Invoke(new SignInResult()
                    {
                        SignInPlatform = Platform,
                        Error = "Got Error: " + error.Status + " " + error.Message,
                    });
                }
                else
                {
                    Log("Got Unexpected Exception?!?" + task.Exception);
                    SetPlatformFlag(true);
                    onSignInFinished?.Invoke(new SignInResult()
                    {
                        SignInPlatform = SignInPlatform,
                        Error = "Got Unexpected Exception?!?" + task.Exception,
                    });
                }
            }
        }
        else if (task.IsCanceled)
        {
            Log("Canceled");
            SetPlatformFlag(true);
            onSignInFinished?.Invoke(new SignInResult()
            {
                SignInPlatform = SignInPlatform,
                Error = "Canceled",
            });
        }
        else
        {
            Log("Welcome: " + task.Result.DisplayName + "!");
            SetPlatformFlag();
            onSignInFinished?.Invoke(new SignInResult()
            {
                SignInPlatform = this.SignInPlatform,
                OpenID = task.Result.UserId,
                Token = task.Result.ToJson(),
            });
        }
    }

    Action<SignInResult> onSignInFinished;

    public void SignIn(Action<SignInResult> finished)
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestAuthCode = true;

        onSignInFinished = finished;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }

    public void SignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }
}
