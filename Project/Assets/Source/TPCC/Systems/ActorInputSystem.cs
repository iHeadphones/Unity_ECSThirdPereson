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
    public struct Data
    {
        public readonly int Length;
        public GameObjectArray GameObject;
        public EntityArray Entity;
        public ComponentDataArray<ActorInput> ActorInput;
    }

    [Inject] private Data data;
    private ActorInput actorInput;

    protected override void OnUpdate()
    {
        for (var i = 0; i < data.Length; i++)
        {
            var entity = data.Entity[i];
            actorInput = data.ActorInput[i];


            actorInput.crouchPreviousFrame = actorInput.crouch;
            actorInput.actionToDoIndex = 0;

            UpdateAsAI(entity);
            UpdateAsPlayer(entity);

            //Write Entity Input
            data.ActorInput[i] = actorInput;
        }
    }

    private void UpdateAsAI(Entity entity)
    {
        if (EntityManager.HasComponent(entity, typeof(ActorPlayer)))
            return;
    }

    private void UpdateAsPlayer(Entity entity)
    {
        if (!EntityManager.HasComponent(entity, typeof(ActorPlayer)))
            return;


        //Get Player Input
        {
            //AXIS
            actorInput.movement.x = GInput.GetAxisRaw(GAxis.LEFTHORIZONTAL);
            actorInput.movement.z = GInput.GetAxisRaw(GAxis.LEFTVERTICAL);

            //BUTTONS
            if (GInput.GetButtonDown(GButton.BOTTOM)) actorInput.actionToDoIndex = 1;
            if (GInput.GetButtonDown(GButton.L3)) actorInput.crouch = (byte)(actorInput.crouch == 1 ? 0 : 1);
            actorInput.action = (byte)(GInput.GetButtonDown(GButton.LEFT) ? 1 : 0);
            actorInput.sprint = (byte)(GInput.GetButton(GButton.TOP) ? 1 : 0);
            actorInput.strafe = (byte)(GInput.GetButton(GButton.L2) ? 1 : 0);

            //actorInput.walk = 1;
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
