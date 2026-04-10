using UnityEngine;

public class IAmABarrel : MonoBehaviour
{

    public int ThisIsMyDamageCount;

    public bool AndThisIsWhetherIllExplode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            movement_dj.Health -= ThisIsMyDamageCount;
            Destroy(this.gameObject);
        }
    }
}
