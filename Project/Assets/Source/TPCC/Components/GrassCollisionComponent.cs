using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct GrassCollision : ISharedComponentData
{
    public float distance;
}
public class GrassCollisionComponent : SharedComponentDataProxy<GrassCollision> { }