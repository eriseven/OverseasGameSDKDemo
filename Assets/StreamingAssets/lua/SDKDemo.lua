---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by Avg.
--- DateTime: 1/13/2022 5:44 PM
---

local demo = {}
local _demo_ui



local _test_cases = {}

_test_cases["IAP Init"] = function()
    print("IAP Init")
    CS.IAPManager.Get():InitializePurchasing({"com.dreality.crazyones.iaptest.5coin"}, function(result)
        print(result)   
    end)
end

_test_cases["IAP Buy"] = function()
    print("IAP Buy: com.dreality.crazyones.iaptest.5coin")
    CS.IAPManager.Get():StartPurchase("com.dreality.crazyones.iaptest.5coin", function(result)
        print(result)
    end)
end

_test_cases["IAP Complete Purchase"] = function()
    print("IAP Complete Purchase")
    local pending = CS.IAPManager.Get():GetPendingPurchase()
    if pending ~= nil then
        CS.IAPManager.Get():ConfirmPendingPurchase()
    else
        print("No pending purchase found!")
    end
end
--[[

_test_cases["GoogleSignIn"] = function()
    print("GoogleSignIn")
    CS.SignInManager.SignIn("Google", function(result)
        print(result)   
    end)
end

_test_cases["FacebookSignIn"] = function()
    print("FacebookSignIn")
    CS.SignInManager.SignIn("Facebook", function(result)
        print(result)
    end)
end

_test_cases["AppleSignIn"] = function()
    print("AppleSignIn")
    CS.SignInManager.SignIn("Apple", function(result)
        print(result)
    end)
end

_test_cases["QuickSignIn"] = function()
    print("QuickSignIn")
    CS.SignInManager.TryQuickSignIn(function(result)
        print(result)
    end)
end

_test_cases["SignOut"] = function()
    print("SignOut")
    CS.SignInManager.SignOut()
end
]]

function demo.Init(demo_ui)
    print("SDKDemo.Init")
    CS.SDKWrapper.OnUnity3dSendMessage('+', function(json) print(json)  end)

    _demo_ui = demo_ui

    if not _demo_ui then
        return
    end

    for k, v in pairs(_test_cases) do
         _demo_ui:AddTestAction(k, v)
    end
    
end


return demo