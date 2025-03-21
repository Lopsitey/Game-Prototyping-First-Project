using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region Movement Variables
    //Default to -1.0f, 0.0f which is facing left so if the player tries to fire a bullet without moving it will always have a valid direction
    private Vector2 m_LastDirection = new Vector2(-1.0f, 0.0f);

    //The inputs that we need to retrieve from the input system.
    private InputAction m_moveAction;

    //The components that we need to edit to make the player move smoothly.
    private Rigidbody2D m_rigidBody;

    //The direction that the player is moving in.
    private Vector2 m_playerDirection;

    //If the player is currently jumping
    private bool m_startedJump = false;

    //For checking how long the jump key has been held down
    private float m_jumpStartTime;

    //To check if the player is on the ground
    private bool m_onGround = true;

    //To decide if the jetpack can be activated
    private int m_jumpCounter = 0;

    [Header("Movement Parameters")]
    //The speed at which the player moves
    [SerializeField] private float m_playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMaxSpeed = 1000f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMinSpeed = 50f;
    //For the jumping and the jetpack
    [SerializeField] private float m_jumpHeight = 25.0f;
    //Fall speed
    [SerializeField] private float m_maxFallSpeed = -50.0f;
    [SerializeField] private float m_jetpackFuel = 0.4f;//set to 0.4 so the player always has a bit of extra jump height when holding the button down by default
    [SerializeField] private float m_jetpackFuelMax = 15f;
    [SerializeField] public float m_jumpMultiplier = 0.5f;

    #endregion

    #region Combat Variables
    [Header("Combat Parameters")]
    [SerializeField] private float m_currentAmmo;
    [SerializeField] private float m_maxAmmo;

    #endregion

    #region Miscellaneous Variables

    private Animator m_animator;
    private GameObject m_playerObject;
    private Transform m_player;

    private GameObject m_fuelBar1;
    private GameObject m_fuelBar2;
    private GameObject m_fuelBar3;

    private int m_currentWeight = 0;
    private const int m_maxWeight = 0;
    private bool m_firstCall = false;

    [Header("Miscellaneous Parameters")]
    [SerializeField] private LayerMask m_platform;
    

    #endregion
    private void Awake()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_playerObject = gameObject;
        m_player = m_playerObject.transform;

        m_fuelBar1 = m_player.transform.Find("Jetpack/Fuel Bar 1").gameObject;
        m_fuelBar2 = m_player.transform.Find("Jetpack/Fuel Bar 2").gameObject;
        m_fuelBar3 = m_player.transform.Find("Jetpack/Fuel Bar 3").gameObject;
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
        if (m_rigidBody.linearVelocityY < 0)//Only affects fall speed
        {
            m_rigidBody.linearVelocityY = Mathf.Clamp(m_rigidBody.linearVelocityY, m_maxFallSpeed, 0);
            //maxFallSpeed is in the minimum spot because fall speed increases negatively so the minimum is the maximum
        }

        //if not on the ground and the jump button is pressed, use the jetpack if it has fuel
        if (!m_onGround && m_startedJump && m_jumpCounter == 2 && m_jetpackFuel > 0)
        {
            m_animator.SetBool("IsFlying", true);
            m_jetpackFuel -= Time.deltaTime * 10; // uses the fuel that was stored up

            if (m_jetpackFuel <= 0.4)//if the jetpack fuel is at the default value, add a bit more to the jump height
                m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_jumpHeight * (m_jumpMultiplier + 0.25f));
            else
            {
                //if the jetpack fuel is not at the default value, use the default jump height
                m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_jumpHeight * m_jumpMultiplier);
                //m_rigidBody.AddForce(Vector2.up * m_jumpHeight * m_jumpMultiplier, ForceMode2D.Force);
            }
            UpdateFuelBars();//Keeps the fuel bars updated whilst the player is flying
        }
        else if (m_jetpackFuel <= 0)
        {
            m_fuelBar3.SetActive(false);
            FinishedJump();
        }
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
            if (m_playerDirection.magnitude > 0)//if the player's direction changes
            {
                //This prevents the player from being flipped until they face right
                if (m_playerDirection == new Vector2(1.0f, 0.0f) || m_firstCall)//They face left by default so it makes sense not to flip them until otherwise
                    FlipSprites(m_playerObject);//Flips the sprite to the opposite direction
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
        if (ctx.started)
        {
            m_startedJump = true;
            //Iterates the jump counter between 0 and 2
            m_jumpCounter += m_jumpCounter == 2 ? -2 : 1;//used to alternate between jumping and using the jetpack
        }

        if (ctx.performed && m_onGround) // jump started
        {
            m_onGround = false;
            m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_jumpHeight);
        }

        if (ctx.canceled) // jump ended
        {
            if (m_startedJump)//if finished jump hasn't already been called by an empty jetpack
                FinishedJump();
        }
    }


    /*bool onGround() 
    {
        return Physics2D.OverlapBox(m_player.position, new Vector2(1.0f, 1.0f), 0, m_platform);
        //return Physics2D.Raycast(transform.position, -Vector2.up, 1f);
    }*/

    public void addFuel(float fuelAmount)
    {
        m_jetpackFuel = m_jetpackFuel == 0.4f ? 0 : m_jetpackFuel;//ensures the tank is filled from zero and not the amount used for a double jump
        m_jetpackFuel += fuelAmount;
        m_jetpackFuel = Mathf.Clamp(m_jetpackFuel, 0, m_jetpackFuelMax);//Clamp the fuel to the maximum fuel which would be 2 pickups worth
        UpdateFuelBars();//Updates the fuel when fuel is collected
    }

    public void addAmmo(float ammoAmount)
    {
        m_currentAmmo += ammoAmount;
        m_currentAmmo = Mathf.Clamp(m_currentAmmo, 0, m_maxAmmo);
    }

    private void FinishedJump()
    {
        m_animator.SetBool("IsFlying", false);
        m_startedJump = false;
        if (m_rigidBody.linearVelocityY > 0)
        {
            //reduces the velocity by 60% after the jump has been completed to prevent further upward motion
            m_rigidBody.linearVelocity = new Vector2(m_rigidBody.linearVelocityX, m_rigidBody.linearVelocityY * 0.6f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)//If the ground is hit, reset the jetpack fuel
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            m_jumpCounter = 0;
            m_jetpackFuel = m_jetpackFuel <= 0 ? 0.4f : m_jetpackFuel;//ensures that the jetpack fuel is only reset if it is empty
            m_fuelBar3.SetActive(true);
            m_onGround = true;
        }
    }

    private void FlipSprites(GameObject parentObject)
    {
        m_firstCall = true;
        Vector3 scale = parentObject.transform.localScale;
        scale.x *= -1;//Reverses sprite direction
        parentObject.transform.localScale = scale;
    }

    /// <summary>
    ///Updates the fuel gauge on the jetpack
    /// </summary>
    private void UpdateFuelBars()
    {
        float fuelPercent = math.trunc((m_jetpackFuel / m_jetpackFuelMax) * 100);//converts the fuel to a percentage to make the calculations easier
                                                                                 //The values don't have to be hardcoded in and will be changed dynamically if a percentage is used
        if (fuelPercent > 66)
        {
            m_fuelBar1.SetActive(true);
            m_fuelBar2.SetActive(true);
            m_fuelBar3.SetActive(true);
        }
        else if (fuelPercent <= 66 && fuelPercent > 33)
        { 
            m_fuelBar1.SetActive(false);
            m_fuelBar2.SetActive(true);
        }
        else if (fuelPercent <= 33)
            m_fuelBar2.SetActive(false);
    }
}