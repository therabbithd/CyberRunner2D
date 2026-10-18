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
            Invoke(nameof(goToNextLevel), reloadDelay); // Avanza al siguiente nivel tras el retardo
        }
    }
    /// <summary>
    /// Avanza al siguiente nivel al cruzar la bandera de meta. Si el nivel actual
    /// es "LevelN", carga "LevelN+1" (si existe en Build Settings). Tras el ultimo
    /// nivel vuelve al selector de niveles.
    /// </summary>
    void goToNextLevel()
    {
        string current = SceneManager.GetActiveScene().name;
        int n;
        if (current.StartsWith("Level") && current.Length > 5 && int.TryParse(current.Substring(5), out n))
        {
            string next = "Level" + (n + 1);
            // Comprueba que el siguiente nivel existe en Build Settings antes de cargarlo.
            if (SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + next + ".unity") >= 0)
            {
                SceneManager.LoadScene(next);
                return;
            }
        }
        SceneManager.LoadScene("LevelSel");
    }
}
