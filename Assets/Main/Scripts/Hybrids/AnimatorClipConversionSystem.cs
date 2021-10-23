using UnityEngine;
using Unity.Entities;
using Unity.Animation.Hybrid;
using Unity.Animation;


public struct AnimatorClip : IBufferElementData
{
    public BlobAssetReference<Clip> Clip;
}

#if UNITY_EDITOR
[DisableAutoCreation]
public class AnimatorClipConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Animator animator) =>
        {
            var skinnedMeshs = animator.GetComponentsInChildren<SkinnedMeshRenderer>();
            var animatorEntity = GetPrimaryEntity(animator);
            var clipsBuffer = DstEntityManager.AddBuffer<AnimatorClip>(animatorEntity);
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                DeclareAssetDependency(animator.gameObject, clip);
                var convertedClip = BlobAssetStore.GetClip(clip);
                clipsBuffer.Add(new AnimatorClip { Clip = convertedClip });
            }
        });
    }
}
#endif