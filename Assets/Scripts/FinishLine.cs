using UnityEngine;
using UnityEngine.SceneManagement;
public class FinishLine : MonoBehaviour
{
    [SerializeField] private float reloadDelay = 1f; // Delay before reloading the scene after crossing the finish line
    [SerializeField] private ParticleSystem finishParticles; // Particle system for finish line effects
    // Delay before reloading the scene after crossing the finish line
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has crossed the finish line!");
            finishParticles.Play(); // Play the finish line particle effect
            Invoke(nameof(reloadScene), reloadDelay); // Reload the scene after the specified delay
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
