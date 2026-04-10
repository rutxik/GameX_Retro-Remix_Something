using System.Collections;
using UnityEngine;

/// <summary>
/// NormalBarrelController – attach to the normal barrel prefab.
/// Rolls along the ground, bounces off walls, can be parried back at the boss.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class NormalBarrelController : MonoBehaviour
{
    bool hasHitPlayer = false;

    
    #region Inspector

    [Header("General")]
    [Tooltip("How long before this barrel self-destructs")]
    public float lifetime         = 8f;
    [Tooltip("Damage dealt to the player on contact")]
    public float playerDamage     = 15f;

    [Header("Rolling")]
    [Tooltip("Speed the barrel rolls along the ground")]
    public float rollSpeed        = 5f;

    [Header("Parry")]
    [Tooltip("Speed the barrel travels back toward the boss after a parry")]
    public float parryReturnSpeed = 14f;

    [Header("Layers")]
    public LayerMask groundLayer;

    #endregion

    #region Runtime (set by BossController)

    [HideInInspector] public BossController owner;
    [HideInInspector] public bool canBeParried = true;
    [HideInInspector] public bool isRainBarrel = false;

    #endregion

    #region Private

    Rigidbody2D rb;
    bool        hasBeenParried = false;
    bool        isAlive        = true;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D col)
{
    if (col.gameObject.CompareTag("Player"))
    {
        if (!isAlive) return;
        isAlive = false;  // ← stops any further collisions immediately
        movement_dj player = col.gameObject.GetComponent<movement_dj>();
        if (player != null) movement_dj.Health -= (int)playerDamage;
        Destroy(gameObject);
        return;
    }
}

    #endregion

    #region Parry

    /// <summary>
    /// Call this from the player's parry hitbox when it overlaps with this barrel.
    /// Returns true if the parry was accepted.
    /// </summary>
    public bool TryParry()
    {
        if (!canBeParried || hasBeenParried || !isAlive) return false;

        hasBeenParried = true;
        canBeParried   = false;

        if (owner != null)
        {
            Vector2 dir = ((Vector2)(owner.transform.position - transform.position)).normalized;
            rb.linearVelocity = dir * parryReturnSpeed;
            rb.gravityScale   = 0.2f;
        }

        Debug.Log("[NormalBarrel] Parried! Flying back at the boss.");
        return true;
    }

    #endregion

    #region Gizmos
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
#endif
    #endregion
}
