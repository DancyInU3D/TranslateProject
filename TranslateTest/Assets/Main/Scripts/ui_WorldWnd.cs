using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class ui_WorldWnd : MonoBehaviour
{
    [SerializeField] Button btn_openLua;

    private void Awake()
    {
        btn_openLua.onClick.AddListener(OnClickOpen);
    }

    private void OnClickOpen()
    {
        LuaEnv env = MyLuaBase.Require("UI_Panda");

    }
}
