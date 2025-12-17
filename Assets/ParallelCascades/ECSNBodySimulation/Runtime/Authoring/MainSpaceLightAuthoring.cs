using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class MainSpaceLightAuthoring : MonoBehaviour
    {
        private class MainSpaceLightAuthoringBaker : Baker<MainSpaceLightAuthoring>
        {
            public override void Bake(MainSpaceLightAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MainSpaceLightSingletonTag>(entity);
            }
        }
    }
}