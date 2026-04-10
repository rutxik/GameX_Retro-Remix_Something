using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playeranim_cutscene2: MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float pauseBetweenSteps = 0.2f;
    public GameObject textbox;
    public SpriteRenderer sr;
    public Transform player;
    Vector3 pos;
    bool shaking = false;
    float starttime;
    void Start()
    {
        // Start the sequence as soon as the game begins
        StartCoroutine(PlayMoveSequence());
    }

    IEnumerator PlayMoveSequence()
    {
         // 4. Stop
        sr.DOFade(100, 3);
        starttime = Time.time;
        shaking = true;
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(0);
    }

    IEnumerator MoveInDirection(Vector2 direction, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            // Move the sprite based on speed and time
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    }
    private void Update()
    {
        if (shaking)
        {
            player.position = pos + new Vector3(Mathf.Sin((starttime - Time.time) * 20), 0, 0);
        }
    }
}