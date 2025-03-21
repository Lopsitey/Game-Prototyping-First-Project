using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    #region Movement variables

    [Header("Movement Parameters")]
    [SerializeField] public float m_speed;
    [SerializeField] public float m_stoppingDistance;
    [SerializeField] private float m_jumpHeight = 25.0f;
    [SerializeField] private float m_jumpSpeed = 2.0f;

    #endregion

    #region Combat Variables

    [Header("Damage Parameters")]
    [SerializeField] private int m_attackDamage = 25;
    [SerializeField] private float m_attackSpeed = 2f;
    private PlayerHealth m_playerHealth;

    #endregion

    #region Miscellaneous variables

    private Transform m_player;
    private SpriteRenderer m_spriteRenderer;
    private Rigidbody2D m_body;
    private bool m_attacking = false;
    private bool m_onGround;

    private enum EnemyStates
    {
        Idle,
        MovingToPlayer,
        Attack
    }
    private EnemyStates m_enemyStates;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_player = FindFirstObjectByType<PlayerController>().transform;
        m_playerHealth = m_player.GetComponent<PlayerHealth>();

        m_body = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_enemyStates = EnemyStates.Idle;//Idle by default
        
        StartCoroutine(JumpDelay());//Jumps every 2 seconds
    }

    /// <summary>
    /// When a fixed update loop is called, it runs at a constant rate, regardless of pc performance.
    /// This ensures that physics are calculated properly.
    /// </summary>
    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //If the player is in sight, is outside of the stopping distance and the enemy on the ground
        if (Vector2.Distance(m_body.position, m_player.position) > m_stoppingDistance && m_enemyStates == EnemyStates.MovingToPlayer)// && m_onGround)
        {
            Vector2 direction = (m_player.position - (Vector3)transform.position).normalized;//Normalized to ensure that only the direction is extracted from the vector 
            m_body.linearVelocity = new Vector2(direction.x * m_speed, m_body.linearVelocity.y);
            //m_body.SetDestination(m_player.position);// would also work

            if (m_body.linearVelocity.x > 0)//flips the sprite in the left or right direction so it's always facing the player
            {
                m_spriteRenderer.flipX = false; // Facing right
            }
            else if (m_body.linearVelocity.x < 0)
            {
                m_spriteRenderer.flipX = true; // Facing left
            }
        }
        //If the stopping distance has been met then an attack can occur
        else if (Vector2.Distance(transform.position, m_player.position) <= m_stoppingDistance && m_enemyStates == EnemyStates.MovingToPlayer)
        {
            m_enemyStates = EnemyStates.Attack;
        }

        /*if (!m_onGround)//prevents movement when in the air
        {
            m_agent.linearVelocityX = 0;
        }*/

        switch (m_enemyStates)
        {
            case EnemyStates.Idle:
                m_body.linearVelocity = new Vector2(0, m_body.linearVelocity.y);//Only stop horizontal movement
                break;

            case EnemyStates.MovingToPlayer:
                break;

            case EnemyStates.Attack:
                AttackPlayer();
                break;
        }
    }

    void AttackPlayer()
    {
        if (!m_attacking)//If the attack isn't on cooldown
        {
            m_attacking = true;
            m_playerHealth.TakeDamage(m_attackDamage, true);//Apply damage but do show the mask
            StartCoroutine(AttackDelay());//Delay for next attack
        }
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(m_attackSpeed);//Attacks once every two seconds by default
        m_attacking = false;
        m_enemyStates = EnemyStates.MovingToPlayer;//Resume chasing after attacking
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Character")
        {
            m_enemyStates = EnemyStates.MovingToPlayer;//Switches to moving state if the player is detected
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Character")
        {
            m_enemyStates = EnemyStates.Idle;
        }
    }

    IEnumerator JumpDelay()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_jumpSpeed);
            if (m_onGround && !m_attacking)
            {
                m_body.linearVelocity = new Vector2(m_body.linearVelocityX, m_jumpHeight);
                m_onGround = false;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)//If the ground is hit, reset the jetpack fuel
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            m_onGround = true;
        }
    }
}