using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildCoins
{
    const string SpritePath = "Assets/SnowSurferAssets/coin_cyberpunk.png";
    const int FloorLayer = 3;

    const float CoinScale = 0.12f;
    const float BaseOffset = 1.45f;
    const float ArcHeight = 2.5f;
    const float CoinSpacing = 1.35f;
    const float SpawnMargin = 4f;
    const float FinishMargin = 7f;
    const float MinGap = 4f;
    const float MaxGap = 9f;
    const float ObstacleClearance = 1.1f;

    static readonly string[] Levels = { "Level1", "Level2", "Level3", "Level4" };

    public static string All()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Levels.Length; i++)
            sb.AppendLine(Build(Levels[i], i + 1));
        return sb.ToString();
    }

    public static string Level1() => Build("Level1", 1);
    public static string Level2() => Build("Level2", 2);
    public static string Level3() => Build("Level3", 3);
    public static string Level4() => Build("Level4", 4);

    static string Build(string sceneName, int seed)
    {
        string path = "Assets/Scenes/" + sceneName + ".unity";
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null)
            return "SKIP " + sceneName + " (missing)";

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null) return "ERROR: coin sprite not found at " + SpritePath;

        var old = GameObject.Find("coins");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        var player = GameObject.Find("player");
        var finish = GameObject.Find("FinishLine");
        if (player == null || finish == null)
            return "ERROR " + sceneName + ": player or FinishLine missing";

        float minX = player.transform.position.x + SpawnMargin;
        float maxX = finish.transform.position.x - FinishMargin;

        var root = new GameObject("coins");
        var rng = new System.Random(1000 + seed);

        float x = minX;
        int placed = 0;
        int groups = 0;
        while (x < maxX)
        {
            int pattern = rng.Next(3); // 0=fila, 1=arco, 2=grupo doble
            float len;
            placed += PlaceGroup(root.transform, sprite, x, pattern, rng, out len);
            groups++;
            x += len + MinGap + (float)rng.NextDouble() * (MaxGap - MinGap);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return sceneName + ": " + placed + " monedas en " + groups + " grupos";
    }

    static int PlaceGroup(Transform parent, Sprite sprite, float startX, int pattern, System.Random rng, out float groupLen)
    {
        int placed = 0;
        float spacing = CoinSpacing;

        if (pattern == 2)
        {
            int rowCount = 3 + rng.Next(2); // 3-4 por fila
            for (int row = 0; row < 2; row++)
            {
                float yOff = BaseOffset + row * 1.15f;
                for (int i = 0; i < rowCount; i++)
                    if (TryPlace(parent, sprite, startX + i * spacing, yOff)) placed++;
            }
            groupLen = (rowCount - 1) * spacing;
        }
        else
        {
            int count = pattern == 1 ? 5 + rng.Next(3) : 4 + rng.Next(3); // arco 5-7, fila 4-6
            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                float yOff = pattern == 1 ? BaseOffset + ArcHeight * Mathf.Sin(t * Mathf.PI) : BaseOffset;
                if (TryPlace(parent, sprite, startX + i * spacing, yOff)) placed++;
            }
            groupLen = (count - 1) * spacing;
        }

        return placed;
    }

    static bool TryPlace(Transform parent, Sprite sprite, float x, float yOffset)
    {
        float groundY = GroundY(x, out bool onObstacle);
        if (float.IsNaN(groundY) || onObstacle) return false;

        float y = groundY + yOffset;

        // Evita colocar monedas pegadas a rocas/obstaculos solidos de la capa Floor.
        if (Physics2D.OverlapCircle(new Vector2(x, y), ObstacleClearance, 1 << FloorLayer) != null)
            return false;

        MakeCoin(parent, sprite, x, y);
        return true;
    }

    static void MakeCoin(Transform parent, Sprite sprite, float x, float y)
    {
        var go = new GameObject("Coin");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * CoinScale;
        go.layer = 0;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sprite.bounds.extents.x; // en espacio local; la escala del transform lo reduce

        var coinType = FindType("Coin");
        if (coinType != null) go.AddComponent(coinType);
    }

    /// <summary>
    /// Devuelve la Y de la superficie de SUELO en x (capa Floor), ignorando el techo.
    /// onObstacle=true si la primera superficie solida es una roca (para no plantar la moneda encima).
    /// </summary>
    static float GroundY(float x, out bool onObstacle)
    {
        onObstacle = false;
        var hits = Physics2D.RaycastAll(new Vector2(x, 30f), Vector2.down, 70f, 1 << FloorLayer);
        if (hits.Length == 0) return float.NaN;
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (IsCeiling(h.collider.transform)) continue;
            string n = h.collider.name;
            onObstacle = n.StartsWith("Rock") || n.StartsWith("CeilRock");
            return h.point.y;
        }
        return float.NaN;
    }

    static bool IsCeiling(Transform t)
    {
        while (t != null)
        {
            if (t.name.StartsWith("Ceil") || t.name == "LevelSpriteShape (2)")
                return true;
            t = t.parent;
        }
        return false;
    }

    static Type FindType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }
}
