using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VerifyCoins
{
    static readonly string[] Levels = { "Level1", "Level2", "Level3", "Level4" };

    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in Levels)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            var root = GameObject.Find("coins");
            int count = root != null ? root.transform.childCount : 0;
            int withScript = 0, triggers = 0;

            if (root != null)
            {
                foreach (Transform c in root.transform)
                {
                    if (c.GetComponent("Coin") != null) withScript++;
                    var col = c.GetComponent<Collider2D>();
                    if (col != null && col.isTrigger) triggers++;
                }
            }
            sb.AppendLine(name + ": monedas=" + count + " conCoin=" + withScript + " triggers=" + triggers);
        }
        return sb.ToString();
    }
}
