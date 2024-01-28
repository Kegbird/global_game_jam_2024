using UnityEngine;

public class KnifeBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (!GetComponent<SpriteRenderer>().isVisible)
            gameObject.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            collision.gameObject.GetComponent<IEnemy>().Kill();
        }
    }
}
