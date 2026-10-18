using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DumpUI
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return "no canvas";
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
            sb.AppendLine($"CanvasScaler mode={scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight} screenMatch={scaler.screenMatchMode}");
        Dump(canvas.transform, 0, sb);
        return sb.ToString();
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        var rt = t as RectTransform;
        string info = "";
        if (rt != null)
            info = $" aMin={rt.anchorMin} aMax={rt.anchorMax} pos={rt.anchoredPosition} size={rt.sizeDelta}";
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
