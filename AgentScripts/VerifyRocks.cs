using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VerifyRocks
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var root = GameObject.Find("rocks");
        sb.Append("root=").Append(root != null).Append(" children=").Append(root != null ? root.transform.childCount : 0).Append('\n');

        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (!t.name.StartsWith("Rock (")) continue;
            var pc = t.GetComponent<PolygonCollider2D>();
            var bc = t.GetComponent<BoxCollider2D>();
            var sr = t.GetComponent<SpriteRenderer>();
            sb.Append(t.name)
              .Append(" pos=").Append(t.position.x.ToString("F1")).Append(',').Append(t.position.y.ToString("F1"))
              .Append(" layer=").Append(t.gameObject.layer)
              .Append(" sprite=").Append(sr != null && sr.sprite != null ? sr.sprite.name : "NULL")
              .Append(" poly=").Append(pc != null ? pc.pathCount.ToString() : "-")
              .Append(" box=").Append(bc != null ? "yes" : "-")
              .Append(" trigger=").Append(pc != null ? pc.isTrigger.ToString() : "-")
              .Append('\n');
        }

        sb.Append("sceneDirty=").Append(SceneManager.GetActiveScene().isDirty);
        return sb.ToString();
    }
}
