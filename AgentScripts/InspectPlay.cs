using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class InspectPlay
{
    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"isPlaying={Application.isPlaying} screen={Screen.width}x{Screen.height}");
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var crt = (RectTransform)canvas.transform;
        var scaler = canvas.GetComponent<CanvasScaler>();
        sb.AppendLine($"canvas rect={crt.rect} scaleFactor={canvas.scaleFactor} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");
        var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        var row = (RectTransform)hl.transform;
        sb.AppendLine($"row rect={row.rect} align={hl.childAlignment} ctrlW={hl.childControlWidth} ctrlH={hl.childControlHeight} expandW={hl.childForceExpandWidth} expandH={hl.childForceExpandHeight}");
        for (int i = 0; i < row.childCount; i++)
        {
            var c = (RectTransform)row.GetChild(i);
            sb.AppendLine($"  {c.name}: rect={c.rect.size} pos={c.anchoredPosition} sd={c.sizeDelta}");
            for (int j = 0; j < c.childCount; j++)
            {
                var g = (RectTransform)c.GetChild(j);
                sb.AppendLine($"     child {g.name}: rect={g.rect.size} pos={g.anchoredPosition} sd={g.sizeDelta}");
            }
        }
        return sb.ToString();
    }
}
