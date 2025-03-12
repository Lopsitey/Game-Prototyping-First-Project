using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Android.Gradle.Manifest;

public class PlayerController : MonoBehaviour
{
    #region Movement Variables
    //Default to 0.0f, -1.0f which is facing down so if the player tries to fire a bullet without moving it will always have a valid direction
    private Vector2 m_LastDirection = new Vector2(0.0f, -1.00f);

    //The inputs that we need to retrieve from the input system.
    private InputAction m_moveAction;

    //The components that we need to edit to make the player move smoothly.
    private Rigidbody2D m_rigidBody;

    //The direction that the player is moving in.
    private Vector2 m_playerDirection;

    //The amount of jumps the player has performed
    private float m_jumpCounter;

    //If the player is currently jumping
    private bool m_isJumping;

    private Vector2 m_vecGravity;

    [Header("Movement Parameters")]
    //The speed at which the player moves
    [SerializeField] private float m_playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMaxSpeed = 1000f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMinSpeed = 50f;
    [SerializeField] private float m_jumpHeight = 25.0f;
    [SerializeField] public float m_jumpTime = 0.4f;//essentially fuel
    //For the jetpack
    [SerializeField] public float m_jumpMultiplier = 2.0f;

    #endregion

    private int m_currentWeight = 0;
    private const int m_maxWeight = 0;

    public Transform m_player;
    public LayerMask m_platform;

    private void Awake()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_vecGravity = new Vector2(0, -Physics2D.gravity.y);
    }

    /// <summary>
    /// Called after Awake(), and is used to initialize variables e.g. set values on the player
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// When a fixed update loop is called, it runs at a constant rate, regardless of pc performance.
    /// This ensures that physics are calculated properly.
    /// </summary>
    private void FixedUpdate()
    {
        //Clamp the speed to the maximum speed for if the speed has been changed in code
        float speed = Mathf.Clamp(m_playerSpeed, m_playerMinSpeed, m_playerMaxSpeed);
        //apply the movement to the character using the clamped speed value.
        m_rigidBody.linearVelocity = new Vector2(m_playerDirection.x * (speed * Time.fixedDeltaTime), m_rigidBody.linearVelocityY);// m_playerDirection * (speed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// When the update loop is called, it runs every frame.
    /// Therefore, this will run more or less frequently depending on performance.
    /// Used to catch changes in variables or input.
    /// </summary>
    void Update()
    {
        //Store any movement inputs into m_playerDirection - this will be used in FixedUpdate to move the player.
        m_playerDirection = m_moveAction.ReadValue<Vector2>();
        if (m_playerDirection != m_LastDirection)
        {
            // If there is movement, set the directional values to ensure the character is facing the way they are moving.
            if (m_playerDirection.magnitude > 0)
            {
                m_LastDirection = m_playerDirection;//Sets the last facing direction of the player
            }
        }

        if (m_rigidBody.linearVelocityY > 0 && m_isJumping && m_hasFuel())
        {
            Debug.Log("Jumped!");
            m_jumpCounter += Time.deltaTime;
            if (m_jumpCounter > m_jumpTime)
                m_isJumping = false;//stop jumping if you've been jumping for too long
            m_rigidBody.linearVelocity += m_vecGravity * m_jumpMultiplier * Time.deltaTime;
        }
    }

    public void AddWeight(int weightGiven)
    {
        //Reduces the speed by 5% for every 2 objects collected
        if (m_currentWeight % 10 == 0)
        {
            m_playerSpeed *= 0.95f;
            Debug.Log("Speed reduced by 5% to " + m_playerSpeed);
        }

        // Add the weight to the player's weight
        m_currentWeight += weightGiven;

        // Clamp the weight to the maximum weight
        m_currentWeight = Mathf.Clamp(m_currentWeight, 0, m_maxWeight);
    }

    public void HandleJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && onGround())//jump started
        {
            m_isJumping = true;
            m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_jumpHeight);//m_rigidBody.AddForce(Vector2.up * m_jumpHeight, ForceMode2D.Force);
        }
        if (ctx.canceled)//jump ended
        {
            m_isJumping= false;
            m_jumpCounter = 0;
            if (m_rigidBody.linearVelocityY > 0) 
            {
                //reduces the velocity by 60% after the jump has been completed to prevent further upward motion
                m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_rigidBody.linearVelocityY * 0.6f);
            }
        }
    }

    bool onGround() 
    {
        return Physics2D.OverlapBox(m_player.position, new Vector2(1.0f, 1.0f), 0, m_platform);
        //return Physics2D.Raycast(transform.position, -Vector2.up, 1f);
    }

    //If the player is currently jumping
    bool m_hasFuel()
    {
        return true;//need to change this to check if jump time is greater than 0
    }
}
