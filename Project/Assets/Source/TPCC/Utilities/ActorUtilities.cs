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
            entityManager.AddComponentData(entity, new ActorPlayer());
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

    public static Transform GetTransform(Transform owner, string name)
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

    public static bool IsTransformEmpty(Transform owner, string name)
    {
        var a = owner.GetComponentInChildren<Animator>();

        var socket = GetTransform(owner, name);

        if (socket != null)
            return socket.childCount <= 0;

        return false;
    }

    public static Transform GetFirstEmptyTransform(Transform owner, string[] names)
    {
        foreach (var i in names)
        {
            var t = GetTransform(owner, i);
            if (t != null && t.childCount <= 0)
                return t;
        }

        return null;
    }

    public static void PickupItem(EntityCommandBuffer postUpdateCommands, EntityManager entityManager, Transform socket, Transform itemTransform, Entity itemEntity, ActorItem actorItem, Transform inventoryTransform, Entity inventoryEntity, ref ActorInventory inventory)
    {
        //Get Index of socket
        var targetSocketIndex = 0;
        //Get Index of targt socket
        for (int i = 0; i < actorItem.sockets.Length; i++)
            if (actorItem.sockets[i] == socket.name)
            {
                targetSocketIndex = i;
                break;
            }


        //Add Parent tag to item
        postUpdateCommands.AddComponent(itemEntity, new Parent());

        //Disable Physics | Collision
        itemTransform.GetComponent<Rigidbody>().useGravity = false;
        itemTransform.GetComponent<Rigidbody>().isKinematic = true;
        itemTransform.GetComponent<Collider>().enabled = false;

        //Set Parent | local position | local euelr angle
        itemTransform.parent = socket;
        itemTransform.localPosition = actorItem.socketOffsetPositions[targetSocketIndex];
        itemTransform.localEulerAngles = actorItem.socketEulerAngles[targetSocketIndex];

        //Set inventory equped entity | Mark if actor can melee attack ?
        if (actorItem.socketIsMain[targetSocketIndex])
        {
            inventory.equippedEntiy = itemEntity;
            inventory.isEquipped = 1;


            inventoryTransform.GetComponentInChildren<Animator>().SetFloat("itemType", actorItem.itemAnimationIndex);

            if (entityManager.HasComponent<ActorMeleeWeapon>(itemEntity) && !entityManager.HasComponent<MarkerCanMeleeAttack>(inventoryEntity))
                postUpdateCommands.AddComponent(inventoryEntity, new MarkerCanMeleeAttack());
        }
    }

    public static void DropItem(EntityCommandBuffer postUpdateCommands, EntityManager entityManager, Entity itemEntity, Transform inventoryTransform, Entity inventoryEntity, ref ActorInventory actorInventory)
    {
        //Item is no longer parent of a entity
        postUpdateCommands.RemoveComponent(actorInventory.equippedEntiy, typeof(Parent));

        //Rest transform parent, physics and collision
        var itemTransform = entityManager.GetComponentObject<Transform>(actorInventory.equippedEntiy);
        itemTransform.GetComponent<Rigidbody>().useGravity = true;
        itemTransform.GetComponent<Rigidbody>().isKinematic = false;
        itemTransform.GetComponent<Collider>().enabled = true;
        itemTransform.SetParent(null, true);

        //Were dropping main entity
        if (actorInventory.equippedEntiy == itemEntity)
        {
            actorInventory.isEquipped = 0;
            inventoryTransform.GetComponentInChildren<Animator>().SetFloat("itemType", 0.0f);
            if (entityManager.HasComponent(inventoryEntity,typeof(MarkerCanMeleeAttack)))
                postUpdateCommands.RemoveComponent<MarkerCanMeleeAttack>(inventoryEntity);
        }
    }

    public static bool IsGrounded(Actor actor, Transform transform)
    {
        var position = transform.position;
        position.y = transform.GetComponent<Collider>().bounds.min.y + 0.5f;
        Debug.DrawRay(position, Vector3.down * 0.61f, Color.blue);
        return Physics.Raycast(position, Vector3.down, 0.61f, actor.collision.groundMask);
    }

    public static bool IsHeadFree(Actor actor, Transform transform)
    {
        var collider = transform.GetComponent<Collider>();
        var length = actor.collision.collisionBounds[0].size.y + 0.25f;
        var position = transform.position;

        Debug.DrawRay(position, Vector3.up * length, Color.blue);
        return !Physics.Raycast(position, Vector3.up, length, actor.collision.obsticleMask);
    }
}