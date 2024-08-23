using LuaCommon;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LoadDll : MonoBehaviour
{
    private string commonPath = "/common";
    void Start()
    {
        StartLoadDll();
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
        //Type type = HotFixAss.GetType("Test");//获取类名
        //type.GetMethod("Run").Invoke(null, null);
    }

    private void LoadUI()
    {
        GameObject panel = GameObject.Find("Canvas/Panel");
        LoadLuaAsset.LoadUI("ui_login",
            (asset) =>
            {
                Instantiate(asset, panel.transform);
            });

        LoadLuaAsset.LoadUI("ui_world",
            (asset) =>
            {
                Instantiate(asset, panel.transform);
            });

    }



}
