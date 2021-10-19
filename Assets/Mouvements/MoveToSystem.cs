using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class MoveToNavMeshAgentSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    EntityQuery navMeshAgentQueries;
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
       var commandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
       Entities
       .WithStoreEntityQueryInField(ref navMeshAgentQueries)
       .WithoutBurst()
       .WithAll<Mouvement>()
       .ForEach(( NavMeshAgent agent, ref Translation position,ref Mouvement mouvement, in  MoveTo moveTo)=>{
           if(agent.isOnNavMesh) {
                agent.SetDestination( moveTo.Position);
                position.Value = agent.transform.position; 
                Debug.Log("Moving toward: " + agent.destination);
                mouvement.Velocity = new Velocity{Linear = agent.velocity, Angular = agent.angularSpeed};
           }
       }).Run();

       Entities.ForEach((int entityInQueryIndex,Entity e, in MoveTo moveTo, in LocalToWorld localToWorld)=> {
           if(moveTo.Position.Equals( localToWorld.Position)) {
               commandBuffer.RemoveComponent<MoveTo>(entityInQueryIndex, e);
               Debug.Log("Arrive at destination");
           }
       }).Schedule();
       endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}
