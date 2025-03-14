using UnityEngine;

public class CollectibleBehavior : MonoBehaviour
{
    [Header("Type Parameters")]
    [SerializeField] private int m_fuelGiven = 5;
    [SerializeField] private int m_healthGiven = 10;
    [SerializeField] private int m_ammoGiven = 3;
    [SerializeField] private string m_objectType;

    private void OnTriggerEnter2D(Collider2D collision)//Ensures that it isn't triggered twice before its destroyed
    {
        PlayerController m_player = collision.gameObject.GetComponent<PlayerController>();
        if (collision.gameObject.name == "Character")
        {
            if (m_objectType == "Fuel") 
            {
                m_player.addFuel(m_fuelGiven);
            }
            else if (m_objectType == "Medkit")
            {
                m_player.addHealth(m_healthGiven);
            }
            else if (m_objectType == "Ammo")
            {
                m_player.addAmmo(m_ammoGiven);
            }
            Destroy(gameObject);//Destroyed once collected
        }
    }
}
