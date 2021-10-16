using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

public class UIDocumentConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((UIDocument ui)=>{
            AddHybridComponent(ui);
        });
    }
}
public class DisplaySystem : SystemBase
{
    protected override void OnUpdate()
    {
        var state = GetSingleton<CountState>();
        Entities
        .WithoutBurst()
        .ForEach((UIDocument document) => {
            var root = document.rootVisualElement;
            root.Q<Label>("Count").text = state.Count.ToString();
        }).Run();
    }
}
