using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine;

[UpdateBefore(typeof(ActorCharacterPickupDropSystem))]
public class ActorCharacterWallHugSystem : ComponentSystem
{
    private Dictionary<int, Vector3> wallHuggingDirections = new Dictionary<int, Vector3>();

    private float dt;
    private float animationTransitionRate = 0.1f;
    private float deadPoint = 0.25f;

    protected override void OnUpdate()
    {
        dt = Time.deltaTime;

        Entities.WithAll<Transform, Actor, ActorInput, ActorCharacter, ActorCharacterWallHug>().ForEach((Entity entity, Transform transform, ref ActorInput actorInput) =>
         {
            // Data
            var actor = EntityManager.GetSharedComponentData<Actor>(entity);
             var actorCharacterWallHug = EntityManager.GetSharedComponentData<ActorCharacterWallHug>(entity);

            //MonoBehaviours
            var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();
             var animator = transform.GetComponentInChildren<Animator>();
             var rigidbody = transform.GetComponent<Rigidbody>();

            //Update Movement
            OnWallHugMovement(transform, animationEventManager, animator, rigidbody, entity, actor, ref actorInput, actorCharacterWallHug);
         });
    }

    private void OnWallHugMovement(Transform transform, AnimationEventManager animationEventManager, Animator animator, Rigidbody rigidbody, Entity entity, Actor actor, ref ActorInput actorInput, ActorCharacterWallHug actorCharacterWallHug)
    {
        // start wall hugging
        if (actorInput.actionToDo == 1 && actorInput.crouch == 0 && actorInput.action == 0)
        {
            // get all surrounding walls  | Get first hit as our main hit   
            var hits = Physics.SphereCastAll(transform.position, 0.5f, Vector3.one * 0.5f, 0.5f, actorCharacterWallHug.wallHugMask);

            if (hits.Length >= 1)
            {
                var hit = hits[0];
                var directionToWall = (hit.transform.position - transform.position).normalized;
                var distanceToWall = Vector3.Distance(hit.transform.position, transform.position);
                RaycastHit wallHit;

                if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), directionToWall, out wallHit, distanceToWall, actorCharacterWallHug.wallHugMask))
                {
                    wallHuggingDirections[entity.Index] = Quaternion.LookRotation(GetMeshColliderNormal(wallHit)).eulerAngles;

                    Debug.DrawLine(transform.position + new Vector3(0, 0.5f, 0), wallHit.point, Color.red, 1);
                    Debug.DrawRay(wallHit.point, GetMeshColliderNormal(wallHit) * 5, Color.red, 1);

                    actorInput.actionToDo = 0;
                    actorInput.action = 99;
                }

            }
        }

        //Stop wall hugging
        if (actorInput.actionToDo == 1 && actorInput.action == 99)
        {
            actorInput.actionToDo = 0;
            actorInput.action = 0;
        }

        //Update Wall hugging
        if (actorInput.action == 99)
        {
            bool forceStop = false;

            //Move
            if (actorInput.movement.magnitude >= deadPoint)
            {
                //Movement
                var movement = actorInput.movement;
                var movementToRightDotProduct = Vector3.Dot(movement, transform.right);
                rigidbody.velocity = transform.right * movementToRightDotProduct * actorCharacterWallHug.speed;

                //Check if there is wall ahead of where we are going if not than force a stop
                {
                    //Do it
                    var raycastPoint = transform.position;
                    raycastPoint += Vector3.up * 0.1f;
                    raycastPoint += (transform.right * movementToRightDotProduct).normalized * 0.5f;
                    forceStop = !Physics.Raycast(raycastPoint, -transform.forward, 0.5f, actorCharacterWallHug.wallHugMask);

                    //Debug
                    Debug.DrawRay(raycastPoint, -transform.forward * 0.5f, forceStop ? Color.red : Color.green, 0);
                }

                //Animation
                if (!forceStop)
                {
                    animator.SetFloat("movementX", movementToRightDotProduct * deadPoint, animationTransitionRate, dt);
                    animator.SetFloat("movementY", 0, animationTransitionRate, dt);
                }
            }

            //Stop Move
            if (actorInput.movement.magnitude <= deadPoint || forceStop)
            {
                //Movement
                rigidbody.velocity = Vector3.zero;

                //Animation
                animator.SetFloat("movementX", 0, animationTransitionRate, dt);
                animator.SetFloat("movementY", 0, animationTransitionRate, dt);
            }

            //Rotate to face wall normal
            transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, wallHuggingDirections[entity.Index], Time.deltaTime * 6);

            //Sound
            if (animationEventManager.RequestEvent("footStep") && actorCharacterWallHug.footStepAudioEvent != null && actorInput.movement.magnitude != 0)
                actorCharacterWallHug.footStepAudioEvent.Play(transform.position);

        }
    }

    private Vector3 GetMeshColliderNormal(RaycastHit hit)
    {
        MeshCollider collider = (MeshCollider)hit.collider;
        Mesh mesh = collider.sharedMesh;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        // Extract local space normals of the triangle we hit
        Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];

        // interpolate using the barycentric coordinate of the hitpoint | Use barycentric coordinate to interpolate normal
        Vector3 baryCenter = hit.barycentricCoordinate;
        Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
        interpolatedNormal = interpolatedNormal.normalized;

        return interpolatedNormal;
    }
}