using UnityEngine;
using Unity.Entities;
using Unity.Animation.Hybrid;
using Unity.Animation;

public struct SkinnedMeshElement : IBufferElementData
{
    public Entity Entity;
}
public struct AnimatorComponent : IComponentData
{
    public Entity Entity;
}
public struct RootBone : IComponentData
{
    public Entity Entity;
}
public struct Bone : IComponentData
{

}

public struct SkinnedMeshBone : IBufferElementData
{
    public Entity Entity;
    public Entity Parent;

}

public struct AnimatorClip : IBufferElementData
{
    public BlobAssetReference<Clip> Clip;
}
#if UNITY_EDITOR
public class SkinnedMeshRendererConvertionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {

        Entities.ForEach((Transform transform) =>
        {
            if (transform.GetComponentInParent<Animator>() != null)
            {
                AddHybridComponent(transform);
                var entity = GetPrimaryEntity(transform);
                DstEntityManager.AddComponent<Bone>(entity);
            }
        });
        Entities.ForEach((SkinnedMeshRenderer skinnedMesh) =>
        {
            skinnedMesh.updateWhenOffscreen = true;
            skinnedMesh.enabled = false;
            AddHybridComponent(skinnedMesh);
            var skinnedMeshEntity = GetPrimaryEntity(skinnedMesh);
            var rootBoneEntity = GetPrimaryEntity(skinnedMesh.rootBone);
            DstEntityManager.AddComponentData<RootBone>(skinnedMeshEntity, new RootBone { Entity = rootBoneEntity });
            var boneBuffer = DstEntityManager.AddBuffer<SkinnedMeshBone>(skinnedMeshEntity);
            foreach (var bone in skinnedMesh.bones)
            {
                var boneEntity = GetPrimaryEntity(bone);
                var boneParentEntity = GetPrimaryEntity(bone.parent);
                boneBuffer.Add(new SkinnedMeshBone { Entity = boneEntity, Parent = boneParentEntity });
            }
            /*         var newSkinnedMesh= EntityManager.GetComponentObject<SkinnedMeshRenderer>(skinnedMeshEntity); */
        });
        Entities.ForEach((Animator animator) =>
        {
            AddHybridComponent(animator);
            var skinnedMeshs = animator.GetComponentsInChildren<SkinnedMeshRenderer>();
            var animatorEntity = GetPrimaryEntity(animator);
            DstEntityManager.AddComponent<DeltaTime>(animatorEntity);
            foreach (var skinnedMesh in skinnedMeshs)
            {
                var skinnedMeshEntity = GetPrimaryEntity(skinnedMesh);
                DstEntityManager.AddComponentData(skinnedMeshEntity, new AnimatorComponent { Entity = animatorEntity });
            }
            var skinnedMeshsBuffer = DstEntityManager.AddBuffer<SkinnedMeshElement>(animatorEntity);
            foreach (var skinnedMesh in skinnedMeshs)
            {
                var skinnedMeshEntity = GetPrimaryEntity(skinnedMesh);
                skinnedMeshsBuffer.Add(new SkinnedMeshElement { Entity = skinnedMeshEntity });
            }
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
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SkinnedMeshRendererBoneLikeConvertionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, SkinnedMeshRenderer skinnedMesh, ref AnimatorComponent animatorComponent, ref RootBone rootBone) =>
        {

            var transform = EntityManager.GetComponentObject<Transform>(rootBone.Entity);
            var animator = EntityManager.GetComponentObject<Animator>(animatorComponent.Entity);
            var bones = EntityManager.GetBuffer<SkinnedMeshBone>(e);

            transform.parent = animator.transform;
            skinnedMesh.rootBone = transform;
            skinnedMesh.transform.parent = animator.transform;
            for (int i = 0; i < bones.Length; i++)
            {
                var boneTransform = EntityManager.GetComponentObject<Transform>(bones[i].Entity);
                var boneParent = EntityManager.GetComponentObject<Transform>(bones[i].Parent);
                boneTransform.parent = boneParent;
                skinnedMesh.bones[i] = boneTransform;
            }

        });
        Entities.ForEach((Entity entity, ref Bone bone) =>
        {
            /*   EntityManager.AddComponent<CopyTransformFromGameObject>(entity); */
        });
    }
}


public class SkinnedMeshRendererHybridSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        Entities.WithoutBurst().ForEach((Entity e, SkinnedMeshRenderer renderer) =>
        {
            /* var mesh = renderer.sharedMesh;
            renderer.BakeMesh(mesh);
            RenderMeshUtility.AddComponents(e,commandBuffer, new RenderMeshDescription(mesh,renderer.sharedMaterial)); */
        }).Run();
        var deltaTime = Time.DeltaTime;
        Entities
        .WithoutBurst()
        .ForEach(
        (Entity e, Animator animator, ref DynamicBuffer<AnimatedLocalToWorld> animatedToWorld, in DynamicBuffer<AnimatorClip> clips) =>
        {
            animator.Update(deltaTime);
            /* var skinnedMeshsEntities = EntityManager.GetBuffer<SkinnedMeshElement>(e);
            var skinnedMeshEntity = skinnedMeshsEntities[0].Entity;
            var bonesEntities = EntityManager.GetBuffer<SkinnedMeshBone>(skinnedMeshEntity);
            for (int i = 0; i < bonesEntities.Length; i++)
            {
                var boneTransform = EntityManager.GetComponentObject<Transform>(bonesEntities[i].Entity);
                trnasf
                animatedToWorld[i] = new AnimatedLocalToWorld { Value = boneTransform.worldToLocalMatrix };
            } */
            var currentClip = animator.GetCurrentAnimatorClipInfo(0);
            Unity.Animation.Debug.Log("Playing clip:" + currentClip[0].clip.name);
            commandBuffer.AddComponent(e, new PlayClip { Clip = clips[0].Clip });
        }).Run();
    }
}