using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public struct ActorParent : IComponentData
{
}
public class ActorParentComponent : ComponentDataProxy<ActorParent> { }