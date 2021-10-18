using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PlayerControlled : IComponentData {

}



public class ConvertPlayerControlled : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<PlayerControlled>().ForEach((Transform t) =>{
            var entity = GetPrimaryEntity(t);
            DstEntityManager.AddComponentData(entity, new MouseClick());
        });
    }
}