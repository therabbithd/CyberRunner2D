using UnityEngine;

/// <summary>
/// Moneda recogible cyberpunk. Al chocar con el jugador suma <see cref="scoreValue"/>
/// puntos (50 por defecto) al marcador y desaparece. Solo puntua una vez.
/// </summary>
public class Coin : MonoBehaviour
{
    [SerializeField] private int scoreValue = 50;
    [SerializeField] private ParticleSystem collectParticles; // Efecto opcional al recoger
    [SerializeField] private ScoreManager scoreManager;

    bool collected;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        if (scoreManager != null)
            scoreManager.AddScore(scoreValue);

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (collectParticles != null)
        {
            collectParticles.transform.SetParent(null, true);
            collectParticles.Play();
            Destroy(collectParticles.gameObject, collectParticles.main.duration + collectParticles.main.startLifetime.constantMax);
        }

        Destroy(gameObject);
    }
}
