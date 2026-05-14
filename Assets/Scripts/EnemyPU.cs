using UnityEngine;

public class EnemyPU : MonoBehaviour
{
    public float speed = 2f;

    public Transform wallCheck;
    public Transform edgeCheck;

    public LayerMask groundLayer;

    public Vector2 wallCheckSize = new Vector2(0.1f, 0.3f);
    public Vector2 edgeCheckSize = new Vector2(0.2f, 0.1f);

    public float deathY = -13f;
    private int direction = 1;
    private bool justTurned;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            direction * speed,
            rb.linearVelocity.y
        );
        if (transform.position.y < deathY)
        {
            Die();
        }
        if (!justTurned)
        {
            CheckTurn();
        }
        
    }

    void CheckTurn()
    {
        // WALL CHECK
        bool wallAhead = Physics2D.OverlapBox(
            wallCheck.position,
            wallCheckSize,
            0f,
            groundLayer
        );

        // EDGE CHECK
        bool groundAhead = Physics2D.OverlapBox(
            edgeCheck.position,
            edgeCheckSize,
            0f,
            groundLayer
        );

        if (wallAhead || !groundAhead)
        {
            TurnAround();
        }
    }
    
    void TurnAround()
    {
        justTurned = true;
        direction *= -1;

        /*Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;*/

        // move checks to correct side
        Vector3 wallPos = wallCheck.localPosition;
        wallPos.x = Mathf.Abs(wallPos.x) * direction;
        wallCheck.localPosition = wallPos;

        Vector3 edgePos = edgeCheck.localPosition;
        edgePos.x = Mathf.Abs(edgePos.x) * direction;
        edgeCheck.localPosition = edgePos;
        justTurned = false;
    }
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.HandleEnemyContact();
        }
    }
}

private void GivePlayerPowerup()
{
    PlayerMovement player = FindAnyObjectByType<PlayerMovement>();

    if (player != null)
    {
        player.EnableDashBreak();
    }
}
    public void Die()
    {
        GivePlayerPowerup();
        Destroy(gameObject);
    }
}