
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
       Entities.ForEach((NavMeshObstacle obstacle)=>{
          AddHybridComponent(obstacle);
       });
       Entities.ForEach((NavMeshAgent agent)=>{
          AddHybridComponent(agent);
       });
    }
}
