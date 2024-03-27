﻿using UnityEngine;
public class RollState : MovementState
{
    public RollState(MovementContext context, MovementStateMachine.EMovementState estate) : base(context, estate)
    {
        MovementContext Context = context;
    }
    private float startTime;
    public override void EnterState()
    {
       Context.AnimationModel.IsRolling = true;
       Context.StatusController.RollController.Roll();
       startTime = Time.time;
        
        
    }
    public override void ExitState() 
    {
        Context.AnimationModel.IsRolling = false;
    }
    public override void UpdateState() {
    }
    public override
    MovementStateMachine.EMovementState GetNextState()
    {
        if (Time.time - startTime < Context.StatusController.RollModel.RollDuration) return StateKey;

        if (Context.PlayerInput.Player.Jump.triggered)
            return MovementStateMachine.EMovementState.Jumping;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude == 0)
            return MovementStateMachine.EMovementState.Idle;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0 && Context.PlayerInput.Player.Sprint.IsPressed() && (!Context.MovementModel.ShouldConsumeStamina || Context.MovementModel.ShouldConsumeStamina && Context.StatusController.StaminaManager.HasEnoughStamina(Context.MovementModel.AmountOfSprintStaminaCost)))
            return MovementStateMachine.EMovementState.Running;

        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0 && Context.PlayerInput.Player.Crouch.IsPressed() && (!Context.MovementModel.ShouldConsumeStamina || Context.MovementModel.ShouldConsumeStamina && Context.StatusController.StaminaManager.HasEnoughStamina(Context.MovementModel.AmountOfCrouchStaminaCost)))
            return MovementStateMachine.EMovementState.Crouching;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0)
            return MovementStateMachine.EMovementState.Walking;

        if (!Context.AnimationModel.IsRolling)
            return MovementStateMachine.EMovementState.Idle;


        return StateKey;
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

}