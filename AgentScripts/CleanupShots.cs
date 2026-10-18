public static class CleanupShots
{
    public static string Run()
    {
        var dir = "Assets/AgentScripts";
        if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
        UnityEditor.AssetDatabase.Refresh();
        return System.IO.Directory.Exists(dir) ? "still-there" : "deleted";
    }
}
