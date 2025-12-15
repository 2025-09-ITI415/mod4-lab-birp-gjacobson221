using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D capsuleCollider;

    private Vector2 velocity;
    private float inputAxis;
    private float idleTimer = 0f;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump Physics")]
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        rb.isKinematic = false;
        capsuleCollider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

    private void OnDisable()
    {
        rb.isKinematic = true;
        capsuleCollider.enabled = false;
        velocity = Vector2.zero;
        inputAxis = 0f;
        jumping = false;
    }

    private void Update()
    {
        HorizontalMovement();

        grounded = CastCheck(Vector2.down, 0.1f);

        if (grounded)
        {
            GroundedMovement();
        }

        ApplyGravity();

        if (Mathf.Abs(velocity.x) < 0.1f) // player not moving
            idleTimer += Time.deltaTime;
        else
            idleTimer = 0f; // reset when moving

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetBool("IsJumping", jumping);
        animator.SetFloat("IdleTimer", idleTimer);
        animator.SetBool("Grounded", grounded);

        if (transform.position.y < -20f)
        {
            Die();
        }

    }

    private void Die()
    {
        enabled = false;
        rb.velocity = Vector2.zero;
        SceneManager.LoadScene("game over");
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        position += velocity * Time.fixedDeltaTime;
        rb.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(
            velocity.x,
            inputAxis * moveSpeed,
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(velocity.x) > 0.01f &&
            CastCheck(Vector2.right * Mathf.Sign(velocity.x), 0.05f))
        {
            velocity.x = 0f;
        }

        if (velocity.x > 0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f); 
        }
        else if (velocity.x < -0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f); 
        }

    }

    private void GroundedMovement()
    {
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            jumping = true;
        }
    }

    private void ApplyGravity()
    {
        bool fallingNow = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = fallingNow ? 2f : 1f;

        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int powerUpLayer = LayerMask.NameToLayer("PowerUp");

        if (collision.gameObject.layer == enemyLayer)
        {
            if (HitFromAbove(collision))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != powerUpLayer)
        {
            if (HitFromBelow(collision))
            {
                velocity.y = 0f;
            }
        }
    }

    private bool CastCheck(Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int count = capsuleCollider.Cast(direction, hits, distance);
        return count > 0;
    }

    private bool HitFromAbove(Collision2D col)
    {
        return Vector2.Dot((col.transform.position - transform.position).normalized, Vector2.down) > 0.25f;
    }

    private bool HitFromBelow(Collision2D col)
    {
        return Vector2.Dot((col.transform.position - transform.position).normalized, Vector2.up) > 0.25f;
    }
}
