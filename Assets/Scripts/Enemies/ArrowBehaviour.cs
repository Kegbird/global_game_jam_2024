using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    private GameManager game_manager;

    private void Start()
    {
        game_manager = GameManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            game_manager.DamagePlayer(1);
            gameObject.SetActive(false);
        }
    }
}
