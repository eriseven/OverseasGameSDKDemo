using System;
using System.Collections;
using System.Collections.Generic;
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
        
    }
    
    public static void TryQuickSignIn(Action<SignInResult> finished)
    {
    }

    public static void SignIn(string signInPlatform, Action<SignInResult> finished)
    {
    }

    public static void SignOut()
    {
    }
}
