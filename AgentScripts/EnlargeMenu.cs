using System.Text;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EnlargeMenu
{
    const string ScenePath = "Assets/Scenes/Menu.unity";

    static readonly Vector2 ButtonSize = new Vector2(440f, 96f);
    const float ButtonFontSize = 52f;
    const float Spacing = 32f;
    const float TitleScale = 3f;
    static readonly Vector2 TitleSize = new Vector2(760f, 130f);
    const float TitleY = -110f;

    public static string Run()
    {
        var sb = new StringBuilder();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Canvas canvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>())
            if (c.isRootCanvas) { canvas = c; break; }
        if (canvas == null) return "no canvas";

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        var layout = canvas.GetComponentInChildren<VerticalLayoutGroup>(true);
        if (layout != null)
        {
            layout.spacing = Spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            sb.AppendLine($"layout spacing={layout.spacing}");
        }

        var buttons = layout != null ? layout.GetComponentsInChildren<Button>(true) : new Button[0];
        foreach (var b in buttons)
        {
            var rt = (RectTransform)b.transform;
            rt.sizeDelta = ButtonSize;
            foreach (var tmp in b.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = ButtonFontSize;
            }
            sb.AppendLine($"  button '{b.name}' size={rt.sizeDelta} font={ButtonFontSize}");
        }

        foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (layout != null && tmp.transform.IsChildOf(layout.transform)) continue;
            var rt = (RectTransform)tmp.transform;
            rt.localScale = new Vector3(TitleScale, TitleScale, 1f);
            rt.sizeDelta = TitleSize;
            rt.anchoredPosition = new Vector2(0f, TitleY);
            sb.AppendLine($"  title '{tmp.name}' scale={rt.localScale} size={rt.sizeDelta} pos={rt.anchoredPosition}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("saved " + ScenePath);
        return sb.ToString();
    }
}
