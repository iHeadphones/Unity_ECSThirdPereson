using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;


public class ActorInputSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<ActorInput>().ForEach((Entity entity, ref ActorInput actorInput) =>
        {
            UpdateAsAI(entity, ref actorInput);
            UpdateAsPlayer(entity, ref actorInput);
        });
    }

    private void UpdateAsAI(Entity entity, ref ActorInput actorInput)
    {
        if (EntityManager.HasComponent(entity, typeof(ActorPlayer)))
            return;
    }

    private void UpdateAsPlayer(Entity entity, ref ActorInput actorInput)
    {
        if (!EntityManager.HasComponent(entity, typeof(ActorPlayer)))
            return;


        //Get Player Input
        {
            //AXIS
            actorInput.movement.x = GInput.GetAxisRaw(GAxis.LEFTHORIZONTAL);
            actorInput.movement.z = GInput.GetAxisRaw(GAxis.LEFTVERTICAL);

            //Reset
            actorInput.actionToDoIndex = 0;
            actorInput.action = 0;

            //BUTTONS
            if (GInput.GetButtonDown(GButton.BOTTOM)) actorInput.actionToDoIndex = 1;
            if (GInput.GetButtonDown(GButton.LEFT)) actorInput.actionToDoIndex = 2;
            if (GInput.GetButtonDown(GButton.L3)) actorInput.crouch = (byte)(actorInput.crouch == 1 ? 0 : 1);
            actorInput.action = (byte)(GInput.GetButtonDown(GButton.RIGHT) ? 1 : 0);
            actorInput.sprint = (byte)(GInput.GetButton(GButton.TOP) ? 1 : 0);
            actorInput.strafe = (byte)(GInput.GetButton(GButton.L2) ? 1 : 0);
        }

        //Convert Actor Movement to Camera
        {
            var cameraForward = (Camera.main.transform.TransformDirection(Vector3.forward).normalized);
            cameraForward.y = 0;
            var camreaRight = new Vector3(cameraForward.z, 0, -cameraForward.x);
            actorInput.movement = actorInput.movement.x * camreaRight + actorInput.movement.z * cameraForward;
            actorInput.movement.Normalize();
        }
    }
}
