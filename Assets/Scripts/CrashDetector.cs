using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] private ParticleSystem finishParticles; // Particle system for finish line effects

    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            Debug.Log("Player has crashed!");
            finishParticles.Play(); // Play the finish line particle effect
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