using UnityEngine;
using UnityEngine.InputSystem; // Import the Input System namespace

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f; // Speed of the player
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private Vector2 moveInput; // Store the movement input
    private Animator animator;
    private float afktimer = 0f;

    [SerializeField] string checkpointID;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the player
        animator = GetComponent<Animator>();
        transform.position = new Vector3(-1.76f, -0.12f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (MapTransitionScenes.IsTransitioning || MapTransition.IsTransitioning)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
        else
            rb.linearVelocity = moveInput * moveSpeed; // Set the velocity of the Rigidbody2D based on input and speed

        if (afktimer >= 5f)
        {
            animator.SetBool("isAFK", true);
        }

        afktimer += Time.deltaTime;

    }

    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isAFK", false);

        if (context.canceled)
        {
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        if (MapTransitionScenes.IsTransitioning || MapTransition.IsTransitioning)
        {
            return;
        }
            
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        afktimer = 0f;
    }
    
    public void ForceStop()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        // zera quaisquer estados pendentes
    }
}
