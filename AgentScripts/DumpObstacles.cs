using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DumpObstacles
{
    public static string Run()
    {
        var sb = new StringBuilder();
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity", OpenSceneMode.Single);
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (!t.name.StartsWith("Rock (") && !t.name.StartsWith("CeilRock (")) continue;
            var go = t.gameObject;
            var sr = t.GetComponent<SpriteRenderer>();
            var bc = t.GetComponent<BoxCollider2D>();
            var pc = t.GetComponent<PolygonCollider2D>();
            string col = bc != null ? $"Box size={bc.size} off={bc.offset} trig={bc.isTrigger}" :
                         pc != null ? $"Poly paths={pc.pathCount} pts={pc.GetTotalPointCount()}" : "no collider";
            sb.AppendLine($"{t.name} parent={t.parent.name} pos={t.position} local={t.localScale} lossy={t.lossyScale} layer={go.layer} sprite={(sr != null && sr.sprite != null ? sr.sprite.name : "null")} {col}");
        }
        return sb.ToString();
    }
}
