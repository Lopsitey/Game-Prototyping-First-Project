using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    #region Health Variables

    [Header("Health Parameters")]
    [SerializeField] private int m_currentHealth = 100;
    [SerializeField] private int m_maxHealth = 100;

    private GameObject m_mediumHealth;
    private GameObject m_lowHealth;
    private GameObject m_dead;

    #endregion

    void Start()
    {
        m_currentHealth = m_maxHealth;

        PlayerController playerController = GetComponent<PlayerController>();

        m_mediumHealth = playerController.transform.Find("Medium Health").gameObject;
        m_lowHealth = playerController.transform.Find("Low Health").gameObject;
        m_dead = playerController.transform.Find("Dead").gameObject;
    }

    public void TakeDamage(int amount, bool showMask)//Essentially a setter
    {
        m_currentHealth = Mathf.Clamp(m_currentHealth - amount, 0, m_maxHealth);
        CheckHealth();   
    }

    public void Heal(int value)//Another setter
    {
        m_currentHealth = Mathf.Clamp(m_currentHealth + value, 0, m_maxHealth);
        CheckHealth();
    }

    private void CheckHealth() 
    {
        if (m_currentHealth > 66)
        {
            m_mediumHealth.SetActive(false);
        }
        if (m_currentHealth <= 66 && m_currentHealth > 33)
        {
            m_mediumHealth.SetActive(true);
            m_lowHealth.SetActive(false);
        }
        if (m_currentHealth <= 33 && m_currentHealth > 0)
        {
            m_lowHealth.SetActive(true);
        }
        else if (m_currentHealth == 0)
        {
            m_dead.SetActive(true);
            //Player died!
        }
    }
}