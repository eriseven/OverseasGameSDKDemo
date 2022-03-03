using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase.Auth;
using UnityEngine;
using Object = System.Object;

public class AppleSignIn : MonoBehaviour, ISignInInterface
{
    private IAppleAuthManager appleAuthManager;
    FirebaseAuth auth;

    public string AppleUserIdKey { get; private set; }

    void Start()
    {
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var deserializer = new PayloadDeserializer();
            this.appleAuthManager = new AppleAuthManager(deserializer);
        }
    }

    void Update()
    {
        if (this.appleAuthManager != null)
        {
            this.appleAuthManager.Update();
        }
    }

    private void SignInWithApple()
    {
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);

        this.appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                // Obtained credential, cast it to IAppleIDCredential
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    // Apple User ID
                    // You should save the user ID somewhere in the device
                    var userId = appleIdCredential.User;
                    PlayerPrefs.SetString(AppleUserIdKey, userId);

                    // Identity token
                    var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);

                    // Authorization code
                    var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);


                    // And now you have all the information to create/login a user in your system
                    Debug.Log("identityToken " + identityToken);
                    Debug.Log("authorizationCode " + authorizationCode);
                    authWithFirebase(identityToken, rawNonce, authorizationCode);
                }
                else
                {
                    SetPlatformFlag(true);
                    onFinished.InvokeMainThread(new SignInResult()
                    {
                        Error = "IAppleIDCredential Casting Error.",
                        SignInPlatform = SignInPlatform,
                    });
                }
            },
            error =>
            {
                // Something went wrong
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogError("authorizationErrorCode " + authorizationErrorCode);

                onFinished.InvokeMainThread(new SignInResult()
                {
                    Error = error.LocalizedDescription,
                    SignInPlatform = SignInPlatform,
                });
            });
    }

    private void authWithFirebase(string appleIdToken, string rawNonce, string authorizationCode)
    {
        auth = FirebaseAuth.DefaultInstance;
        Firebase.Auth.Credential credential =
            Firebase.Auth.OAuthProvider.GetCredential("apple.com", appleIdToken, rawNonce, authorizationCode);
        Firebase.Auth.FirebaseUser newUser = null;
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    return Task.FromResult("SignInWithCredentialAsync was canceled.");
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return Task.FromException<string>(task.Exception);
                }

                newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);

                return newUser.TokenAsync(false);
            })
            .Unwrap().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log($"TokenAsync failed!");
                    
                    SetPlatformFlag(true);
                    onFinished?.InvokeMainThread(new SignInResult()
                    {
                        SignInPlatform = SignInPlatform,
                        Error = task.Exception.Message,
                    });
                }
                else
                {
                    Debug.Log($"TokenAsync {task.Result}!");
                    SetPlatformFlag();
                    onFinished?.InvokeMainThread(new SignInResult()
                    {
                        SignInPlatform = SignInPlatform,
                        OpenID = newUser.UserId,
                        Token = task.Result,
                    });
                }
            })
            ;
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

    private static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }

    public static string Platform => "Apple";

    private static AppleSignIn instance = null;

    public static AppleSignIn Get()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<AppleSignIn>();

            if (instance == null)
            {
                var go = new GameObject(nameof(AppleSignIn), typeof(AppleSignIn));
                instance = go.GetComponent<AppleSignIn>();
                DontDestroyOnLoad(go);
            }
        }

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public string SignInPlatform => Platform;

    Action<SignInResult> onFinished;

    public void TryQuickSignIn(Action<SignInResult> finished)
    {
        throw new NotImplementedException();
    }

    public void SignIn(Action<SignInResult> finished)
    {
        onFinished = finished;
        SignInWithApple();
    }

    public void SignOut()
    {
        throw new NotImplementedException();
    }
}