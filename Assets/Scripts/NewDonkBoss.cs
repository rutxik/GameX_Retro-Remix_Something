using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Diagnostics.Contracts;

public class NewDonkBoss : MonoBehaviour
{
    public float MaxHealth;
    float health;
    public Transform GroundLevelAndLeftBoundary;
    public Transform JumpLevelAndRightBoundary;
    float TargetXLevel;
    float SourceXLevel;
    bool attacking;
    public GameObject Barrel;
    public movement_dj Player;
    public SpriteRenderer dksprite;
    public Sprite[] sprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(JumpStompAttack());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!attacking)
        {
            int randomnumber = Random.Range(0, 60);
            if (randomnumber == 0)
            {
                StartCoroutine(JumpStompAttack());
            }
            if (randomnumber == 1)
            {
                StartCoroutine(BarrelThrowAttack());
            }
        }
    }

    IEnumerator JumpStompAttack()
    {
        attacking = true;
        dksprite.sprite = sprites[4];
        SourceXLevel = transform.position.x;
        TargetXLevel = Mathf.Clamp(SourceXLevel + (SourceXLevel + (SourceXLevel - Player.transform.position.x) > 0? -4:4), GroundLevelAndLeftBoundary.position.x, JumpLevelAndRightBoundary.position.x);
        transform.DOMoveX(TargetXLevel,0.8f).SetEase(Ease.OutCirc);
        transform.DOMoveY(JumpLevelAndRightBoundary.position.y, 1.2f).SetEase(Ease.OutCirc);
        yield return new WaitForSeconds(1.2f);
        dksprite.sprite = sprites[2];
        transform.DOMoveY(GroundLevelAndLeftBoundary.position.y, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.4f);
        attacking = false;
        dksprite.sprite = sprites[1];
    }

    IEnumerator BarrelThrowAttack()
    {
        attacking = true;
        dksprite.sprite = sprites[0];
        yield return new WaitForSeconds(1);
        dksprite.sprite = sprites[3];
        yield return new WaitForSeconds(1);
        dksprite.sprite = sprites[5];
        yield return new WaitForSeconds(1);
        dksprite.sprite = sprites[1];
        attacking = false;
    }
}
