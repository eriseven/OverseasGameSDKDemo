using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLua;

static class DemoExtensions
{
    public static string AppendToURL(this string baseURL, params string[] segments)
    {
        {
            return string.Join("/", new[] {baseURL.TrimEnd('/')}.Concat(segments.Select(s => s.Trim('/'))));
        }
    }

    public static object[] Call(this LuaEnv luaEnv, string funcStr, params object[] args)
    {
        var func = luaEnv.Global.Get<LuaFunction>(funcStr);
        if (func == null)
        {
            var name = funcStr;
            string[] sArray = name.Split('.');
            if (sArray.Length <= 0)
            {
            }
            else if (sArray.Length == 1)
            {
                func = luaEnv.Global.Get<LuaFunction>(name);
            }
            else
            {
                LuaTable t = luaEnv.Global;

                for (int i = 0; i < sArray.Length - 1; i++)
                {
                    t = t.Get<LuaTable>(sArray[i]);
                    if (t == null)
                        return null;
                }

                func = t.Get<LuaFunction>(sArray[sArray.Length - 1]);
            }
        }

        if (func == null)
        {
            // LogError
            return null;
        }

        var objs = func?.Call(args);
        func?.Dispose();
        func = null;
        return objs;
    }
}

public class Demo : MonoBehaviour
{
    private LuaEnv _luaEnv;

    private void Awake()
    {
        // UnityEngine.Debug.Log();

        BetterStreamingAssets.Initialize();
        _luaEnv = new LuaEnv();
        _luaEnv.AddLoader(StreamingAssetsLuaLoader);
    }

    private static byte[] StreamingAssetsLuaLoader(ref string filepath)
    {
        if (string.IsNullOrEmpty(filepath)) return null;

        var pathInStreamingAssets = "lua".AppendToURL(filepath.Replace('.', '/')) + ".lua";

        return BetterStreamingAssets.FileExists(pathInStreamingAssets)
            ? BetterStreamingAssets.ReadAllBytes(pathInStreamingAssets)
            : null;
    }

    [SerializeField] private DemoUI demoUI;

    private void Start()
    {
        _luaEnv?.DoString(@"require ""Main""");
        _luaEnv?.Call("SDKDemo.Init", demoUI);
    }

    private void Update()
    {
        _luaEnv?.Tick();
    }

    private void OnDestroy()
    {
        _luaEnv?.Dispose();
    }
}