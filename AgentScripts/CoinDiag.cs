using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoinDiag
{
    const int FloorLayer = 3;

    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level3", "Level4" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            sb.AppendLine("== " + name + " ==");
            var hitsAny = 0; var hitsFloor = 0; var probes = 0;
            for (float x = -5f; x <= 130f; x += 5f)
            {
                probes++;
                var all = Physics2D.RaycastAll(new Vector2(x, 30f), Vector2.down, 70f);
                if (all.Length > 0) hitsAny++;
                var floor = Physics2D.Raycast(new Vector2(x, 30f), Vector2.down, 70f, 1 << FloorLayer);
                if (floor.collider != null)
                {
                    hitsFloor++;
                    if (x % 25f == 0f)
                        sb.AppendLine("  x=" + x.ToString("F0") + " -> " + floor.collider.name + "@" + floor.point.y.ToString("F1"));
                }
            }
            sb.AppendLine("  probes=" + probes + " anyLayerHits=" + hitsAny + " floorHits=" + hitsFloor);
        }
        return sb.ToString();
    }
}
