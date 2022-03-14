using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPResult
{
    public string Error = "";
    public string Product = "";
}

internal static class IAPManagerExtension
{
    // public static void InvokeMainThread<T>(this Action<T> func, T args)
    // {
    //     UnityMainThreadDispatcher.Instance().Enqueue(() => { func?.Invoke(args); });
    // }
}

public class IAPManager : IStoreListener
{
    private static IAPManager _instance = null;
    public static IAPManager Get()
    {
        if (_instance == null)
        {
            _instance = new IAPManager();
        }

        return _instance;
    }

    IAPManager()
    {
        
    }
    
    private Action<IAPResult> onInitResultCallback;
    private Action<IAPResult> onPurchaseResultCallback;
    
    IStoreController m_StoreController; // The Unity Purchasing system.

    public void InitializePurchasing(List<string> iapIDs, Action<string> initializedResult)
    {
        InitializePurchasing(iapIDs, (IAPResult r) =>
        {
            initializedResult.Invoke(JsonMapper.ToJson(r));
        });
    }
    
    [XLua.BlackList]
    public void InitializePurchasing(List<string> iapIDs, Action<IAPResult> initializedResult)
    {
        Debug.Log("IAPManager.Init");
        onInitResultCallback = initializedResult;
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var iapID in iapIDs)
        {
            Debug.Log($"product: {iapID}");
            builder.AddProduct(iapID, ProductType.Consumable);
        }
        
        UnityPurchasing.Initialize(this, builder);
    }

    public bool StartPurchase(string iapID, Action<string> onPurchaseResult)
    {
        return StartPurchase(iapID, (IAPResult r) =>
        {
            onPurchaseResult.Invoke(JsonMapper.ToJson(r));
        });
    }   
     
    [XLua.BlackList]
    public bool StartPurchase(string iapID, Action<IAPResult> onPurchaseResult)
    {
        if (onPurchaseResultCallback != null)
        {
            Debug.LogError("StartPurchase: onPurchaseResultCallback isn't null. Some purchasing request is pending! Try finish it first!");
            return false;
        }

        if (pendingPurchase.Count > 0)
        {
            Debug.LogError("StartPurchase: pendingPurchase.Count > 0. Some purchasing request is pending! Try finish it first!");
        }
        
        onPurchaseResultCallback = onPurchaseResult;
        m_StoreController.InitiatePurchase(iapID);

        return true;
    }

    public void ConfirmPendingPurchase(Product pending)
    {
        Debug.Log($"IAPManager.ConfirmPendingPurchase: product {pending.definition.id}");
        m_StoreController.ConfirmPendingPurchase(pending);
        var result = pendingPurchase.Remove(pending);
        if (!result)
        {
            Debug.LogError("ConfirmPendingPurchase: Can not find product info in pending list!");
        }
    }

    [XLua.BlackList]
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        onInitResultCallback.InvokeMainThread(new IAPResult()
        {
            Error = error.ToString(),
        });
        onInitResultCallback = null;
    }

    private List<Product> pendingPurchase = new List<Product>();
    
    [XLua.BlackList]
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        pendingPurchase.Add(purchaseEvent.purchasedProduct);
        
        Debug.Log($"IAPManager.ProcessPurchase Pending: {purchaseEvent.purchasedProduct.definition.id}");
        onPurchaseResultCallback.InvokeMainThread(new IAPResult()
        {
            Product = JsonMapper.ToJson(purchaseEvent.purchasedProduct),
        });
        onPurchaseResultCallback = null;
        
        return PurchaseProcessingResult.Pending;
    }

    public Product GetPendingPurchase()
    {
        if (pendingPurchase.Count > 0)
        {
            return pendingPurchase[0];
        }

        return null;
    }
    
    [XLua.BlackList]
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"IAPManager.OnPurchaseFailed({failureReason.ToString()}): {product.definition.id}");
        onPurchaseResultCallback.InvokeMainThread(new IAPResult()
        {
            Error = failureReason.ToString(),
        });
        onPurchaseResultCallback = null;
    }

    [XLua.BlackList]
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log($"IAPManager.OnInitialized");
        m_StoreController = controller;
        onInitResultCallback.InvokeMainThread(new IAPResult()
        {
            
        });
        onInitResultCallback = null;
    }
}
