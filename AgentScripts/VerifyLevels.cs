using System.Text;
using UnityEngine;
using UnityEngine.U2D;

public static class VerifyLevels
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        sb.AppendLine("scene=" + scene.name + " path=" + scene.path);

        foreach (var ssc in Object.FindObjectsByType<SpriteShapeController>(FindObjectsSortMode.None))
        {
            var ec = ssc.edgeCollider;
            var eff = ssc.GetComponent<SurfaceEffector2D>();
            sb.AppendLine(string.Format("{0,-12} layer={1} pts={2} usedByEff={3} trigger={4} speed={5} pos={6}",
                ssc.gameObject.name, ssc.gameObject.layer,
                ec != null ? ec.pointCount : -1,
                ec != null ? ec.usedByEffector.ToString() : "-",
                ec != null ? ec.isTrigger.ToString() : "-",
                eff != null ? eff.speed.ToString() : "-",
                ssc.transform.position.ToString("F1")));
        }

        var rocks = GameObject.Find("rocks");
        sb.AppendLine("rocks children=" + (rocks != null ? rocks.transform.childCount : -1));

        var kz = GameObject.Find("KillZone");
        if (kz != null)
        {
            var box = kz.GetComponent<BoxCollider2D>();
            sb.AppendLine("killzone pos=" + kz.transform.position.ToString("F1") + " size=" + (box != null ? box.size.ToString("F1") : "-")
                + " trigger=" + (box != null ? box.isTrigger.ToString() : "-") + " script=" + (kz.GetComponent("FallKillZone") != null));
        }
        else sb.AppendLine("killzone=NONE");

        var p = GameObject.Find("player");
        sb.AppendLine("player=" + (p != null ? p.transform.position.ToString("F2") : "-"));
        var f = GameObject.Find("FinishLine");
        sb.AppendLine("finish=" + (f != null ? f.transform.position.ToString("F2") : "-"));
        return sb.ToString();
    }

    public static string Focus()
    {
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv == null) return "no scene view";
        sv.pivot = new Vector3(66f, 6f, 0f);
        sv.size = 130f;
        sv.Repaint();
        return "focused";
    }
}
