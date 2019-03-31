using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorRendereringProperteis
{
    public GameObject model;
    public RuntimeAnimatorController animationController;
}

[System.Serializable]
public struct ActorCollisionProperties
{
    public ActorCollisionType type;
    public ActorCollisionBounds[] collisionBounds;
    public RigidbodyConstraints constraints;
    public LayerMask groundMask;
    public LayerMask obsticleMask;
    public PhysicMaterial physicsMaterial;
}

[System.Serializable]
public enum ActorCollisionType
{
    Capsule, Box
}

[System.Serializable]
public struct ActorCollisionBounds
{
    public string id;
    public Vector3 center;
    public Vector3 size;
}

[System.Serializable]
public struct Actor : ISharedComponentData
{
    public ActorRendereringProperteis rendering;
    public ActorCollisionProperties collision;
}

public class ActorComponent : SharedComponentDataProxy<Actor> { } 