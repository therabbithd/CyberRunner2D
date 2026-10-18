using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

public static class FloorDiag
{
    public static string Level3()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level3.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        foreach (var ssc in Object.FindObjectsByType<SpriteShapeController>(FindObjectsInactive.Include))
        {
            if (!ssc.gameObject.name.StartsWith("Floor")) continue;
            var sp = ssc.spline;
            float sMin = 1e9f, sMax = -1e9f;
            for (int i = 0; i < sp.GetPointCount(); i++)
            {
                float wx = ssc.transform.TransformPoint(sp.GetPosition(i)).x;
                if (wx < sMin) sMin = wx;
                if (wx > sMax) sMax = wx;
            }
            var ec = ssc.edgeCollider;
            float cMin = 1e9f, cMax = -1e9f;
            if (ec != null) foreach (var p in ec.points)
            {
                float wx = ssc.transform.TransformPoint(p).x;
                if (wx < cMin) cMin = wx;
                if (wx > cMax) cMax = wx;
            }
            sb.AppendLine($"{ssc.gameObject.name} splineX {sMin:F1}..{sMax:F1} (pts={sp.GetPointCount()}) colX {cMin:F1}..{cMax:F1} (pts={(ec != null ? ec.pointCount : -1)})");
        }
        return sb.ToString();
    }
}
