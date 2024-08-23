using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class UI_WorldWnd : MonoBehaviour
{
    [SerializeField] Button btn_openLua;

    private void Awake()
    {
        btn_openLua.onClick.AddListener(OnClickOpen);
    }
    [CSharpCallLua]
    delegate void Show();

    private void OnClickOpen()
    {
        LuaEnv env = MyLuaBase.Require("UI_Panda");
        Show showAction = env.Global.Get<Show>("Show");
        showAction();
    }
}
