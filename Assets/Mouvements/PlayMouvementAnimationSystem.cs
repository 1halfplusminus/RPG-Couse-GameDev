
using Unity.Entities;

public class PlayMouvementAnimationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref BlendTree1DData player,in Mouvement mouvement)=>{
            player.paramX = mouvement.Velocity.Linear.z;
        }).ScheduleParallel();
    }
}