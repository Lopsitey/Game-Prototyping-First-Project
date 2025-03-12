using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region Framework Variables

    //The inputs that we need to retrieve from the input system.
    private InputAction m_moveAction;

    //The components that we need to edit to make the player move smoothly.
    private Rigidbody2D m_rigidBody;

    //The direction that the player is moving in.
    private Vector2 m_playerDirection;

    [Header("Movement Parameters")]
    //The speed at which the player moves
    [SerializeField] private float m_playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMaxSpeed = 1000f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMinSpeed = 50f;
    [SerializeField] private float m_jumpHeight = 1.0f;

    #endregion

    //Default to 0.0f, -1.0f which is facing down so if the player tries to fire a bullet without moving it will always have a valid direction
    private Vector2 m_LastDirection = new Vector2(0.0f, -1.00f);
    private int m_currentWeight = 0;
    private const int m_maxWeight = 0;

    private void Awake()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_rigidBody = GetComponent<Rigidbody2D>();
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
        m_rigidBody.linearVelocity = m_playerDirection * (speed * Time.fixedDeltaTime);
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
        if (ctx.performed)
        {
            Debug.Log("Jumping");
            m_rigidBody.AddForce(Vector2.up * m_jumpHeight, ForceMode2D.Impulse);
        }
    }
}
