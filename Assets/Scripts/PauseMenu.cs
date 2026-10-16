using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu de pausa: se abre/cierra con Escape.
/// Construye su propio Canvas en runtime, no hay que cablear nada en el Inspector.
/// Botones: Continuar, Reiniciar, Menu principal.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";

    private UnityEngine.Canvas pauseCanvas;
    private bool isPaused;

    void Awake()
    {
        // Por si una escena anterior quedo con el tiempo congelado.
        Time.timeScale = 1f;
        BuildUI();
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
        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        pauseCanvas = canvasGO.AddComponent<UnityEngine.Canvas>();
        pauseCanvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Fondo oscuro semitransparente a pantalla completa (bloquea clics al juego).
        GameObject dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        var dimRect = dimGO.AddComponent<UnityEngine.RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        var dimImg = dimGO.AddComponent<UnityEngine.UI.Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.6f);

        // Panel centrado.
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<UnityEngine.RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 560f);
        panelRect.anchoredPosition = Vector2.zero;
        var panelImg = panelGO.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);
        var layout = panelGO.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 36, 36);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddTitle(panelGO.transform, "PAUSA");
        AddButton(panelGO.transform, "Continuar", Resume);
        AddButton(panelGO.transform, "Reiniciar", Restart);
        AddButton(panelGO.transform, "Menu principal", QuitToMenu);

        canvasGO.SetActive(false);
    }

    private void AddTitle(Transform parent, string text)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 64f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var le = go.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 110f;
    }

    private void AddButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(text);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.45f, 0.9f, 1f);
        var btn = go.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(onClick);

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelRect = labelGO.AddComponent<UnityEngine.RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 34f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        var le = go.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 80f;
    }

    private void EnsureEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
            return;
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }
}
