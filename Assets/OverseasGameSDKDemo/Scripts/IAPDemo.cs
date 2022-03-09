using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
// using Product = Facebook.Unity.Product;

public class IAPDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public int userCredits = 10;

    public void GrantCredits(int credits)
    {
        userCredits = userCredits + credits;
        Debug.Log("You received " + credits + " Credits!");
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        if (p == PurchaseFailureReason.PurchasingUnavailable)
        {
            // IAP may be disabled in device settings.
        }
        Debug.LogError(i.ToString() + p.ToString());
    }
}