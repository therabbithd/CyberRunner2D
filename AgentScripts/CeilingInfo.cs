using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class CeilingInfo
{
    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            sb.AppendLine("== " + name + " ==");
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                bool isCeil = t.name.StartsWith("Ceiling") || t.name.Contains("LevelSpriteShape (2)");
                if (!isCeil) continue;
                var ssc = t.GetComponent<SpriteShapeController>();
                var ec = ssc != null ? ssc.edgeCollider : null;
                float minY = 999f, maxY = -999f;
                if (ec != null && ec.pointCount > 0)
                {
                    foreach (var p in ec.points)
                    {
                        float wy = t.TransformPoint(p).y;
                        if (wy < minY) minY = wy;
                        if (wy > maxY) maxY = wy;
                    }
                }
                var eff = t.GetComponent<SurfaceEffector2D>();
                sb.AppendLine($"  {t.name} pos={t.position.x:F0},{t.position.y:F1} colY={minY:F1}..{maxY:F1} pts={(ec != null ? ec.pointCount : -1)} speed={(eff != null ? eff.speed.ToString() : "-")}");
            }
            var player = GameObject.Find("player");
            if (player != null) sb.AppendLine("  player pos=" + player.transform.position.ToString("F1"));
        }
        return sb.ToString();
    }
}
