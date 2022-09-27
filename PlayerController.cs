using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    Rigidbody2D rb;
    CapsuleCollider2D playerCollider;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 10f;
    public float dashForce = 5f;
    public int maxJumps = 2;
    public int currentJumpsLeft;

    [Header("DashTimes")]
    public float dashTime = 0.5f;
    public float dashCooldown = 3f;
    
    [Header("Smoothing/Gravity")]
    public float gravityScale = 2f;
    public float moveSmoothTime = 0.3f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float inAirSmoothing = .5f;
    public float fallMultiplierBuffer = 1f;

    [Header("Ground")]
    public LayerMask groundLayer;

    bool fastFalling;

    bool canDash = true;
    bool isDashing;

    bool grounded;
    float groundedRaycastLength = .3f;

    float targetInput;
    private float currentInput;
    private float currentInputVelocity;

    private float currentSpeed;
    private float currentSpeedChange;

    // Start is called before the first frame update
    void Start() {
        currentJumpsLeft = maxJumps;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update() {
        targetInput = Input.GetAxisRaw("Horizontal");
        currentInput = Mathf.SmoothDamp(currentInput, targetInput, ref currentInputVelocity, moveSmoothTime);

        if (Input.GetButtonDown("Jump") && currentJumpsLeft > 0) {
            Jump();
        }

        if (Input.GetAxisRaw("Vertical") < 0 && rb.velocity.y != 0) {
            fastFalling = true;
        }
        else {
            fastFalling = false;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate() {

        if (isDashing) {
            return;
        }

        GroundCheck();

        // Better gravity

        if (rb.velocity.y < fallMultiplierBuffer) {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (fastFalling) {
            rb.gravityScale = gravityScale * 2.5f;
        }
        else {
            rb.gravityScale = gravityScale;
        }

        float targetVelocity = currentInput * speed * Time.fixedDeltaTime;
        print(rb.velocity.y);
        // Set player velocity
        if (grounded) {
            currentSpeed = targetVelocity;
        }
        else {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetVelocity, ref currentSpeedChange, inAirSmoothing);
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    void GroundCheck() {
        //Ground Check
        float margin = 0.03f;

        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(playerCollider.bounds.min.x + margin, transform.position.y), Vector2.down, groundedRaycastLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(playerCollider.bounds.max.x - margin, transform.position.y), Vector2.down, groundedRaycastLength, groundLayer);

        if ((hitLeft.collider != null || hitRight.collider != null)) {
            if (!grounded) {
                currentJumpsLeft = maxJumps;
            }
            grounded = true;
        }
        else {
            grounded = false;
        }

        Debug.DrawLine(new Vector2(playerCollider.bounds.min.x + margin, transform.position.y), new Vector2(playerCollider.bounds.min.x + margin, transform.position.y - groundedRaycastLength), Color.red);
        Debug.DrawLine(new Vector2(playerCollider.bounds.max.x - margin, transform.position.y), new Vector2(playerCollider.bounds.max.x - margin, transform.position.y - groundedRaycastLength), Color.red);

    }

    void Jump() {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        currentJumpsLeft--;
        if (isDashing) {
            print("BANG");
        }
    }

    IEnumerator Dash() {
        canDash = false;
        isDashing = true;
        float gravScale = rb.gravityScale;
        rb.gravityScale = 0;

        if(targetInput < 0) {
            rb.velocity = new Vector2(-dashForce, 0);
            currentSpeed = -dashForce;
        }
        else if(targetInput > 0) {
            rb.velocity = new Vector2(dashForce, 0);
            currentSpeed = dashForce;
        }

        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravScale;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
