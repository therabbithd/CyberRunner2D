using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] private ParticleSystem bloodParticles; // Particle system for blood effects
    PlayerController playerController; // Reference to the PlayerController script
    SpiderGravityController spider; // Para ignorar el vuelo hacia el techo tras invertir
    bool killed; // Evita muertes/recargas duplicadas (crash + zona de muerte + salida por arriba)
    void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>(); // Find the PlayerController in the scene
        spider = GetComponent<SpiderGravityController>(); // Mismo GO que el player
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            // Tras ToggleGravity el jugador viaja a la otra superficie: durante
            // la ventana de gracia el roce con Floor no es un golpe mortal
            // (antes moria atravesando el techo por tocarlo con la cabeza).
            if (spider != null && spider.IsCrashImmune) return;
            Debug.Log("Player has crashed!");
            Kill();
        }
    }

    /// <summary>
    /// Muerte del jugador: quita el control, lanza el efecto de sangre y
    /// recarga la escena. Publico para reutilizarlo desde otras causas de
    /// muerte (p. ej. SpiderGravityController cuando el jugador sale por
    /// arriba al caer hacia arriba con la gravedad invertida).
    /// </summary>
    public void Kill()
    {
        if (killed) return;
        killed = true;
        if (playerController != null)
            playerController.CanControlPlayer1 = false; // Disable player control
        if (bloodParticles != null)
            bloodParticles.Play(); // Play the blood particle effect
        Invoke(nameof(reloadScene), 1f); // Reload the scene after a 1-second delay
    }
    /// <summary>
    /// Reloads the current scene when called. This can be used to reset the game state after the player crosses the finish line or crashes.
    /// </summary>
    void reloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}