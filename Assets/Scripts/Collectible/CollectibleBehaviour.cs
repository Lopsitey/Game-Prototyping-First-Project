using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

public class CollectibleBehavior : MonoBehaviour
{
    #region Collectible Variables

    [Header("Type Parameters")]
    [SerializeField] private int m_fuelGiven = 5;
    [SerializeField] private int m_healthGiven = 10;
    [SerializeField] private int m_ammoGiven = 3;
    [SerializeField] private string m_objectType;
    [SerializeField] private float m_respawnTime = 5.0f;

    #endregion

    #region Miscellaneaous Variables

    private bool m_deactivated = false;

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)//Ensures that it isn't triggered twice before its destroyed
    {
        PlayerController m_player = collision.gameObject.GetComponent<PlayerController>();
        PlayerHealth m_playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (collision.gameObject.name == "Character" && !m_deactivated)
        {
            if (m_objectType == "Fuel") 
            {
                m_player.addFuel(m_fuelGiven);
            }
            else if (m_objectType == "Medkit")
            {
                m_playerHealth.Heal(m_healthGiven);
            }
            else if (m_objectType == "Ammo")
            {
                m_player.addAmmo(m_ammoGiven);
            }
            else if (m_objectType == "Win")
            {
                Application.Quit();
            }
            StartCoroutine(RespawnDelay(m_respawnTime));
        }
    }

    private IEnumerator RespawnDelay(float delay)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)//the win condition has no sprite renderer
        {
            spriteRenderer.enabled = false;  // Temporarily disable the sprite renderer
            m_deactivated = true;  // Prevents further hits

            yield return new WaitForSeconds(delay);

            spriteRenderer.enabled = true;  // Reactivate the sprite renderer after delay
            m_deactivated = false;  // Reactivated after delay
        }
    }
}
