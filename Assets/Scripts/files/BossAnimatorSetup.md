# Boss Animator Setup Guide
## Unity Animator State Machine — Donkey Kong Style Boss

---

## Parameters to Create

| Name         | Type    | Set By                    |
|--------------|---------|---------------------------|
| IsGrounded   | Bool    | BossController            |
| Jump         | Trigger | BossController            |
| Smash        | Trigger | BossController            |
| Throw        | Trigger | BossController            |
| Hurt         | Trigger | BossController            |
| Die          | Trigger | BossController            |
| ChestBeat    | Trigger | BossAnimationController   |
| RainAttack   | Trigger | BossAnimationController   |
| Speed        | Float   | BossAnimationController   |

---

## State Machine Layout

```
[Any State] ──Hurt──────────────────────────────► [Hurt]      ──► (return to idle)
[Any State] ──Die───────────────────────────────► [Die]       (no exit)

                    ┌──────────────────────────────────────────────┐
                    │               BASE LAYER                     │
                    │                                              │
              ┌─────▼──────┐   ChestBeat    ┌────────────────┐    │
              │            │ ─────────────► │   ChestBeat    │    │
              │    Idle    │ ◄───(exit)──── │  (has exit)    │    │
              │ (blend w/  │               └────────────────┘    │
              │  Speed)    │   Throw        ┌────────────────┐    │
              │            │ ─────────────► │  ThrowBarrel   │    │
              │            │ ◄───(exit)──── │  (has exit)    │    │
              │            │               └────────────────┘    │
              │            │   RainAttack   ┌────────────────┐    │
              │            │ ─────────────► │  RainAttack    │    │
              │            │ ◄───(exit)──── │  (has exit)    │    │
              └─────┬──────┘               └────────────────┘    │
                    │                                              │
                    │ Jump (Trigger)                               │
                    ▼                                              │
              ┌─────────────┐                                      │
              │  JumpRise   │  IsGrounded = true                   │
              │             │ ──────────────────────────────────►  │
              │  (no exit   │                         ┌───────────┐│
              │   time)     │                         │  Smash /  ││
              └─────────────┘                         │  Landing  ││
                                                      │(has exit) ││
                                                      └─────┬─────┘│
                                                            │       │
                                                            ▼       │
                                                       back to Idle ─┘
```

---

## State Details

### Idle
- **Clip:** `Boss_Idle`
- **Loop:** Yes
- **Blend Tree (optional):** Blend `Boss_Idle` → `Boss_Walk` using the `Speed` float  
  - Speed = 0 → Idle pose  
  - Speed = 3 → Walk cycle (if boss paces the platform)

### ChestBeat
- **Clip:** `Boss_ChestBeat`
- **Loop:** No
- **Has Exit Time:** Yes (let full animation play)
- **Transitions out:** Exit Time → back to Idle
- **Animation Events to add:**
  - Frame where fist hits chest → `OnChestBeatImpactFrame()`
  - (Optional) second hit frame → `OnChestBeatImpactFrame()` again

### ThrowBarrel
- **Clip:** `Boss_Throw`
- **Loop:** No
- **Has Exit Time:** Yes
- **Animation Events to add:**
  - Frame where barrel leaves hand → `OnThrowReleaseFrame()`

### JumpRise
- **Clip:** `Boss_Jump` (or separate `Boss_JumpRise` + `Boss_JumpFall`)
- **Loop:** No for rise, Yes for fall
- **Transition to Fall:** When vertical velocity < 0 (use a script or sub-state machine)
- **Animation Events to add:**
  - First frame of liftoff → `OnJumpLiftoffFrame()`

### Smash / Landing
- **Clip:** `Boss_Smash`
- **Loop:** No
- **Has Exit Time:** Yes
- **Animation Events to add:**
  - Frame of ground impact → `OnSmashImpactFrame()`

### RainAttack
- **Clip:** `Boss_RainAttack` (or reuse `Boss_ChestBeat` temporarily)
- **Loop:** No
- **Has Exit Time:** Yes

### Hurt
- **Clip:** `Boss_Hurt`
- **Loop:** No
- **Has Exit Time:** Yes (short clip, ~0.3s)
- **Transition from:** Any State (high priority)

### Die
- **Clip:** `Boss_Die`
- **Loop:** No
- **Has Exit Time:** Yes (plays once, never exits)
- **Animation Events to add:**
  - Final frame → `OnDeathAnimationComplete()`

---

## Small Code Changes to BossController.cs

Add a public property so `BossAnimationController` can read health cleanly,
and hook in the notification calls:

```csharp
// ── In BossController.cs ──────────────────────────────────

// 1. Expose current health
public float CurrentHealth => currentHealth;

// 2. In EnterPhase2(), add:
void EnterPhase2()
{
    isPhase2 = true;
    rainTimer = rainCooldown * 0.5f;
    throwCooldown = Mathf.Max(throwCooldown - 0.5f, 1f);
    GetComponent<BossAnimationController>()?.NotifyPhase2(); // ← ADD THIS
    Debug.Log("[Boss] Phase 2!");
}

// 3. In Die(), add:
void Die()
{
    isDead = true;
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Static;
    anim.SetTrigger(HashDie);
    GetComponent<BossAnimationController>()?.NotifyDead();  // ← ADD THIS
    Debug.Log("[Boss] Defeated!");
}

// 4. At the start of BarrelRain coroutine, add:
IEnumerator BarrelRain()
{
    GetComponent<BossAnimationController>()?.TriggerRainAttackAnim(); // ← ADD THIS
    for (int i = 0; i < rainBarrelCount; i++) { ... }
}
```

---

## Tips

- **Transitions with Trigger parameters:** Set "Has Exit Time" to **false** and
  "Transition Duration" to **0** for Hurt and Die — they should interrupt immediately.
- **ChestBeat should NOT interrupt Throw/Jump/Smash:** Only allow the ChestBeat
  transition from the Idle state, not from Any State.
- **Jump sub-state machine:** If you want separate Rise/Fall clips, make a
  sub-state machine: `JumpRise → JumpFall` using a `VerticalVelocity` float parameter
  (`rb.linearVelocity.y`, written each frame in BossAnimationController).
