using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ActorInitalize : ISharedComponentData
{
}

public class ActorInitalizeComponent : SharedComponentDataProxy<ActorInitalize> { } 
