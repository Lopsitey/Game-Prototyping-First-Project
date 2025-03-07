using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CollectibleController : MonoBehaviour
{
    [Header("Weight Parameters")]
    [SerializeField] private int m_weightGiven = 5;

    private void Awake()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)//Ensures that it isn't triggered twice before its destroyed
    {
        if (collision.gameObject.name == "Character")
        {
            collision.gameObject.GetComponent<PlayerController>().AddWeight(m_weightGiven);
            Destroy(gameObject);//Destroyed once collected
        }
    }
}
