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
    public struct InventoryData
    {
        public readonly int Length;
        public EntityArray Entity;
        public ComponentDataArray<ActorInventory> ActorInventory;
        public ComponentArray<Transform> Transform;

        public ComponentDataArray<ActorInput> ActorInput;

    }
    public struct ItemData
    {
        public readonly int Length;
        public EntityArray Entity;
        [ReadOnly] public SharedComponentDataArray<ActorItem> ActorItem;
        public ComponentArray<Transform> Transform;

    }
    [Inject] private InventoryData actorInventoryData;
    [Inject] private ItemData itemData;

    protected override void OnUpdate()
    {
        bool atemptedToPickUp = false;

        for (int i = 0; i < actorInventoryData.Length; i++)
        {
            var inventoryEntity = actorInventoryData.Entity[i];
            var actorInventory = actorInventoryData.ActorInventory[i];
            var inventoryTransform = actorInventoryData.Transform[i];
            var actorInput = actorInventoryData.ActorInput[i];

            if (actorInput.action != 1 && actorInput.actionIndex == 0)
                continue;

            for (int j = 0; j < itemData.Length; j++)
            {
                var itemEntity = itemData.Entity[j];
                var actorItem = itemData.ActorItem[j];
                var itemTransform = itemData.Transform[j];

                if (Vector3.Distance(inventoryTransform.position, itemTransform.position) <= 0.5f && !EntityManager.HasComponent(itemEntity, typeof(Parent)))
                {
                    //Create a list of sockets from item data
                    var scokets = new List<string>();
                    foreach (var k in (actorItem.equippedSocket + ',' + actorItem.idleSocket).Split(','))
                        scokets.Add(k);

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
                        itemTransform.localPosition = actorItem.socketOffsetPositions[targetSocketIndex];
                        itemTransform.localEulerAngles = actorItem.socketEulerAngles[targetSocketIndex];
                    }

                    atemptedToPickUp = true;
                }
            }

            if (!atemptedToPickUp)
            {

            }
        }
    }
}
