using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct GrassEffector : ISharedComponentData
{
    public float distance;
}
public class GrassEffectorComponent : SharedComponentDataProxy<GrassEffector> { }