using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Physics;
using static ExtensionMethods.EcsConversionExtension;
public struct MouseClick: IComponentData {
    public float3 ScreenCordinate;
    public Unity.Physics.Ray Ray;
}

public class PlayerMouseInputSystem : JobComponentSystem
{
    EntityCommandBufferSystem entityCommandBufferSystem;
    GameInput input;
    protected override void OnCreate() { 
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        input = new GameInput();
        input.Enable();
        input.Gameplay.Click.performed += (ctx)=>{
            var mouseClick = EntityManager.CreateEntity(typeof(MouseClick));  
            float2 value = Pointer.current.position.ReadValue();
            var ray = FromEngineRay(Camera.main.ScreenPointToRay(new float3(value,0f)));
            EntityManager.AddComponentData(mouseClick, new MouseClick{ScreenCordinate = ray.Origin,Ray = ray });        
        };
    }   
    protected override void OnDestroy() {
        input.Disable();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var handles = Entities.ForEach((Entity e,int entityInQueryIndex,in MouseClick m)=>{
            commandBuffer.RemoveComponent<MouseClick>(entityInQueryIndex,e);
        }).Schedule(inputDeps);
        entityCommandBufferSystem.AddJobHandleForProducer(handles);
        return handles;
    }
}

public class DebugPlayerMouseInputSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var camera = Camera.main;
        Entities.ForEach((ref MouseClick c)=>{
            Debug.DrawRay(c.Ray.Origin,c.Ray.Displacement * 100f,Color.red,10f);
            Debug.Log("Mouse clicked " + c.ScreenCordinate);
        });
    }
}