using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// 因为使用lua的热更新，所以不放在hotfix程序集中
/// </summary>
public class UI_Lua : MonoBehaviour
{
    LuaEnv luaEnv = new LuaEnv();
    // Start is called before the first frame update
    void Start()
    {
        Fun5();
    }

    private void Fun1()
    {
        luaEnv.DoString("print('hello world')");

        luaEnv.DoString("CS.UnityEngine.Debug.Log('hello world')");
    }

    /// <summary>
    /// 通过文件加载
    /// </summary>
    private void Fun2()
    {
        TextAsset asset = Resources.Load<TextAsset>("lua01.lua");//不需要加文件后缀
        luaEnv.DoString(asset.text);
    }

    /// <summary>
    /// 通过require加载(默认通过Resources文件夹加载)
    /// </summary>
    private void Fun3()
    {
        luaEnv.DoString("require('lua01')");
    }

    /// <summary>
    /// 通过自定义目录加载
    /// </summary>
    private void Fun4()
    {
        luaEnv.AddLoader(myLoader);
        luaEnv.DoString("require('lua02')");
    }
    private byte[] myLoader(ref string filepath)
    {
        filepath = Application.dataPath + "/Scripts/Lua/" + filepath + ".lua.txt";
        return File.ReadAllBytes(filepath);
    }

    private void Fun5()
    {
        LuaEnv env = MyLuaBase.Require("lua03");
        int a = env.Global.Get<int>("a");
        string name = env.Global.Get<string>("name");
        Debug.Log(a);
        Debug.Log(name);
    }
}
