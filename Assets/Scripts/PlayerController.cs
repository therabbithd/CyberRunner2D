using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueamount = 1f;
    [Tooltip("Grados/segundo de giro con A/D o flechas cuando torqueamount = 1. Los power-ups de Torque lo escalan.")]
    [SerializeField] private float rotationSpeed = 360f;
    [Tooltip("Grados/segundo^2 de aceleracion del giro manual. Evita que el personaje gire de golpe.")]
    [SerializeField] private float rotationAcceleration = 3600f;
    [SerializeField] private float boostSpeed = 30f;
    [SerializeField] private ScoreManager scoreManager; // Reference to the ScoreManager component
    [SerializeField] private SpriteRenderer[] characterSprites; // Reference to the SpriteRenderer component for character sprites
    private int ActivePowerUpCount = 0; // Counter for active power-ups
    private bool CanControlPlayer = true; // Flag to control player input
    private int characterIndex = 1; // Index of the selected character

    float baseSpeed ;
    float baseTorqueAmount;

    SurfaceEffector2D surfaceEffector2D;
    SurfaceEffector2D[] allEffectors;
    InputAction moveAction;
    SpiderGravityController spider;
    Vector2 moveInput;
    Rigidbody2D rb;
    int flipCount;
    float previousRotation;
    float totalRotation;
    float playerAngularVelocity;

    public bool CanControlPlayer1 { get => CanControlPlayer; set => CanControlPlayer = value; }

    /// <summary>
    /// SpiderGravityController lo pone a -1 cuando la gravedad esta invertida
    /// para que A/D no se sientan espejados en el techo. 1 = normal.
    /// </summary>
    public int TorqueDirection { get; set; } = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 1); // Retrieve the selected character index from PlayerPrefs (1-based: 1=Dino, 2=Frog)
        ApplySelectedCharacter();
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
        // El giro lo controla SOLO el input (A/D o flechas). Congelamos la
        // rotacion fisica para que ningun obstaculo pueda girar al personaje.
        rb.freezeRotation = true;
        rb.angularVelocity = 0f;
        allEffectors = FindObjectsByType<SurfaceEffector2D>(); // incluye pista de techo si existe
        surfaceEffector2D = FindGroundEffector(allEffectors);
        spider = GetComponent<SpiderGravityController>();
        // Magnitud SIEMPRE positiva de la pista de SUELO. Antes se tomaba el signo de
        // FindAnyObjectByType, que podia devolver el techo (speed -10): baseSpeed quedaba
        // en -10 y BoostPlayer ponia el suelo a -10, con lo que el jugador se frenaba solo.
        baseSpeed = surfaceEffector2D != null ? Mathf.Abs(surfaceEffector2D.speed) : 10f;
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
        BoostPlayer();
        CalculateFlips();

    }

    // El giro se aplica en FixedUpdate (junto a la fisica) y NO con AddTorque:
    // al tener freezeRotation activo, ningun obstaculo puede impartir spin.
    void FixedUpdate()
    {
        ApplyPlayerRotation();
    }

    /// <summary>
    /// Calculates the number of flips the player has performed based on their rotation.
    /// </summary>
    private void CalculateFlips()
    {
        float currentRotation = transform.rotation.eulerAngles.z;
        // Ignora el salto brusco del snap de gravedad para no regalar un flip.
        if (spider != null && spider.ConsumeSuppressFlipCount())
        {
            previousRotation = currentRotation;
            totalRotation = 0f;
            return;
        }
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation);
        if (Math.Abs(totalRotation) > 340 || Math.Abs(totalRotation) < -340)
        {
            flipCount++;
            Debug.Log("Flips: " + flipCount);
            totalRotation = 0; // Reset the total rotation after a flip
            scoreManager.AddScore(100); // Cada voltereta suma 100 puntos (ScoreManager acumula; las monedas suman aparte)
        }
        previousRotation = currentRotation;
    }

    /// <summary>
    /// Rota al jugador SOLO segun la entrada horizontal (A/D o flechas).
    /// Como la rotacion fisica esta congelada, los choques con obstaculos no
    /// aportan giro: rb.angularVelocity se mantiene a cero y el angulo lo
    /// fijamos manualmente con MoveRotation.
    /// </summary>
    void ApplyPlayerRotation()
    {
        if (rb == null) return;

        if (!CanControlPlayer)
        {
            playerAngularVelocity = 0f;
            rb.angularVelocity = 0f;
            return;
        }

        float dir = 0f;
        if (moveInput.x > 0f) dir = -1f;
        else if (moveInput.x < 0f) dir = 1f;
        dir *= torqueamount * TorqueDirection;

        float target = dir * rotationSpeed;
        playerAngularVelocity = Mathf.MoveTowards(playerAngularVelocity, target, rotationAcceleration * Time.fixedDeltaTime);

        if (playerAngularVelocity != 0f)
            rb.MoveRotation(rb.rotation + playerAngularVelocity * Time.fixedDeltaTime);

        // Refuerzo: la fisica nunca debe acumular spin propio.
        rb.angularVelocity = 0f;
    }
    /// <summary>
    /// Devuelve la superficie de SUELO (la que empuja hacia +X, speed positiva).
    /// Evita que FindAnyObjectByType devuelva el techo (speed -10) y deje baseSpeed
    /// en negativo, lo que hacia que el jugador se frenara solo en algunas partidas.
    /// </summary>
    private static SurfaceEffector2D FindGroundEffector(SurfaceEffector2D[] effectors)
    {
        if (effectors == null) return null;
        SurfaceEffector2D fallback = null;
        foreach (var eff in effectors)
        {
            if (eff == null) continue;
            if (fallback == null) fallback = eff;
            if (eff.speed > 0f) return eff;
        }
        return fallback;
    }

    void BoostPlayer()
    {
        // Increase the speed when the arraw up is pressed
        // Se aplica a TODOS los effectors (suelo + techo) para que el boost funcione boca abajo.
        // Cada effector conserva su signo: el techo necesita speed negativa para empujar
        // hacia +X (la normal apunta hacia abajo e invierte la tangente del SurfaceEffector).
        // Sin esto, poner el techo a -10 duraba un frame: el boost lo reescribia a +30
        // y el jugador iba marcha atras en el techo.
        // Se usa Mathf.Abs por seguridad: baseSpeed debe ser siempre la magnitud (positiva).
        float baseTarget = moveInput.y > 0 ? boostSpeed : Mathf.Abs(baseSpeed);
        if (allEffectors != null && allEffectors.Length > 0)
        {
            foreach (var eff in allEffectors)
            {
                if (eff == null) continue;
                float dir = eff.speed < 0f ? -1f : 1f;
                eff.speed = baseTarget * dir;
            }
        }
        else if (surfaceEffector2D != null)
        {
            float dir = surfaceEffector2D.speed < 0f ? -1f : 1f;
            surfaceEffector2D.speed = baseTarget * dir;
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
        else if (powerUp.PowerUpType1 == "Spider")
        {
            // Sube la velocidad del impulso al entrar en modo arana (snap al techo).
            if (spider != null) spider.AddSnapSpeed(powerUp.PowerUpValue1);
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
        else if (powerUp.PowerUpType1 == "Spider")
        {
            if (spider != null) spider.RemoveSnapSpeed(powerUp.PowerUpValue1);
        }
    }
}
    
