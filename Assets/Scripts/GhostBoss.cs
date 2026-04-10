using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem.XR;

public class GhostBoss : MonoBehaviour
{
    public SpriteRenderer GhostSprite;
    public Transform[] AttackPositions;
    float Health;
    public float MaxHealth;
    int CurrentAttackPositionIndex;
    public GameObject[] SmallGhost;
    bool attacking;
    bool invisible;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GoToPositionAndThenAttack());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!attacking && Random.Range(0,5) == 0)
        {
            StartCoroutine(GoToPositionAndThenAttack());
        }
    }

    IEnumerator GoToPositionAndThenAttack()
    {
        attacking = true;
        int lastattackposindex = CurrentAttackPositionIndex;
        while(CurrentAttackPositionIndex == lastattackposindex)
        {
            CurrentAttackPositionIndex = Random.Range(0,AttackPositions.Length);
        }
        invisible = true;
        GhostSprite.DOFade(0, 0.1f);
        transform.DOMove(AttackPositions[CurrentAttackPositionIndex].position, 2f).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(3);
        invisible = false;
        GhostSprite.DOFade(0.5f, 0.1f);
        while (Random.Range(0, 4) != 0)
        {
            Instantiate(SmallGhost[Random.Range(0,4)], transform.position, transform.rotation);
        }
        yield return new WaitForSeconds(5);
        attacking = false;
    }

}
