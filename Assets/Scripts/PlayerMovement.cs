using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor.ShaderGraph;
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 15f;
    public float deathY = -13f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundLayer;
    public Sprite normalSprite;
    public Sprite leftSprintSprite;
    public Sprite rightSprintSprite;
    public TileBreaker tileBreaker;
    public float canDashBreakTiles = 0;
    public float maxDashBreakCharges = 3;
    private SpriteRenderer sr;
    
    private bool sprinting;
    private bool isGrounded;
    private Vector3 spawnPoint;
    private Rigidbody2D rb;
    //private bool isGrounded;

    private PlayerControls controls;
    private Vector2 moveInput;
    private float facingDirection = 1f;
    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;
    private float dashCooldownTimer = 0f;
    private bool consumedThisDash = false;
    private bool isDashing = false;
    private bool canDash = true;
    [Header("Downward Dash")]
    public float downwardDashSpeed = 20f;
    public float maxFallSpeed = 15f;
    public float downwardDashDuration = 0.15f;

    public Tilemap spikeTilemap;
    private bool isDownwardDashing = false;

    private PlayerState state = PlayerState.Normal;
    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        controls.Player.Jump.performed += ctx =>
        {
            Jump();
        };
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        spawnPoint = transform.position;
    }
    enum PlayerState
    {
    Normal,
    Dashing,
    //DownwardDashing
    }
    void FixedUpdate()
    {
    if (isDownwardDashing)
    {
        rb.linearVelocity = new Vector2(
            0f,
            -downwardDashSpeed
        );
        Vector3 targetPos = transform.position + Vector3.down * 0.5f;
        CheckEnemies(targetPos + Vector3.right * 0.21f);
        CheckEnemies(targetPos + Vector3.left * 0.21f);
        tileBreaker.BreakTileCluster(targetPos + Vector3.right * 0.21f);
        tileBreaker.BreakTileCluster(targetPos + Vector3.left * 0.21f);
            
        //CheckBreakables();
    }
    if (state == PlayerState.Dashing)
        {
            rb.linearVelocity = new Vector2(
            facingDirection * dashSpeed,
            0f
            );
            Vector3 targetPos = transform.position + Vector3.right * facingDirection * 0.5f;
            CheckEnemies(targetPos + Vector3.up * 0.21f);
            CheckEnemies(targetPos + Vector3.down * 0.21f);
            if (canDashBreakTiles > 0)
            {
            
            if (tileBreaker.WillBreakTile(targetPos + Vector3.up * 0.21f))
                {
                    consumedThisDash = true;
                }
                else if (tileBreaker.WillBreakTile(targetPos + Vector3.down * 0.21f))
                {
                    consumedThisDash = true;
                }
            tileBreaker.BreakTileCluster(targetPos + Vector3.up * 0.21f);
            tileBreaker.BreakTileCluster(targetPos + Vector3.down * 0.21f);
            }
        }
    }
    void Update()
    {
        //CheckSpikeDeath();
        Debug.Log("Grounded: " + isGrounded);
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        if (
        Mouse.current.leftButton.wasPressedThisFrame &&
        canDash &&
        state == PlayerState.Normal
        )
        {
        StartCoroutine(Dash());
        }
        if (Mouse.current.rightButton.isPressed)
        {
        isDownwardDashing = true;
        }
        else
        {
        isDownwardDashing = false;
        }
        if (transform.position.y < deathY)
        {
            Respawn();
        }
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            new Vector2(0.45f, 0.3f),
            0f,
            groundLayer
        );

        /*isGrounded = Physics2D.Raycast(
        groundCheck.position,
        Vector2.down,
        groundDistance,
        groundLayer
        );*/
        Move();
        Jump();
        ToggleSprint();
    }
    public void EnableDashBreak()
    {
        if (canDashBreakTiles < maxDashBreakCharges)
        {
            canDashBreakTiles += 1;
        }
        
    }
    public bool IsDashReady()
    {
        return dashCooldownTimer <= 0f;
    }
    public float GetDashCooldownNormalized()
    {
        return 1f - Mathf.Clamp01(dashCooldownTimer / dashCooldown);
    }
    void CheckEnemies(Vector3 position)
    {
    Collider2D[] hits = Physics2D.OverlapBoxAll(
        position,
        new Vector2(0.3f, 0.3f),
        0f
    );

    foreach (Collider2D hit in hits)
    {
        if (hit.TryGetComponent<EnemyPatrol>(out var enemy))
        {
            enemy.Die();
        }
        if (hit.TryGetComponent<EnemyPU>(out var enemy2))
        {
            enemy2.Die();
        }
        /*if (hit.TryGetComponent<DashBuffEnemy>(out var buffEnemy))
        {
            EnableDashBreak();
            Destroy(buffEnemy.gameObject);
        }*/
    }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            Respawn();
        }
    }       
    public void HandleEnemyContact()
    {
    if (state == PlayerState.Dashing || isDownwardDashing)
    {
        return; // player is invulnerable during attack
    }

    Respawn();
    }
    void Respawn()
    {
    /*rb.linearVelocity = Vector2.zero;
    transform.position = spawnPoint;*/
        RestartLevel();
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(
        SceneManager.GetActiveScene().name
    );
    }
    void Move()
    {
        if (state != PlayerState.Normal)
        {
            return;
        }
        if (isDownwardDashing)
        {
        return;
        }
    float currentSpeed = moveSpeed;

    bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;
    //bool sprinting = Keyboard.current.leftShiftKey.isPressed && isMoving;
    bool isleft = Keyboard.current.aKey.isPressed && isMoving;
    //bool isright = Keyboard.current.dKey.isPressed && isMoving;
    if (moveInput.x > 0)
    {
    facingDirection = 1f;
    //sr.flipX = false;
    }
    else if (moveInput.x < 0)
    {
    facingDirection = -1f;
    //sr.flipX = true;
    }   
    if (sprinting)
    {
        currentSpeed = sprintSpeed;
        if (isleft && isMoving)
            {
                sr.sprite = leftSprintSprite;
                
                //sr.flipX = false;
            }
            else if (isMoving)
            {
                sr.sprite = rightSprintSprite;
                
                //sr.flipX = true;
            }
        
    }
    else
    {
        sr.sprite = normalSprite;
    }

    float x = moveInput.x * currentSpeed;
    
    rb.linearVelocity = new Vector2(
    x,
    Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)
    );
    //rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    } 

    void ToggleSprint()
    {
    if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
    {
        sprinting = !sprinting;
    }
    }
    void Jump()
    {
    if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    }
    
    IEnumerator Dash()
    {
    canDash = false;
    consumedThisDash = false;
    state = PlayerState.Dashing;
    dashCooldownTimer = dashCooldown;
    float originalGravity = rb.gravityScale;
    rb.gravityScale = 0f;

    //float dashDirection = facingDirection;

    

    yield return new WaitForSeconds(dashDuration);
    
    //CheckBreakables();
    rb.gravityScale = originalGravity;
    state = PlayerState.Normal;
    if (consumedThisDash)
    {
    canDashBreakTiles -= 1;
    }
    yield return new WaitForSeconds(dashCooldown);
    
    canDash = true;
    }
    
/*  private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }*/
}

