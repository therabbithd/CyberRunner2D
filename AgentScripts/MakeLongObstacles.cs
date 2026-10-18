using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MakeLongObstacles
{
    const float BarrierLength = 5f;
    const float Embed = 0.15f;
    const int FloorLayer = 3;
    const float RayStartY = 14f;
    const float RayLength = 40f;
    const string MaterialPath = "Assets/SnowSurferAssets/RockFrictionless.physicsMaterial2D";

    static readonly string[] Scenes = { "Level1", "Level2", "Level3", "Level4" };

    struct Info
    {
        public string name;
        public Vector3 pos;
        public Vector3 scale;
        public Sprite sprite;
        public bool flipX;
        public bool flipY;
        public bool ceiling;
    }

    public static string All()
    {
        var sb = new StringBuilder();
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);

        foreach (var name in Scenes)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            var root = GameObject.Find("rocks");
            if (root == null) { sb.AppendLine(name + ": no rocks root"); continue; }

            var data = new List<Info>();
            for (int i = 0; i < root.transform.childCount; i++)
            {
                var t = root.transform.GetChild(i);
                var sr = t.GetComponent<SpriteRenderer>();
                data.Add(new Info
                {
                    name = t.name,
                    pos = t.position,
                    scale = t.lossyScale,
                    sprite = sr != null ? sr.sprite : null,
                    flipX = sr != null && sr.flipX,
                    flipY = sr != null && sr.flipY,
                    ceiling = t.name.StartsWith("CeilRock")
                });
            }

            foreach (var t in new List<Transform>(GetChildren(root.transform)))
                Object.DestroyImmediate(t.gameObject);

            int ground = 0, ceil = 0, pu = 0;
            foreach (var d in data)
            {
                if (d.sprite == null) continue;

                if (d.ceiling)
                {
                    MakeCrate(root.transform, "CeilRock (" + (++ceil) + ")", d.sprite, d.pos, d.scale,
                        d.flipX, d.flipY, mat);
                    continue;
                }

                float crateW = d.sprite.bounds.size.x * Mathf.Abs(d.scale.x);
                float crateH = d.sprite.bounds.size.y * Mathf.Abs(d.scale.y);
                int n = Mathf.Max(1, Mathf.RoundToInt(BarrierLength / crateW));
                float spacing = crateW * 0.98f;
                float rowHalf = (n - 1) * spacing * 0.5f;

                for (int j = 0; j < n; j++)
                {
                    float x = d.pos.x - rowHalf + j * spacing;
                    float groundY;
                    var hit = Physics2D.Raycast(new Vector2(x, RayStartY), Vector2.down, RayLength, 1 << FloorLayer);
                    groundY = hit.collider != null ? hit.point.y : d.pos.y - crateH * 0.5f + Embed;
                    float y = groundY + crateH * 0.5f - Embed;
                    bool flip = (ground % 2 == 1);
                    MakeCrate(root.transform, "Rock (" + (++ground) + ")", d.sprite,
                        new Vector3(x, y, 0f), d.scale, flip, false, mat);
                }
            }

            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t == null) continue;
                if (t.name == "PowerUpJump") { Object.DestroyImmediate(t.gameObject); pu++; }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            sb.AppendLine($"{name}: groundSegments={ground} ceilRocks={ceil} removedJumpPU={pu}");
        }

        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    static IEnumerable<Transform> GetChildren(Transform t)
    {
        for (int i = 0; i < t.childCount; i++) yield return t.GetChild(i);
    }

    static void MakeCrate(Transform parent, string name, Sprite sprite, Vector3 pos, Vector3 scale,
        bool flipX, bool flipY, PhysicsMaterial2D mat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        go.layer = 0; // Default: bloquea pero no mata (CrashDetector filtra Floor)

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 0;
        sr.flipX = flipX;
        sr.flipY = flipY;

        var box = go.AddComponent<BoxCollider2D>();
        box.size = sprite.bounds.size;
        box.offset = Vector2.zero;
        box.isTrigger = false;
        if (mat != null) box.sharedMaterial = mat;
    }
}
