using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class GapSigns
{
    const string SpritePath = "Assets/SnowSurferAssets/caution.png";
    const float SignScale = 0.28f;
    const float BackFromEdge = 2.8f;

    public static string All()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null) return "ERROR sprite " + SpritePath;
        float h = sprite.bounds.size.y * SignScale;

        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);

            // idempotencia: borrar señales previas
            var rootOld = GameObject.Find("GapSigns");
            if (rootOld != null) Object.DestroyImmediate(rootOld);

            // recoge tramos de suelo por spline
            var segs = new List<(SpriteShapeController ssc, float min, float max)>();
            foreach (var ssc in Object.FindObjectsByType<SpriteShapeController>(FindObjectsInactive.Include))
            {
                if (!(ssc.gameObject.name.StartsWith("Floor") || ssc.gameObject.name.Contains("LevelSpriteShape (1)"))) continue;
                var sp = ssc.spline;
                if (sp == null || sp.GetPointCount() == 0) continue;
                float min = 1e9f, max = -1e9f;
                for (int i = 0; i < sp.GetPointCount(); i++)
                {
                    float wx = ssc.transform.TransformPoint(sp.GetPosition(i)).x;
                    if (wx < min) min = wx;
                    if (wx > max) max = wx;
                }
                segs.Add((ssc, min, max));
            }
            segs.Sort((a, b) => a.min.CompareTo(b.min));

            var root = new GameObject("GapSigns");
            int signs = 0;
            for (int i = 1; i < segs.Count; i++)
            {
                float gapStart = segs[i - 1].max;
                float gapEnd = segs[i].min;
                if (gapEnd - gapStart < 0.5f) continue;

                float signX = gapStart - BackFromEdge;
                float groundY = TopSurfaceY(segs[i - 1].ssc, signX);

                var go = new GameObject("GapSign " + (++signs));
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(signX, groundY + h * 0.5f + 0.4f, 0f);
                go.transform.localScale = new Vector3(SignScale, SignScale, 1f);
                go.layer = 0;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 2;
                sr.color = Color.white; // se conserva el neón original

                sb.AppendLine($"  {name}: gap {gapStart:F1}..{gapEnd:F1} -> sign x={signX:F1} y={go.transform.position.y:F1}");
            }
            if (signs == 0) sb.AppendLine("  " + name + ": sin huecos");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    // Y de la superficie superior del tramo en la X dada (usa la frontera alta del spline).
    static float TopSurfaceY(SpriteShapeController ssc, float worldX)
    {
        var sp = ssc.spline;
        int n = sp.GetPointCount();
        var pts = new List<Vector3>();
        float minY = 1e9f, maxY = -1e9f;
        for (int i = 0; i < n; i++)
        {
            var wp = ssc.transform.TransformPoint(sp.GetPosition(i));
            pts.Add(wp);
            if (wp.y < minY) minY = wp.y;
            if (wp.y > maxY) maxY = wp.y;
        }
        float mid = (minY + maxY) * 0.5f;
        float bestY = maxY, bestDx = 1e9f;
        foreach (var p in pts)
        {
            if (p.y < mid) continue; // solo frontera superior
            float dx = Mathf.Abs(p.x - worldX);
            if (dx < bestDx) { bestDx = dx; bestY = p.y; }
        }
        return bestY;
    }
}
