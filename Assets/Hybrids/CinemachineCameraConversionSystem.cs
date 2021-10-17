using Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Follow: IComponentData {
    public Entity Entity;
}

public struct LookAt : IComponentData {
    public Entity Entity;
}

public class CinemachineCameraConversionSystem : GameObjectConversionSystem {
    // TODO: Clean up put all follow target in a same parent game object
    protected override void OnUpdate () { 
        Entities.ForEach((CinemachineVirtualCamera virtualCamera)=> {
            AddHybridComponent(virtualCamera);
            if(virtualCamera.m_Follow != null) {
                var followedEntity = TryGetPrimaryEntity(virtualCamera.m_Follow.gameObject);
                if(followedEntity != Entity.Null) {
                
                  /*   DeclareDependency(virtualCamera, virtualCamera.m_Follow);
                    AddHybridComponent(virtualCamera.m_Follow); */
                    Debug.Log("Follow " + followedEntity.Index);
                    DstEntityManager.AddComponentData(GetPrimaryEntity(virtualCamera), new Follow() { Entity = followedEntity}); 
                }
            }
            if(virtualCamera.m_LookAt != null) {
                var lookAtEntity = TryGetPrimaryEntity(virtualCamera.m_LookAt.gameObject);
                if(lookAtEntity != Entity.Null) {
                    Debug.Log("Look At " + lookAtEntity.Index);
                    DstEntityManager.AddComponentData(GetPrimaryEntity(virtualCamera), new LookAt() { Entity = lookAtEntity}); 
                }
                AddHybridComponent(virtualCamera.m_LookAt);
            }
            
        });
    }
}

public class CinemachineVirtualCameraHybriSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
        .WithoutBurst()
        .ForEach((CinemachineVirtualCamera camera, in Follow target)=>{
            if(EntityManager.HasComponent<LocalToWorld>(target.Entity)) {
                // TODO: Put Somewhere else
                camera.enabled = false;
                camera.enabled = true;
             
                var transform = EntityManager.GetComponentObject<Transform>(target.Entity);
                var targetPosition = EntityManager.GetComponentData<LocalToWorld>(target.Entity);
                camera.m_Follow = transform;
            }
        }).Run();
         Entities
        .WithoutBurst()
        .ForEach((CinemachineVirtualCamera camera, in LookAt target)=>{
            if(EntityManager.HasComponent<LocalToWorld>(target.Entity)) {
                var transform = EntityManager.GetComponentObject<Transform>(target.Entity);
                var targetPosition = EntityManager.GetComponentData<LocalToWorld>(target.Entity);
                camera.m_LookAt = transform;
            }
        }).Run();
    }
}