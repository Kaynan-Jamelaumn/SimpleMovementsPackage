﻿using UnityEngine;
public class JumpingState : MovementState
{
    public JumpingState(MovementContext context, MovementStateMachine.EMovementState estate) : base(context, estate)
    {
        MovementContext Context = context;
    }
    public override void EnterState()
    {
        Context.MovementModel.CurrentSpeed = Context.MovementModel.Speed;
    }
    public override void ExitState() { }
    public override void UpdateState() {
        GetNextState();
    }
    public override
    MovementStateMachine.EMovementState GetNextState()
    {
        if (Context.MovementController.IsGrounded())
            return StateKey;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude == 0)
            return MovementStateMachine.EMovementState.Idle;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0 && Context.PlayerInput.Player.Sprint.IsPressed())//Context.PlayerInput.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
            return MovementStateMachine.EMovementState.Running;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0 && Context.PlayerInput.Player.Crouch.IsPressed())//Context.PlayerInput.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
            return MovementStateMachine.EMovementState.Crouching;
        if (Context.PlayerInput.Player.Movement.ReadValue<Vector2>().sqrMagnitude > 0)//Context.PlayerInput.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
            return MovementStateMachine.EMovementState.Walking;

        return StateKey;
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }

}