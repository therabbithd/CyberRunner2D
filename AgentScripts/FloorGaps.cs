using System.Collections.Generic;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FloorGaps
{
    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            var ranges = new List<(string n, float min, float max)>();
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                var ec = t.GetComponent<UnityEngine.U2D.SpriteShapeController>()?.edgeCollider;
                if (ec == null || ec.pointCount == 0) continue;
                if (!(t.name.StartsWith("Floor") || t.name.Contains("LevelSpriteShape (1)"))) continue;
                float min = 1e9f, max = -1e9f;
                foreach (var p in ec.points)
                {
                    float wx = t.TransformPoint(p).x;
                    if (wx < min) min = wx;
                    if (wx > max) max = wx;
                }
                ranges.Add((t.name, min, max));
            }
            ranges.Sort((a, b) => a.min.CompareTo(b.min));
            sb.AppendLine("== " + name + " ==");
            for (int i = 0; i < ranges.Count; i++)
            {
                string gap = i > 0 ? $"  gap={ranges[i].min - ranges[i - 1].max:F1}" : "";
                sb.AppendLine($"  {ranges[i].n}: x {ranges[i].min:F1}..{ranges[i].max:F1}{gap}");
            }
        }
        return sb.ToString();
    }
}
