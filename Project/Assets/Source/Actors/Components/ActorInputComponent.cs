using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ActorInput : IComponentData
{
    //Movement
    public Vector3 movement;

    //Actions
    public byte action;
    public byte actionToDo;

    //States
    public byte sprint;
    public byte strafe;
    public byte crouch;
    public byte walk;

    //Keep track of things
    public byte crouchPreviousFrame;
    public byte isJumping;
}

public class ActorInputComponent : ComponentDataProxy<ActorInput> { }