using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DiagCoin
{
    const int FloorLayer = 3;

    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level3" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            sb.AppendLine("== " + name + " ==");
            for (float x = -5f; x <= 20f; x += 5f)
            {
                float remaining = 70f;
                Vector2 origin = new Vector2(x, 30f);
                sb.Append("x=" + x.ToString("F0"));
                for (int i = 0; i < 6 && remaining > 0f; i++)
                {
                    var h = Physics2D.Raycast(origin, Vector2.down, remaining, 1 << FloorLayer);
                    if (h.collider == null) { sb.Append(" | null"); break; }
                    sb.Append(" | " + h.collider.name + "@" + h.point.y.ToString("F1") + " ceil=" + IsCeiling(h.collider.transform));
                    float consumed = origin.y - h.point.y + 0.1f;
                    remaining -= consumed;
                    origin = new Vector2(x, h.point.y - 0.1f);
                    if (consumed < 0.05f) { sb.Append(" LOOP"); break; }
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    static float GroundY(float x)
    {
        float remaining = 70f;
        Vector2 origin = new Vector2(x, 30f);
        for (int i = 0; i < 12 && remaining > 0f; i++)
        {
            var hit = Physics2D.Raycast(origin, Vector2.down, remaining, 1 << FloorLayer);
            if (hit.collider == null) return float.NaN;
            if (IsCeiling(hit.collider.transform))
            {
                float consumed = origin.y - hit.point.y + 0.1f;
                remaining -= consumed;
                origin = new Vector2(x, hit.point.y - 0.1f);
                continue;
            }
            return hit.point.y;
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
