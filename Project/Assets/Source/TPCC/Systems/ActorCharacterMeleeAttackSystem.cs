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
    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput>().ForEach((Entity entity, Transform transform, ref ActorInventory actorInventory, ref MarkerCanMeleeAttack markerCanMeleeAttack, ref ActorInput actorInput) =>
        {
            var rigidbody = transform.GetComponentInChildren<Rigidbody>();
            var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();
            var animator = transform.GetComponentInChildren<Animator>();
            var actorMeleeWeapon = EntityManager.GetSharedComponentData<ActorMeleeWeapon>(actorInventory.equippedEntiy);

            //Start Attack
            if (actorInput.actionToDoIndex == 2 && actorInput.actionIndex == 0)
            {
                actorInput.actionIndex = 2;
                rigidbody.velocity = Vector3.zero;
                animator.SetTrigger("attackMeleeStart");
                animator.SetBool("disableUpperBody",true);
                animator.SetFloat("movementX",0);
                animator.SetFloat("movementY",0);
                animator.applyRootMotion = true;
            }

            //Is Attacking
            if (actorInput.actionIndex == 2)
            {

                animator.transform.localPosition = Vector3.zero;

                //Sound
                if (animationEventManager.RequestEvent("swing") && actorMeleeWeapon.swingAudioEvent != null)
                    actorMeleeWeapon.swingAudioEvent.Play(transform.position);
            }

            //Done Attacking
            if (actorInput.actionIndex == 2 && animationEventManager.RequestEvent("attackEnd"))
            {
                actorInput.actionIndex = 0;
                animator.applyRootMotion = false;
                animator.SetTrigger("attackMeleeEnd");
            }
        });
    }
}