using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
// using Firebase.Auth;
using System;
using UnityEngine.UI;


public class FacebookSignIn: MonoBehaviour
{
    // FirebaseAuth auth;
    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallBack, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }
    private void InitCallBack()
    {
        Debug.Log(nameof(InitCallBack));
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.LogWarning("Failed to find facebook app");
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

    [IngameDebugConsole.ConsoleMethod("fb-login", "facebook login")]
    public static void Facebook_Login()
    {
        var permission = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(permission, AuthCallBack);
    }

    [IngameDebugConsole.ConsoleMethod("fb-logout", "facebook logout")]
    public static void Facebook_LogOut()
    {
        FB.LogOut();
        Debug.Log(nameof(Facebook_LogOut));
    }
    
    private static void AuthCallBack(ILoginResult result)
    {
        Debug.Log(nameof(AuthCallBack));
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }
    
    
    // public void authwithfirebase(string accesstoken)
    // {
    //     auth = FirebaseAuth.DefaultInstance;
    //     Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(accesstoken);
    //     auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
    //     {
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("singin encountered error" + task.Exception);
    //         }
    //         Firebase.Auth.FirebaseUser newuser = task.Result;
    //         Debug.Log(newuser.DisplayName);
    //     });
    // }
}
