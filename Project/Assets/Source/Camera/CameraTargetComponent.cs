using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[Serializable]
public struct CameraTarget : ISharedComponentData
{
    public CameraTargetData data;
}

[Serializable]
public struct CameraTargetData
{
    [Header("Distance")]
    public float defaultDistance;
    public float minDistance;
    public float maxDistance;

    [Header("Camera Lag")]
    public float cameraLag;

    [Header("Rotation")]
    public float rotationSpeed;
    public float minRotatonY;
    public float maxRotationY;

    [Header("Collision")]
    public bool doCollision;
}

public class CameraTargetComponent : SharedComponentDataProxy<CameraTarget> { }