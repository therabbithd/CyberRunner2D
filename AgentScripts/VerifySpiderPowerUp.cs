using System.Text;
using UnityEditor;
using UnityEngine;

public static class VerifySpiderPowerUp
{
    public static string Check()
    {
        var sb = new StringBuilder();
        var data = AssetDatabase.LoadAssetAtPath<PowerUpScriptableObject>("Assets/PowerUpsData/PowerUpSpider.asset");
        if (data == null) return "FAIL: asset not found";
        sb.AppendLine("type=" + data.PowerUpType1 + " time=" + data.TimeLimit1 + " value=" + data.PowerUpValue1);

        var spider = Object.FindAnyObjectByType<SpiderGravityController>();
        if (spider == null) return sb.ToString() + "FAIL: no SpiderGravityController in scene";
        var so = new SerializedObject(spider);
        float imp0 = so.FindProperty("snapImpulse").floatValue;
        float max0 = so.FindProperty("maxSnapSpeed").floatValue;
        spider.AddSnapSpeed(data.PowerUpValue1);
        so.Update();
        float imp1 = so.FindProperty("snapImpulse").floatValue;
        float max1 = so.FindProperty("maxSnapSpeed").floatValue;
        spider.RemoveSnapSpeed(data.PowerUpValue1);
        so.Update();
        float imp2 = so.FindProperty("snapImpulse").floatValue;
        float max2 = so.FindProperty("maxSnapSpeed").floatValue;
        sb.AppendLine($"impulse {imp0} -> +{data.PowerUpValue1} = {imp1} -> revert = {imp2}");
        sb.AppendLine($"maxSnap {max0} -> +{data.PowerUpValue1} = {max1} -> revert = {max2}");
        bool ok = Mathf.Approximately(imp1, imp0 + data.PowerUpValue1)
                  && Mathf.Approximately(max1, max0 + data.PowerUpValue1)
                  && Mathf.Approximately(imp2, imp0)
                  && Mathf.Approximately(max2, max0);
        sb.AppendLine(ok ? "PASS" : "FAIL");
        return sb.ToString();
    }
}
