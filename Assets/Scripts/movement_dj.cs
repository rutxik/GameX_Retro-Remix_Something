using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class movement_dj : MonoBehaviour
{

    public Rigidbody2D PlayerRigidBody;
    public BoxCollider2D GroundCheckCol;
    //public Sprite[] sprites;
    //public GameObject jumps;
    public SpriteRenderer sr;
    //public GameObject idles;
    //public GameObject runs;
    bool grounded;
    bool Jumping;
    public LayerMask GroundLayer;
    [Header("Stats")]
    public float MovementSpeed = 8.5f;
    public float JumpHeight = 4.75f;
    public float JumpModifier = 1.12f;
    public int MaxHealth;
    public static int Health;
    int JumpTimer;
    public Slider healthbar;
    bool CanDoubleJump;
    bool buffered;
    public GameObject hammer;
    bool attacking;
    public Transform hammerbeforeattackingposition;
    public Transform hammerafterattackingposition;
    public GameObject playerattack;
    public bool invincible;

    // Start is called before the first frame update
    void Start()
    {
        invincible = false;
        Health = MaxHealth - 10;
        CanDoubleJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

        CheckJumpStart();

        CheckJumpEnd();

        if (Input.GetMouseButtonDown(0) && !attacking)
        {
            StartCoroutine(hammerattack());
        }
    }

    private void FixedUpdate()
    {

        ApplyVelocity();

        //UpdateAnimation(); //PLEASE change this to like using the animator or something this function SUCKS ASS

        AlignRotation();

        UpdateHealthbar();
    }

    void CheckGrounded()
    {
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(0, -1), 0.6f,GroundLayer);
        grounded = GroundCheckCol.IsTouchingLayers(GroundLayer);
        if (grounded)
        {
            CanDoubleJump = true;
        }
    }

    void CheckJumpStart()
    {
        if (grounded && isJumpInputDown())
        {
            Jump();
        }
        else if (CanDoubleJump && isJumpInputDown())
        {
            Jump();
            CanDoubleJump = false;
        }
    }

    void CheckJumpEnd()
    {
        if (Jumping && JumpTimer > 0)
        {
            if (isJumpInput())
            {
                Jumping = true;
            }
            else
            {
                Jumping = false;
                JumpTimer = -1;
            }
        }
        else
        {
            Jumping = false;
        }
    }

    void Jump()
    {
        JumpTimer = 20;
        Jumping = true;
        PlayerRigidBody.linearVelocity = new Vector2(PlayerRigidBody.linearVelocity.x, JumpHeight);
    }

    bool isJumpInput()
    {
        return Input.GetKey(KeyCode.Space) || (Input.GetKey(KeyCode.W));
    }
    bool isJumpInputDown()
    {
        return Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.W));
    }

    void AlignRotation()
    {
        transform.rotation = Quaternion.Euler(0, (PlayerRigidBody.linearVelocity.x > 5f) ? 0 : (PlayerRigidBody.linearVelocity.x < -5f) ? 180 : transform.rotation.eulerAngles.y, 0);
    }

    void UpdateHealthbar()
    {
        print(Health);
        healthbar.value = (float)((Health + 10.0) / MaxHealth);
        if (Health < 10) SceneManager.LoadScene(0);
    }

    void ApplyVelocity()
    {
        PlayerRigidBody.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * MovementSpeed, PlayerRigidBody.linearVelocity.y);

        if (Jumping)
        {
            PlayerRigidBody.linearVelocity += new Vector2(0, JumpModifier * Mathf.Clamp(JumpTimer / 20f, 0f, 1f));
        }

        JumpTimer--;
    }

    IEnumerator hammerattack()
    {
        attacking = true;
        playerattack.SetActive(true);
        hammer.SetActive(true);
        hammer.transform.DORotate(hammerafterattackingposition.rotation.eulerAngles,0.3f).SetEase(Ease.InOutBack);
        yield return new WaitForSeconds(0.3f);
        hammer.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        hammer.transform.rotation = hammerbeforeattackingposition.rotation;
        playerattack.SetActive(false);
        attacking = false;
    }

    public void TakeDamageFunc(int damage)
    {
        StartCoroutine(TakeDamage(damage));
    }

    IEnumerator TakeDamage(int damage)
    {
        Health -= damage;
        invincible = true;
        sr.DOFade(0, 0);
        Time.timeScale = 0f;    
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        sr.DOFade(1, 0);
        yield return new WaitForSecondsRealtime(0.1f);
        sr.DOFade(0, 0); 
        yield return new WaitForSecondsRealtime(0.1f);
        sr.DOFade(1, 0);
        yield return new WaitForSecondsRealtime(0.1f);
        sr.DOFade(0, 0); 
        yield return new WaitForSecondsRealtime(0.1f);
        sr.DOFade(1, 0);
        invincible = false;
    }

    /*
    void UpdateAnimation()
    {
        if (grounded && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
        {
            runs.SetActive(true);
            jumps.SetActive(false);
            idles.SetActive(false);
        }
        else if (grounded)
        {
            runs.SetActive(false);
            jumps.SetActive(false);
            idles.SetActive(true);
        }
        else
        {
            runs.SetActive(false);
            jumps.SetActive(true);
            idles.SetActive(false);
            if (PlayerRigidBody.velocity.y > 1f)
            {
                jumpsr.sprite = sprites[0];
            }
            else if (PlayerRigidBody.velocity.y < -1f)
            {
                jumpsr.sprite = sprites[2];
            }
            else
            {
                jumpsr.sprite = sprites[1];
            }
        }
    }
    */
}
