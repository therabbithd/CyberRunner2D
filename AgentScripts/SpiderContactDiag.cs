using System.Text;
using UnityEngine;

public static class SpiderContactDiag
{
    public static string Run()
    {
        var sb = new StringBuilder();
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        var spider = player.GetComponent<SpiderGravityController>();
        sb.AppendLine($"t={Time.time} pos={player.transform.position} rot={player.transform.rotation.eulerAngles}");
        foreach (Transform c in player.transform)
            sb.AppendLine($"child: {c.name} localPos={c.localPosition} worldPos={c.position}");
        var gc = player.transform.Find("GroundCheck");
        var cc = player.transform.Find("CeilingCheck");
        int floorLayer = LayerMask.NameToLayer("Floor");
        int floorMask = LayerMask.GetMask("Floor");
        sb.AppendLine($"Floor layer={floorLayer} mask={floorMask}");
        if (gc != null)
        {
            foreach (float r in new float[]{0.25f, 0.5f, 1.0f})
            {
                var hit = Physics2D.OverlapCircle(gc.position, r, floorMask);
                sb.AppendLine($"GroundCheck r={r} hit={(hit != null ? hit.name + " layer=" + LayerMask.LayerToName(hit.gameObject.layer) + " pos=" + hit.transform.position : "NULL")}");
            }
            // qué hay debajo sin máscara
            var anyHit = Physics2D.OverlapCircleAll(gc.position, 1.0f);
            sb.AppendLine($"OverlapAll r=1.0 count={anyHit.Length}");
            foreach (var h in anyHit)
                sb.AppendLine($"  - {h.name} layer={LayerMask.LayerToName(h.gameObject.layer)} pos={h.transform.position}");
            // distancia al suelo: raycast hacia abajo
            var rc = Physics2D.Raycast(gc.position, Vector2.down, 10f, floorMask);
            sb.AppendLine("RayDown from GroundCheck: " + (rc.collider != null ? rc.collider.name + " dist=" + rc.distance : "NULL"));
        }
        if (cc != null)
        {
            var hit = Physics2D.OverlapCircle(cc.position, 0.25f, floorMask);
            sb.AppendLine($"CeilingCheck r=0.25 hit={(hit != null ? hit.name : "NULL")}");
        }
        var cols = player.GetComponents<Collider2D>();
        foreach (var c in cols)
            sb.AppendLine($"player collider: {c.GetType().Name} offset={c.offset} isTrigger={c.isTrigger} enabled={c.enabled} bounds={c.bounds}");
        sb.AppendLine($"spider flags: grounded={spider.IsGrounded} ceiling={spider.IsCeilinged}");
        return sb.ToString();
    }
}
