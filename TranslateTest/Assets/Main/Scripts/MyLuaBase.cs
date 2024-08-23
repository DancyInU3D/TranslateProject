using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class MyLuaBase
{
    private static LuaEnv m_LuaEnv;
    public static LuaEnv MLuaEnv
    {
        get {
            if (m_LuaEnv == null)
                m_LuaEnv = new LuaEnv();
            return m_LuaEnv;
        }

    }

    public static void Log(string str)
    {
        MLuaEnv.DoString(str);
    }

    public static LuaEnv Require(string str)
    {
        MLuaEnv.AddLoader(myLoader);
        MLuaEnv.DoString($"require '{str}'");
        return MLuaEnv;
    }

    private static byte[] myLoader(ref string filepath)
    {
        filepath = Application.dataPath + "/Main/Lua/" + filepath + ".lua.txt";
        return File.ReadAllBytes(filepath);
    }
}
