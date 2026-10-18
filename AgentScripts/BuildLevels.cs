using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public static class BuildLevels
{
    const string BaseScene = "Assets/Scenes/Level1.unity";
    const string RockSpritePath = "Assets/SnowSurferAssets/rock_cyberpunk.png";
    const int FloorLayer = 3;
    const float FloorBottomY = -12f;
    const float CeilingThickness = 3f;
    const float TopStep = 1f;
    const float SpawnX = -11f;

    class Cfg
    {
        public int n;
        public float startX = -14f;
        public float endX;
        public float speed;
        public float amp;
        public float ceilY;
        public int rockCount;
        public int ceilRockCount;
        public readonly List<Vector2> floorSegs = new List<Vector2>();
        public readonly List<Vector2> ceilSegs = new List<Vector2>();
    }

    public static string Level2() => Build(MakeCfg(2));
    public static string Level3() => Build(MakeCfg(3));
    public static string Level4() => Build(MakeCfg(4));
    public static string All() => Level2() + "\n" + Level3() + "\n" + Level4();

    static Cfg MakeCfg(int n)
    {
        var c = new Cfg { n = n };
        switch (n)
        {
            case 2:
                c.endX = 110f; c.speed = 12f; c.amp = 1.00f; c.ceilY = 15f;
                c.rockCount = 14; c.ceilRockCount = 0;
                c.floorSegs.Add(new Vector2(-14f, 40f));
                c.floorSegs.Add(new Vector2(48f, 78f));
                c.floorSegs.Add(new Vector2(86f, 110f));
                c.ceilSegs.Add(new Vector2(-14f, 110f));
                break;
            case 3:
                c.endX = 130f; c.speed = 14f; c.amp = 1.15f; c.ceilY = 15f;
                c.rockCount = 22; c.ceilRockCount = 4;
                c.floorSegs.Add(new Vector2(-14f, 32f));
                c.floorSegs.Add(new Vector2(40f, 64f));
                c.floorSegs.Add(new Vector2(72f, 98f));
                c.floorSegs.Add(new Vector2(106f, 130f));
                c.ceilSegs.Add(new Vector2(-14f, 60f));
                c.ceilSegs.Add(new Vector2(70f, 130f));
                break;
            case 4:
                c.endX = 150f; c.speed = 16f; c.amp = 1.30f; c.ceilY = 15f;
                c.rockCount = 30; c.ceilRockCount = 7;
                c.floorSegs.Add(new Vector2(-14f, 28f));
                c.floorSegs.Add(new Vector2(36f, 58f));
                c.floorSegs.Add(new Vector2(66f, 88f));
                c.floorSegs.Add(new Vector2(96f, 118f));
                c.floorSegs.Add(new Vector2(126f, 150f));
                c.ceilSegs.Add(new Vector2(-14f, 45f));
                c.ceilSegs.Add(new Vector2(55f, 100f));
                c.ceilSegs.Add(new Vector2(110f, 150f));
                break;
        }
        return c;
    }

    static float Profile(float x, float amp)
    {
        return 2.5f + amp * (1.6f * Mathf.Sin((x + 8f) * 0.06f) + 2.2f * Mathf.Sin(x * 0.017f + 0.6f));
    }

    static string Build(Cfg c)
    {
        var log = new StringBuilder();
        string path = "Assets/Scenes/Level" + c.n + ".unity";
        try
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
            if (!AssetDatabase.CopyAsset(BaseScene, path))
                return "ERR: could not copy base scene to " + path;
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            log.AppendLine("opened " + path);

            // La escena base puede traer monedas copiadas de Level1; se regeneran luego con BuildCoins.
            var oldCoins = GameObject.Find("coins");
            if (oldCoins != null) UnityEngine.Object.DestroyImmediate(oldCoins);

            var floor = GameObject.Find("LevelSpriteShape (1)");
            var ceil = GameObject.Find("LevelSpriteShape (2)");
            if (floor == null || ceil == null)
                return "ERR: base shapes not found in " + path;
            log.AppendLine("floor=" + floor.name + " ceil=" + ceil.name);

            var sct = ceil.GetComponent("SpiderCeilingTrack");
            if (sct != null) UnityEngine.Object.DestroyImmediate(sct);

            var floorObjs = new List<GameObject>();
            for (int i = 0; i < c.floorSegs.Count; i++)
            {
                GameObject go = i == 0 ? floor : UnityEngine.Object.Instantiate(floor, floor.transform.parent);
                go.name = "Floor " + i;
                Normalize(go);
                BuildFloor(go, c.floorSegs[i].x, c.floorSegs[i].y, c.amp);
                SetSpeed(go, c.speed);
                floorObjs.Add(go);
            }
            log.AppendLine("floor segments: " + floorObjs.Count);

            var ceilObjs = new List<GameObject>();
            for (int i = 0; i < c.ceilSegs.Count; i++)
            {
                GameObject go = i == 0 ? ceil : UnityEngine.Object.Instantiate(ceil, ceil.transform.parent);
                go.name = "Ceiling " + i;
                Normalize(go);
                BuildCeiling(go, c.ceilSegs[i].x, c.ceilSegs[i].y, c.ceilY);
                SetSpeed(go, -c.speed);
                ceilObjs.Add(go);
            }
            log.AppendLine("ceiling segments: " + ceilObjs.Count);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RockSpritePath);
            int rocks = BuildRocks(c, sprite);
            log.AppendLine("rocks: " + rocks);

            PlacePlayer(c);
            PlaceFinish(c);
            BuildKillZone(c);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            log.AppendLine("saved " + path);
            return log.ToString();
        }
        catch (Exception e)
        {
            return "EXCEPTION building Level" + c.n + ": " + e.Message + "\n" + e.StackTrace + "\n" + log;
        }
    }

    static void Normalize(GameObject go)
    {
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        go.transform.localScale = Vector3.one;
        go.layer = FloorLayer;
    }

    static void BuildFloor(GameObject go, float x0, float x1, float amp)
    {
        var ssc = go.GetComponent<SpriteShapeController>();
        var pts = new List<Vector3>();
        int topCount = Mathf.Max(2, Mathf.RoundToInt((x1 - x0) / TopStep) + 1);
        for (int i = 0; i < topCount; i++)
        {
            float t = (float)i / (topCount - 1);
            float x = Mathf.Lerp(x0, x1, t);
            pts.Add(new Vector3(x, Profile(x, amp), 0f));
        }
        pts.Add(new Vector3(x1, FloorBottomY, 0f));
        pts.Add(new Vector3(x0, FloorBottomY, 0f));
        ApplyPolygon(ssc, pts, topCount);
    }

    static void BuildCeiling(GameObject go, float x0, float x1, float y)
    {
        var ssc = go.GetComponent<SpriteShapeController>();
        var pts = new List<Vector3>
        {
            new Vector3(x0, y, 0f),
            new Vector3(x1, y, 0f),
            new Vector3(x1, y + CeilingThickness, 0f),
            new Vector3(x0, y + CeilingThickness, 0f),
        };
        ApplyPolygon(ssc, pts, 0);
    }

    static void ApplyPolygon(SpriteShapeController ssc, List<Vector3> pts, int smoothTopCount)
    {
        var sp = ssc.spline;
        sp.isOpenEnded = false;
        sp.Clear();
        int n = pts.Count;
        for (int i = 0; i < n; i++) sp.InsertPointAt(i, pts[i]);
        for (int i = 0; i < n; i++)
        {
            sp.SetHeight(i, 1f);
            sp.SetSpriteIndex(i, 0);
            sp.SetCorner(i, false);
        }
        for (int i = 0; i < n; i++)
        {
            if (i < smoothTopCount && smoothTopCount > 1)
            {
                sp.SetTangentMode(i, ShapeTangentMode.Continuous);
                Vector3 prev = pts[Mathf.Max(0, i - 1)];
                Vector3 next = pts[Mathf.Min(smoothTopCount - 1, i + 1)];
                Vector3 f = (next - prev) * 0.33f;
                sp.SetLeftTangent(i, -f);
                sp.SetRightTangent(i, f);
            }
            else
            {
                sp.SetTangentMode(i, ShapeTangentMode.Linear);
                sp.SetLeftTangent(i, Vector3.zero);
                sp.SetRightTangent(i, Vector3.zero);
            }
        }
        ssc.RefreshSpriteShape();
        ssc.BakeCollider();
        if (ssc.edgeCollider != null)
        {
            // BakeCollider() reutiliza el segmento cacheado del Shape Profile base y
            // deja el EdgeCollider con la geometria vieja (el techo seguia en y=-5..-11
            // del Level1), por eso el jugador atravesaba el techo en los niveles nuevos.
            // Forzamos los puntos del poligono para garantizar la colision correcta.
            if (smoothTopCount == 0)
            {
                var cp = new Vector2[pts.Count + 1];
                for (int i = 0; i < pts.Count; i++) cp[i] = new Vector2(pts[i].x, pts[i].y);
                cp[pts.Count] = cp[0]; // cerrar el loop
                ssc.edgeCollider.points = cp;
                // Sincroniza la cache interna del SpriteShape para que un futuro
                // rebake no vuelva a aplicar el segmento viejo del Shape Profile base.
                var so = new SerializedObject(ssc);
                var seg = so.FindProperty("m_ColliderSegment");
                if (seg != null && seg.isArray)
                {
                    seg.arraySize = cp.Length;
                    for (int i = 0; i < cp.Length; i++)
                        seg.GetArrayElementAtIndex(i).vector2Value = cp[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            ssc.edgeCollider.usedByEffector = true;
            ssc.edgeCollider.isTrigger = false;
        }
    }

    static void SetSpeed(GameObject go, float speed)
    {
        var eff = go.GetComponent<SurfaceEffector2D>();
        if (eff != null) eff.speed = speed;
    }

    static int BuildRocks(Cfg c, Sprite sprite)
    {
        if (sprite == null) return -1;
        var old = GameObject.Find("rocks");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject("rocks");

        float total = c.endX - c.startX;
        int idx = 0;
        int placed = 0;
        foreach (var seg in c.floorSegs)
        {
            float len = seg.y - seg.x;
            int k = Mathf.Max(1, Mathf.RoundToInt(c.rockCount * len / total));
            for (int j = 0; j < k; j++)
            {
                float t = (j + 0.5f) / k;
                float x = Mathf.Lerp(seg.x + 3f, seg.y - 3f, t);
                float scale = (idx % 2 == 0) ? 0.14f : 0.12f;
                float height = sprite.bounds.size.y * scale;
                idx++;
                if (x < SpawnX + 5f || x > c.endX - 8f) continue;
                float groundY = Profile(x, c.amp);
                MakeRock(root.transform, "Rock (" + (++placed) + ")", sprite, x, groundY + height * 0.5f - 0.15f,
                    scale, placed % 2 == 1, false);
            }
        }

        int ceilPlaced = 0;
        if (c.ceilRockCount > 0 && c.ceilSegs.Count > 0)
        {
            float ceilTotal = 0f;
            foreach (var s in c.ceilSegs) ceilTotal += s.y - s.x;
            foreach (var seg in c.ceilSegs)
            {
                float len = seg.y - seg.x;
                int k = Mathf.Max(1, Mathf.RoundToInt(c.ceilRockCount * len / ceilTotal));
                for (int j = 0; j < k; j++)
                {
                    float t = (j + 0.5f) / k;
                    float x = Mathf.Lerp(seg.x + 4f, seg.y - 4f, t);
                    float scale = 0.12f;
                    float height = sprite.bounds.size.y * scale;
                    MakeRock(root.transform, "CeilRock (" + (++ceilPlaced) + ")", sprite, x,
                        c.ceilY - height * 0.5f + 0.15f, scale, ceilPlaced % 2 == 1, true);
                }
            }
        }
        return placed + ceilPlaced;
    }

    static void MakeRock(Transform parent, string name, Sprite sprite, float x, float y, float scale, bool flipX, bool flipY)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        go.layer = FloorLayer;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 0;
        sr.flipX = flipX;
        sr.flipY = flipY;

        var mat = GetOrCreateRockMaterial();
        var poly = go.AddComponent<PolygonCollider2D>();
        if (poly.pathCount == 0)
        {
            UnityEngine.Object.DestroyImmediate(poly);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = sprite.bounds.size;
            box.sharedMaterial = mat;
        }
        else
        {
            poly.sharedMaterial = mat;
        }
    }

    static PhysicsMaterial2D GetOrCreateRockMaterial()
    {
        const string path = "Assets/SnowSurferAssets/RockFrictionless.physicsMaterial2D";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("RockFrictionless");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0f;
        mat.bounciness = 0f;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void PlacePlayer(Cfg c)
    {
        var player = GameObject.Find("player");
        if (player == null) return;
        float x = SpawnX;
        float y = Profile(x, c.amp) + 3f;
        player.transform.SetPositionAndRotation(new Vector3(x, y, 0f), Quaternion.identity);

        var spider = player.GetComponent("SpiderGravityController");
        if (spider != null)
        {
            var so = new SerializedObject(spider);
            var r = so.FindProperty("checkRadius");
            if (r != null) r.floatValue = 0.35f;
            var d = so.FindProperty("checkDistance");
            if (d != null) d.floatValue = 1.75f;
            so.ApplyModifiedProperties();
        }
    }

    static void PlaceFinish(Cfg c)
    {
        var finish = GameObject.Find("FinishLine");
        if (finish == null) return;
        float x = c.endX - 6f;
        float y = Profile(x, c.amp);
        finish.transform.position = new Vector3(x, y, 0f);
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

    static void BuildKillZone(Cfg c)
    {
        var old = GameObject.Find("KillZone");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        var go = new GameObject("KillZone");
        float width = (c.endX - c.startX) + 60f;
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(width, 50f);
        float centerX = (c.startX + c.endX) * 0.5f;
        go.transform.position = new Vector3(centerX, -10f - 25f, 0f);
        var killType = FindType("FallKillZone");
        if (killType != null) go.AddComponent(killType);
    }
}
