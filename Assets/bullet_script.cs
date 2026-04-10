using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float maxY= 1200f;
    

    void Start()
    {
    }

    void Update()
    {
        //Destroy(gameObject, lifeTime); // auto destroy after time
        transform.Translate(Vector2.up * 1f);
        if (transform.position.y > maxY)
        {
            Destroy(gameObject); // destroy bullet if it goes off screen
        }
    
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject); // kill enemy
            Destroy(gameObject);       // destroy bullet
        }
    }
}