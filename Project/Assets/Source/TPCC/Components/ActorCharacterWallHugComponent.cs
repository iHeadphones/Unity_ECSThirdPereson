using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[Serializable]
public struct ActorCharacterWallHug : ISharedComponentData
{
    public LayerMask wallHugMask;
    public AudioEvent footStepAudioEvent;
    public float speed;
}

public class ActorCharacterWallHugComponent : SharedComponentDataProxy<ActorCharacterWallHug> { }