using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct Grass : ISharedComponentData
{
    public GameObject deathClone;
    public AudioEvent cutAudioEvent;
    public float bendDownSpeed;
    public float bendUpSpeed;
    public float bendUpDelay;
    public float stampAmount;
    public float bendAmount;

}
public class GrassComponent : SharedComponentDataProxy<Grass> { }