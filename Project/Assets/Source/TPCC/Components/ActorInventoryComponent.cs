using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorInventory : IComponentData
{
    public Entity equipedEntity;
    
}
public class ActorInventoryComponent : ComponentDataProxy<ActorInventory> { }