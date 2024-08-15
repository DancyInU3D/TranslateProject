using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LoadDll : MonoBehaviour
{
    private string commonPath = "/common";
    void Start()
    {
        StartLoadDll();
        Debug.Log("==========");
        LoadUI();
    }

    private void StartLoadDll()
    {
        AssetBundle dllAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + commonPath);
#if !UNITY_EDITOR
        TextAsset dllBytes = dllAB.LoadAsset<TextAsset>("HotFix.dll.bytes");
        Assembly HotFixAss=Assembly.Load(dllBytes.bytes);
#else
        Assembly Assembly_CSharpAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp");
        Assembly HotFixAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotFix");
#endif
        Type type = HotFixAss.GetType("Test");//获取类名
        type.GetMethod("Run").Invoke(null, null);
    }

    private void LoadUI()
    {
        AssetBundle dllAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/ui_login");
        GameObject asset = dllAB.LoadAsset<GameObject>("ui_login");
        GameObject ui= Instantiate(asset);
    }

}
