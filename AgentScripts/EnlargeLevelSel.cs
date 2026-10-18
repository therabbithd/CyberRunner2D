using System.Text;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EnlargeLevelSel
{
    const string ScenePath = "Assets/Scenes/LevelSel.unity";

    static readonly Vector2 CellSize = new Vector2(360f, 220f);
    static readonly Vector2 GridSpacing = new Vector2(40f, 40f);
    static readonly RectOffset GridPadding = new RectOffset(20, 20, 20, 20);
    const float ButtonFontSize = 56f;

    static readonly Vector2 BackSize = new Vector2(320f, 80f);
    static readonly Vector2 BackPos = new Vector2(180f, -55f);
    const float BackFontSize = 36f;

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

        var grid = canvas.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid != null)
        {
            grid.cellSize = CellSize;
            grid.spacing = GridSpacing;
            grid.padding = GridPadding;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.MiddleCenter;
            sb.AppendLine($"grid cell={grid.cellSize} spacing={grid.spacing}");
        }

        if (grid != null)
        {
            foreach (var tmp in grid.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = ButtonFontSize;
            }
            sb.AppendLine($"  {grid.GetComponentsInChildren<Button>(true).Length} buttons font={ButtonFontSize}");
        }

        foreach (var b in canvas.GetComponentsInChildren<Button>(true))
        {
            if (grid != null && b.transform.IsChildOf(grid.transform)) continue;
            var rt = (RectTransform)b.transform;
            rt.sizeDelta = BackSize;
            rt.anchoredPosition = BackPos;
            foreach (var tmp in b.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = BackFontSize;
            }
            sb.AppendLine($"  back '{b.name}' size={rt.sizeDelta} pos={rt.anchoredPosition} font={BackFontSize}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("saved " + ScenePath);
        return sb.ToString();
    }
}
