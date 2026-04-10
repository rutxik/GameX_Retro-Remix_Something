using UnityEngine;

public class IAmABarrel : MonoBehaviour
{

    public int ThisIsMyDamageCount;

    public bool AndThisIsWhetherIllExplode;

    public bool TickThisOneIfIAmNotActuallyABarrel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            if (collision.gameObject.GetComponent<movement_dj>().invincible) return;
            collision.gameObject.GetComponent<movement_dj>().TakeDamageFunc(ThisIsMyDamageCount);
            Destroy(this.gameObject);
        }
    }

}
