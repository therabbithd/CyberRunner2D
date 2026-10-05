using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] private ParticleSystem bloodParticles; // Particle system for blood effects
    PlayerController playerController; // Reference to the PlayerController script
    void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>(); // Find the PlayerController in the scene
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            Debug.Log("Player has crashed!");
            playerController.CanControlPlayer1 = false; // Disable player control
            bloodParticles.Play(); // Play the blood particle effect
            Invoke(nameof(reloadScene), 1f); // Reload the scene after a 1-second delay
        }
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