using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ResponsiveCards
{
    public static string Apply()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var row = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        for (int i = 0; i < row.transform.childCount; i++)
        {
            var card = row.transform.GetChild(i);
            var vlg = card.GetComponentInChildren<VerticalLayoutGroup>(true);
            var vrt = (RectTransform)vlg.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0f;
            vlg.padding = new RectOffset(0, 0, 0, 0);

            var imgT = vlg.transform.Find("Image");
            var img = imgT.GetComponent<Image>();
            img.preserveAspect = true;
            var ile = imgT.gameObject.GetComponent<LayoutElement>() ?? imgT.gameObject.AddComponent<LayoutElement>();
            ile.minHeight = 50f;
            ile.preferredHeight = 110f;
            ile.flexibleHeight = 1f;

            var txtT = vlg.transform.Find("Text (TMP)");
            var tle = txtT.gameObject.GetComponent<LayoutElement>() ?? txtT.gameObject.AddComponent<LayoutElement>();
            tle.preferredHeight = 36f;
            tle.flexibleHeight = 0f;

            sb.AppendLine(card.name + ": VLG stretch=" + vrt.anchorMin + ".." + vrt.anchorMax + " img(preserve=" + img.preserveAspect + " flexH=" + ile.flexibleHeight + ") txt(prefH=" + tle.preferredHeight + ")");
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return sb.ToString();
    }
}
