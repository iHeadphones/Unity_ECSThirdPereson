using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;


[System.Serializable]
public struct ActorItem : ISharedComponentData
{
    public int worth;
    public int itemAnimationIndex;
    public string[] sockets;
    public float3[] socketOffsetPositions;
    public float3[] socketEulerAngles;
    public bool[] socketIsMain;
}
public class ActorItemComponent : SharedComponentDataProxy<ActorItem> { }