using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueamount = 1f;

    InputAction moveAction;
    Vector2 moveInput;
    Rigidbody2D rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        
        PlayerTorque();

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
}
