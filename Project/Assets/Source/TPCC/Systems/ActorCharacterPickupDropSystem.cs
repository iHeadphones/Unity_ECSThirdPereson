using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;

public class ActorCharacterPickupDropSystem : ComponentSystem
{
    private ActorInventory newActorInventory;
    bool attemptToPickUp;

    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput>().ForEach((Entity inventoryEntity, Transform inventoryTransform, ref ActorInventory actorInventory, ref ActorInput actorInput) =>
        {
            newActorInventory = actorInventory;
            attemptToPickUp = false;

            if (actorInput.action == 1 && actorInput.actionIndex == 0)
            {
                Entities.WithAll<Transform, ActorItem>().ForEach((Entity itemEntity, Transform itemTransform) =>
                {
                    //Get Item shared component
                    var actorItem = EntityManager.GetSharedComponentData<ActorItem>(itemEntity);

                    //Check if were in distance and not already picked up
                    if (Vector3.Distance(inventoryTransform.position, itemTransform.position) <= 1f && !EntityManager.HasComponent(itemEntity, typeof(Parent)))
                    {
                        //Set that we attempted to pick a item up
                        attemptToPickUp = true;

                        //Get Target Socket
                        var targetSocket = ActorUtilities.GetFirstEmptyTransform(inventoryTransform, actorItem.sockets);
                        var targetSocketIndex = 0;

                        //Get Index of targt socket
                        for (int i = 0; i < actorItem.sockets.Length; i++)
                            if (actorItem.sockets[i] == targetSocket.name)
                            {
                                targetSocketIndex = i;
                                break;
                            }


                        if (targetSocket != null)
                        {
                            //Add Parent tag to item
                            PostUpdateCommands.AddComponent(itemEntity, new Parent());

                            //Disable Physics | Collision
                            itemTransform.GetComponent<Rigidbody>().useGravity = false;
                            itemTransform.GetComponent<Rigidbody>().isKinematic = true;
                            itemTransform.GetComponent<Collider>().enabled = false;

                            //Set Parent | local position | local euelr angle
                            itemTransform.parent = targetSocket;
                            itemTransform.localPosition = actorItem.socketOffsetPositions[targetSocketIndex];
                            itemTransform.localEulerAngles = actorItem.socketEulerAngles[targetSocketIndex];

                            //Set inventory equped entity | Mark if actor can melee attack ?
                            if (actorItem.socketIsMain[targetSocketIndex])
                            {
                                newActorInventory.equippedEntiy = itemEntity;
                                newActorInventory.isEquipped = 1;

                                inventoryTransform.GetComponentInChildren<Animator>().SetFloat("itemType",actorItem.itemAnimationIndex);

                                if (EntityManager.HasComponent<ActorMeleeWeapon>(itemEntity) && !EntityManager.HasComponent<MarkerCanMeleeAttack>(inventoryEntity))
                                    PostUpdateCommands.AddComponent(inventoryEntity,new MarkerCanMeleeAttack());

                            }
                        }
                    }
                });
            }

            actorInventory = newActorInventory;
        });
    }
}
