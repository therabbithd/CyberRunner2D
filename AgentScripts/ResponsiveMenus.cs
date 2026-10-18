using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ResponsiveMenus
{
    static readonly string[] Scenes =
    {
        "Assets/Scenes/Menu.unity",
        "Assets/Scenes/LevelSel.unity",
        "Assets/Scenes/Credits.unity",
        "Assets/Scenes/CharacterMenu.unity",
    };

    public static string Apply()
    {
        var sb = new StringBuilder();
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var canvas = FindRootCanvas();
            if (canvas == null) { sb.AppendLine(path + ": NO CANVAS"); continue; }

            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            sb.AppendLine(path + ": scaler " + scaler.uiScaleMode + " ref=" + scaler.referenceResolution + " match=" + scaler.matchWidthOrHeight);

            if (path.EndsWith("CharacterMenu.unity"))
            {
                var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
                var rt = (RectTransform)hl.transform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(-160f, 150f);
                rt.anchoredPosition = Vector2.zero;
                hl.childAlignment = TextAnchor.MiddleCenter;
                hl.spacing = 20f;
                hl.childControlWidth = true;
                hl.childForceExpandWidth = true;
                hl.childControlHeight = false;
                hl.childForceExpandHeight = false;
                sb.AppendLine("  row stretch " + rt.anchorMin + ".." + rt.anchorMax + " size=" + rt.sizeDelta + " spacing=" + hl.spacing + " ctrlW=" + hl.childControlWidth + " expandW=" + hl.childForceExpandWidth);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        return sb.ToString();
    }

    static Canvas FindRootCanvas()
    {
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.isRootCanvas) return c;
        return null;
    }
}
