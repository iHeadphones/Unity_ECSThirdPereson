using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine;

public class CameraSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public ComponentArray<Transform> Transform;
        [ReadOnly] public SharedComponentDataArray<CameraTarget> CameraTarget;
    }

    public struct ClipPlanePoints
    {
        public Vector3[] points;
        public float hitDistance;
        public bool didCollide;
    }

    [Inject] private Data data;
    private Dictionary<int, float> currentDistances = new Dictionary<int, float>();
    private LayerMask collisionLayers = ~0;
    private float inputX;
    private float inputY;

    protected override void OnUpdate()
    {
        if (data.Length == 0)
            return;

        //Get Target Entity from priority
        var dt = Time.deltaTime;
        var cameraTransform = Camera.main.transform;
        var targetEntity = data.Entity[0];
        var targetTransform = data.Transform[0];
        var cameraTargetData = data.CameraTarget[0].data;

        //Get Entity Target From Prirorty
        for (int i = 0; i < data.Length; i++)
            if (data.CameraTarget[i].data.priority > cameraTargetData.priority)
            {
                targetEntity = data.Entity[i];
                targetTransform = data.Transform[i];
                cameraTargetData = data.CameraTarget[i].data;
            }

        //Get Camera Socekt
        targetTransform = targetTransform.Find("CameraSocket");

        //Rotate
        {
            inputX += GInput.GetAxisRaw(GAxis.RIGHTHORIZONTAL) * dt * cameraTargetData.rotationSpeed;
            inputY += GInput.GetAxisRaw(GAxis.RIGHTVERTICAL) * dt * cameraTargetData.rotationSpeed;
            inputY = Mathf.Clamp(inputY, cameraTargetData.minRotatonY, cameraTargetData.maxRotationY);
            cameraTransform.eulerAngles = new Vector3(inputY, inputX);
        }

        //Zoom
        {
            if (!currentDistances.ContainsKey(targetEntity.Index))
                currentDistances.Add(targetEntity.Index, cameraTargetData.defaultDistance);

           // currentDistances[targetEntity.Index] += Input.GetAxis("Mouse ScrollWheel") * cameraTargetData.zoomSpeed * dt;
            currentDistances[targetEntity.Index] = Mathf.Clamp(currentDistances[targetEntity.Index], cameraTargetData.minDistance, cameraTargetData.maxDistance);
        }

        //Move To Default Position
        cameraTransform.position = (targetTransform.position) - cameraTransform.forward * currentDistances[targetEntity.Index];

        //Collision
        {
            if (!cameraTargetData.doCollision)
                return;

            ClipPlanePoints nearClipPlanePoints = GetCameraClipPlanePoints();
            DetectCollision(ref nearClipPlanePoints, targetTransform);

            //Move To Position based on collision
            cameraTransform.position = (targetTransform.position) - cameraTransform.forward * ((nearClipPlanePoints.didCollide) ? nearClipPlanePoints.hitDistance : currentDistances[targetEntity.Index]);
        }
    }

    private ClipPlanePoints GetCameraClipPlanePoints()
    {
        //Variables
        ClipPlanePoints clipPlanePoints = new ClipPlanePoints();
        Transform transform = Camera.main.transform;

        float length = Camera.main.nearClipPlane;
        float height = Mathf.Tan((Camera.main.fieldOfView) * Mathf.Deg2Rad) * length;
        float width = height * Camera.main.aspect;

        clipPlanePoints.points = new Vector3[5];

        //Get Points
        clipPlanePoints.points[0] = (transform.position + transform.forward * length) + (transform.right * width) - (transform.up * height);
        clipPlanePoints.points[1] = (transform.position + transform.forward * length) - (transform.right * width) - (transform.up * height);
        clipPlanePoints.points[2] = (transform.position + transform.forward * length) + (transform.right * width) + (transform.up * height);
        clipPlanePoints.points[3] = (transform.position + transform.forward * length) - (transform.right * width) + (transform.up * height);
        clipPlanePoints.points[4] = (transform.position + transform.forward * length);

        return clipPlanePoints;
    }

    public void DetectCollision(ref ClipPlanePoints clipPlanePoints, Transform target)
    {
        RaycastHit hit;
        clipPlanePoints.hitDistance = -1f;

        for (int i = 0; i < clipPlanePoints.points.Length; i++)
        {
            if (Physics.Raycast(target.position, (clipPlanePoints.points[i] - target.position), out hit, Vector3.Distance(target.position, clipPlanePoints.points[i]), collisionLayers))
            {
                Debug.DrawLine(target.position, hit.point, Color.red);
                clipPlanePoints.didCollide = true;

                if (clipPlanePoints.hitDistance < 0 || hit.distance < clipPlanePoints.hitDistance)
                    clipPlanePoints.hitDistance = hit.distance;
            }
            else
                Debug.DrawLine(target.position, clipPlanePoints.points[i]);
        }
    }
}