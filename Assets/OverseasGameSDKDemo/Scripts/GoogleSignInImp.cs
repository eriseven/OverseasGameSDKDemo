using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleSignInImp : ISignInInterface
{
    public string SignInPlatform => "Google";
    
    public void TryQuickSignIn(Action<SignInResult> finished)
    {
        throw new NotImplementedException();
    }

    public void SignIn(Action<SignInResult> finished)
    {
        throw new NotImplementedException();
    }

    public void SignOut()
    {
        throw new NotImplementedException();
    }
}
