public static class RefreshNow
{
    public static string Run()
    {
        UnityEditor.AssetDatabase.Refresh();
        var dir = "Assets/AgentScripts";
        return System.IO.File.Exists("Assets/AgentScripts.meta") || System.IO.Directory.Exists(dir)
            ? "pollution-remains"
            : "clean";
    }
}
