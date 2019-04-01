using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ActorPlayer : IComponentData
{
}

public class ActorPlayerComponent : ComponentDataProxy<ActorPlayer> { } 