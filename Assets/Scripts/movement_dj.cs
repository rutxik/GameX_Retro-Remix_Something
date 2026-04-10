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
    //public SpriteRenderer jumpsr;
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

    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
        CanDoubleJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

        CheckJumpStart();

        CheckJumpEnd();
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
        healthbar.value = (Health + 10) / 110f;
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
