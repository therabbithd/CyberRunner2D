using System.Threading.Tasks;
using UnityEngine;

public static class CaptureShot
{
    public static async Task<string> Run()
    {
        var player = GameObject.Find("player");
        var rb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        Time.timeScale = 0f;

        var cam = Camera.main;
        string info = "cam=" + (cam != null ? cam.name : "NULL");
        if (cam != null)
        {
            foreach (var mb in cam.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                if (mb.GetType().Name.Contains("CinemachineBrain")) { mb.enabled = false; info += " brainOff"; }
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(13f, 2.0f, -10f);
        }

        if (player != null)
        {
            player.transform.position = new Vector3(-100f, -100f, 0f);
            if (rb != null) { rb.simulated = false; }
        }

        for (int i = 0; i < 25; i++) await Task.Delay(30);
        UnityEngine.ScreenCapture.CaptureScreenshot("/tmp/opencode/measure2.png");
        for (int i = 0; i < 10; i++) await Task.Delay(30);
        Time.timeScale = 1f;
        return "shot " + info + " W=" + Screen.width + " H=" + Screen.height;
    }
}
