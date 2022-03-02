using Google;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LitJson;

using Firebase;
using Firebase.Auth;

internal static class GoogleSignInExtension
{
    public static string ToJson(this GoogleSignInUser user) { return JsonMapper.ToJson(user); }

    
}


internal class GoogleSignInImp : ISignInInterface
{
    private string webClientId = "<your client id here>";
    private GoogleSignInConfiguration configuration;

    private FirebaseAuth auth => FirebaseAuth.DefaultInstance;
    
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
                    onSignInFinished?.InvokeMainThread(new SignInResult()
                    {
                        SignInPlatform = Platform,
                        Error = "Got Error: " + error.Status + " " + error.Message,
                    });
                }
                else
                {
                    Log("Got Unexpected Exception?!?" + task.Exception);
                    SetPlatformFlag(true);
                    onSignInFinished?.InvokeMainThread(new SignInResult()
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
            onSignInFinished?.InvokeMainThread(new SignInResult()
            {
                SignInPlatform = SignInPlatform,
                Error = "Canceled",
            });
        }
        else
        {
            Log("Welcome: " + task.Result.DisplayName + "!");
            
            SignInWithGoogleOnFirebase(task.Result.IdToken);
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
    
    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        FirebaseUser fu = null;
        if (auth == null)
        {
            Debug.LogError("Can not load FirebaseAuth");
            return;
        }
        
        auth?.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                AggregateException ex = task.Exception;
                if (ex != null)
                {
                    if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                        Debug.LogError("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);

                    return Task.FromException<string>(ex);
                }
                else
                {
                    Debug.Log("Sign In Successful.");
                    fu = task.Result;
                    return task.Result.TokenAsync(false);
                }
            })
            .Unwrap().ContinueWith(task =>
            {
                var result = new SignInResult();
                AggregateException ex = task.Exception;
                if (ex != null)
                {
                    Debug.Log($"TokenAsync failed!");
                    onSignInFinished?.InvokeMainThread(new SignInResult()
                    {
                        SignInPlatform = this.SignInPlatform,
                        Error = ex.Message,
                    });
                }
                else
                {
                    
                    Debug.Log($"TokenAsync {task.Result}!");
                    SetPlatformFlag();
                    onSignInFinished?.InvokeMainThread(new SignInResult()
                    {
                        SignInPlatform = this.SignInPlatform,
                        OpenID = fu.UserId,
                        Token = task.Result,
                    });   
                }
            })
            ;
    }

    public void SignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }
}
