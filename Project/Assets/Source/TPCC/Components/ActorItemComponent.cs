using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorItem : ISharedComponentData
{
    public int cost;
    public string idleSocket;
    public string equippedSocket;
}
public class ActorItemComponent : SharedComponentDataProxy<ActorItem> { }