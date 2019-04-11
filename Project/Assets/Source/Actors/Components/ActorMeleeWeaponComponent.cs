using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;


[System.Serializable]
public struct ActorMeleeWeapon : ISharedComponentData
{
    public AudioEvent swingAudioEvent;
}
public class ActorMeleeWeaponComponent : SharedComponentDataProxy<ActorMeleeWeapon> { }