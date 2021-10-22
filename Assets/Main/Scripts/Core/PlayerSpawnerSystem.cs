using Unity.Entities;

public struct PlayerSpawner: MonoBehaviour {
    GameObject Prefabs;
}
[GenerateAuthoringComponent]
public struct SpawPlayer : IComponentData {
    Entity Prefabs;
}

public class PlayerSpawnerSystem : GameObjectConversionSystem {
    
}