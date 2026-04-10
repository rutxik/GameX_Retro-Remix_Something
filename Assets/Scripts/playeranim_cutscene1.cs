using UnityEngine;
using System.Collections;

public class playeranim_cutscene1 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float pauseBetweenSteps = 0.2f;

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
        yield return new WaitForSeconds(4.0f);



        // 3. Move to the left
        yield return StartCoroutine(MoveInDirection(Vector2.right, 1.2f));
        yield return new WaitForSeconds(pauseBetweenSteps);

        // 4. Stop
        Debug.Log("Movement Sequence Complete");
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
}