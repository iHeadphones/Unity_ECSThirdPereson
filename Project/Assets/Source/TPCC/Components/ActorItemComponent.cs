using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorItem : ISharedComponentData
{
    public int worth;
    public string equippedSocket;
    public string idleSocket;
    public float3[] socketOffsetPositions;
    public float3[] socketEulerAngles;
}
public class ActorItemComponent : SharedComponentDataProxy<ActorItem> { }