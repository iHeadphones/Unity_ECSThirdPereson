using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;

public class ActorSpawnPointSystem : ComponentSystem
{

    private List<ActorSpawnPoint> spawnPontsToUse = new List<ActorSpawnPoint>();
    private List<float3> spawnPontsToUsePositions = new List<float3>();

    protected override void OnUpdate()
    {
        //Que spawns
        Entities.WithAll<Transform, ActorSpawnPoint>().WithNone<Frozen>().ForEach((Entity entity, Transform transform) =>
        {
            var spawnPoint = EntityManager.GetSharedComponentData<ActorSpawnPoint>(entity);
            PostUpdateCommands.AddComponent(entity, new Frozen());
            spawnPontsToUse.Add(spawnPoint);
            spawnPontsToUsePositions.Add(transform.position);
        });

        //Spawn outside of entity loop
        {
            for (int i = 0; i < spawnPontsToUse.Count; i++)
                ActorUtilities.Spawn(EntityManager,spawnPontsToUsePositions[i], spawnPontsToUse[i].sourceGameObject, spawnPontsToUse[i].spawnAsPlayer);

            spawnPontsToUse.Clear();
            spawnPontsToUsePositions.Clear();
        }
    }
}
