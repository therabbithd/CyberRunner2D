using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SquareBoard
{
    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            var player = GameObject.Find("player");
            if (player == null) { sb.AppendLine(name + " NO player"); continue; }

            var cap = player.GetComponent<CapsuleCollider2D>();
            var box = player.GetComponent<BoxCollider2D>();

            Vector2 offset = cap != null ? cap.offset : (box != null ? box.offset : new Vector2(0.066147424f, -1.2763247f));
            Vector2 size = cap != null ? cap.size : (box != null ? box.size : new Vector2(3.3277125f, 0.5810779f));
            PhysicsMaterial2D mat = cap != null ? cap.sharedMaterial : (box != null ? box.sharedMaterial : null);
            bool isTrigger = cap != null ? cap.isTrigger : false;
            bool usedByEffector = cap != null ? cap.usedByEffector : false;

            if (cap != null) Object.DestroyImmediate(cap);
            if (box == null) box = player.AddComponent<BoxCollider2D>();
            box.offset = offset;
            box.size = size;
            box.isTrigger = isTrigger;
            box.usedByEffector = usedByEffector;
            box.sharedMaterial = mat;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            sb.AppendLine($"{name} board=BoxCollider2D size={size} offset={offset} trig={isTrigger}");
        }
        return sb.ToString();
    }
}
