using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class TerrainConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Terrain terrain)=>{
            AddHybridComponent(terrain);
            DeclareAssetDependency(terrain.gameObject,terrain.terrainData);
        });
    }
}
