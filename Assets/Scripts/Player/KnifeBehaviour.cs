using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeBehaviour : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            collision.gameObject.GetComponent<IEnemy>().Kill();
        }
    }
    private void Update()
    {
        if(!GetComponent<SpriteRenderer>().isVisible)
            gameObject.SetActive(false);
    }

}
