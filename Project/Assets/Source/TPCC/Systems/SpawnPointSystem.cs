using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class SpawnPointSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public ComponentArray<Transform> Transform;
        [ReadOnly] public SharedComponentDataArray<SpawnPoint> SpawnPoint;
    }

    [Inject] Data data;

    protected override void OnUpdate()
    {
        for (var i = 0; i < data.Length; i++)
        {
            //Read Data
            var entity = data.Entity[i];
            var transform = data.Transform[i];
            var spawnPoint = data.SpawnPoint[i];

            if (!EntityManager.HasComponent(entity, typeof(Frozen)))
            {
                ActorUtilities.Spawn(EntityManager, transform.position, spawnPoint.sourceGameObject, spawnPoint.spawnAsPlayer);
                PostUpdateCommands.AddComponent(entity, new Frozen());
            }
        }
    }
}
