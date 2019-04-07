using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorInventory : IComponentData
{
    public Entity equippedEntiy;  
    public byte isEquipped;
}
public class ActorInventoryComponent : ComponentDataProxy<ActorInventory> { }