using System.Text;
using UnityEngine;

public static class SpiderPlayTest
{
    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== SPIDER PLAY TEST ===");
        sb.AppendLine("time=" + Time.time);

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        if (player == null) return "FAIL: player NULL";
        var spider = player.GetComponent<SpiderGravityController>();
        var pc = player.GetComponent<PlayerController>();
        var rb = player.GetComponent<Rigidbody2D>();
        if (spider == null) return "FAIL: SpiderGravityController NULL";
        if (rb == null) return "FAIL: Rigidbody2D NULL";

        // Estado inicial
        Vector3 pos0 = player.transform.position;
        Vector2 vel0 = rb.linearVelocity;
        sb.AppendLine($"INIT pos={pos0} vel={vel0} grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} inverted={spider.IsInverted} gscale={rb.gravityScale} torqueDir={(pc != null ? pc.TorqueDirection.ToString() : "?")}");

        var vf = typeof(SpiderGravityController).GetField("visualsRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vr = (Transform)vf.GetValue(spider);
        float visY0 = vr != null ? vr.localScale.y : 999f;
        sb.AppendLine("visualsRoot=" + (vr != null ? vr.name + " scaleY=" + visY0 : "NULL"));

        // TEST 1: Toggle directo (mecánica pura)
        spider.ToggleGravity();
        bool t1_inv = spider.IsInverted;
        float t1_gs = rb.gravityScale;
        int t1_torque = pc != null ? pc.TorqueDirection : 999;
        float t1_visY = vr != null ? vr.localScale.y : 999f;
        bool t1_suppress = spider.ConsumeSuppressFlipCount();
        bool t1_suppress2 = spider.ConsumeSuppressFlipCount();
        sb.AppendLine($"T1-TOGGLE inverted={t1_inv} gscale={t1_gs} torque={t1_torque} visY={t1_visY} suppress1={t1_suppress} suppress2={t1_suppress2}");
        bool t1_ok = t1_inv == true && t1_gs < 0f && t1_torque == -1 && t1_visY < 0f && t1_suppress && !t1_suppress2;
        sb.AppendLine("T1 " + (t1_ok ? "PASS" : "FAIL"));

        // TEST 2: revertir
        // Esperar cooldown manualmente: lastToggleTime es privado; forzamos espera via Time? En su lugar llamamos ToggleGravity de nuevo (bypass cooldown, es directo).
        spider.ToggleGravity();
        bool t2_inv = spider.IsInverted;
        float t2_gs = rb.gravityScale;
        sb.AppendLine($"T2-REVERT inverted={t2_inv} gscale={t2_gs}");
        bool t2_ok = t2_inv == false && t2_gs > 0f;
        sb.AppendLine("T2 " + (t2_ok ? "PASS" : "FAIL"));
        spider.ConsumeSuppressFlipCount(); // limpiar flag del revert

        // TEST 3: cooldown bloquea TryToggle inmediato
        // Tras T2, lastToggleTime=ahora mismo -> TryToggle debe ignorarse por cooldown
        bool before3 = spider.IsInverted;
        spider.TryToggle();
        bool after3 = spider.IsInverted;
        sb.AppendLine($"T3-COOLDOWN before={before3} after={after3} (debe ser igual)");
        bool t3_ok = before3 == after3;
        sb.AppendLine("T3 " + (t3_ok ? "PASS" : "FAIL"));

        // TEST 4: rechazo en aire (teleport alto, TryToggle no debe cambiar nada)
        // Primero restauramos velocidad para estado estable
        rb.linearVelocity = new Vector2(vel0.x, 0f);
        Vector3 airPos = new Vector3(pos0.x, pos0.y + 30f, pos0.z);
        player.transform.position = airPos;
        // Forzar update de físicas: el propio TryToggle llama UpdateContactFlags
        bool before4 = spider.IsInverted;
        // Pequeña espera activa para que transform se propague? No hay awaits; OverlapCircle usa posición actual, vale.
        spider.TryToggle();
        bool after4 = spider.IsInverted;
        sb.AppendLine($"T4-AIR pos={airPos} grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} before={before4} after={after4} (debe ser igual)");
        bool t4_ok = before4 == after4;
        sb.AppendLine("T4 " + (t4_ok ? "PASS (rechazo en aire OK)" : "FAIL (tunel en aire: cambió sin contacto)"));
        // Restaurar posición
        player.transform.position = pos0;
        rb.linearVelocity = vel0;

        // TEST 5: TryToggle con contacto (si está grounded ahora, debe invertir; si no, SKIP informativo)
        // Han pasado <0.2s desde T2? T4 no tocó lastToggleTime si fue rechazado. Pero T2 sí. Damos margen: si cooldown aún activo, el test no es concluyente -> reportar.
        sb.AppendLine($"T5-GATE grounded={spider.IsGrounded} ceiling={spider.IsCeilinged} inverted={spider.IsInverted}");
        sb.AppendLine("T5 INFO: TryToggle con contacto se valida con input real (Espacio) según guía; gate automático=" + (spider.IsGrounded || spider.IsCeilinged ? "CONTACTO OK" : "SIN CONTACTO (player en movimiento, SKIP)"));

        bool allCore = t1_ok && t2_ok && t3_ok && t4_ok;
        sb.AppendLine(allCore ? "RESULT: CORE-PASS" : "RESULT: CORE-FAIL");
        sb.AppendLine("=== END PLAY TEST ===");
        return sb.ToString();
    }
}
