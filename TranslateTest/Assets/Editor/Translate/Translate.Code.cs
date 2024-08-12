using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Translante
{
    public partial class Translate
    {
        static string systemTipPath = "/S/systemtip.txt";
        private static void ExcuteGetMonoScript()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(Application.dataPath + systemTipPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;
                string[] contents = lines[i].Split('=');
                if (contents.Length < 2 || dic.ContainsKey(contents[0])) continue;
                dic.Add(contents[0], contents[1]);
            }

            string path = Application.dataPath + "/CodeTranslate.xlsx";
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
            else
                File.Delete(path);
            WriteExcel(Application.dataPath + "/CodeTranslate.xlsx", dic);
        }
    }
}
