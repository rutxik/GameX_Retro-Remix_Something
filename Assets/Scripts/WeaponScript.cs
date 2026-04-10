using Unity.Hierarchy;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public Collider2D collision;
    public bool isParrying = false;
    public bool isAttacking = false;
    public int AttackTimer;
    [SerializeField] private int cooldown;
    [SerializeField] private int ParryWindow;
    public LayerMask l1;
    Animator animator;
    SpriteRenderer sr;

    private void Awake()
    {
        collision = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();   
    }
    void Start()
    {
      sr.enabled = false;  
    }

    void Update()
    {
        acceptinput();
        if (AttackTimer <= 0)
        {
            sr.enabled = false;
        }
    }
    void acceptinput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)&& !isAttacking)
        {
            if (timercheck())
            {
                attack();
            }
            
        }
    }

    public void EnableHitbox()
    {
        collision.enabled = true;
    }

    public void DisableHitbox()
    {
        collision.enabled = false;
    }
    private void FixedUpdate()
    {
        AttackTimer = Mathf.Max(0,AttackTimer-1);
        isAttacking = AttackTimer > 0; 
        parrycheck();

    }

    bool inParryWindow()
    {
        return isAttacking && AttackTimer > (cooldown - ParryWindow);
    }

    void parrycheck()
    {
        if (inParryWindow()&& collision.IsTouchingLayers(l1))
        { 
            isParrying = true;
        }
        else
        {
            isParrying = false;
        }
        
    }

    bool timercheck()
    {
        return AttackTimer <= 0;
    }

    void attack()
    {
        if (!isAttacking)
        {
            sr.enabled = true;
            isAttacking = true;
            AttackTimer = cooldown;
            animator.SetTrigger("AttackTrigger");
        }
    }
};

