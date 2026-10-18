using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShowSigns
{
    public static string Level2Gap()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level2.unity", OpenSceneMode.Single);
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return "no scene view";
        sv.pivot = new Vector3(37.5f, 8.0f, 0f);
        sv.size = 22f;
        sv.Repaint();
        return "focused";
    }
}
