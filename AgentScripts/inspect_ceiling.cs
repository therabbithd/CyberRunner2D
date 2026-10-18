var sb = new System.Text.StringBuilder();
string[] names = { "LevelSpriteShape (2)", "Ceiling 0", "Ceiling 1", "LevelSpriteShape (1)", "Floor 0" };
foreach (var name in names)
{
    var go = UnityEngine.GameObject.Find(name);
    if (go == null) { sb.Append(name + " = NULL\n"); continue; }
    sb.Append("== " + name + " ==\n");
    sb.Append("  pos=" + go.transform.position.ToString("F2") + " rot=" + go.transform.eulerAngles.ToString("F0") + " scale=" + go.transform.localScale.ToString("F2") + "\n");
    var ssc = go.GetComponent("SpriteShapeController");
    if (ssc != null)
    {
        var t = ssc.GetType();
        var spline = t.GetProperty("spline").GetValue(ssc);
        var st = spline.GetType();
        int count = (int)st.GetProperty("pointCount").GetValue(spline);
        var open = st.GetProperty("isOpenEnded").GetValue(spline);
        var off = t.GetProperty("colliderOffset").GetValue(ssc);
        sb.Append("  SSC pts=" + count + " openEnded=" + open + " colliderOffset=" + off + "\n");
        for (int i = 0; i < count && i < 8; i++)
        {
            var p = st.GetMethod("GetPosition").Invoke(spline, new object[] { i });
            var h = st.GetMethod("GetHeight").Invoke(spline, new object[] { i });
            sb.Append("    p[" + i + "]=" + p + " h=" + h + "\n");
        }
    }
    var ec = go.GetComponent<UnityEngine.EdgeCollider2D>();
    if (ec != null)
    {
        var b = ec.bounds;
        sb.Append("  EdgeCollider pts=" + ec.points.Length + " offset=" + ec.offset + " trigger=" + ec.isTrigger + " usedByEffector=" + ec.usedByEffector + "\n");
        sb.Append("    bounds center=" + b.center.ToString("F2") + " size=" + b.size.ToString("F2") + "\n");
        string ps = "";
        for (int i = 0; i < ec.points.Length && i < 8; i++) ps += ec.points[i].ToString("F2") + " ";
        sb.Append("    pts: " + ps + "\n");
    }
    var eff = go.GetComponent<UnityEngine.SurfaceEffector2D>();
    if (eff != null) sb.Append("  Effector speed=" + eff.speed + " forceScale=" + eff.forceScale + " useColliderMask=" + eff.useColliderMask + "\n");
}
return sb.ToString();
