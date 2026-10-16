using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class SpiderCheck
{
    public static string All()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== SPIDER STATIC CHECK ===");
        sb.AppendLine("Scene: " + SceneManager.GetActiveScene().name + " path=" + SceneManager.GetActiveScene().path);

        // Player lookup
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("player");
        sb.AppendLine("Player found: " + (player != null ? player.name : "NULL"));
        if (player == null) return sb.ToString();

        var spider = player.GetComponent<SpiderGravityController>();
        var pc = player.GetComponent<PlayerController>();
        var rb = player.GetComponent<Rigidbody2D>();
        sb.AppendLine("SpiderGravityController: " + (spider != null));
        sb.AppendLine("PlayerController: " + (pc != null));
        sb.AppendLine("Rigidbody2D: " + (rb != null));
        if (rb != null)
            sb.AppendLine($"RB gravityScale={rb.gravityScale} bodyType={rb.bodyType} simulated={rb.simulated} collisionDetection={rb.collisionDetectionMode} angularDamping={rb.angularDamping} linearDamping={rb.linearDamping}");

        // Checks (hijos)
        var gc = player.transform.Find("GroundCheck");
        var cc = player.transform.Find("CeilingCheck");
        sb.AppendLine("GroundCheck child: " + (gc != null ? gc.localPosition.ToString() : "NULL (se autocrea en Play)"));
        sb.AppendLine("CeilingCheck child: " + (cc != null ? cc.localPosition.ToString() : "NULL (se autocrea en Play)"));

        // groundMask via reflexion
        if (spider != null)
        {
            var f = typeof(SpiderGravityController).GetField("groundMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gm = (LayerMask)f.GetValue(spider);
            sb.AppendLine($"groundMask value={gm.value} mask->Floor={LayerMask.GetMask("Floor")} NameToLayer Floor={LayerMask.NameToLayer("Floor")}");
            var vf = typeof(SpiderGravityController).GetField("visualsRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var vr = (Transform)vf.GetValue(spider);
            sb.AppendLine("visualsRoot asignado: " + (vr != null ? vr.name + " scale=" + vr.localScale : "NULL (autoFind en Awake)"));
            var sf = typeof(SpiderGravityController).GetField("spiderActionName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sb.AppendLine("spiderActionName=" + (string)sf.GetValue(spider));
        }

        // Visuals candidatos
        int sprites = player.GetComponentsInChildren<SpriteRenderer>(true).Length;
        sb.AppendLine("SpriteRenderers bajo player: " + sprites);

        // Input actions
        try
        {
            var acts = InputSystem.actions;
            sb.AppendLine("InputSystem.actions: " + (acts != null));
            if (acts != null)
            {
                var aShort = acts.FindAction("Spider", false);
                var aFull = acts.FindAction("Player/Spider", false);
                var aMove = acts.FindAction("Move", false);
                sb.AppendLine("FindAction(Spider)=" + (aShort != null ? aShort.name + " enabled=" + aShort.enabled + " bindings=" + aShort.bindings.Count : "NULL"));
                sb.AppendLine("FindAction(Player/Spider)=" + (aFull != null ? aFull.name : "NULL"));
                sb.AppendLine("FindAction(Move)=" + (aMove != null ? aMove.name : "NULL"));
                if (aShort != null)
                    foreach (var b in aShort.bindings) sb.AppendLine("  Spider binding: " + b.path);
            }
        }
        catch (Exception e) { sb.AppendLine("Input check EX: " + e.Message); }

        // Ceiling tracks
        var tracks = UnityEngine.Object.FindObjectsByType<SpiderCeilingTrack>(FindObjectsSortMode.None);
        sb.AppendLine("SpiderCeilingTrack count: " + tracks.Length);
        foreach (var t in tracks)
        {
            var box = t.GetComponent<BoxCollider2D>();
            var eff = t.GetComponent<SurfaceEffector2D>();
            sb.AppendLine($"- {t.name} pos={t.transform.position} layer={LayerMask.LayerToName(t.gameObject.layer)} box={(box != null ? box.size.ToString() + " trigger=" + box.isTrigger + " usedByEffector=" + box.usedByEffector : "NULL")} effSpeed={(eff != null ? eff.speed.ToString() : "NULL")}");
        }
        var effs = UnityEngine.Object.FindObjectsByType<SurfaceEffector2D>(FindObjectsSortMode.None);
        sb.AppendLine("SurfaceEffector2D total: " + effs.Length);

        // TorqueDirection
        if (pc != null) sb.AppendLine("TorqueDirection=" + pc.TorqueDirection + " CanControl=" + pc.CanControlPlayer1);

        sb.AppendLine("=== END CHECK ===");
        return sb.ToString();
    }
}
