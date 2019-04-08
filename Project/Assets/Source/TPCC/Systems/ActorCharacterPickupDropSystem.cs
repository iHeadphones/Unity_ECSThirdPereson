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
    private ActorInput newInventoryActorInput;
    bool attemptToPickUp;

    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput>().ForEach((Entity inventoryEntity, Transform inventoryTransform, ref ActorInventory actorInventory, ref ActorInput actorInput) =>
        {
            newActorInventory = actorInventory;
            newInventoryActorInput = actorInput;
            attemptToPickUp = false;

            if (actorInput.action == 1 && actorInput.actionIndex == 0)
            {
                Entities.WithAll<Transform, ActorItem>().ForEach((Entity itemEntity, Transform itemTransform) =>
                {
                    //Get Item shared component
                    var actorItem = EntityManager.GetSharedComponentData<ActorItem>(itemEntity);

                    //Check if were in distance and not already picked up
                    if (!attemptToPickUp && Vector3.Distance(inventoryTransform.position, itemTransform.position) <= 1f && !EntityManager.HasComponent(itemEntity, typeof(Parent)))
                    {
                        //Set that we attempted to pick a item up
                        attemptToPickUp = true;

                        //Get Target Socket
                        var targetSocket = ActorUtilities.GetFirstEmptyTransform(inventoryTransform, actorItem.sockets);

                        //Do Pickup | Set action to 0
                        if (targetSocket != null)
                        {
                            ActorUtilities.PickupItem(PostUpdateCommands, EntityManager, targetSocket, itemTransform, itemEntity, actorItem, inventoryTransform, inventoryEntity, ref newActorInventory);
                            newInventoryActorInput.action = 0;
                        }
                    }
                });
            }

            //Drop | Set Action to 0
            if (actorInput.action == 1 && actorInput.actionIndex == 0 && attemptToPickUp == false && newActorInventory.isEquipped == 1)
            {
                ActorUtilities.DropItem(PostUpdateCommands, EntityManager, newActorInventory.equippedEntiy, inventoryTransform, inventoryEntity, ref newActorInventory);
                newInventoryActorInput.action = 0;
            }

            actorInventory = newActorInventory;
            actorInput = newInventoryActorInput;
        });
    }
}
