using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DumpMenus
{
    static readonly string[] Scenes =
    {
        "Assets/Scenes/Menu.unity",
        "Assets/Scenes/LevelSel.unity",
        "Assets/Scenes/Credits.unity",
        "Assets/Scenes/CharacterMenu.unity",
    };

    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var path in Scenes)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            sb.AppendLine("===== " + path);
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) { sb.AppendLine("no canvas"); continue; }
            var rt = canvas.GetComponent<RectTransform>();
            sb.AppendLine($"canvas '{canvas.name}' scale={rt.localScale} renderMode={canvas.renderMode}");
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                sb.AppendLine($"  scaler mode={scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");
            for (int i = 0; i < canvas.transform.childCount; i++)
                Dump(canvas.transform.GetChild(i), 1, sb);
        }
        return sb.ToString();
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        var rt = t as RectTransform;
        string info = rt != null ? $" aMin={rt.anchorMin} aMax={rt.anchorMax} pos={rt.anchoredPosition} size={rt.sizeDelta}" : "";
        var comps = new StringBuilder();
        foreach (var c in t.GetComponents<Component>())
        {
            var n = c.GetType().Name;
            if (n == "RectTransform" || n == "CanvasRenderer" || n == "Transform") continue;
            comps.Append(n).Append(",");
        }
        sb.AppendLine(new string(' ', depth * 2) + t.name + info + " [" + comps + "]");
        for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, sb);
    }
}
