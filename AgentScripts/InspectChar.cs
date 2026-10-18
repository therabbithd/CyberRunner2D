using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InspectChar
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        for (int i = 0; i < hl.transform.childCount; i++)
        {
            var btn = hl.transform.GetChild(i);
            var img = btn.GetComponentInChildren<Image>(true);
            var vlg = btn.GetComponentInChildren<VerticalLayoutGroup>(true);
            string spriteName = img != null && img.sprite != null ? img.sprite.name : "NULL";
            sb.AppendLine($"{btn.name}: imageSprite={spriteName} preserveAspect={(img != null ? img.preserveAspect.ToString() : "-")} imgSize={(img != null ? img.rectTransform.sizeDelta.ToString() : "-")} imgAnchors={(img != null ? img.rectTransform.anchorMin + ".." + img.rectTransform.anchorMax : "-")}");
            if (vlg != null)
                sb.AppendLine($"   VLG align={vlg.childAlignment} ctrlW={vlg.childControlWidth} ctrlH={vlg.childControlHeight} expandW={vlg.childForceExpandWidth} expandH={vlg.childForceExpandHeight} spacing={vlg.spacing} padding={vlg.padding}");
        }
        return sb.ToString();
    }
}
