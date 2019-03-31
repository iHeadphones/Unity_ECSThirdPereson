using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Player : IComponentData
{
}

public class PlayerComponent : ComponentDataProxy<Player> { } 