using System.Text;
using UnityEngine;
using UnityEngine.U2D;

public static class FloorProbe
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var floor = GameObject.Find("LevelSpriteShape (1)");
        if (floor == null) { sb.AppendLine("no floor"); return sb.ToString(); }
        var ssc = floor.GetComponent<SpriteShapeController>();
        var sr = floor.GetComponent<SpriteShapeRenderer>();

        var so = new UnityEditor.SerializedObject(ssc);
        var off = so.FindProperty("m_ColliderOffset");
        sb.AppendLine("m_ColliderOffset=" + (off != null ? off.floatValue.ToString() : "?"));

        if (sr != null)
            sb.AppendLine("renderer bounds min=" + sr.bounds.min + " max=" + sr.bounds.max);

        var sp = ssc.spline;
        sb.AppendLine("spline points (world top edge):");
        for (int i = 0; i < sp.GetPointCount(); i++)
        {
            Vector3 w = floor.transform.TransformPoint(sp.GetPosition(i));
            if (w.x >= 0f && w.x <= 14f) sb.AppendLine($"  [{i}] {w}");
        }

        var ec = ssc.edgeCollider;
        if (ec != null)
        {
            sb.AppendLine("edge collider (world):");
            foreach (var p in ec.points)
            {
                Vector3 w = floor.transform.TransformPoint(p);
                if (w.x >= 0f && w.x <= 14f) sb.AppendLine($"  {w}");
            }
        }

        // also sprite shape renderer mesh bounds via reflection of generated mesh
        sb.AppendLine("--- rocks ---");
        var root = GameObject.Find("rocks");
        if (root != null)
            foreach (Transform t in root.transform)
                if (t.position.x <= 14f) sb.AppendLine($"  {t.name} pos={t.position}");

        return sb.ToString();
    }
}
