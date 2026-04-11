using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class playeranim_cutscene2: MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float pauseBetweenSteps = 0.2f;
    public GameObject textbox;
    public GameObject textbox_new;
    public SpriteRenderer sr;
    public Transform player;
    Vector3 pos;
    bool shaking = false;
    float starttime;
    void Start()
    {
        textbox.SetActive(false);
        textbox_new.SetActive(false);
        // Start the sequence as soon as the game begins
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        StartCoroutine(PlayMoveSequence());
        pos = new Vector3(4.5f, 0.5f, 0);
    }

    IEnumerator PlayMoveSequence()
    {
         // 4. Stop
        sr.DOFade(1, 3);
        starttime = Time.time;
        shaking = true;
        yield return new WaitForSeconds(3.0f);
        shaking = false;
        textbox.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        textbox.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        textbox_new.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        textbox_new.SetActive(false);
        //SceneManager.LoadScene(3); //change this as it directly loads the new scene
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
            float elapsed = Time.time - starttime;
            float intensity = Mathf.Clamp01(1 - (elapsed / 3f));
            player.position = pos + new Vector3(Mathf.Sin(elapsed * 20) * intensity, 0, 0);
        }
    }
}