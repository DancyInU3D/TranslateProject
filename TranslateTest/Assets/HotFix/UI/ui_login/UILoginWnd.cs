using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UILoginWnd : MonoBehaviour
{
    [SerializeField] Text txt_title;
    [SerializeField] Text txt_content01;
    void Start()
    {
        Debug.Log("执行了start脚本");
        //LoadDll();
        SetTitle();
    }

//    private void LoadDll()
//    {
//#if !UNITY_EDITOR
//        Assembly Assembly_CSharpAss = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/Assembly-CSharp.dll.bytes"));
//        Assembly HotFixAss = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotFix.dll.bytes"));
//#else
//        Assembly Assembly_CSharpAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp");
//        Assembly HotFixAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotFix");
//#endif
//        Type type = HotFixAss.GetType("Test");//获取类名f
//        type.GetMethod("Run").Invoke(null, null);
//    }

    public void SetTitle()
    {
        txt_title.text = "背包";
        txt_content01.text = "熊猫,  国宝耶";
    }


}


