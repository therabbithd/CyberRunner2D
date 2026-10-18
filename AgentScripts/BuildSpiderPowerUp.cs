using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildSpiderPowerUp
{
    const string ScenePath = "Assets/Scenes/Level1.unity";
    const string PrefabPath = "Assets/Prefabs/PowerUp.prefab";
    const string AssetPath = "Assets/PowerUpsData/PowerUpSpider.asset";
    const int FloorLayer = 3;
    const float X = 45f;
    const float YOffset = 1.6f;

    public static string Place()
    {
        var sb = new StringBuilder();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var data = AssetDatabase.LoadAssetAtPath<PowerUpScriptableObject>(AssetPath);
        if (prefab == null) return "ERROR: prefab not found " + PrefabPath;
        if (data == null) return "ERROR: asset not found " + AssetPath;

        var old = GameObject.Find("PowerUpSpider");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        float groundY = GroundY(X);
        sb.AppendLine("groundY@" + X + " = " + groundY);
        if (float.IsNaN(groundY)) return sb.ToString() + "ERROR: no ground";

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "PowerUpSpider";
        go.transform.position = new Vector3(X, groundY + YOffset, 0f);

        var mgr = go.GetComponent<PowerUpManager>();
        if (mgr == null) return sb.ToString() + "ERROR: PowerUpManager missing on prefab";
        var so = new SerializedObject(mgr);
        so.FindProperty("PowerUp").objectReferenceValue = data;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        sb.AppendLine("OK PowerUpSpider placed at x=" + X + " y=" + (groundY + YOffset));
        return sb.ToString();
    }

    static float GroundY(float x)
    {
        var hits = Physics2D.RaycastAll(new Vector2(x, 30f), Vector2.down, 70f, 1 << FloorLayer);
        if (hits.Length == 0) return float.NaN;
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (IsCeiling(h.collider.transform)) continue;
            return h.point.y;
        }
        return float.NaN;
    }

    static bool IsCeiling(Transform t)
    {
        while (t != null)
        {
            if (t.name.StartsWith("Ceil") || t.name == "LevelSpriteShape (2)") return true;
            t = t.parent;
        }
        return false;
    }
}
