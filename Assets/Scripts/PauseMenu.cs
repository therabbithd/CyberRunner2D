using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del menu de pausa: se abre/cierra con Escape.
/// La UI vive en el prefab PauseMenu.prefab (editable en el Editor); este
/// script solo referencia el Canvas y lo muestra/oculta. Los botones del
/// prefab llaman a Resume / Restart / QuitToMenu.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private GameObject pauseMenuRoot;

    private bool isPaused;

    void Awake()
    {
        // Por si una escena anterior quedo con el tiempo congelado.
        Time.timeScale = 1f;
        SetPaused(false);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }
}
