using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VerifyObstacles
{
    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            int rocks = 0, boxSolid = 0, poly = 0, wrongSprite = 0, onFloor = 0;
            bool sizeOk = true;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!t.name.StartsWith("Rock (") && !t.name.StartsWith("CeilRock (")) continue;
                rocks++;
                if (t.gameObject.layer == 3) onFloor++;
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr == null || sr.sprite == null || !sr.sprite.name.StartsWith("box_cyberpunk")) wrongSprite++;
                var pc = t.GetComponent<PolygonCollider2D>();
                var bc = t.GetComponent<BoxCollider2D>();
                if (pc != null) poly++;
                if (bc != null)
                {
                    if (!bc.isTrigger) boxSolid++;
                    if (Vector2.Distance(bc.size, new Vector2(6.17f, 6.08f)) > 0.01f) sizeOk = false;
                }
            }
            sb.AppendLine($"{name} rocks={rocks} solid={boxSolid} poly={poly} wrongSprite={wrongSprite} onFloor={onFloor} sizeOk={sizeOk}");
        }
        return sb.ToString();
    }
}
