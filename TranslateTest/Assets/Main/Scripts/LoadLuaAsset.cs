using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuaCommon
{

    public class LoadLuaAsset
    {
        private static GameObject m_prefab;

        public static void LoadUI(string name, Action<GameObject> callback)
        {
            AssetBundle dllAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
            GameObject asset = dllAB.LoadAsset<GameObject>(name);
            callback?.Invoke(asset);
        }

        //TODO：做一个生成ui的方法，并且要挂载到固定ui下 参考NGUIAssetItem.cs的AddToUIRoot和Instantiate 这两个方法
        public static void LuaLoadUI(string name)
        {
            AssetBundle dllAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
            m_prefab = dllAB.LoadAsset<GameObject>(name);
            GameObject panel = GameObject.Find("Canvas/Panel");
            GameObject.Instantiate(m_prefab, panel.transform);
        }

    }
}

