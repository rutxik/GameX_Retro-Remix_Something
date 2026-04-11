using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class boss_anim_cutscene : MonoBehaviour
{

    public Transform xpos;
    public GameObject textbox;
    public AudioClip clip;
    public AudioSource source;
    public Transform healthbar;
    public Transform healthbarpos;
    public AudioClip approaching;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //healthbar.DOMoveY(-443.53f, 1.5f).SetEase(Ease.OutCirc);
        StartCoroutine(Boss_Move());
        xpos.DOMoveX(15.21f, 1.5f).SetEase(Ease.OutCirc);
        textbox.SetActive(false);
    }

    IEnumerator Boss_Move()
    {
        yield return new WaitForSeconds(13.4f);
        source.PlayOneShot(approaching);
        xpos.DOMoveX(10.08f, 1.5f);
        source.PlayOneShot(clip);
        healthbar.DOMoveY(healthbarpos.position.y, 1.5f).SetEase(Ease.OutCirc);
        yield return new WaitForSeconds(0.2f);
        textbox.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        //textbox.SetActive(false);
        SceneManager.LoadScene(3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
