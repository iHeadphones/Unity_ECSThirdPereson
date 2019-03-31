using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;


public class ActorPickupDropSystem : ComponentSystem
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

        //FIX - Set Items local postion and rotation every frame to get rid of bugs
        for (int i = 0; i < itemData.Length; i++)
            if (EntityManager.HasComponent(itemData.Entity[i], typeof(ActorParent)))
            {
                var transform = itemData.Transform[i];
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }

        for (int i = 0; i < actorInventoryData.Length; i++)
        {
            var inventoryEntity = actorInventoryData.Entity[i];
            var actorInventory = actorInventoryData.ActorInventory[i];
            var inventoryTransform = actorInventoryData.Transform[i];
            var actorInput = actorInventoryData.ActorInput[i];

            if (actorInput.action != 1)
                continue;

            for (int j = 0; j < itemData.Length; j++)
            {
                var itemEntity = itemData.Entity[j];
                var actorItem = itemData.ActorItem[j];
                var itemTransform = itemData.Transform[j];

                if (Vector3.Distance(inventoryTransform.position, itemTransform.position) <= 0.5f && !EntityManager.HasComponent(itemEntity, typeof(ActorParent)))
                {

                    if (ActorUtilities.IsSocketEmpty(inventoryTransform, actorItem.equippedSocket))
                    {
                        //Attatch to socket
                        var socket = ActorUtilities.GetSocket(inventoryTransform, actorItem.equippedSocket);
                        itemTransform.parent = socket;
                        PostUpdateCommands.AddComponent(itemEntity, new ActorParent());
                        actorInventory.equipedEntity = itemEntity;


                        //Trun off physics | Collider
                        itemTransform.GetComponent<Rigidbody>().useGravity = false;
                        itemTransform.GetComponent<Rigidbody>().isKinematic = true;
                        itemTransform.GetComponent<Collider>().enabled = false;
                    }

                    else if (ActorUtilities.IsSocketEmpty(inventoryTransform, actorItem.idleSocket))
                    {
                        var socket = ActorUtilities.GetSocket(inventoryTransform, actorItem.idleSocket);
                        itemTransform.parent = socket;
                        PostUpdateCommands.AddComponent(itemEntity, new ActorParent());


                        //Trun off physics | Collider
                        itemTransform.GetComponent<Rigidbody>().useGravity = false;
                        itemTransform.GetComponent<Rigidbody>().isKinematic = true;
                        itemTransform.GetComponent<Collider>().enabled = false;
                    }

                }
            }

        }
    }
}
