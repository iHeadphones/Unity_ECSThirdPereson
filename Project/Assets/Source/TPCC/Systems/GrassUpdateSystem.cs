using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;


public class GrassUpdateSystem : ComponentSystem
{
    public struct GrassData
    {
        public readonly int Length;
        [ReadOnly] public SharedComponentDataArray<Grass> Grass;
        public ComponentArray<MeshRenderer> MeshRenderer;
        public ComponentArray<Transform> Transform;
        public EntityArray Entity;
    }

    public struct GrassCollisionData
    {
        public readonly int Length;
        [ReadOnly] public SharedComponentDataArray<GrassCollision> GrassCollision;
        public ComponentArray<Transform> Transform;
    }

    [Inject] private GrassData grassData;
    [Inject] private GrassCollisionData grassCollisionData;
    private Dictionary<int, float> grassIntervals = new Dictionary<int, float>();
    private List<GameObject> grassGameObjectsToDestroy = new List<GameObject>();
    private MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    private string offsetID = "Vector3_8CDD10F8";

    protected override void OnUpdate()
    {

        for (int i = 0; i < grassData.Length; i++)
        {
            var grass = grassData.Grass[i];
            var grassRenderer = grassData.MeshRenderer[i];
            var grassTransform = grassData.Transform[i];
            var grassEntity = grassData.Entity[i];

            grassRenderer.GetPropertyBlock(materialPropertyBlock);

            if (!grassIntervals.ContainsKey(grassEntity.Index))
            {

                grassIntervals.Add(grassEntity.Index, 0);

            }

            for (int j = 0; j < grassCollisionData.Length; j++)
            {
                var grassCollsion = grassCollisionData.GrassCollision[j];
                var grassCollisionTransform = grassCollisionData.Transform[j];

                if (Vector3.Distance(grassTransform.position, grassCollisionTransform.position) <= grassCollsion.distance)
                {

                    grassIntervals[grassEntity.Index] = grass.bendUpDelay;
                    var direction = (grassTransform.position - grassCollisionTransform.position).normalized;
                    direction *= grass.bendAmount;
                    direction.y = -grass.stampAmount;
                    materialPropertyBlock.SetVector(offsetID, Vector3.Slerp(materialPropertyBlock.GetVector(offsetID), direction, grass.bendDownSpeed));

                    //PLACE HOLDER CODE FOR GRASS CUTTING BY PRESSING KEY AND IN RADIUS
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        GameObject.Instantiate(grass.deathClone, grassTransform.position, Quaternion.identity);
                        grass.cutAudioEvent.Play(grassTransform.position);
                        grassGameObjectsToDestroy.Add(grassTransform.gameObject);
                        PostUpdateCommands.DestroyEntity(grassEntity);
                    }
                }
                else
                {

                    grassIntervals[grassEntity.Index] -= 1 * Time.deltaTime;

                    if (grassIntervals[grassEntity.Index] <= 0)
                        materialPropertyBlock.SetVector(offsetID, Vector3.Slerp(materialPropertyBlock.GetVector(offsetID), Vector3.zero, grass.bendUpSpeed));
                }
            }

            grassRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        //Destroy Left over  game object outside entity loop
        {
            if (grassGameObjectsToDestroy.Count == 0)
                return;

            foreach (var i in grassGameObjectsToDestroy)
                GameObject.Destroy(i);
            grassGameObjectsToDestroy.Clear();
        }
    }
}
