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
    //Helpful Variables
    private Dictionary<int, float> grassIntervals = new Dictionary<int, float>();
    private List<GameObject> grassGameObjectsToDestroy = new List<GameObject>();
    private MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    private string offsetID = "Vector3_8CDD10F8";

    protected override void OnUpdate()
    {

        Entities.WithAll<Transform, Grass>().ForEach((Entity grassEntity, Transform grassTransform) =>
        {
            var grass = EntityManager.GetSharedComponentData<Grass>(grassEntity);
            var grassRenderer = grassTransform.GetComponent<MeshRenderer>();

            //Keep track of grass intevals by entity ID
            if (!grassIntervals.ContainsKey(grassEntity.Index))
                grassIntervals.Add(grassEntity.Index, 0);

            //Get Current Materail property block for grass
            grassRenderer.GetPropertyBlock(materialPropertyBlock);

            Entities.WithAll<Transform, GrassCollision>().ForEach((Entity grassCollisionEntity, Transform grassCollisionTransform) =>
            {
                var grassCollision = EntityManager.GetSharedComponentData<GrassCollision>(grassCollisionEntity);

                //Check if werew colliding with grass based on a distance check
                if (Vector3.Distance(grassTransform.position, grassCollisionTransform.position) <= grassCollision.distance)
                {
                    //Bend Grass
                    var direction = (grassTransform.position - grassCollisionTransform.position).normalized;
                    direction *= grass.bendAmount;
                    direction.y = -grass.stampAmount;
                    materialPropertyBlock.SetVector(offsetID, Vector3.Slerp(materialPropertyBlock.GetVector(offsetID), direction, grass.bendDownSpeed));

                    //Reset Grass Bend interval
                    grassIntervals[grassEntity.Index] = grass.bendUpDelay;

                    //PLACE HOLDER CODE FOR GRASS CUTTING BY PRESSING KEY AND IN RADIUS
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        GameObject.Instantiate(grass.deathClone, grassTransform.position, Quaternion.identity);
                        grass.cutAudioEvent.Play(grassTransform.position);
                        grassGameObjectsToDestroy.Add(grassTransform.gameObject);
                        PostUpdateCommands.DestroyEntity(grassEntity);
                    }
                }

                //Bend Grass Back up when interval is 0
                else
                {
                    grassIntervals[grassEntity.Index] -= 1 * Time.deltaTime;

                    if (grassIntervals[grassEntity.Index] <= 0)
                        materialPropertyBlock.SetVector(offsetID, Vector3.Slerp(materialPropertyBlock.GetVector(offsetID), Vector3.zero, grass.bendUpSpeed));
                }


                grassRenderer.SetPropertyBlock(materialPropertyBlock);
            });
        });

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
