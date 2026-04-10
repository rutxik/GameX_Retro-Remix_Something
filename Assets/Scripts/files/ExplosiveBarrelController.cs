using System.Collections;
using UnityEngine;

/// <summary>
/// ExplosiveBarrelController – attach to the explosive barrel prefab.
/// Spawned by DK at Phase 2 (below 50% HP).
/// Rolls along the ground like a normal barrel but:
///   • Explodes on hitting a wall (instead of bouncing)
///   • Explodes on hitting the player
///   • Can be parried back at the boss — explodes on boss hit
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ExplosiveBarrelController : MonoBehaviour
{
    #region Inspector

    [Header("General")]
    [Tooltip("How long before this barrel self-destructs")]
    public float lifetime          = 8f;
    [Tooltip("Damage dealt to the player on direct contact")]
    public float playerDamage      = 15f;

    [Header("Rolling")]
    [Tooltip("Speed the barrel rolls along the ground")]
    public float rollSpeed         = 5f;

    [Header("Explosion")]
    public float explosionRadius   = 2.5f;
    public float explosionDamage   = 50f;
    public GameObject explosionVFX;

    [Header("Parry")]
    public float parryReturnSpeed  = 14f;

    [Header("Layers")]
    public LayerMask playerLayer;

    #endregion

    #region Runtime (set by BossController)

    [HideInInspector] public BossController owner;
    [HideInInspector] public bool canBeParried = true; // explosive barrels CAN be parried

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
        if (!isAlive) return;

        // Hit the player → explode
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(playerDamage);
            Explode();
            return;
        }

        // Parried barrel hits the boss → explodes ON the boss
        if (hasBeenParried && col.gameObject.CompareTag("Boss"))
        {
            if (owner != null) owner.TakeParryDamage();
            Explode();
            return;
        }

        // Hit a wall → explode instead of bouncing
        if (col.gameObject.CompareTag("Wall"))
        {
            Explode();
            return;
        }

        // Hit ground → flatten out and roll at constant speed in same direction
        if (col.gameObject.CompareTag("Ground"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x > 0 ? rollSpeed : -rollSpeed, 0f);
            return;
        }
    }

    #endregion

    #region Parry

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

        Debug.Log("[ExplosiveBarrel] Parried! Flying back at the boss.");
        return true;
    }

    #endregion

    #region Explosion

    void Explode()
    {
        if (!isAlive) return;
        isAlive = false;

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // AOE damage in radius
        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (hit != null)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(explosionDamage);
        }

        CameraShake.Instance?.Shake(0.3f, 0.4f);
        Debug.Log("[ExplosiveBarrel] Boom!");
        Destroy(gameObject);
    }

    #endregion

    #region Gizmos
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
    #endregion
}
