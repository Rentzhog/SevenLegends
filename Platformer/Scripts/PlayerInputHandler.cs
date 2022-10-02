using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour {

    private PlayerInput playerInput;
    private PlayerController playerController;
    private PlayerCombat playerCombat;

    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction fastFallingAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;


    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
        playerCombat = GetComponent<PlayerCombat>();

        playerInput.ActivateInput();

        movementAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        fastFallingAction = playerInput.actions["Fast Falling"];
        lightAttackAction = playerInput.actions["Light Attack"];
        heavyAttackAction = playerInput.actions["Heavy Attack"];

        jumpAction.started += OnJump;
        dashAction.started += OnDash;
        lightAttackAction.started += OnLightAttack;
        heavyAttackAction.started += OnHeavyAttack;

    }

    private void Update() {

        if (!playerController.canControll) return;

        playerController.rawInput = movementAction.ReadValue<Vector2>();

        playerController.holdingJump = jumpAction.IsPressed();

        playerController.fastFalling = (fastFallingAction.IsPressed());

    }

    void OnJump(CallbackContext ctx) {
        if (!playerController.canControll) return;
        print("jump");
        if (playerController.currentJumpsLeft > 0) {
            playerController.Jump();
        }
    }

    void OnDash(CallbackContext ctx) {
        if (!playerController.canControll) return;
        print("DASh");
        PlayerController p = playerController;

        if (!(p.onLeftWall || p.onRightWall) && p.canDash && p.rawInput.magnitude > 0.1f) {
            p.StartCoroutine(p.Dash());
        }
    }

    void OnLightAttack(CallbackContext ctx) {
        if (!playerController.canControll) return;
        print("Light ATtack");
        playerCombat.LightAttack();
    }

    void OnHeavyAttack(CallbackContext ctx) {
        if (!playerController.canControll) return;
        print("Light ATtack");
        playerCombat.HeavyAttack();
    }

}
