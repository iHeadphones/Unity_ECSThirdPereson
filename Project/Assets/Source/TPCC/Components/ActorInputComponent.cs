using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ActorInput : IComponentData
{
    public Vector3 movement;
    public byte actionIndex;
    public byte continueActionIndex;
    public byte actionToDoIndex;
   
    public byte sprint;
    public byte strafe;
    public byte crouch;
    public byte walk;
    public byte applyRootMotion;

    public byte crouchPreviousFrame;
    
    //Action Indexs 
    // 0 : nothing
    // 1 : jump / fly enter
    // 2 : fly
    // 3 : pickup
    // 4 5 6 7 : change weapon

    // 4 : melee Attack
    // 5 : shoot
}

public class ActorInputComponent : ComponentDataProxy<ActorInput> { } 