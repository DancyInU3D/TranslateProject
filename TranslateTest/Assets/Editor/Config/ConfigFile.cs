using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public partial class ConfigFile
{
    private string m_fileName;
    private string m_configName;

    private string[] m_fieldNames;//变量名
    private string[] m_types;//变量类型
    private string[] m_annotations;//变量注释
    private string[][] m_valueLines;//变量值

    private static string ConfigTextPath = "/Scripts/Config/Define/";

    public string ItemClassName
    {
        get { return string.Format("Item_{0}", m_fileName); }
    }

    public string ConfigClassName
    {
        get { return string.Format("Config_{0}", m_fileName); }
    }

    public ConfigFile(string folder,string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("filename 为空");

        m_fileName = GetFileNameWithoutEx(fileName);
        DocodeFile(folder, fileName, out m_fieldNames, out m_types, out m_annotations, out m_valueLines);
    }

    public void DocodeFile(string folder,string fileName,out string[] fieldNames,out string[] types,out string[]annotations,out string[][] valueLines)
    {
        //string path = Path.Combine(folder, fileName);
        using(FileStream fileStream =new FileStream(folder, FileMode.Open, FileAccess.Read))
        {
            using(ExcelPackage excel=new ExcelPackage(fileStream))
            {
                ExcelWorksheet sheet = excel.Workbook.Worksheets[1];
                List<string> listAnnotations = new List<string>();
                //拿表头
                for (int i = 1; i <= sheet.Dimension.End.Column; i++)//按照列读取，先拿到所有的中文注释
                {
                    ExcelRange excelRange = sheet.Cells[1,i];
                    string value = (excelRange.Value ?? "").ToString().Trim();//先拿到表格的值
                    if (!string.IsNullOrEmpty(value))
                    {
                        ExcelComment comment = excelRange.Comment;//然后拿到批注的值
                        string commentStr = comment != null ? string.Format("{0}", comment.Text.Replace("\n", " ").Replace("\r", " ")) : " ";
                        string stringAnnotation = string.Format("{0}{1}", value, commentStr);
                        listAnnotations.Add(stringAnnotation);
                    }
                    else
                        break;
                }
                annotations = listAnnotations.ToArray();
                //拿字段名字和类型
                int maxColum = listAnnotations.Count;
                fieldNames = new string[maxColum];
                types = new string[maxColum];
                for (int i = 1; i <= maxColum; i++)
                {
                    int index = i - 1;
                    fieldNames[index] = (sheet.Cells[2, i].Value ?? "").ToString();//字段名
                    types[index] = (sheet.Cells[3, i].Value ?? "").ToString();//类型
                }

                
                List<string[]> listValue = new List<string[]>();
                for (int r = 5; r <= sheet.Dimension.Rows; r++)
                {
                    object obj = sheet.Cells[r, 1].Value;
                    if (obj != null)
                    {
                        string[] valueArray = new string[maxColum];
                        //valueArray[0] = obj.ToString();
                        for (int c = 1; c <= maxColum; c++)
                        {
                            valueArray[c - 1] = (sheet.Cells[r, c].Text ?? "").ToString();//拿值，一行
                        }
                        listValue.Add(valueArray);
                    }
                    else
                        break;

                }
                valueLines = listValue.ToArray();

            }
        }
    }

    public void ConvertToCS()
    {
        string itemClassString = GetCSharpString();

        string assetPath = string.Format("{0}{1}.cs", ConfigTextPath, ConfigClassName);
        string filePath=GetFullName(assetPath);
        if (File.Exists(filePath))
        {
            string oldText = File.ReadAllText(filePath);
            if (oldText == itemClassString) return;
        }

        if (!Directory.Exists(Application.dataPath + ConfigTextPath))
            Directory.CreateDirectory(Application.dataPath + ConfigTextPath);
        File.WriteAllText(filePath, itemClassString);
        Debug.Log("转换完成");
    }


    public string GetCSharpString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("//本文件中的代码为生成的代码，不允许手动修改");
        sb.AppendLine("using System;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();

        //itemClass
        sb.AppendLine("[Serializable]");
        sb.AppendFormat("public partial class {0}", ItemClassName);
        sb.AppendLine();
        sb.AppendLine("{");

       
        for (int i = 0; i < m_types.Length; i++) //所有的列
        {
            string type = m_types[i];//先拿到类型
            string name = m_fieldNames[i];//拿到变量名
            string annotation = m_annotations[i];//拿到注释
            string cSharpType = null;
            string[] values = new string[m_valueLines.Length];//这一列,同一个变量类型的全部value
            for (int j = 0; j < m_valueLines.Length; j++)//所有的行
            {
                values[j] = m_valueLines[j][i];//第几行j，第几列i
            }
            cSharpType = GetCSharpType(type, values);
        }

        sb.AppendLine("}");
        sb.AppendFormat(@"
public partial class {0} : ScriptableObject
{{
    public {1}[] Array;

    public static {0} Singleton {{ get; private set; }}

    public static void Init()
    {{
#if UNITY_EDITOR
        //if (GameConfig.UseLocalAsset)
            LoadFromLocal();
        //else
            //LoadFromBundle();
#else
            LoadFromBundle();
#endif
    }}
/*
    static void LoadFromBundle()
    {{
        var item = new NormalAssetItem(""data/{2}.u"");
        item.Load(() =>
        {{
            Singleton = item.AssetObj as {0};
            item.Release();
        }});
    }}
*/
#if UNITY_EDITOR
    static void LoadFromLocal()
    {{
        string path = ""Assets/Config/Asset/{2}.asset"";
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<{0}>(path);
        Singleton = obj as {0};
    }}
#endif

}}", ConfigClassName, ItemClassName, m_fileName);

        return sb.ToString();
    }

    private string GetCSharpType(string type,string[] values)
    {
        type = type.Trim();

        string target;
        string typeLower = type.ToLowerInvariant();
        switch (typeLower)
        {
            case "uint32":
                target = "uint";
                break;
            case "int32":
                target = "int";
                break;
            case "string":
                target = "string";
                break;
            default:
                throw new InvalidOperationException("Unkown type : " + type);
        }
        return target;
    }

    private string GetFileNameWithoutEx(string filename)
    {
        string[] str = filename.Split('.');
        return str[0];
    }

    private string GetFullName(string assetPath)
    {
        return Application.dataPath + assetPath;
    }

}
