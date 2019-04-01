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
    private ComponentGroup inventoryGroup;
    private ComponentGroup itemGroup;

    protected override void OnCreateManager()
    {
        inventoryGroup = GetComponentGroup(typeof(ActorInventory), typeof(Transform), typeof(ActorInput));
        itemGroup = GetComponentGroup(typeof(ActorItem), typeof(Transform));
    }

    protected override void OnUpdate()
    {
        //Get Inventories Data
        var inventoryTransforms = inventoryGroup.ToComponentArray<Transform>();
        var inventories = inventoryGroup.ToComponentDataArray<ActorInventory>(Allocator.TempJob);
        var actorInputs = inventoryGroup.ToComponentDataArray<ActorInput>(Allocator.TempJob);

        //Get Items Data
        var itemEntities = itemGroup.ToEntityArray(Allocator.TempJob);
        var itemTransforms = itemGroup.ToComponentArray<Transform>();
        var items = itemGroup.GetSharedComponentDataArray<ActorItem>();

        for (int i = 0; i < inventories.Length; i++)
        {
            if (actorInputs[i].action == 0 || actorInputs[i].actionIndex != 0)
                continue;

            for (int j = 0; j < items.Length; j++)
            {
                //Check if were in distance and not already picked up
                if (Vector3.Distance(inventoryTransforms[i].position, itemTransforms[j].position) <= 0.5f && !EntityManager.HasComponent(itemEntities[j], typeof(Parent)))
                {
                    //Create a list of sockets from item data
                    var scokets = new List<string>();
                    foreach (var k in (items[j].equippedSocket + ',' + items[j].idleSocket).Split(','))
                        scokets.Add(k);

                    //Get Target Socket | Get Target Socket Index 
                    var targetSocket = ActorUtilities.GetFirstEmptyTransform(inventoryTransforms[i], scokets.ToArray());
                    int targetSocketIndex = scokets.IndexOf(targetSocket.name);

                    if (targetSocket != null)
                    {
                        //Add Parent tag to item
                        PostUpdateCommands.AddComponent(itemEntities[j], new Parent());

                        //Disable Physics | Collision
                        itemTransforms[j].GetComponent<Rigidbody>().useGravity = false;
                        itemTransforms[j].GetComponent<Rigidbody>().isKinematic = true;
                        itemTransforms[j].GetComponent<Collider>().enabled = false;

                        //Set Parent | local position | local euelr angle
                        itemTransforms[j].parent = targetSocket;
                        itemTransforms[j].localPosition = items[j].socketOffsetPositions[targetSocketIndex];
                        itemTransforms[j].localEulerAngles = items[j].socketEulerAngles[targetSocketIndex];
                    }
                }
            }
        }

        //Cleanup
        inventories.Dispose();
        actorInputs.Dispose();
        itemEntities.Dispose();
    }
}
