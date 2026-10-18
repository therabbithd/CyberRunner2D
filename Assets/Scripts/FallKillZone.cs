using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zona de muerte por caida. Coloca un BoxCollider2D trigger bien por debajo
/// del nivel; cuando el jugador entra (o permanece) en ella se desactiva el
/// control y se recarga la escena. Usa Enter + Stay para que no se escape la
/// ventana de gracia del SpiderGravityController (que ignora los golpes justo
/// tras invertir la gravedad).
/// </summary>
public class FallKillZone : MonoBehaviour
{
    [Tooltip("Retardo antes de recargar la escena, para que se vea la caida.")]
    [SerializeField] private float reloadDelay = 0.6f;

    private bool killed;

    private void OnTriggerEnter2D(Collider2D other) => TryKill(other);
    private void OnTriggerStay2D(Collider2D other) => TryKill(other);

    private void TryKill(Collider2D other)
    {
        if (killed || other == null) return;
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        killed = true;
        player.CanControlPlayer1 = false;
        Invoke(nameof(ReloadScene), reloadDelay);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
