using System;
using UnityEngine;

namespace Common
{
    public class LoadAsset
    {
        public static LoadAsset Instance;

        public void LoadUI(string name, Action<GameObject> callback)
        {
            if (Instance == null) Instance = this;
            AssetBundle dllAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
            GameObject asset = dllAB.LoadAsset<GameObject>(name);
            callback?.Invoke(asset);
        }
    }
}

