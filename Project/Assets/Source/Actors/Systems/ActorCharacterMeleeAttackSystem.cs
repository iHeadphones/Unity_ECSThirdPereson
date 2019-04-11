using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;

public class ActorCharacterMeleeAttackSystem : ComponentSystem
{
    private Dictionary<int, bool> doNextAttacks = new Dictionary<int, bool>();
    private Dictionary<int, bool> isPlayingAttackAnimations = new Dictionary<int, bool>();

    private byte actionIndex = 3;

    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput, ActorTagCanMeleeAttack>().ForEach((Entity entity, Transform transform, ref ActorInventory actorInventory, ref ActorInput actorInput) =>
        {
            var rigidbody = transform.GetComponentInChildren<Rigidbody>();
            var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();
            var animator = transform.GetComponentInChildren<Animator>();
            var actorMeleeWeapon = EntityManager.GetSharedComponentData<ActorMeleeWeapon>(actorInventory.equippedEntiy);

            //Make sure combo attacks are not null 
            if (!doNextAttacks.ContainsKey(entity.Index))
            {
                doNextAttacks.Add(entity.Index, false);
                isPlayingAttackAnimations.Add(entity.Index, false);
            }

            //Start Attack
            if (actorInput.actionToDo == actionIndex && actorInput.action == 0)
            {
                rigidbody.velocity = Vector3.zero;
                animator.SetTrigger("attackMeleeStart");
                animator.SetBool("disableUpperBody", true);
                animator.SetBool("inAirDisabled",true);
                animator.SetFloat("movementX", 0);
                animator.SetFloat("movementY", 0);
                actorInput.actionToDo = 0;
                actorInput.action = actionIndex;
            }

            //Update Attacking
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("attackMelee"))
            {
                isPlayingAttackAnimations[entity.Index] = true;
                animator.transform.localPosition = Vector3.zero;

                //Sound
                if (animationEventManager.RequestEvent("swing") && actorMeleeWeapon.swingAudioEvent != null)
                    actorMeleeWeapon.swingAudioEvent.Play(transform.position);
            }

            //Done Attacking
            if (animationEventManager.RequestEvent("attackEnd") || !animator.GetCurrentAnimatorStateInfo(0).IsTag("attackMelee") && isPlayingAttackAnimations[entity.Index])
            {
                animator.SetBool("disableUpperBody", false);
                animator.SetFloat("attackMeleeCombo", 0);
                animator.SetBool("inAirDisabled",false);
                animationEventManager.RemoveEvent("attackNext");
                isPlayingAttackAnimations[entity.Index] = false;
                actorInput.action = 0;
                actorInput.actionToDo = 0;
            }

            //Set to move to next attack
            if (actorInput.action == actionIndex && actorInput.actionToDo == actionIndex && !doNextAttacks[entity.Index])
                doNextAttacks[entity.Index] = true;

            //Do next attack
            if (animationEventManager.RequestEvent("attackNext") && doNextAttacks[entity.Index])
            {
                animator.SetFloat("attackMeleeCombo", animator.GetFloat("attackMeleeCombo") + 1);
                animator.SetTrigger("attackMeleeStart");
                doNextAttacks[entity.Index] = false;
            }
        });
    }
}