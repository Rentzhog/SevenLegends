using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    public AttackScriptableObject sideAttack_Light;
    public AttackScriptableObject neutralAttack_Light;

    public Collider2D combatHitbox;

    Animator animator;

    AnimatorState attackState;
    AnimatorState stunState;

    PlayerController playerController;

    Coroutine currentCoroutine;

    bool setTriggerNextFrame;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        AnimatorController animatorController = (AnimatorController)animator.runtimeAnimatorController;

        foreach (ChildAnimatorState state in animatorController.layers[0].stateMachine.states) {
            if(state.state.name == "Attack") {
                attackState = state.state;
            }

            if (state.state.name == "Stun") {
                stunState = state.state;
            }
        }

    }

    private void Update() {

    }

    public void LightAttack() {
        // GROUND ATTACK IF ATTACKING WHILE DASHING
        attackState.motion = sideAttack_Light.attackAnimation;
        StartCoroutine(SetTriggerNextFrame());

    }

    public void HeavyAttack() {
        attackState.motion = neutralAttack_Light.attackAnimation;
        StartCoroutine(SetTriggerNextFrame());
    }

    IEnumerator SetTriggerNextFrame() {
        yield return new WaitForEndOfFrame();
        if (playerController.canControll) {
            animator.SetTrigger("Punch");
        }
        yield return new WaitForEndOfFrame();
        animator.ResetTrigger("Punch");
    }

    public IEnumerator HitPlayer(float stunTime, Transform attackingPlayer) {
        playerController.ResetInputs();
        playerController.EnableGravity();
        combatHitbox.gameObject.SetActive(false);

        playerController.canControll = false;

        animator.ResetTrigger("Punch");
        animator.SetTrigger("Stun");
        animator.SetBool("Stunned", true);

        playerController.lowFriction = true;
        playerController.stunned = true;

        print(attackingPlayer.localScale.x);
        float xVelocity = 20;
        playerController.GetComponent<Rigidbody2D>().velocity = new Vector2(xVelocity * attackingPlayer.lossyScale.x, 0);

        yield return new WaitForSeconds(stunTime);

        playerController.canControll = true;
        playerController.stunned = false;

        animator.SetBool("Stunned", false);
    }

    public void OnHitboxEnter(Collider2D hitBox, Collider2D other) {
        PlayerCombat playerWeHit = other.GetComponent<PlayerCombat>();

        if (playerWeHit) {
            if(playerWeHit.currentCoroutine != null) {
                playerWeHit.StopCoroutine(playerWeHit.currentCoroutine);
            }

            playerWeHit.currentCoroutine = playerWeHit.StartCoroutine(playerWeHit.HitPlayer(1f, hitBox.transform));
        }
    }
}
