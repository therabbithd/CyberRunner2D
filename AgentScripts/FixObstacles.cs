using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixObstacles
{
    const string SpritePath = "Assets/SnowSurferAssets/box_cyberpunk.png";
    const string MaterialPath = "Assets/SnowSurferAssets/RockFrictionless.physicsMaterial2D";

    public static string All()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null) return "ERROR: sprite not found " + SpritePath;
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);

        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            string path = "Assets/Scenes/" + name + ".unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int n = 0;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!t.name.StartsWith("Rock (") && !t.name.StartsWith("CeilRock (")) continue;

                var go = t.gameObject;

                var sr = t.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprite;

                var poly = t.GetComponent<PolygonCollider2D>();
                if (poly != null) Object.DestroyImmediate(poly);

                var box = t.GetComponent<BoxCollider2D>();
                if (box == null) box = go.AddComponent<BoxCollider2D>();
                box.size = sprite.bounds.size;
                box.offset = Vector2.zero;
                box.isTrigger = false;             // solido: para al jugador
                if (mat != null) box.sharedMaterial = mat;

                // Fuera de Floor: no muere al tocarlo (CrashDetector filtra Floor)
                // y el ground check (groundMask = Floor) no lo trata como suelo,
                // asi el jugador no "surfea" encima. Sigue en Default -> colisiona
                // con el jugador (Default) y por tanto lo bloquea/para.
                go.layer = 0;

                n++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            sb.AppendLine(name + " rocks=" + n);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return sb.ToString();
    }
}
