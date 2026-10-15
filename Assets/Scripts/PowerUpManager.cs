using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] private PowerUpScriptableObject PowerUp;
    [SerializeField] private ParticleSystem PowerUpParticles; // Particle system for power-up effects
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    PlayerController playerController; // Reference to the PlayerController scrip
    SpriteRenderer powerUpSpriteRenderer; // Reference to the SpriteRenderer component of the player

    float TimeLeft;
    PowerUpScriptableObject activePowerUp; // Power-up currently applied to the player

    private void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>(); // Find the PlayerController in the scene
        powerUpSpriteRenderer = GetComponentInChildren<SpriteRenderer>(); // Get the SpriteRenderer component of the children
        if (PowerUp != null)
        {
            TimeLeft = PowerUp.TimeLimit1;
        }
    }
    private void Update()
    {
        CountDownPowerUpTime();
    }
    private void CountDownPowerUpTime()
    {
        if (powerUpSpriteRenderer == null || powerUpSpriteRenderer.enabled) return;
        if (TimeLeft > 0)
        {
            TimeLeft -= Time.deltaTime; // Decrease the time left for the power-up
        }
        if (TimeLeft <= 0)
        {
            TimeLeft = 0; // Ensure time left doesn't go below zero
            powerUpSpriteRenderer.enabled = true; // Enable the power-up sprite renderer to show it again
            if (activePowerUp != null && playerController != null)
            {
                playerController.RemovePowerUp(activePowerUp); // Revert the temporary boost
                activePowerUp = null;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (PowerUp == null || playerController == null) return;
        if(other.CompareTag("Player") && powerUpSpriteRenderer != null && powerUpSpriteRenderer.enabled )
        {
            powerUpSpriteRenderer.enabled = false;
            if (PowerUpParticles != null) PowerUpParticles.Play(); // Play the power-up particle effect
            playerController.ApplyPowerUp(PowerUp); // Call the ApplyPowerUp method in the PlayerController script
            activePowerUp = PowerUp;
            TimeLeft = PowerUp.TimeLimit1; // Restart the effect duration on each pickup
        }
    }
}
