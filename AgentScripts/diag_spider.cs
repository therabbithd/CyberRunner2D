var go = UnityEngine.GameObject.Find("player");
if (go == null) return "no player";
var s = go.GetComponent("SpiderGravityController");
var pc = go.GetComponent("PlayerController");
var t = s.GetType();
var gp = t.GetProperty("IsGrounded").GetValue(s);
var cp = t.GetProperty("IsCeilinged").GetValue(s);
var gm = t.GetField("groundMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
var cd = t.GetField("checkDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
var cr = t.GetField("checkRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
float gx = go.transform.position.x;
float gy = go.transform.position.y;
var hit = UnityEngine.Physics2D.Raycast(new UnityEngine.Vector2(gx, gy), UnityEngine.Vector2.down, 8f, 1 << 3);
return "pos=" + go.transform.position + " grounded=" + gp + " ceiling=" + cp
    + " groundMask=" + gm + " checkDist=" + cd + " checkRad=" + cr
    + " rayDown3=" + (hit.collider != null ? hit.point.y.ToString() : "none")
    + " pcEnabled=" + ((UnityEngine.MonoBehaviour)pc).enabled;
