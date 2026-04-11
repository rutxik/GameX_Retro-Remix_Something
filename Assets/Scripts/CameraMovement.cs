using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static float shakeamount;
    Vector3 originalposition;
    public static float startTime;
    public static bool enabled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enabled = true;
        originalposition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            transform.position = originalposition + new Vector3(0, Mathf.Sin((Time.time - startTime) * 5) * shakeamount, 0);
            shakeamount = Mathf.Clamp(shakeamount - 0.5f * Time.deltaTime, 0, 999);
        }
    }
}
