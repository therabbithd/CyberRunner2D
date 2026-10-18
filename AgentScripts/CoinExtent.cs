using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoinExtent
{
    public static string All()
    {
        var sb = new StringBuilder();
        foreach (var name in new[] { "Level1", "Level2", "Level3", "Level4" })
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            var player = GameObject.Find("player");
            var finish = GameObject.Find("FinishLine");
            var coins = GameObject.Find("coins");
            float px = player != null ? player.transform.position.x : float.NaN;
            float fx = finish != null ? finish.transform.position.x : float.NaN;
            int n = coins != null ? coins.transform.childCount : 0;
            sb.AppendLine(name + ": playerX=" + px.ToString("F1") + " finishX=" + fx.ToString("F1")
                + " span=" + (fx - px).ToString("F1") + " coins=" + n);
        }
        return sb.ToString();
    }
}
