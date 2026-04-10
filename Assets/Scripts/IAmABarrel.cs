using DG.Tweening;
using System.Collections;
using UnityEngine;

public class IAmABarrel : MonoBehaviour
{

    public int ThisIsMyDamageCount;

    public bool AndThisIsWhetherIllExplode;

    public bool TickThisOneIfIAmNotActuallyABarrelAndIGoLeft;
    public bool TickThisOneIfIAmNotActuallyABarrelAndIGoRight;

    Vector3 startpos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startpos = transform.position;
        if (TickThisOneIfIAmNotActuallyABarrelAndIGoRight)
        {
            
            transform.DOMove(startpos + new Vector3(1, 0, 0),0.5f).SetEase(Ease.OutCubic);
            gameObject.GetComponent<SpriteRenderer>().DOFade(0,0.5f);
            Destroy(gameObject, 2);
            Destroy(gameObject.GetComponent<Rigidbody2D>());

        }
        if (TickThisOneIfIAmNotActuallyABarrelAndIGoLeft)
        {
            transform.DOMove(startpos + new Vector3(-1, 0, 0),0.5f).SetEase(Ease.OutCubic);
            gameObject.GetComponent<SpriteRenderer>().DOFade(0,0.5f);
            Destroy(gameObject, 2);
            Destroy(gameObject.GetComponent<Rigidbody2D>());
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TickThisOneIfIAmNotActuallyABarrelAndIGoLeft || TickThisOneIfIAmNotActuallyABarrelAndIGoRight) return;
        if (collision.gameObject.layer == 14)
        {
            StartCoroutine(Parried());
        }
    }

    IEnumerator Parried()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1;
        gameObject.layer = 14;
        gameObject.GetComponent<Rigidbody2D>().linearVelocity = gameObject.GetComponent<Rigidbody2D>().linearVelocity * (-2);
        gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
    }
}
