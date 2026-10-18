using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InspectCard
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var hl = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        var btn = hl.transform.GetChild(0);
        var vlg = btn.GetComponentInChildren<VerticalLayoutGroup>(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)vlg.transform);
        sb.AppendLine($"card '{btn.name}' rect={((RectTransform)btn).rect.size}");
        sb.AppendLine($"VLG rect={((RectTransform)vlg.transform).rect.size} pad={vlg.padding} align={vlg.childAlignment} ctrlW={vlg.childControlWidth} ctrlH={vlg.childControlHeight} expandW={vlg.childForceExpandWidth} expandH={vlg.childForceExpandHeight} spacing={vlg.spacing}");
        for (int i = 0; i < vlg.transform.childCount; i++)
        {
            var c = vlg.transform.GetChild(i);
            var rt = (RectTransform)c;
            string extra = "";
            var le = c.GetComponent<LayoutElement>();
            if (le != null) extra = $" LE(min={le.minHeight},pref=({le.preferredWidth},{le.preferredHeight}),flex=({le.flexibleWidth},{le.flexibleHeight}))";
            var img = c.GetComponent<Image>();
            if (img != null) extra += $" sprite={img.sprite?.name} preserve={img.preserveAspect}";
            sb.AppendLine($"  {c.name}: rect={rt.rect.size} anMin={rt.anchorMin} anMax={rt.anchorMax} pivot={rt.pivot} pos={rt.anchoredPosition}{extra}");
        }
        return sb.ToString();
    }
}
