using UnityEngine;
using Unity.Entities;
using Unity.Animation;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

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
    public Entity Parent;
}

public struct SkinnedMeshBone : IBufferElementData
{
    public Entity Entity;
    public Entity Parent;

}

[DisableAutoCreation]
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
                DstEntityManager.AddComponentData<Bone>(entity, new Bone { Parent = GetPrimaryEntity(transform.parent) });
                DeclareLinkedEntityGroup(transform.gameObject);
            }
        });
        Entities.ForEach((SkinnedMeshRenderer skinnedMesh) =>
        {
            AddHybridComponent(skinnedMesh);
            var skinnedMeshEntity = GetPrimaryEntity(skinnedMesh);
            var rootBoneEntity = GetPrimaryEntity(skinnedMesh.rootBone);
            DstEntityManager.AddComponentData<RootBone>(skinnedMeshEntity, new RootBone { Entity = rootBoneEntity });
            var boneBuffer = DstEntityManager.AddBuffer<SkinnedMeshBone>(skinnedMeshEntity);
            DeclareLinkedEntityGroup(skinnedMesh.rootBone.gameObject);
            foreach (var bone in skinnedMesh.bones)
            {
                DeclareLinkedEntityGroup(bone.gameObject);
                var boneEntity = GetPrimaryEntity(bone);
                var boneParentEntity = GetPrimaryEntity(bone.parent);
                boneBuffer.Add(new SkinnedMeshBone { Entity = boneEntity, Parent = boneParentEntity });
            }
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

        });
    }
}
[DisableAutoCreation]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SkinnedMeshRendererBoneLinkConvertionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, SkinnedMeshRenderer skinnedMesh, ref AnimatorComponent animatorComponent, ref RootBone rootBone) =>
        {
            skinnedMesh.enabled = false;
            skinnedMesh.updateWhenOffscreen = true;
            skinnedMesh.gameObject.hideFlags = HideFlags.None;
            var rootBoneTransform = EntityManager.GetComponentObject<Transform>(rootBone.Entity);
            var animator = EntityManager.GetComponentObject<Animator>(animatorComponent.Entity);
            var animatorTransform = EntityManager.GetComponentObject<Transform>(animatorComponent.Entity);
            var bones = EntityManager.GetBuffer<SkinnedMeshBone>(e);
            animator.enabled = false;

            skinnedMesh.rootBone = rootBoneTransform;
            skinnedMesh.transform.parent = animatorTransform;
            for (int i = 0; i < bones.Length; i++)
            {
                var boneTransform = EntityManager.GetComponentObject<Transform>(bones[i].Entity);

                skinnedMesh.bones[i] = boneTransform;
            }
            /*     rootBoneTransform.parent.parent = animatorTransform; */
            animator.Rebind();

        });
        Entities.WithAll<Parent>().ForEach((Entity entity, ref Bone bone, Transform transform) =>
        {
            var boneParent = EntityManager.GetComponentObject<Transform>(bone.Parent);
            transform.gameObject.name = transform.gameObject.name.Replace("(Clone)", "");
            /*       transform.gameObject.hideFlags = HideFlags.None; */
            transform.parent = boneParent;
            /*    EntityManager.AddComponent<CopyTransformFromGameObject>(entity); */
        });
    }
}

[DisableAutoCreation]
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
        Entities
        .WithoutBurst()
        .ForEach((Entity e, SkinnedMeshRenderer renderer) =>
        {
            /* var mesh = renderer.sharedMesh;
            renderer.BakeMesh(mesh);
            RenderMeshUtility.AddComponents(e, commandBuffer, new Unity.Rendering.RenderMeshDescription(mesh, renderer.sharedMaterial)); */
        }).Run();

        Entities
        .WithoutBurst()
        .ForEach(
        (Entity e, DeltaTime deltaTime, Animator animator, ref DynamicBuffer<AnimatedLocalToWorld> animatedToWorld, ref DynamicBuffer<AnimatedData> animatedDatas) =>
        {
            animator.Update(deltaTime.Value);

            var skinnedMeshsEntities = EntityManager.GetBuffer<SkinnedMeshElement>(e);
            var skinnedMeshEntity = skinnedMeshsEntities[0].Entity;
            var bonesEntities = EntityManager.GetBuffer<SkinnedMeshBone>(skinnedMeshEntity);
            /* for (int i = 0; i < bonesEntities.Length; i++)
            {
                UnityEngine.Debug.Log("update animated local to wordl");
                var boneEntity = bonesEntities[i];
                var boneTransform = EntityManager.GetComponentObject<Transform>(boneEntity.Entity);
                 commandBuffer.AddComponent<AnimationLocalToWorldOverride>(boneEntity.Entity, new AnimationLocalToWorldOverride());
                animatedToWorld[i] = new AnimatedLocalToWorld{Value = boneTransform.localToWorldMatrix};
                commandBuffer.AddComponent<AnimationLocalToWorldOverride>(boneEntity.Entity, new AnimationLocalToWorldOverride());
                animatedToWorld[i] = new AnimatedLocalToWorld{Value = boneTransform.localToWorldMatrix}; 
                animatedDatas[i] = new AnimatedData{ Value = 1.0f};
                animatedToWorld[i] = new AnimatedLocalToWorld{Value = boneTransform.localToWorldMatrix.inverse}; 
            } */
            var currentClip = animator.GetCurrentAnimatorClipInfo(0);
            UnityEngine.Debug.Log("Playing clip:" + currentClip[0].clip.name);
        }).Run();
    }
}