using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixLevelSel
{
    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "LevelSel") return "ERR: active scene is '" + scene.name + "', expected LevelSel";

        var buttonType = FindType("UnityEngine.UI.Button");
        if (buttonType == null) return "ERR: UnityEngine.UI.Button not found";
        var tmpType = FindType("TMPro.TextMeshProUGUI") ?? FindType("TMPro.TMP_Text");

        var sb = new StringBuilder();
        var buttons = UnityEngine.Object.FindObjectsByType(buttonType, FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            var comp = (Component)b;
            var go = comp.gameObject;
            string label = GetLabel(comp, tmpType);
            int level = ParseLevel(label);

            var so = new SerializedObject(comp);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            string method = "";
            if (calls != null && calls.arraySize > 0)
            {
                var call0 = calls.GetArrayElementAtIndex(0);
                var mprop = call0.FindPropertyRelative("m_MethodName");
                if (mprop != null) method = mprop.stringValue;
                var arg = call0.FindPropertyRelative("m_Arguments.m_IntArgument");
                if (arg != null && method == "gotolevel" && level >= 1 && level <= 4) arg.intValue = level;
            }
            so.ApplyModifiedProperties();

            if (level >= 5) go.SetActive(false);
            sb.AppendLine(string.Format("{0,-12} label='{1}' method={2} level={3} active={4}",
                go.name, label, method, level, go.activeSelf));
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    static string GetLabel(Component button, Type tmpType)
    {
        if (tmpType == null) return "";
        var c = button.GetComponentInChildren(tmpType, true);
        if (c == null) return "";
        var p = tmpType.GetProperty("text");
        return p != null ? (p.GetValue(c) as string) ?? "" : "";
    }

    static int ParseLevel(string label)
    {
        if (string.IsNullOrEmpty(label)) return -1;
        int i = label.Length - 1;
        while (i >= 0 && char.IsDigit(label[i])) i--;
        string num = label.Substring(i + 1);
        int v;
        return int.TryParse(num, out v) ? v : -1;
    }

    static Type FindType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }
}
