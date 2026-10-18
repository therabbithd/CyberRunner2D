using System.Text;
using UnityEngine;

public static class GlueObstacles
{
    public static string Report() => Inspect(false);
    public static string Fix() => Inspect(true);

    static string Inspect(bool apply)
    {
        var sb = new StringBuilder();
        var root = GameObject.Find("rocks");
        if (root == null) { sb.AppendLine("no 'rocks' root"); return sb.ToString(); }

        Physics2D.SyncTransforms();
        int floorMask = LayerMask.GetMask("Floor");
        int count = 0, moved = 0;

        foreach (Transform t in root.transform)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            var box = t.GetComponent<BoxCollider2D>();
            float halfH = sr != null ? sr.bounds.extents.y : (box != null ? box.bounds.extents.y : 0f);
            float bottom = t.position.y - halfH;

            var hit = Physics2D.Raycast(new Vector2(t.position.x, t.position.y + halfH + 5f), Vector2.down, 100f, floorMask);
            if (hit.collider == null)
            {
                sb.AppendLine($"  {t.name} x={t.position.x:F2} bottom={bottom:F2} FLOOR? none");
                continue;
            }
            float floorY = hit.point.y;
            float gap = bottom - floorY;
            count++;
            if (apply && Mathf.Abs(gap) > 0.01f)
            {
                var p = t.position;
                t.position = new Vector3(p.x, floorY + halfH, p.z);
                moved++;
                sb.AppendLine($"  {t.name} x={p.x:F2} gap {gap:F2} -> 0 (y {p.y:F2}->{floorY + halfH:F2})");
            }
            else
            {
                sb.AppendLine($"  {t.name} x={t.position.x:F2} y={t.position.y:F2} halfH={halfH:F2} bottom={bottom:F2} floorY={floorY:F2} gap={gap:F2}");
            }
        }
        sb.AppendLine($"total={count} moved={moved}");
        return sb.ToString();
    }
}
