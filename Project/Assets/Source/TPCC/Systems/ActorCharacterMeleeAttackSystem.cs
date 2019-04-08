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
    private Dictionary<int, bool> doComboAttacks = new Dictionary<int, bool>();

    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput>().ForEach((Entity entity, Transform transform, ref ActorInventory actorInventory, ref MarkerCanMeleeAttack markerCanMeleeAttack, ref ActorInput actorInput) =>
        {
            var rigidbody = transform.GetComponentInChildren<Rigidbody>();
            var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();
            var animator = transform.GetComponentInChildren<Animator>();
            var actorMeleeWeapon = EntityManager.GetSharedComponentData<ActorMeleeWeapon>(actorInventory.equippedEntiy);

            //Make sure combo attacks are not null 
            if (!doComboAttacks.ContainsKey(entity.Index))
                doComboAttacks.Add(entity.Index, false);

            //Start Attack
            if (actorInput.actionToDoIndex == 2 && actorInput.actionIndex == 0 || actorInput.actionIndex == 0 && doComboAttacks[entity.Index] == true)
            {
                actorInput.actionIndex = 2;
                rigidbody.velocity = Vector3.zero;
                animator.SetTrigger("attackMeleeStart");
                animator.SetBool("disableUpperBody", true);
                animator.SetFloat("movementX", 0);
                animator.SetFloat("movementY", 0);
                doComboAttacks[entity.Index] = false;
                actorInput.actionToDoIndex = 0;
            }

            //Update Attacking
            if (actorInput.actionIndex == 2)
            {
                animator.transform.localPosition = Vector3.zero;

                if (actorInput.actionToDoIndex == 2)
                    doComboAttacks[entity.Index] = true;

                //Sound
                if (animationEventManager.RequestEvent("swing") && actorMeleeWeapon.swingAudioEvent != null)
                    actorMeleeWeapon.swingAudioEvent.Play(transform.position);
            }

            //Done Attacking
            if (actorInput.actionIndex == 2 && animationEventManager.RequestEvent("attackEnd"))
            {
                actorInput.actionIndex = 0;
                animator.SetTrigger("attackMeleeEnd");
                animator.SetBool("disableUpperBody",false);
                
                if (doComboAttacks[entity.Index])
                {
                    animator.SetFloat("attackMeleeCombo", animator.GetFloat("attackMeleeCombo") + 1);
                    if (animator.GetFloat("attackMeleeCombo") > 1)
                        animator.SetFloat("attackMeleeCombo", 0);
                }
                else
                {
                    animator.SetFloat("attackMeleeCombo", 0);
                }
            }
        });

        //If were attacking set do combo attack to true on entity
        //When done attacking check if entity combo attac is true than start attack over

    }
}