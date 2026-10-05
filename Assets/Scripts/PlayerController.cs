using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueamount = 1f;
    [SerializeField] private float boostSpeed = 30f;
    [SerializeField] private ParticleSystem SnowParticles; // Particle system for boost effects
    
    private bool CanControlPlayer = true; // Flag to control player input
    float baseSpeed ;
    SurfaceEffector2D surfaceEffector2D;
    InputAction moveAction;
    Vector2 moveInput;
    Rigidbody2D rb;

    public bool CanControlPlayer1 { get => CanControlPlayer; set => CanControlPlayer = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
        surfaceEffector2D = FindAnyObjectByType<SurfaceEffector2D>();
        baseSpeed = surfaceEffector2D.speed; // Store the initial speed of the SurfaceEffector2D
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanControlPlayer) return; // If player control is disabled, exit the Update method
        moveInput = moveAction.ReadValue<Vector2>();
        PlayerTorque();
        BoostPlayer();

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
}
    
