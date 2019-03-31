using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine;

public class ActorCharacterMovementSystem : ComponentSystem
{

    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public ComponentArray<Transform> Transform;
        public ComponentDataArray<ActorInput> ActorInput;
        [ReadOnly] public SharedComponentDataArray<Actor> Actor;
        [ReadOnly] public SharedComponentDataArray<CharacterMovement> CharacterMovement;
    }

    [Inject] private Data data;
    private Dictionary<int, float> jumpIntervals = new Dictionary<int, float>();
    private Dictionary<int, bool> isJumpings = new Dictionary<int, bool>();
    private float dt;
    private float animationTransitionRate = 0.1f;
    private float jumpInterval = 0.3f;
    private float deadPoint = 0.25f;
    private bool isGrounded = false;
    private bool isHeadFree = false;

    protected override void OnUpdate()
    {
        dt = Time.deltaTime;

        for (var i = 0; i < data.Length; i++)
        {
            // Data
            var transform = data.Transform[i];
            var entity = data.Entity[i];
            var actorInput = data.ActorInput[i];
            var actor = data.Actor[i];
            var characterMovement = data.CharacterMovement[i];

            //TO FIX LATER WHEN ECS MATURES
            var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();
            var animator = transform.GetComponentInChildren<Animator>();
            var rigidbody = transform.GetComponent<Rigidbody>();

            //Get Grounded state | Head State
            isGrounded = IsGrounded(actor, transform);
            isHeadFree = IsHeadFree(actor, transform);

            //Update Movement
            GetMovement(ref actorInput);
            OnFallMovement(transform, animationEventManager, animator, rigidbody, entity, actor, ref actorInput, characterMovement);
            OnJumpMovement(transform, animationEventManager, animator, rigidbody, entity, actor, ref actorInput, characterMovement);
            OnGroundMovement(transform, animationEventManager, animator, rigidbody, entity, actor, ref actorInput, characterMovement);

            //Write back updated input
            data.ActorInput[i] = actorInput;
        }
    }

    private void GetMovement(ref ActorInput actorInput)
    {
        if (actorInput.movement.x <= deadPoint && actorInput.movement.x >= -deadPoint && actorInput.movement.z <= deadPoint && actorInput.movement.z >= -deadPoint)
            actorInput.movement = Vector3.zero;
    }

    private void OnGroundMovement(Transform transform, AnimationEventManager animationEventManager, Animator animator, Rigidbody rigidbody, Entity entity, Actor actor, ref ActorInput actorInput, CharacterMovement characterMovement)
    {
        //Return if doing diffrent action | Not Grounded
        if (actorInput.actionIndex != 0 || !isGrounded || isJumpings[entity.Index])
            return;

        //Get movement type
        {
            if (!isHeadFree && actorInput.crouchPreviousFrame == 1)
                actorInput.crouch = 1;

            if (actorInput.movement.magnitude == 0 || actorInput.crouch == 1 && !isHeadFree)
                actorInput.sprint = 0;

            if (actorInput.sprint == 1)
                actorInput.crouch = 0;

            if (actorInput.walk == 1 && actorInput.sprint == 0 && actorInput.crouch == 0)
                actorInput.movement *= deadPoint;
        }

        //Move
        {
            //Set Velocity
            var velocity = actorInput.movement;
            velocity *= actorInput.sprint == 1 ? characterMovement.sprintSpeed : actorInput.crouch == 1 ? characterMovement.crouchSpeed : characterMovement.runSpeed;
            velocity.y = -1;
            rigidbody.velocity = velocity;
        }

        //Collider
        {
            //Set Collider
            if (actorInput.crouch == 1 && actorInput.crouchPreviousFrame == 0)
                ActorUtilities.UpdateCollider(actor, transform, "Crouching");
            else if (actorInput.crouch == 0 && actorInput.crouchPreviousFrame == 1)
                ActorUtilities.UpdateCollider(actor, transform, "Standing");
        }

        //Rotate
        {
            if (actorInput.movement.magnitude != 0 && actorInput.strafe == 0 || actorInput.movement.magnitude != 0 && actorInput.sprint == 1)
            {
                var lookRotation = Quaternion.LookRotation(actorInput.movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, dt * characterMovement.rotationSpeed);
            }
        }

        //Animate
        {
            //Is Sprinting
            if (actorInput.sprint == 1)
                actorInput.movement = new float3(0, 0, 2f);

            //Strafing
            else if (actorInput.strafe == 1)
                actorInput.movement = transform.InverseTransformDirection(actorInput.movement);

            //Not Strafing
            else actorInput.movement = new float3(0, 0, Mathf.Abs(actorInput.movement.magnitude));

            //Set Animator Properties
            actorInput.applyRootMotion = 0;
            animator.SetBool("crouch", actorInput.crouch == 1 && actorInput.sprint == 0);
            animator.SetFloat("movementX", actorInput.movement.x, animationTransitionRate, dt);
            animator.SetFloat("movementY", actorInput.movement.z, animationTransitionRate, dt);
            animator.SetFloat("movementAmount", actorInput.movement.magnitude);
        }

        //Sound
        if (animationEventManager.RequestEvent("footStep") && characterMovement.footStepAudioEvent != null && actorInput.movement.magnitude != 0)
            characterMovement.footStepAudioEvent.Play(transform.position);
    }

    private void OnFallMovement(Transform transform, AnimationEventManager animationEventManager, Animator animator, Rigidbody rigidbody, Entity entity, Actor actor, ref ActorInput actorInput, CharacterMovement characterMovement)
    {
        if (!isGrounded)
        {
            var newMovement = rigidbody.velocity;
            newMovement.y -= characterMovement.fallForce * dt;
            rigidbody.velocity = newMovement;

            if (actorInput.crouch == 1)
            {
                ActorUtilities.UpdateCollider(actor, transform, "Standing");
                actorInput.crouch = 0;
            }

            animator.SetBool("inAir", true);
        }
        else animator.SetBool("inAir", false);

    }

    private void OnJumpMovement(Transform transform, AnimationEventManager animationEventManager, Animator animator, Rigidbody rigidbody, Entity entity, Actor actor, ref ActorInput actorInput, CharacterMovement characterMovement)
    {
        //Save Jump Data
        if (!jumpIntervals.ContainsKey(entity.Index))
        {
            jumpIntervals.Add(entity.Index, jumpInterval);
            isJumpings.Add(entity.Index, false);
        }

        //Decrease jump interval if goruned
        if (isGrounded && rigidbody.velocity.y <= 0)
        {
            jumpIntervals[entity.Index] -= 1 * dt;
            isJumpings[entity.Index] = false;
        }

        //If were not grounded reset jump interval
        else
            jumpIntervals[entity.Index] = jumpInterval;

        //Do Jump
        if (actorInput.actionIndex == 0 && actorInput.actionToDoIndex == 1 && isGrounded && characterMovement.jumpForce != 0 && isHeadFree && jumpIntervals[entity.Index] <= 0)
        {
            var velocity = rigidbody.velocity;
            velocity.y = characterMovement.jumpForce;
            rigidbody.velocity = velocity;

            animator.SetTrigger("jump");

            isJumpings[entity.Index] = true;

            if (actorInput.crouch == 1)
                ActorUtilities.UpdateCollider(actor, transform, "standing");

            if (actorInput.movement.magnitude != 0)
                transform.forward = actorInput.movement;
        }

        //Sound
        {
            if (animationEventManager.RequestEvent("jumpGrunt") && characterMovement.jumpGruntAudioEvent != null)
                characterMovement.jumpGruntAudioEvent.Play(transform.position);
            else if (animationEventManager.RequestEvent("jumpStart") && characterMovement.jumpStartAudioEvent != null)
                characterMovement.jumpStartAudioEvent.Play(transform.position);
            else if (animationEventManager.RequestEvent("jumpLand") && characterMovement.jumpLandAudioEvent != null)
                characterMovement.jumpLandAudioEvent.Play(transform.position);
        }
    }

    private bool IsGrounded(Actor actor, Transform transform)
    {
        var position = transform.position;
        position.y = transform.GetComponent<Collider>().bounds.min.y + 0.5f;
        Debug.DrawRay(position, Vector3.down * 0.61f, Color.blue);
        return Physics.Raycast(position, Vector3.down, 0.61f, actor.collision.groundMask);
    }

    private bool IsHeadFree(Actor actor, Transform transform)
    {
        var collider = transform.GetComponent<Collider>();
        var length = collider.bounds.size.y * 0.55f;
        var position = transform.position;
        position.y = collider.bounds.max.y;

        Debug.DrawRay(position, Vector3.up * length, Color.blue);
        return !Physics.Raycast(position, Vector3.up, length, actor.collision.obsticleMask);
    }
}