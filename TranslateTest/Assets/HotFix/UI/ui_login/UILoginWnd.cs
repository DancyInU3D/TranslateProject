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
        SetTitle();
    }


    public void SetTitle()
    {
        txt_title.text = "动物园";
        txt_content01.text = "熊猫,  国宝耶";
    }

}


