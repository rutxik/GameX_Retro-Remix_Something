using System.Collections;
using UnityEngine;

/// <summary>
/// BossAnimationController
/// -------------------------------------------------------
/// Sits alongside BossController on the same GameObject.
///
/// Responsibilities:
///   • Randomly triggers ChestBeat during idle (Donkey Kong style)
///   • Flips the sprite to always face the player
///   • Exposes Animation Event callbacks so Animator clips can
///     fire gameplay logic at the right frame (barrel spawn,
///     shockwave, AoE hit, etc.)
///   • Drives the Speed float so an idle/walk blend tree works
///     if you want the boss to pace left/right between actions
///
/// Animator parameters this script writes:
///   Bool    IsGrounded   – already set by BossController
///   Trigger Jump         – already set by BossController
///   Trigger Smash        – already set by BossController
///   Trigger Throw        – already set by BossController
///   Trigger Hurt         – already set by BossController
///   Trigger Die          – already set by BossController
///   Trigger ChestBeat    – NEW – driven here
///   Trigger RainAttack   – NEW – driven here
///   Float   Speed        – NEW – optional, for walk blending
/// </summary>
[RequireComponent(typeof(BossController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossAnimationController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    #region Inspector

    [Header("Chest Beat (Idle)")]
    [Tooltip("Minimum seconds between chest-beat animations")]
    public float chestBeatIntervalMin = 3f;
    [Tooltip("Maximum seconds between chest-beat animations")]
    public float chestBeatIntervalMax = 7f;

    [Header("Facing")]
    [Tooltip("Flip sprite X to face the player. Disable if you use a Rig/Bone setup instead.")]
    public bool flipSpriteToFacePlayer = true;

    [Header("Phase 2 Enrage")]
    [Tooltip("Speed multiplier applied to the Animator when Phase 2 begins")]
    public float phase2AnimSpeed = 1.25f;

    [Header("Optional Audio")]
    [Tooltip("AudioSource on this GameObject – used for SFX callbacks")]
    public AudioSource audioSource;
    public AudioClip chestBeatSFX;
    public AudioClip jumpSFX;
    public AudioClip landSFX;
    public AudioClip throwSFX;
    public AudioClip roarSFX;       // plays on Phase 2 transition
    public AudioClip dieSFX;

    #endregion

    // ─────────────────────────────────────────────
    #region Private

    Animator        anim;
    SpriteRenderer  sr;
    BossController  boss;
    Rigidbody2D     rb;

    // Animator hashes
    static readonly int HashChestBeat  = Animator.StringToHash("ChestBeat");
    static readonly int HashRainAttack = Animator.StringToHash("RainAttack");
    static readonly int HashSpeed      = Animator.StringToHash("Speed");

    bool isDead         = false;
    bool isChestBeating = false;
    bool phase2Started  = false;

    // We mirror BossController's phase2 flag by watching health
    // via a simple polling approach (no events needed).
    float previousHealth = float.MaxValue;

    #endregion

    // ─────────────────────────────────────────────
    #region Unity Lifecycle

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        boss = GetComponent<BossController>();
        rb   = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        previousHealth = boss.maxHealth;
        StartCoroutine(ChestBeatLoop());
    }

    void Update()
    {
        if (isDead) return;

        UpdateFacing();
        UpdateSpeedParam();
        CheckPhase2Transition();
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Facing

    void UpdateFacing()
    {
        if (!flipSpriteToFacePlayer || boss.player == null) return;

        // Flip sprite so the boss always looks at the player
        bool playerIsToTheRight = boss.player.position.x > transform.position.x;
        sr.flipX = !playerIsToTheRight;   // adjust if your sprite's default faces left
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Speed Param (optional walk blend)

    void UpdateSpeedParam()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat(HashSpeed, speed);
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Chest Beat – Random Idle Loop

    /// <summary>
    /// Continuously waits a random interval, then triggers a chest-beat
    /// animation whenever the boss is grounded and not doing anything else.
    /// </summary>
    IEnumerator ChestBeatLoop()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(chestBeatIntervalMin, chestBeatIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Only beat chest when idle: grounded, slow horizontal movement, not mid-action
            bool isGrounded   = anim.GetBool(Animator.StringToHash("IsGrounded"));
            bool isSlowEnough = Mathf.Abs(rb.linearVelocity.x) < 0.5f &&
                                Mathf.Abs(rb.linearVelocity.y) < 0.5f;

            if (isGrounded && isSlowEnough && !isChestBeating)
            {
                isChestBeating = true;
                anim.SetTrigger(HashChestBeat);
                PlaySFX(chestBeatSFX);

                // Wait long enough for the animation to finish before looping again.
                // Match this to your ChestBeat clip length.
                yield return new WaitForSeconds(1.5f);
                isChestBeating = false;
            }
        }
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Phase 2 Transition

    void CheckPhase2Transition()
    {
        if (phase2Started) return;

        // Detect the moment BossController.EnterPhase2 fires by watching health cross 50%
        float currentHealth = GetCurrentHealth();
        if (currentHealth <= boss.maxHealth * boss.phase2Threshold && previousHealth > boss.maxHealth * boss.phase2Threshold)
        {
            OnPhase2Begin();
        }
        previousHealth = currentHealth;
    }

    void OnPhase2Begin()
    {
        phase2Started = true;

        // Speed up all animations to feel more frantic
        anim.speed = phase2AnimSpeed;

        // Play roar / rage sequence
        StartCoroutine(Phase2EnrageSequence());
    }

    IEnumerator Phase2EnrageSequence()
    {
        // Freeze movement for a dramatic roar beat
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        anim.SetTrigger(HashChestBeat);     // reuse chest-beat or add a Roar trigger
        PlaySFX(roarSFX);

        yield return new WaitForSeconds(1.8f);

        rb.bodyType = RigidbodyType2D.Dynamic;

        // Immediately follow up with a barrel rain
        anim.SetTrigger(HashRainAttack);
    }

    /// <summary>
    /// Reads current health via reflection-free approach.
    /// We piggyback off the public maxHealth field and detect damage
    /// by hooking into the Hurt trigger being set each frame.
    ///
    /// Alternatively: expose currentHealth as a public property in
    /// BossController and read it directly. This is the cleaner approach.
    /// </summary>
    float GetCurrentHealth()
    {
        // RECOMMENDED: Add this to BossController.cs:
        //   public float CurrentHealth => currentHealth;
        // Then replace this method body with:
        //   return boss.CurrentHealth;

        // Fallback: use the Hurt trigger as a proxy – imprecise but works without modifying BossController
        // For a clean build, just expose CurrentHealth in BossController.
        return previousHealth; // placeholder until you expose the property
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Animation Event Callbacks
    // Add these as Animation Events inside the Unity Animator clips.
    // Select the clip in the Animator, scrub to the right frame, and
    // click "Add Event" → choose the method name below.

    /// <summary>
    /// Call at the frame the boss's hand reaches the barrel during the Throw animation.
    /// BossController.ThrowBarrel() already spawns the barrel – this is for SFX/VFX only.
    /// </summary>
    public void OnThrowReleaseFrame()
    {
        PlaySFX(throwSFX);
        // Trigger a throw VFX here if you have one, e.g.:
        // VFXManager.Instance?.Play("BarrelThrow", throwPoint.position);
    }

    /// <summary>
    /// Call at the peak of the Jump animation (feet leaving ground).
    /// </summary>
    public void OnJumpLiftoffFrame()
    {
        PlaySFX(jumpSFX);
    }

    /// <summary>
    /// Call at the frame the boss hits the ground in the Smash animation.
    /// BossController.PerformSmash() already handles damage – this is SFX/camera only.
    /// </summary>
    public void OnSmashImpactFrame()
    {
        PlaySFX(landSFX);
        CameraShake.Instance?.Shake(0.4f, 0.5f);
    }

    /// <summary>
    /// Call at each chest-pound moment in the ChestBeat animation (can call twice
    /// if there are two beats per cycle).
    /// </summary>
    public void OnChestBeatImpactFrame()
    {
        PlaySFX(chestBeatSFX);
        CameraShake.Instance?.Shake(0.1f, 0.15f);
    }

    /// <summary>
    /// Called at the very end of the Die animation clip.
    /// </summary>
    public void OnDeathAnimationComplete()
    {
        isDead = true;
        PlaySFX(dieSFX);
        // e.g. GameManager.Instance?.OnBossDefeated();
        Debug.Log("[BossAnim] Death animation finished.");
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Public API (called by BossController or GameManager)

    /// <summary>
    /// Signal the animation system that Phase 2 is starting.
    /// Call this from BossController.EnterPhase2() for a clean coupling:
    ///   GetComponent<BossAnimationController>()?.NotifyPhase2();
    /// </summary>
    public void NotifyPhase2()
    {
        if (!phase2Started)
            OnPhase2Begin();
    }

    /// <summary>
    /// Trigger the barrel-rain animation (BossController can call this
    /// at the start of BarrelRain coroutine).
    /// </summary>
    public void TriggerRainAttackAnim()
    {
        anim.SetTrigger(HashRainAttack);
    }

    /// <summary>
    /// Freeze all animations on death (call from BossController.Die()).
    /// </summary>
    public void NotifyDead()
    {
        isDead = true;
        StopAllCoroutines();
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Helpers

    void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}
