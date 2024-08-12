using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Translante
{
    public partial class Translate
    {
        /// <summary>
        /// 查找所有预制文件夹下的prefab
        /// </summary>
        public static void DoGetFromPrefab()
        {

            List<string> pathes = new List<string>
            {
            "Assets/UIPrefab/UI_Basic",
            "Assets/UIPrefab/UI_New",
            "Assets/UIPrefab/UI_Offline"
            };

            Dictionary<string, string> dic = new Dictionary<string, string>();

            string allPath = EditorUtility.OpenFilePanel("请选择导出的本地化文件", "", "");
            if (!(!string.IsNullOrEmpty(allPath) && File.Exists(allPath)))
                return;
            HashSet<string> existList = new HashSet<string>();
            //先读取翻译文件
            using (var fileStream = new FileStream(allPath, FileMode.Open, FileAccess.Read))
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    var sheet = excel.Workbook.Worksheets[1];
                    if (sheet != null && sheet.Dimension != null)
                    {
                        for (int r = 2; r <= sheet.Dimension.End.Row; r++)
                        {
                            existList.Add(sheet.Cells[r, 1].Value.ToString());
                        }
                    }
                }

            }

            //找ngui预制中文
            for (int i = 0; i < pathes.Count; i++)
            {
                GetComponentsForPath<UILabel>(pathes[i], "t:Prefab", uiLabel =>
                {
                    string text = uiLabel.text.Replace("\r", "\\n").Replace("\n", "\\n");
                    string subText;
                    (text, subText) = FormatTexts(text);

                    if (!string.IsNullOrEmpty(text)
                        && ContainsChinese(text)
                        && !dic.ContainsKey(text)
                        && !existList.Contains(text))
                    {
                        dic.Add(text, string.Empty);
                    }
                });
            }

            //找ugui预制中文
            GetComponentsForPath<Text>("Assets/U", "t:Prefab", uiLabel =>
            {
                string text = uiLabel.text.Replace("\r", "\\n").Replace("\n", "\\n");
                string subText;
                (text, subText) = FormatTexts(text);

                if (!string.IsNullOrEmpty(text)
                    && ContainsChinese(text)
                    && !dic.ContainsKey(text)
                    && !existList.Contains(text))
                {
                    dic.Add(text, string.Empty);
                }
            });

            WriteBehind(allPath, dic);

            EditorUtility.ClearProgressBar();
        }

        private static void WriteBehind(string folder, Dictionary<string, string> dic)
        {
            using (ExcelPackage excel = new ExcelPackage(new FileInfo(folder)))
            {
                var sheet = excel.Workbook.Worksheets[1];
                int sum = dic.Count;
                int index = sheet.Dimension.End.Row + 1;
                foreach (var item in dic)
                {
                    sheet.Cells[index, 1].Value = item.Key;
                    sheet.Cells[index, 2].Value = item.Value;
                    EditorUtility.DisplayProgressBar("正在导出翻译文件", "loading...", index / sum);
                    index++;
                }
                excel.Save();
            }
            EditorUtility.ClearProgressBar();

        }


        /// <summary>
        /// 查找单个预制身上的lable
        /// </summary>
        private static void ExecuteGetSingle()
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


        static Dictionary<string, string> TempDic;
        static List<string> translist;
        static List<string> keylist;
        private static void ImportPrefabTranslate()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string allPath = EditorUtility.OpenFilePanel("请选择翻译好的excel文件", "", "");
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
                    var sheet = excel.Workbook.Worksheets[1];
                    if (sheet != null && sheet.Dimension != null)
                    {
                        for (int r = 2; r <= sheet.Dimension.End.Row; r++)
                        {
                            string key = sheet.Cells[r, 1].Value.ToString();
                            if (TempDic.ContainsKey(key)) continue;
                            if (sheet.Cells[r, 2].Value != null)
                            {
                                keylist.Add(key);
                                translist.Add(sheet.Cells[r, 2].Value.ToString());
                                TempDic.Add(key, sheet.Cells[r, 2].Value.ToString());
                            }

                        }
                    }
                }
            }

            //遍历所有预制
            List<string> pathes = new List<string>
            {
            "Assets/UIPrefab/UI_Basic",
            "Assets/UIPrefab/UI_New",
            "Assets/UIPrefab/UI_Offline"
            };
            for (int i = 0; i < pathes.Count; i++)
            {
                GetPrefabsForPath<UILabel>(pathes[i], "t:Prefab", "UIPrefab", "LN", N_CheckTranlate);
            }

            GetPrefabsForPath<Text>("Assets/U", "t:Prefab", "/U/", "/LU/", U_CheckTranlate);

            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("当前方法运行时间：" + sw.ElapsedMilliseconds);
            sw.Stop();

        }

        private static void N_CheckTranlate(UILabel uiLabel)
        {
            string value = uiLabel.text;
            if (translist.Contains(value) || string.IsNullOrEmpty(value)) return;//如果已经是翻译后的，跳过
            if (TempDic.ContainsKey(value))//存在key，写翻译进去
            {
                uiLabel.text = TempDic[value];
            }
            //else
            //    UnityEngine.Debug.LogError("这个也妹有翻译啊: " + value);
        }

        private static void U_CheckTranlate(Text uiLabel)
        {
            string value = uiLabel.text;
            if (translist.Contains(value) || string.IsNullOrEmpty(value)) return;//如果已经是翻译后的，跳过
            if (TempDic.ContainsKey(value))//存在key，写翻译进去
            {
                uiLabel.text = TempDic[value];
            }
            //else
            //    UnityEngine.Debug.LogError("这个也妹有翻译啊: " + value);
        }

        private static GameObject CreateAnotherPrefab(GameObject obj, string path, string oldDirName, string newDirName)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            string dir = (Application.dataPath + path.Replace(oldDirName, newDirName)).Replace("AssetsAssets", "Assets");
            string[] prefabName = dir.Split('/');
            string name = prefabName[prefabName.Length - 1];
            string dirName = dir.Replace(name, "");
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            GameObject newobj = PrefabUtility.SaveAsPrefabAsset(instance, path.Replace(oldDirName, newDirName));//把原来的UI复制出来，到新的目录下
            if (instance != null)
                DestroyImmediate(instance);
            return newobj;
        }

        /// <summary>
        /// 找到翻译的预制，如果找到了：对比翻译字段；没找到，创建新的。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        private static void GetPrefabsForPath<T>(string path, string filter, string originDir, string localizeDir, Action<T> action)
        {
            var guids = AssetDatabase.FindAssets(filter, new[] { path });
            int sum = guids.Length;
            int index = 0;
            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                UnityEngine.Component[] components = obj.GetComponents<UnityEngine.Component>();
                foreach (var item in components)//判断是否有missing的脚本"ui_recruit"
                {
                    if (item == null)
                        return;
                }


                var newobj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.Replace(originDir, localizeDir));
                if (newobj == null)//如果不存在翻译预制，新建一个新的
                {
                    newobj = CreateAnotherPrefab(obj, prefabPath, originDir, localizeDir);
                }

                var uiLabels = newobj.GetComponentsInChildren<T>(true);
                foreach (var uiLabel in uiLabels)
                {
                    action.Invoke(uiLabel);//prefabPath.Replace(originDir, localizeDir)
                }
                PrefabUtility.SavePrefabAsset(newobj);
                EditorUtility.DisplayProgressBar("正在比对预制中文", prefabPath, index / sum);
                index++;
            }

            EditorUtility.ClearProgressBar();
        }


    }
}

