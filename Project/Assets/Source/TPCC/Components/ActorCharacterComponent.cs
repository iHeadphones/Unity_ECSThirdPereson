using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[Serializable]
public struct ActorCharacter : ISharedComponentData
{
	[Header("Ground")]
	public AudioEvent footStepAudioEvent;
	public float runSpeed;
	public float sprintSpeed;
	public float crouchSpeed;
	public float rotationSpeed;

	[Header("Jump | Fall")]
	public AudioEvent jumpGruntAudioEvent;
	public AudioEvent jumpStartAudioEvent;
	public AudioEvent jumpLandAudioEvent;
	public float jumpForce;
	public float fallForce;

	[Header("Fly")]
	public bool canFly;

	[Header("Wall Hugging")]
	public bool canWallHug;
}

public class ActorCharacterComponent: SharedComponentDataProxy<ActorCharacter> { } 