using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

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

    [Header("Wall")]
    public float wallSlidingSpeed = -80f;

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


    Rigidbody2D rb;
    CapsuleCollider2D playerCollider;
    TrailRenderer tr;
    Animator animator;

    float groundedRaycastLength = .15f;
    float wallRaycastLength = .04f;

    bool fastFalling;
    bool grounded;
    bool onLeftWall;
    bool onRightWall;
    bool wallJumping;

    bool isDashing;
    bool canDash = true;

    public bool canControll = true;


    Vector2 rawInput;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = -1;
        currentJumpsLeft = maxJumps;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        tr = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if (canControll) {
            GetInput();
        }

        if (rawInput.x < 0) {
            transform.localScale = new Vector3(-1f, 1, 1);
        }
        else if (rawInput.x > 0) {
            transform.localScale = new Vector3(1f, 1, 1);
        }

        if (rb.velocity.magnitude < 0.6f) {
            animator.SetBool("isRunning", false);
        }
        else {
            animator.SetBool("isRunning", true);
        }

        animator.SetBool("Grounded", grounded);

    }

    public void EnablePlayerControl() {
        canControll = true;
    }

    public void DisablePlayerControl() {
        rawInput = Vector2.zero;
        canControll = false;
    }

    void GetInput() {
        rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump") && currentJumpsLeft > 0) {
            Jump();
        }

        if (onLeftWall && rawInput.x < 0) {
            rawInput.x = 0;
        }
        if (onRightWall && rawInput.x > 0) {
            rawInput.x = 0;
        }

        if (!(onLeftWall || onRightWall)) {
            fastFalling = (rawInput.y < 0 && rb.velocity.y != 0);

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && rawInput.magnitude > 0.1f) {
                StartCoroutine(Dash());
            }
        }
    }

    private void FixedUpdate() {

        animator.ResetTrigger("StopJumping");

        GroundCheck();

        WallCheck();

        // If we are dashing we don't update our velocity

        if (isDashing) {
            return;
        }

        if (onLeftWall || onRightWall) {
            rb.velocity = new Vector2(0, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed * Time.fixedDeltaTime, float.MaxValue));
        }
        else {
            // Better gravity & Hold to jump higher

            if (rb.velocity.y < fallMultiplierBuffer) {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }

            rb.gravityScale = fastFalling ? gravityScale * fastFallingMultiplier : gravityScale;

            // Additional drag when we release keys

            if (grounded && Mathf.Abs(rawInput.x) < .1f) {
                rb.AddForce(new Vector2(-rb.velocity.x * stopDrag, 0));
            }

            // Drag (For Slow)

            rb.AddForce(new Vector2(-rb.velocity.x * drag, 0));
        }

        // Move with Horizontal Inputs

        rb.velocity += new Vector2(accelleration * rawInput.x * Time.fixedDeltaTime, 0);

        if (rb.velocity.x < -maxSpeed) {
            rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
        }
        else if (rb.velocity.x > maxSpeed) {
            rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
        }



    }

    void GroundCheck() {
        //Ground Check
        float margin = 0.03f;

        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(playerCollider.bounds.min.x + margin, transform.position.y), 
            Vector2.down, groundedRaycastLength, groundLayer);

        RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(playerCollider.bounds.max.x - margin, transform.position.y), 
            Vector2.down, groundedRaycastLength, groundLayer);

        if ((hitLeft.collider != null || hitRight.collider != null)) {
            if (!grounded) {
                currentJumpsLeft = maxJumps;
                animator.SetTrigger("StopJumping");
            }
            grounded = true;
        }
        else {
            grounded = false;
        }

        Debug.DrawLine(new Vector2(playerCollider.bounds.min.x + margin, transform.position.y), 
            new Vector2(playerCollider.bounds.min.x + margin, transform.position.y - groundedRaycastLength), Color.red);

        Debug.DrawLine(new Vector2(playerCollider.bounds.max.x - margin, transform.position.y), 
            new Vector2(playerCollider.bounds.max.x - margin, transform.position.y - groundedRaycastLength), Color.red);

    }

    void WallCheck() {
        float margin = 0.03f;
        RaycastHit2D hitTopLeft = Physics2D.Raycast(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.max.y - margin), 
            Vector2.left, wallRaycastLength, groundLayer);

        RaycastHit2D hitCenterLeft = Physics2D.Raycast(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.center.y), 
            Vector2.left, wallRaycastLength, groundLayer);

        RaycastHit2D hitBottomLeft = Physics2D.Raycast(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y + margin), 
            Vector2.left, wallRaycastLength, groundLayer);


        RaycastHit2D hitTopRight = Physics2D.Raycast(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.max.y - margin),
            Vector2.right, wallRaycastLength, groundLayer);

        RaycastHit2D hitCenterRight = Physics2D.Raycast(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.center.y),
            Vector2.right, wallRaycastLength, groundLayer);

        RaycastHit2D hitBottomRight = Physics2D.Raycast(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y + margin),
            Vector2.right, wallRaycastLength, groundLayer);



        if ((hitTopLeft.collider != null || 
             hitCenterLeft.collider != null || 
             hitBottomLeft.collider != null))
        {
            if (!onLeftWall) {
                currentJumpsLeft = maxJumps;
            }
            if (!wallJumping) {
                onLeftWall = true;
            }
        }
        else {
            wallJumping = false;
            onLeftWall = false;
        }

        if(hitTopRight.collider != null ||
           hitCenterRight.collider != null ||
           hitBottomRight.collider != null) 
        {
            if (!onRightWall) {
                currentJumpsLeft = maxJumps;
            }
            if (!wallJumping) {
                onRightWall = true;
            }
        }
        else {
            wallJumping = false;
            onRightWall = false;
        }


        Debug.DrawLine(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.max.y - margin), 
                       new Vector2(playerCollider.bounds.min.x - wallRaycastLength, playerCollider.bounds.max.y - margin), Color.green);

        Debug.DrawLine(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.center.y), 
                       new Vector2(playerCollider.bounds.min.x - wallRaycastLength, playerCollider.bounds.center.y), Color.green);

        Debug.DrawLine(new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y + margin), 
                       new Vector2(playerCollider.bounds.min.x - wallRaycastLength, playerCollider.bounds.min.y + margin), Color.green);


        Debug.DrawLine(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.max.y - margin),
                       new Vector2(playerCollider.bounds.max.x + wallRaycastLength, playerCollider.bounds.max.y - margin), Color.green);

        Debug.DrawLine(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.center.y),
                       new Vector2(playerCollider.bounds.max.x + wallRaycastLength, playerCollider.bounds.center.y), Color.green);

        Debug.DrawLine(new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y + margin),
                       new Vector2(playerCollider.bounds.max.x + wallRaycastLength, playerCollider.bounds.min.y + margin), Color.green);


    }

    void Jump() {

        animator.SetTrigger("Jump");

        float xVelocity;
        if (onLeftWall) {
            xVelocity = jumpForce * 3f;
        }
        else if (onRightWall) {
            xVelocity = jumpForce * -3f;
        }
        else {
            xVelocity = rb.velocity.x;
        }

        if (onLeftWall) {
            wallJumping = true;
            onLeftWall = false;
            transform.position += new Vector3(wallRaycastLength * 3f, 0, 0);
        }
        else if (onRightWall) {
            wallJumping = true;
            onRightWall = false;
            transform.position -= new Vector3(wallRaycastLength * 3f, 0, 0);
        }

        rb.velocity = new Vector2(xVelocity, jumpForce);
        currentJumpsLeft--;
    }

    IEnumerator Dash() {

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

        tr.emitting = true;

        yield return new WaitForSeconds(dashTime);

        tr.emitting = false;

        isDashing = false;
        rb.gravityScale = gravScale;
        if (dashedUp) {
            rb.velocity = new Vector2(rb.velocity.x, 16);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}