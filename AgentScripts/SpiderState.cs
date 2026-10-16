using System.Text;
using UnityEngine;

public static class SpiderState
{
    public static string Get()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        var spider = player.GetComponent<SpiderGravityController>();
        var rb = player.GetComponent<Rigidbody2D>();
        return $"t={Time.time:F1} pos={player.transform.position} vel={rb.linearVelocity} grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} inverted={spider.IsInverted} gscale={rb.gravityScale}";
    }

    public static string SetupRide()
    {
        var sb = new StringBuilder();
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        var spider = player.GetComponent<SpiderGravityController>();
        var rb = player.GetComponent<Rigidbody2D>();
        // Normalizar: si quedó invertido del test anterior, revertir directo
        if (spider.IsInverted) spider.ToggleGravity();
        spider.ConsumeSuppressFlipCount();
        UnityEngine.Time.timeScale = 1f;
        // Suelo bajo x=15 vía raycast
        Vector2 origin = new Vector2(15f, 30f);
        var rc = Physics2D.Raycast(origin, Vector2.down, 60f, LayerMask.GetMask("Floor"));
        if (rc.collider == null) return "FAIL: no hay suelo bajo x=15";
        sb.AppendLine($"suelo en y={rc.point.y} ({rc.collider.name})");
        Vector3 dest = new Vector3(15f, rc.point.y + 2.2f, 0f);
        player.transform.position = dest;
        player.transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        sb.AppendLine($"teleport a {dest}");
        return sb.ToString();
    }
}
