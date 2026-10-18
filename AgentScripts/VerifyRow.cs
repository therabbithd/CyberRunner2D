using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class VerifyRow
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        sb.AppendLine($"canvas rect={((RectTransform)canvas.transform).rect.size} scale={canvas.scaleFactor} scaler={scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");
        var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        var row = (RectTransform)hl.transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(row);
        sb.AppendLine($"row rect={row.rect.size} min={row.rect.min} max={row.rect.max} worldY={row.position.y} children={row.childCount}");
        for (int i = 0; i < row.childCount; i++)
        {
            var c = (RectTransform)row.GetChild(i);
            sb.AppendLine($"  {c.name}: worldPos=({c.position.x:F1},{c.position.y:F1}) worldRectY={c.rect.min.y:F1}..{c.rect.max.y:F1} size={c.rect.size}");
        }
        return sb.ToString();
    }
}
