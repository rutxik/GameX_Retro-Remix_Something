using System.Collections;
using UnityEngine;

public class SmallGhost : MonoBehaviour
{
    public float speed;
    public LayerMask SolidLayer;
    int direction;
    // wasd - 0123
    public float RaycastLength;
    public SpriteRenderer spriterenderer;
    public Sprite[] lookup;
    public Sprite[] lookright;
    public Sprite[] lookleft;
    public Sprite[] lookdown;
    int spritechangecounter;
    public int SpriteChangeTimeInFrames;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = Random.Range(0, 4);
        spritechangecounter = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector2 movementdir = SetMovementDirection(direction);

        if (Physics2D.Raycast(transform.position,movementdir, RaycastLength, SolidLayer))
        {
            direction = (4 + direction + (Random.Range(0,2)==0?1:-1)) % 4;
            UpdateSprite0();
        }
        print(direction);
        transform.position += (Vector3)movementdir * speed;

        if (spritechangecounter++ % (SpriteChangeTimeInFrames * 2) == 0)
        {
            UpdateSprite0();
        }

        if (spritechangecounter % (SpriteChangeTimeInFrames * 2) == SpriteChangeTimeInFrames)
        {
            UpdateSprite1();
        }
    }

    Vector2 SetMovementDirection(int direction)
    {

        Vector2 movementdir = Vector2.zero;
        switch (direction)
        {
            case 0: movementdir = Vector2.up; break;
            case 1: movementdir = Vector2.right; break;
            case 2: movementdir = Vector2.down; break;
            case 3: movementdir = Vector2.left; break;
        }
        return movementdir;
    }

    void UpdateSprite0()
    {
        switch (direction)
        {
            case 0: spriterenderer.sprite = lookup[0]; break;
            case 1: spriterenderer.sprite = lookright[0]; break;
            case 2: spriterenderer.sprite = lookdown[0]; break;
            case 3: spriterenderer.sprite = lookleft[0]; break;
        }
    }

    void UpdateSprite1()
    {
        switch (direction)
        {
            case 0: spriterenderer.sprite = lookup[1]; break;
            case 1: spriterenderer.sprite = lookright[1]; break;
            case 2: spriterenderer.sprite = lookdown[1]; break;
            case 3: spriterenderer.sprite = lookleft[1]; break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14)
        {
            StartCoroutine(DEATH());
        }
    }

    IEnumerator DEATH()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        Destroy(gameObject);
    }
}
