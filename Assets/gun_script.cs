using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class gun : MonoBehaviour
{

    public Rigidbody2D myRigidbody2d;

    public float minY = 1000;   // lower limit
    public float maxY = 200;    // upper limit (lower 3rd-ish)
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 6f; // bullets per second
    private float nextTimeToFire = 0f;
    public AudioSource audioSource;
    public AudioClip shootSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Clamp logic (only move if within range)
        //if(mouseWorld.y> minY && mouseWorld.y<maxY)
        //{
            //Vector2 transform = new Vector2(mouseWorld.x, mouseWorld.y);
            myRigidbody2d.MovePosition(mouseWorld);



        if (Input.GetKey(KeyCode.Space) && Time.time >= nextTimeToFire)
        {
            Shoot();
            nextTimeToFire = Time.time + 1f / fireRate;
        }


    }
    void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        audioSource.PlayOneShot(shootSound);
    }
}
