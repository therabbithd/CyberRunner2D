using System.Text;
using UnityEngine;

public static class SpiderGroundedTest
{
    public static string Run()
    {
        var sb = new StringBuilder();
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        var spider = player.GetComponent<SpiderGravityController>();
        var rb = player.GetComponent<Rigidbody2D>();
        sb.AppendLine($"STATE t={Time.time} pos={player.transform.position} vel={rb.linearVelocity} grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} inverted={spider.IsInverted} gscale={rb.gravityScale}");
        bool before = spider.IsInverted;
        spider.TryToggle();
        bool after = spider.IsInverted;
        sb.AppendLine($"TryToggle before={before} after={after}");
        if (!spider.IsGrounded && !spider.IsCeilinged)
            sb.AppendLine("SKIP: sin contacto en este frame");
        else if (before == after)
            sb.AppendLine("FAIL: con contacto pero no invirtió (posible cooldown o bug)");
        else
            sb.AppendLine("PASS: invirtió con contacto");
        return sb.ToString();
    }
}
