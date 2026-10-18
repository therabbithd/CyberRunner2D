using System.Text;
using UnityEngine;

public static class FindFloating
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        sb.AppendLine("scene=" + scene.name);
        Physics2D.SyncTransforms();
        int floorMask = LayerMask.GetMask("Floor");

        var roots = new[] { "rocks" };
        foreach (var rn in roots)
        {
            var root = GameObject.Find(rn);
            if (root == null) { sb.AppendLine("no root " + rn); continue; }
            foreach (Transform t in root.transform)
            {
                var sr = t.GetComponent<SpriteRenderer>();
                var col = t.GetComponent<Collider2D>();
                float halfW = sr != null ? sr.bounds.extents.x : 0.5f;
                float halfH = sr != null ? sr.bounds.extents.y : (col != null ? col.bounds.extents.y : 0.5f);
                float bottom = t.position.y - halfH;

                float left = t.position.x - halfW * 0.9f;
                float right = t.position.x + halfW * 0.9f;
                float gl = GroundAt(left, t.position.y + halfH + 4f, floorMask);
                float gc = GroundAt(t.position.x, t.position.y + halfH + 4f, floorMask);
                float gr = GroundAt(right, t.position.y + halfH + 4f, floorMask);
                float minG = Mathf.Min(gl, Mathf.Min(gc, gr));
                float gap = bottom - minG; // >0 => floating
                sb.AppendLine($"  {t.name} x={t.position.x:F2} bottom={bottom:F2} gL={gl:F2} gC={gc:F2} gR={gr:F2} maxGap={gap:F2}");
            }
        }
        return sb.ToString();
    }

    static float GroundAt(float x, float yStart, int mask)
    {
        var hit = Physics2D.Raycast(new Vector2(x, yStart), Vector2.down, 100f, mask);
        return hit.collider != null ? hit.point.y : float.NaN;
    }
}
