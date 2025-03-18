using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    #region Movement variables

    [Header("Movement Parameters")]
    [SerializeField] public Transform m_player;
    [SerializeField] public float m_speed;
    [SerializeField] public float m_stoppingDistance;
    [SerializeField] private float m_jumpHeight = 10.0f;
    [SerializeField] private float m_jumpSpeed = 2.0f;

    #endregion

    #region Combat Variables

    [Header("Damage Parameters")]
    [SerializeField] private int m_attackDamage = 25;
    [SerializeField] private float m_attackSpeed = 2f;
    //Not implemented yet!
    //[SerializeField] private PlayerHealth m_playerHealth;

    #endregion

    #region Miscellaneous variables

    private SpriteRenderer m_spriteRenderer;
    NavMeshAgent m_agent;
    private bool m_attacking = false;

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
        m_agent = GetComponent<NavMeshAgent>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_player = FindFirstObjectByType<PlayerController>().transform;
        m_enemyStates = EnemyStates.Idle;//Idle by default
    }

    /// <summary>
    /// When a fixed update loop is called, it runs at a constant rate, regardless of pc performance.
    /// This ensures that physics are calculated properly.
    /// </summary>
    private void FixedUpdate()
    {
        StartCoroutine(JumpDelay());//Jumps every 2 seconds
    }

    // Update is called once per frame
    void Update()
    {
        //If the player is in sight, is outside of the stopping distance and the enemy is not stunned
        if (Vector2.Distance(transform.position, m_player.position) > m_stoppingDistance && m_enemyStates == EnemyStates.MovingToPlayer)
        {
            m_agent.SetDestination(m_player.position);

            if (m_agent.velocity.x > 0)//flips the sprite in the left or right direction so it's always facing the player
            {
                m_spriteRenderer.flipX = false; // Facing right
            }
            else if (m_agent.velocity.x < 0)
            {
                m_spriteRenderer.flipX = true; // Facing left
            }
        }
        //If the stopping distance has been met then an attack can occur
        else if (Vector2.Distance(transform.position, m_player.position) <= m_stoppingDistance && m_enemyStates == EnemyStates.MovingToPlayer)
        {
            m_enemyStates = EnemyStates.Attack;
        }

        switch (m_enemyStates)
        {
            case EnemyStates.Idle:
                m_agent.isStopped = true;//Stops movement immediately
                m_agent.velocity = Vector3.zero;//Ensures no movement happens
                break;

            case EnemyStates.MovingToPlayer:
                m_agent.isStopped = false;
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
            //not implemented damage yet!
            //m_playerHealth.TakeDamage(m_attackDamage, true);//Apply damage but do show the mask
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
        yield return new WaitForSeconds(m_jumpSpeed);
        m_agent.velocity = new Vector2(m_agent.velocity.x, m_jumpHeight);
    }
}