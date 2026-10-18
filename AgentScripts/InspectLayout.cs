using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InspectLayout
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var crt = (RectTransform)canvas.transform;
        var scaler = canvas.GetComponent<CanvasScaler>();
        sb.AppendLine($"canvas rect={crt.rect} pivot={crt.pivot} scaleFactor={canvas.scaleFactor} scalerRef={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");
        var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        var row = (RectTransform)hl.transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(row);
        sb.AppendLine($"row rect={row.rect} pos={row.anchoredPosition} anchors={row.anchorMin}..{row.anchorMax} align={hl.childAlignment} spacing={hl.spacing} ctrlW={hl.childControlWidth} expandW={hl.childForceExpandWidth} ctrlH={hl.childControlHeight}");
        for (int i = 0; i < row.childCount; i++)
        {
            var c = (RectTransform)row.GetChild(i);
            sb.AppendLine($"  {c.name}: rect={c.rect.size} anchoredPos={c.anchoredPosition} anchors={c.anchorMin}..{c.anchorMax} localPos={c.localPosition} sizeDelta={c.sizeDelta}");
        }
        return sb.ToString();
    }
}
