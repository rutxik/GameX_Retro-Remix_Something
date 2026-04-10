using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playeranim_cutscene1 : MonoBehaviour
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
        // 1. Move to the X axis (Right) a little
        // Parameters: (Direction, Duration)
        yield return StartCoroutine(MoveInDirection(Vector2.right, 0.57f));
        yield return new WaitForSeconds(pauseBetweenSteps);



        yield return StartCoroutine(MoveInDirection(new Vector2(1, -1).normalized, 0.2f));
        yield return new WaitForSeconds(pauseBetweenSteps);

        // 1. Move to the X axis (Right) a little
        // Parameters: (Direction, Duration)
        yield return StartCoroutine(MoveInDirection(Vector2.right, 0.1f));
        yield return new WaitForSeconds(pauseBetweenSteps);



        yield return StartCoroutine(MoveInDirection(new Vector2(1, -1).normalized, 0.2f));
        yield return new WaitForSeconds(pauseBetweenSteps);

        yield return StartCoroutine(MoveInDirection(Vector2.right, 0.3f));
        yield return new WaitForSeconds(pauseBetweenSteps);

        // 2. Move diagonally top-left
        // Vector2(-1, 1) represents left and up
        yield return StartCoroutine(MoveInDirection(new Vector2(1, 1).normalized, 0.8f));
        textbox.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        textbox.SetActive(false);
        


        // 3. Move to the left
        yield return StartCoroutine(MoveInDirection(Vector2.right, 1.2f));
        yield return new WaitForSeconds(pauseBetweenSteps);
        pos = transform.position;
        

        // 4. Stop
        sr.DOFade(0, 3);
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
            player.position = pos + new Vector3(Mathf.Sin((Time.time - starttime) * 10), 0, 0);
        }
    }
}