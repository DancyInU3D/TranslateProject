using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Translante
{
    public partial class Translate
    {
        static string PrefabFilePath = "/UIPrefab/uiprefab.txt";

        static Dictionary<string, string> TempDic;
        static Dictionary<string, string> FileDic;

        static List<string> translist;
        static List<string> keylist;
        static List<string> allPrefabList;

        static MD5 s_md5Obj;
        static MD5 MD5Obj
        {
            get { return s_md5Obj ?? (s_md5Obj = MD5.Create()); }
        }
        /// <summary>
        /// 查找所有预制文件夹下的prefab
        /// </summary>
        public static void ExtractFromPrefab()
        {
            Stopwatch sw = Stopwatch.StartNew();
            TempDic = new Dictionary<string, string>();

            string[] pathes = Directory.GetDirectories("Assets/UIPrefab");
            allPrefabList = new List<string>();
            for (int i = 0; i < pathes.Length; i++)
            {
                GetComponentsForPath<UILabel>(pathes[i], "t:Prefab", AddTranlate);
            }

            string path = Application.dataPath + "/PrefabTranslate.xlsx";
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
            else
                File.Delete(path);
            WriteExcel(Application.dataPath + "/PrefabTranslate.xlsx", TempDic);
            if (!File.Exists(Application.dataPath + PrefabFilePath))
                File.Create(Application.dataPath + PrefabFilePath).Dispose();
            File.WriteAllLines(Application.dataPath + PrefabFilePath, allPrefabList);
            UnityEngine.Debug.Log("当前方法运行时间：" + sw.ElapsedMilliseconds);
            sw.Stop();

        }

        /// <summary>
        /// 查找单个预制身上的lable
        /// </summary>
        private static void ExtractFromSinglePrefab()
        {
            var newTexts = new List<string>();
            for (int i = 0; i < m_list.Count; i++)
            {
                UnityEngine.Object obj = m_list[i];
                string subFolder = AssetDatabase.GetAssetPath(obj);
                FindCompents<UILabel>(subFolder, uiLabel =>
                {
                    string text = uiLabel.text.Replace("\r", "\\n").Replace("\n", "\\n");
                    string subText;
                    (text, subText) = FormatTexts(text);

                    if (!uiLabel.name.StartsWith("_@")
                        && !string.IsNullOrEmpty(text)
                        && ContainsChinese(text)
                        && !newTexts.Contains(text))
                    {
                        newTexts.Add(text);
                    }
                });
                EditorUtility.DisplayProgressBar("正在提取预制中文", "loading...", i / m_list.Count);
            }

            string path = Application.dataPath + "/Translate.xlsx";
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
            else
                File.Delete(path);
            WriteExcel(Application.dataPath + "/Translate.xlsx", newTexts);
        }

        private static void AddTranlate(UILabel uiLabel,string uipath)
        {
            string text = uiLabel.text.Replace("\r", "\\n").Replace("\n", "\\n");
            string subText;
            (text, subText) = FormatTexts(text);

            if (!uiLabel.name.StartsWith("_@")
                && !string.IsNullOrEmpty(text)
                && ContainsChinese(text)
                && !TempDic.ContainsKey(text))
            {
                TempDic.Add(text, text);
            }
            if(!allPrefabList.Contains(uipath))
                allPrefabList.Add(uipath);
        }

        private static void ImportPrefabTranslate()
        {
            string allPath = EditorUtility.OpenFilePanel("请选择翻译好的本地化文件", "", "");
            if (!(!string.IsNullOrEmpty(allPath) && File.Exists(allPath)))
                return;
            //var ext = Path.GetExtension(allPath);

            translist = new List<string>();
            keylist = new List<string>();
            TempDic = new Dictionary<string, string>();
            //读取翻译文件
            using (var fileStream = new FileStream(allPath, FileMode.Open, FileAccess.Read))
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    for (var i = 1; i <= excel.Workbook.Worksheets.Count; i++)
                    {
                        var sheet = excel.Workbook.Worksheets[i];
                        if (sheet == null || sheet.Dimension == null)
                            continue;

                        for (int r = 2; r <= sheet.Dimension.End.Row; r++)
                        {
                            string key = sheet.Cells[r, 2].Value.ToString();
                            if (TempDic.ContainsKey(key)) continue;
                            keylist.Add(key);
                            translist.Add(sheet.Cells[r, 3].Value.ToString());
                            TempDic.Add(key, sheet.Cells[r, 3].Value.ToString());
                        }

                    }
                }
            }
            //TODO:读取所有本地预制
            string[] allNeedPrefab = File.ReadAllLines(Application.dataPath + PrefabFilePath);//所有需要翻译的预制

            //遍历所有预制
            string[] pathes = Directory.GetDirectories("Assets/UIPrefab");
            for (int i = 0; i < pathes.Length; i++)
            {
                GetPrefabsForPath<UILabel>(pathes[i], "t:Prefab", CheckTranlate);
            }

        }

        private static void CheckTranlate(UILabel uiLabel, string path)
        {
            var newobj = AssetDatabase.LoadAssetAtPath<GameObject>(GetLocalizPath(path));
            if (newobj == null)//如果不存在翻译预制，新建一个新的
            {
                GameObject obj = (GameObject)AssetDatabase.LoadMainAssetAtPath(path);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(obj);
                string dir = (Application.dataPath + GetLocalizPath(path)).Replace("AssetsAssets", "Assets");
                string[] prefabName = dir.Split('/');
                string name = prefabName[prefabName.Length - 1];
                string dirName = dir.Replace(name, "");
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                newobj = PrefabUtility.SaveAsPrefabAsset(instance, GetLocalizPath(path));//把原来的UI复制出来，到新的目录下
            }


            FindCompents<UILabel>(GetLocalizPath(path), uiLabel =>
            {
                string value = uiLabel.text;
                if (translist.Contains(value)) return;//如果已经是翻译后的，跳过
                if (TempDic.ContainsKey(value))//存在key，写翻译进去
                {
                    uiLabel.text = TempDic[value];
                }
                else
                    UnityEngine.Debug.LogError("这个也妹有翻译啊: " + value);
            });

            AssetDatabase.SaveAssets();
        }


        /// <summary>
        /// 找到翻译的预制，如果找到了：对比翻译字段；没找到，创建新的。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        private static void GetPrefabsForPath<T>(string path, string filter, Action<T,string> action)
        {
            var guids = AssetDatabase.FindAssets(filter, new[] { path });
            int sum = guids.Length;
            int index = 0;
            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var uiLabels = obj.GetComponentsInChildren<T>(true);
                foreach (var uiLabel in uiLabels)
                {
                    action.Invoke(uiLabel, prefabPath);
                }
                EditorUtility.DisplayProgressBar("正在提取预制中文", prefabPath, index / sum);
                index++;
            }

            EditorUtility.ClearProgressBar();
        }

        private static string GetLocalizPath(string oldPath)
        {
            return oldPath.Replace("UIPrefab", "TranslateUIPrefab");
        }



        private static string GetMD5Conde(string assetPath)
        {
            byte[] buffer = File.ReadAllBytes(assetPath);
            if (buffer == null || buffer.Length < 1)
                return "";

            byte[] hash = MD5Obj.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();

            foreach (var b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

    }
}

