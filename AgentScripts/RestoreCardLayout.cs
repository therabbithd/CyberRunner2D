using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class RestoreCardLayout
{
    public static string Apply()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterMenu.unity", OpenSceneMode.Single);
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var row = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
        var rowRT = (RectTransform)row.transform;
        rowRT.anchorMin = rowRT.anchorMax = new Vector2(0.5f, 0.5f);
        rowRT.pivot = new Vector2(0.5f, 0.5f);
        rowRT.anchoredPosition = Vector2.zero;
        rowRT.sizeDelta = new Vector2(1076f, 100f);
        row.childAlignment = TextAnchor.UpperCenter;
        row.spacing = 0f;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;
        row.childControlWidth = false;
        row.childControlHeight = false;

        for (int i = 0; i < rowRT.childCount; i++)
        {
            var card = rowRT.GetChild(i);
            var vlg = card.GetComponentInChildren<VerticalLayoutGroup>(true);
            var vrt = (RectTransform)vlg.transform;
            vrt.anchorMin = vrt.anchorMax = new Vector2(0.5f, 0.5f);
            vrt.pivot = new Vector2(0.5f, 0.5f);
            vrt.anchoredPosition = Vector2.zero;
            vrt.sizeDelta = new Vector2(100f, 100f);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 0f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.padding = new RectOffset(0, 0, 0, 0);

            var imgT = vlg.transform.Find("Image");
            var img = imgT.GetComponent<Image>();
            img.preserveAspect = false;
            var ile = imgT.gameObject.GetComponent<LayoutElement>();
            if (ile != null) Object.DestroyImmediate(ile, true);
            var imgRT = (RectTransform)imgT;
            imgRT.anchorMin = imgRT.anchorMax = new Vector2(0f, 1f);
            imgRT.pivot = new Vector2(0.5f, 0.5f);
            imgRT.sizeDelta = new Vector2(100f, 100f);

            var txtT = vlg.transform.Find("Text (TMP)");
            var tle = txtT.gameObject.GetComponent<LayoutElement>();
            if (tle != null) Object.DestroyImmediate(tle, true);
            var txtRT = (RectTransform)txtT;
            txtRT.anchorMin = txtRT.anchorMax = new Vector2(0f, 1f);
            txtRT.pivot = new Vector2(0.5f, 0.5f);
            txtRT.sizeDelta = new Vector2(i < 2 ? 200f : 300f, 0f);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "restored original card layout (scaler kept)";
    }
}
