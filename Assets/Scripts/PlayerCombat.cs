using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class PlayerCombat : MonoBehaviour
{

    public AnimationClip sideAttack_Light;
    public AnimationClip neutralAttack_Light;

    public CapsuleCollider2D combatHitbox;

    Animator animator;
    AnimatorController animController;
    AnimatorState groundAttackState;

    bool setTriggerNextFrame;

    // Start is called before the first frame update
    void Start()
    {
        
        animator = GetComponent<Animator>();
        animController = (AnimatorController)animator.runtimeAnimatorController;
        foreach(ChildAnimatorState state in animController.layers[0].stateMachine.states) {
            if(state.state.name == "Ground Punch") {
                groundAttackState = state.state;
                break;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Ground Punch")) {
            print("ye");
            return;
        }

        animator.ResetTrigger("Punch");
        if (setTriggerNextFrame) {
            animator.SetTrigger("Punch");
            setTriggerNextFrame = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.J)) {

            if(groundAttackState.motion != sideAttack_Light) {
                groundAttackState.motion = sideAttack_Light;
                setTriggerNextFrame = true;
            }
            else {
                animator.SetTrigger("Punch");
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) {

            if (groundAttackState.motion != neutralAttack_Light) {
                groundAttackState.motion = neutralAttack_Light;
                setTriggerNextFrame = true;
            }
            else {
                animator.SetTrigger("Punch");
            }
        }
    }
}
