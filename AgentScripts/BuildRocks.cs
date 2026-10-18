using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildRocks
{
    const string SpritePath = "Assets/SnowSurferAssets/rock_cyberpunk.png";
    const string MaterialPath = "Assets/SnowSurferAssets/RockFrictionless.physicsMaterial2D";
    const int FloorLayer = 3;
    const float Embed = 0.15f;
    const float RayStartY = 20f;
    const float RayLength = 60f;

    static readonly float[] Xs = { 8f, 18f, 30f, 42f, 54f, 62f, 74f, 84f };
    static readonly float[] Scales = { 0.14f, 0.12f, 0.16f, 0.13f, 0.14f, 0.12f, 0.16f, 0.14f };

    static PhysicsMaterial2D GetOrCreateMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("RockFrictionless");
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }
        mat.friction = 0f;
        mat.bounciness = 0f;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static string All()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Level1")
            return "ERROR: active scene is '" + scene.name + "', expected Level1";

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
            return "ERROR: could not load sprite at " + SpritePath;

        var existing = GameObject.Find("rocks");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var root = new GameObject("rocks");

        var report = new List<string>();
        for (int i = 0; i < Xs.Length; i++)
        {
            float x = Xs[i];
            float scale = Scales[i];
            float height = sprite.bounds.size.y * scale;

            var hit = Physics2D.Raycast(new Vector2(x, RayStartY), Vector2.down, RayLength, 1 << FloorLayer);
            if (hit.collider == null)
            {
                report.Add("x=" + x + " SKIP (no ground)");
                continue;
            }

            float groundY = hit.point.y;
            float y = groundY + height * 0.5f - Embed;

            var go = new GameObject("Rock (" + (i + 1) + ")");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.layer = FloorLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;
            sr.flipX = (i % 2 == 1);

            var mat = GetOrCreateMaterial();
            var poly = go.AddComponent<PolygonCollider2D>();
            if (poly.pathCount == 0)
            {
                Object.DestroyImmediate(poly);
                var box = go.AddComponent<BoxCollider2D>();
                box.size = sprite.bounds.size;
                box.sharedMaterial = mat;
                report.Add("x=" + x + " (box fallback)");
            }
            else
            {
                poly.sharedMaterial = mat;
            }

            report.Add("x=" + x + " groundY=" + groundY.ToString("F2") + " y=" + y.ToString("F2") + " scale=" + scale);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "Created " + root.transform.childCount + " rocks under 'rocks' and saved scene [" + scene.name + "].\n" + string.Join("\n", report);
    }
}
