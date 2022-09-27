using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    Rigidbody2D rb;
    CapsuleCollider2D playerCollider;

    [Header("Movement")]
    public float maxSpeed = 15f;
    public float accelleration = 10000f;
    public float jumpForce = 15f;
    public int maxJumps = 2;
    public int currentJumpsLeft;

    [Header("Dash")]
    public float dashForce = 30f;
    public float dashTime = .1f;
    public float dashCooldown = .5f;

    [Header("Gravity")]
    public float drag = 5f;
    public float stopDrag = 15f;
    public float gravityScale = 3f;
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2.5f;
    public float fallMultiplierBuffer = 5f;
    public float fastFallingMultiplier = 2.5f;

    [Header("Ground")]
    public LayerMask groundLayer;

    bool fastFalling;

    bool canDash = true;
    bool isDashing;

    bool grounded;
    float groundedRaycastLength = .15f;

    Vector2 rawInput;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = -1;
        currentJumpsLeft = maxJumps;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update() {
        rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump") && currentJumpsLeft > 0) {
            Jump();
        }

        fastFalling = (rawInput.y < 0 && rb.velocity.y != 0);

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && rawInput.magnitude > 0.1f) {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate() {

        // If we are dashing we don't update our velocity

        if (isDashing) {
            return;
        }

        GroundCheck();

        //

        if (grounded && Mathf.Abs(rawInput.x) < .1f) {
            rb.AddForce(new Vector2(-rb.velocity.x * stopDrag, 0));
        }

        // Drag (For Slow)

        rb.AddForce(new Vector2(-rb.velocity.x * drag, 0));

        // Better gravity & Hold to jump higher

        if (rb.velocity.y < fallMultiplierBuffer) {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        rb.gravityScale = fastFalling ? gravityScale * fastFallingMultiplier : gravityScale;

        // Move with Horizontal Inputs

        rb.velocity += new Vector2(accelleration * rawInput.x * Time.fixedDeltaTime, 0);

        if (rb.velocity.x < -maxSpeed) {
            rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
        }else if(rb.velocity.x > maxSpeed) {
            rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
        }

        print(Mathf.Round(rb.velocity.x));
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
        Vector3 startPos = transform.position;

        bool dashedUp = rawInput.y > 0.3f;
        float gravScale = rb.gravityScale;

        canDash = false;
        isDashing = true;

        if (grounded) {
            rb.velocity = new Vector2(rawInput.x * dashForce, 0);
        }
        else {
            rb.gravityScale = 0;
            rb.velocity = rawInput.normalized * dashForce;
        }
        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        rb.gravityScale = gravScale;
        if (dashedUp) {
            rb.velocity = new Vector2(rb.velocity.x, 10);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}