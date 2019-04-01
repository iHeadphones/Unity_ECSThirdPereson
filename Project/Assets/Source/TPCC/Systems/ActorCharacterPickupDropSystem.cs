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
    protected override void OnUpdate()
    {
        Entities.WithAll<Transform, ActorInventory, ActorInput>().ForEach((Entity inventoryEntity, Transform inventoryTransform, ref ActorInventory actorInventory, ref ActorInput actorInput) =>
        {
            if (actorInput.action == 1 && actorInput.actionIndex == 0)
            {
                Entities.WithAll<Transform, ActorItem>().ForEach((Entity itemEntity, Transform itemTransform) =>
                {
                    //Get Item shared component
                    var item = EntityManager.GetSharedComponentData<ActorItem>(itemEntity);

                    //Check if were in distance and not already picked up
                    if (Vector3.Distance(inventoryTransform.position, itemTransform.position) <= 1f && !EntityManager.HasComponent(itemEntity, typeof(Parent)))
                    {
                        //Create a list of sockets from item data
                        var scokets = new List<string>();
                        foreach (var i in (item.equippedSocket + ',' + item.idleSocket).Split(','))
                            scokets.Add(i);

                        //Get Target Socket | Get Target Socket Index 
                        var targetSocket = ActorUtilities.GetFirstEmptyTransform(inventoryTransform, scokets.ToArray());
                        int targetSocketIndex = scokets.IndexOf(targetSocket.name);

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
                            itemTransform.localPosition = item.socketOffsetPositions[targetSocketIndex];
                            itemTransform.localEulerAngles = item.socketEulerAngles[targetSocketIndex];
                        }
                    }
                });
            }
        });
    }
}
