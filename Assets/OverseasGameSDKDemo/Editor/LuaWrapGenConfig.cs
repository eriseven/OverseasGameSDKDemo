using System;
using System.Collections.Generic;

public static class LuaWrapGenConfig
{
    [XLua.LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
            typeof(SignInManager),
            typeof(IAPManager),
            typeof(UnityEngine.Purchasing.Product),
    };
}
