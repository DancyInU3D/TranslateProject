using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public partial class Config
{

    [MenuItem("Config/导入表格")]
    public static void ImportExcel()
    {
        string excelPath = EditorUtility.OpenFilePanel("请选择表格", "", "");
        string[] pathArr = excelPath.Split('/');
        string excelName = pathArr[pathArr.Length - 1];//拿到表格名称

        if (!EditorUtility.DisplayDialog("转换表格", "确定导入表格" + excelName + "嘛？", "confirm", "cancel"))
            return;

    }

    [MenuItem("Config/生成C#类")]
    public static void GenerateClass()
    {

        string excelPath = EditorUtility.OpenFilePanel("请选择表格", "", "");
        string[] pathArr = excelPath.Split('/');
        string excelName = pathArr[pathArr.Length - 1];//拿到表格名称

        if (!EditorUtility.DisplayDialog("生成c#类文件", "确定继续生成表格" + excelName+"嘛？", "confirm", "cancel"))
            return;
        ConfigFile configFile = new ConfigFile(excelPath, excelName);
        configFile.ConvertToCS();
        AssetDatabase.Refresh();

    }






}
