using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine;

public class ActorUtilities
{
    public static void Spawn(EntityManager entityManager, Vector3 worldPosition, GameObject sourceGameObject, bool spawnAsPlayer)
    {
        //Return if source game object is null
        if (sourceGameObject == null)
            return;

        //Spawn
        var gameObject = Object.Instantiate(sourceGameObject, worldPosition, Quaternion.identity);

        //Get Entity
        var entity = gameObject.GetComponent<GameObjectEntity>().Entity;

        //Set If Player
        if (spawnAsPlayer)
            entityManager.AddComponentData(entity, new Player());
    }

    public static void UpdateModel(Actor actor, Transform transform)
    {
        var renderer = transform.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            var o = GameObject.Instantiate(actor.rendering.model, Vector3.zero, Quaternion.identity);
            o.transform.SetParent(transform, false);
        }
    }


    public static void UpdateAnimator(Actor actor, Transform transform)
    {
        var animator = transform.GetComponentInChildren<Animator>();
        var animationEventManager = transform.GetComponentInChildren<AnimationEventManager>();

        if (animator != null)
        {
            animator.runtimeAnimatorController = actor.rendering.animationController;
            if (animationEventManager == null)
                animationEventManager = animator.gameObject.AddComponent<AnimationEventManager>();
        }
    }

    public static void UpdateCollider(Actor actor, Transform transform, string id)
    {
        for (int i = 0; i < actor.collision.collisionBounds.Length; i++)
            if (actor.collision.collisionBounds[i].id == id)
            {
                UpdateCollider(actor, transform, i);
                return;
            }
    }

    public static void UpdateCollider(Actor actor, Transform transform, int actorCollisionBoundsIndex)
    {
        if (actorCollisionBoundsIndex > actor.collision.collisionBounds.Length + 1)
            return;

        var collider = transform.GetComponent<Collider>();
        var actorCollisionBounds = actor.collision.collisionBounds[actorCollisionBoundsIndex];

        if (collider == null)
        {
            if (actor.collision.type == ActorCollisionType.Capsule)
                collider = transform.gameObject.AddComponent<CapsuleCollider>();
            else if (actor.collision.type == ActorCollisionType.Box)
                collider = transform.gameObject.AddComponent<BoxCollider>();
            collider.material = actor.collision.physicsMaterial;
        }

        //Update Size
        if (actor.collision.type == ActorCollisionType.Capsule)
        {
            ((CapsuleCollider)collider).center = actorCollisionBounds.center;
            ((CapsuleCollider)collider).radius = actorCollisionBounds.size.x;
            ((CapsuleCollider)collider).height = actorCollisionBounds.size.y;
        }

        else if (actor.collision.type == ActorCollisionType.Box)
        {
            ((BoxCollider)collider).center = actorCollisionBounds.center;
            ((BoxCollider)collider).size = actorCollisionBounds.size;
        }
    }

    public static void UpdatePhysics(Actor actor, Transform transform)
    {
        var rigidbody = transform.GetComponent<Rigidbody>();

        if (rigidbody == null)
            rigidbody = transform.gameObject.AddComponent<Rigidbody>();

        rigidbody.constraints = actor.collision.constraints;
    }
    public static Transform GetSocket(Transform owner, string name)
    {
        Transform[] children = owner.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

        public static bool IsSocketEmpty(Transform owner, string name)
        {
            var a = owner.GetComponentInChildren<Animator>();

            var socket = GetSocket(owner, name);

            if (socket != null)
                return socket.childCount <= 0;

            return false;
        }
    }