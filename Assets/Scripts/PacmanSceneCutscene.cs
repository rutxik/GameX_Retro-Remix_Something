using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanSceneCutscene : MonoBehaviour
{

    public Transform cam;
    public Transform cameraposition;
    public Tilemap sp1;
    public Tilemap sp2;
    public AudioSource music;
    public GameObject boss;
    bool colorturningon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(startscene());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (colorturningon)
        {

            sp1.color = (Vector4)sp1.color + new Vector4(0, 0, 0, 0.01f);
            sp2.color = (Vector4)sp1.color + new Vector4(0, 0, 0, 0.01f);
        }
    }

    IEnumerator startscene()
    {
        cam.transform.DOMove(cameraposition.position, 1f);
        yield return new WaitForSeconds(1);
        boss.SetActive(true);
        colorturningon = true;
        music.Play();
    }
}
