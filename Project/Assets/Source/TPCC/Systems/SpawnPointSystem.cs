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
         public ExcludeComponent<Frozen> Frozen;
    }

    [Inject] Data data;

    protected override void OnUpdate()
    {
            //Read Data
            var entity = data.Entity[0];
            var transform = data.Transform[0];
            var spawnPoint = data.SpawnPoint[0];
            
            ActorUtilities.Spawn(EntityManager, transform.position, spawnPoint.sourceGameObject, spawnPoint.spawnAsPlayer);
            PostUpdateCommands.AddComponent(entity, new Frozen());
            
    
    }
}
