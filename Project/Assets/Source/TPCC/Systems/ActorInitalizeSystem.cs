using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine;

public class ActorInitalizeSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public ComponentArray<Transform> Transform;
        [ReadOnly] public SharedComponentDataArray<Actor> Actor;
        public ExcludeComponent<Frozen> Frozen;
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
            var entity = data.Entity[0];
            var transform = data.Transform[0];
            var actor = data.Actor[0];


            EntityManager.AddComponentData(entity, new Frozen());
            EntityManager.AddComponentData(entity, new ActorInput());
            ActorUtilities.UpdateModel(actor,transform);
            ActorUtilities.UpdateAnimator(actor, transform);
            ActorUtilities.UpdateCollider(actor, transform, 0);
            ActorUtilities.UpdatePhysics(actor, transform);
    }
}