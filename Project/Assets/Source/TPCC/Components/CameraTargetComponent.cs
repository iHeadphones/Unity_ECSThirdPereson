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
    [Header("Main")]
    public CameraType cameraType;
    public byte priority;
    public bool doCollision;

    [Header("Distance")]
    public float defaultDistance;
	public float minDistance;
	public float maxDistance;

    [Header("Speeds")]
    public float zoomSpeed;
    public float cameraLag;

    [Header("Rotation")]
    public float rotationSpeed;
    public float minRotatonY;
    public float maxRotationY;
}

public class CameraTargetComponent : SharedComponentDataProxy<CameraTarget> { } 