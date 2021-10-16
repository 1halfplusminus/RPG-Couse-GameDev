using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

public struct Counted : IComponentData {

}
[GenerateAuthoringComponent]
public struct CountTag: IComponentData {

}

public struct CountState : IComponentData {
    public int Count;
}
public class CubeCounterSystem : JobComponentSystem
{
    Entity stateEntity;
    EndSimulationEntityCommandBufferSystem commandBufferSystem;
    
    EntityQuery query;
    protected override void OnCreate()
    {
        base.OnCreate();
        stateEntity = EntityManager.CreateEntity();
        EntityManager.SetName(stateEntity, "CubeCounter");
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EntityManager.AddComponentData(stateEntity, new CountState{Count = 0});
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var handle = Entities.WithAll<CountTag>()
        .WithStoreEntityQueryInField(ref query)
        .ForEach((Entity e,int entityInQueryIndex)=>{
            commandBuffer.RemoveComponent<CountTag>(entityInQueryIndex,e);

        }).Schedule(inputDeps);
        commandBufferSystem.AddJobHandleForProducer(handle);

        var counts = new NativeArray<int>(1,Allocator.TempJob);
        counts[0] = query.CalculateEntityCount();
        handle = Entities.ForEach((ref CountState c)=> {
            c.Count += counts[0] ;
        })
        .WithDisposeOnCompletion(counts)
        .Schedule(handle);
        
        handle.Complete();
        
        return handle;
    }

}

