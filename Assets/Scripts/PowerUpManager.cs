using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] private PowerUpScriptableObject PowerUp;
    [SerializeField] private ParticleSystem PowerUpParticles; // Particle system for power-up effects
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    PlayerController playerController; // Reference to the PlayerController scrip
    SpriteRenderer powerUpSpriteRenderer; // Reference to the SpriteRenderer component of the player

    float TimeLeft;

    private void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>(); // Find the PlayerController in the scene
        powerUpSpriteRenderer = GetComponentInChildren<SpriteRenderer>(); // Get the SpriteRenderer component of the children
        TimeLeft = PowerUp.TimeLimit1;
    }
    private void CountDownPowerUpTime()
    {
        if (powerUpSpriteRenderer.enabled == false)
        {
            if (TimeLeft > 0)
            {
                TimeLeft -= Time.deltaTime; // Decrease the time left for the power-up
                if (TimeLeft <= 0)
                {
                    TimeLeft = 0; // Ensure time left doesn't go below zero
                    powerUpSpriteRenderer.enabled = true; // Enable the power-up sprite renderer to show it again
                }
            }
            else
            {
                TimeLeft = 0; // Ensure time left doesn't go below zero
                powerUpSpriteRenderer.enabled = true; // Enable the power-up sprite renderer to show it again
            }
        }
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && powerUpSpriteRenderer.enabled )
        {
            powerUpSpriteRenderer.enabled = false;
            PowerUpParticles.Play(); // Play the power-up particle effect
            playerController.ApplyPowerUp(PowerUp); // Call the ApplyPowerUp method in the PlayerController script
        }
    }
}
