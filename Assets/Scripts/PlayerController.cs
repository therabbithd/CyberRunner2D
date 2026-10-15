using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueamount = 1f;
    [SerializeField] private float boostSpeed = 30f;
    [SerializeField] private ParticleSystem SnowParticles; // Particle system for boost effects
    [SerializeField] private ScoreManager scoreManager; // Reference to the ScoreManager component
    [SerializeField] private SpriteRenderer[] characterSprites; // Reference to the SpriteRenderer component for character sprites
    private int ActivePowerUpCount = 0; // Counter for active power-ups
    private bool CanControlPlayer = true; // Flag to control player input
    private int characterIndex = 1; // Index of the selected character

    float baseSpeed ;
    float baseTorqueAmount;

    SurfaceEffector2D surfaceEffector2D;
    InputAction moveAction;
    Vector2 moveInput;
    Rigidbody2D rb;
    int flipCount;
    float previousRotation;
    float totalRotation;

    public bool CanControlPlayer1 { get => CanControlPlayer; set => CanControlPlayer = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 1); // Retrieve the selected character index from PlayerPrefs (1-based: 1=Dino, 2=Frog)
        ApplySelectedCharacter();
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
        surfaceEffector2D = FindAnyObjectByType<SurfaceEffector2D>();
        baseSpeed = surfaceEffector2D.speed; // Store the initial speed of the SurfaceEffector2D
        baseTorqueAmount = torqueamount; // Store the initial torque so Torque power-ups can be reverted
    }

    /// <summary>
    /// Activates only the selected character GameObject and deactivates the rest.
    /// characterSprites order must match the menu: [0]=Dino (id 1), [1]=Frog (id 2).
    /// </summary>
    private void ApplySelectedCharacter()
    {
        if (characterSprites == null || characterSprites.Length == 0)
        {
            Debug.LogWarning("PlayerController: characterSprites is not assigned, keeping default visuals.");
            return;
        }
        int clampedIndex = Mathf.Clamp(characterIndex, 1, characterSprites.Length);
        if (clampedIndex != characterIndex)
        {
            Debug.LogWarning($"PlayerController: SelectedCharacter {characterIndex} out of range, clamped to {clampedIndex}.");
            characterIndex = clampedIndex;
        }
        for (int i = 0; i < characterSprites.Length; i++)
        {
            if (characterSprites[i] == null) continue;
            characterSprites[i].gameObject.SetActive(i == characterIndex - 1);
        }
        Debug.Log($"PlayerController: character {characterIndex} activated.");
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanControlPlayer) return; // If player control is disabled, exit the Update method
        moveInput = moveAction.ReadValue<Vector2>();
        PlayerTorque();
        BoostPlayer();
        CalculateFlips();

    }

    /// <summary>
    /// Calculates the number of flips the player has performed based on their rotation.
    /// </summary>
    private void CalculateFlips()
    {
        float currentRotation = transform.rotation.eulerAngles.z;
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation);
        if (Math.Abs(totalRotation) > 340 || Math.Abs(totalRotation) < -340)
        {
            flipCount++;
            Debug.Log("Flips: " + flipCount);
            totalRotation = 0; // Reset the total rotation after a flip
            scoreManager.AddScore(flipCount * 100); // Update the score in the ScoreManager
        }
        previousRotation = currentRotation;
    }

    /// <summary>
    /// Applies torque to the player based on the horizontal input from the move action.
    /// </summary>
    void PlayerTorque(){
        if (moveInput.x > 0)
        {
            rb.AddTorque(-torqueamount);
        }
        else if (moveInput.x < 0)
        {
            rb.AddTorque(torqueamount);
        }
    }
    void BoostPlayer()
    {
        // Increase the speed when the arraw up is pressed
        if (moveInput.y > 0)
        {
            surfaceEffector2D.speed = boostSpeed;
            ; // Call the collision method to trigger the boost particle effect
            // Play the boost particle effect
        }
        else
        {
            surfaceEffector2D.speed = baseSpeed;
           // Stop the boost particle effect when not boosting
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        int LayerIndex = LayerMask.NameToLayer("Floor");
        if(collision.gameObject.layer == LayerIndex)
        {
            SnowParticles.Play(); // Play the boost particle effect when colliding with the floor
        }
        
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        int LayerIndex = LayerMask.NameToLayer("Floor");
        if(collision.gameObject.layer == LayerIndex)
        {
            SnowParticles.Stop(); // Stop the boost particle effect when exiting collision with the floor
        }
    }
    public void ApplyPowerUp(PowerUpScriptableObject powerUp)
    {
       if (powerUp == null) return;
       ActivePowerUpCount++;
       if (powerUp.PowerUpType1 == "Speed")
        {
            baseSpeed += powerUp.PowerUpValue1; // Increase the base speed by the power-up value
            boostSpeed += powerUp.PowerUpValue1; // Increase the boost speed by the power-up value
        }
        else if (powerUp.PowerUpType1 == "Torque")
        {
            torqueamount += powerUp.PowerUpValue1; // Increase rotation responsiveness
        }
    }
    public void RemovePowerUp(PowerUpScriptableObject powerUp)
    {
        if (powerUp == null) return;
        ActivePowerUpCount = Mathf.Max(0, ActivePowerUpCount - 1);
        if (powerUp.PowerUpType1 == "Speed")
        {
            baseSpeed -= powerUp.PowerUpValue1; // Decrease the base speed by the power-up value
            boostSpeed -= powerUp.PowerUpValue1; // Decrease the boost speed by the power-up value
        }
        else if (powerUp.PowerUpType1 == "Torque")
        {
            torqueamount = Mathf.Max(baseTorqueAmount, torqueamount - powerUp.PowerUpValue1);
        }
    }
}
    
