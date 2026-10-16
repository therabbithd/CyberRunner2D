using UnityEngine;

public static class SpiderZoneToggle
{
    public static string Run()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        var spider = player.GetComponent<SpiderGravityController>();
        var rb = player.GetComponent<Rigidbody2D>();
        float x = player.transform.position.x;
        string st = $"x={x:F1} y={player.transform.position.y:F1} grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} inverted={spider.IsInverted}";
        if (spider.IsGrounded && x >= 38f && x <= 58f)
        {
            bool before = spider.IsInverted;
            spider.TryToggle();
            return st + $" TOGGLED before={before} after={spider.IsInverted}";
        }
        return st + " WAIT";
    }
}
