using Unity.Entities;
using Unity.Animation;

using Unity.Animation.Hybrid;
using UnityEngine;
using UnityEngine.AddressableAssets;


#if UNITY_EDITOR
using UnityEditor.Animations;


[UpdateAfter(typeof(RigConversion))]
public class BlendTree1DPlayerConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((BlendTree1DPlayer player) =>
        {
            var entity = GetPrimaryEntity(player);
            var rigDefinition = DstEntityManager.GetComponentData<Rig>(entity);
            var clipConfiguration = new ClipConfiguration { Mask = ClipConfigurationMask.LoopTime };
            var bakeOptions = new BakeOptions
            {
                RigDefinition = rigDefinition.Value,
                ClipConfiguration = clipConfiguration,
                SampleRate = 60f
            };
            var blendTreeAsync = player.BlendTree.LoadAssetAsync<BlendTree>();
            blendTreeAsync.Completed += (loaded) =>
            {
                var blendTreeIndex = BlendTreeConversion.Convert(loaded.Result, entity, DstEntityManager, bakeOptions);
                var graphSetup = new BlendTree1DSetup
                {
                    BlendTreeIndex = blendTreeIndex,
                };
                DstEntityManager.AddComponentData(entity, graphSetup);
                DstEntityManager.AddComponent<DeltaTime>(entity);
            };

        });
    }
}
#endif
class BlendTree1DPlayer : MonoBehaviour
{

    public AssetReference BlendTree;

}