using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(returntostart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator returntostart() 
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(0);
    }
}
