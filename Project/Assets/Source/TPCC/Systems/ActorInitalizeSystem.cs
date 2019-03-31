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
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
        for (var i = 0; i < data.Length; i++)
        {
            var entity = data.Entity[i];
            var transform = data.Transform[i];
            var actor = data.Actor[i];

            if (EntityManager.HasComponent(entity, typeof(Frozen)))
                return;


            EntityManager.AddComponentData(entity, new Frozen());
            EntityManager.AddComponentData(entity, new ActorInput());
            ActorUtilities.UpdateModel(actor,transform);
            ActorUtilities.UpdateAnimator(actor, transform);
            ActorUtilities.UpdateCollider(actor, transform, 0);
            ActorUtilities.UpdatePhysics(actor, transform);
        }
    }
}