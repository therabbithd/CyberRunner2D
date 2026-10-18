using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class CeilingRaise
{
    const float TargetBottom = 22f;

    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                bool isCeil = t.name.StartsWith("Ceiling") || t.name.Contains("LevelSpriteShape (2)");
                if (!isCeil) continue;
                var ssc = t.GetComponent<SpriteShapeController>();
                var ec = ssc != null ? ssc.edgeCollider : null;
                if (ec == null || ec.pointCount == 0) continue;

                float minY = 1e9f;
                foreach (var p in ec.points) minY = Mathf.Min(minY, t.TransformPoint(p).y);

                float delta = TargetBottom - minY;
                if (delta > 0.01f)
                {
                    var pos = t.position;
                    pos.y += delta;
                    t.position = pos;
                    sb.AppendLine($"  {name}/{t.name}: bottom {minY:F1} -> {TargetBottom:F1} (+{delta:F1})");
                }
                else
                {
                    sb.AppendLine($"  {name}/{t.name}: bottom {minY:F1} OK");
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        return sb.ToString();
    }
}
