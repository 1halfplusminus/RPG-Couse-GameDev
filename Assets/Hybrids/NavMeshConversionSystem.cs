
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
