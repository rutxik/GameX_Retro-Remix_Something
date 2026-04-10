// ─────────────────────────────────────────────────────────────────────────────
// PlayerHealth.cs  –  Minimal stub.  Replace with your real implementation.
// ─────────────────────────────────────────────────────────────────────────────
using UnityEngine;
using System.Collections;


public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"[Player] Took {amount} dmg. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Debug.Log("[Player] Died!");
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// CameraShake.cs  –  Simple singleton. Wire up to your camera however you like.
// ─────────────────────────────────────────────────────────────────────────────

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <param name="duration">Seconds the shake lasts</param>
    /// <param name="magnitude">Displacement in world units</param>
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 origin = transform.localPosition;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(origin.x + x, origin.y + y, origin.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = origin;
    }
}
