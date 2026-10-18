using System.Linq;
using System.Reflection;
using System.Text;

public static class Probe
{
    public static string All()
    {
        var sb = new StringBuilder();
        var types = new[]
        {
            typeof(UnityEngine.U2D.Spline),
            typeof(UnityEngine.U2D.SpriteShapeController),
            typeof(UnityEngine.U2D.ShapeTangentMode),
        };
        foreach (var t in types)
        {
            sb.AppendLine("### " + t.FullName + (t.IsEnum ? " (enum)" : ""));
            if (t.IsEnum)
            {
                foreach (var n in System.Enum.GetNames(t)) sb.AppendLine("  " + n + " = " + (int)System.Enum.Parse(t, n));
                continue;
            }
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var p in t.GetProperties(flags).OrderBy(p => p.Name))
                sb.AppendLine("  prop " + p.PropertyType.Name + " " + p.Name + " {" + (p.CanRead ? "get;" : "") + (p.CanWrite ? "set;" : "") + "}");
            foreach (var m in t.GetMethods(flags).Where(m => !m.IsSpecialName).OrderBy(m => m.Name))
                sb.AppendLine("  " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
        }
        return sb.ToString();
    }
}
