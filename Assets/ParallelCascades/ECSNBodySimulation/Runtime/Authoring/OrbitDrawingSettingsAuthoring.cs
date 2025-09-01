using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class OrbitDrawingSettingsAuthoring : MonoBehaviour
    {
        public int orbitSamplesCount = 1000;
        
        private void Reset()
        {
            orbitSamplesCount = 1000;
        }

        private class OrbitDrawingSettingsAuthoringBaker : Baker<OrbitDrawingSettingsAuthoring>
        {
            public override void Bake(OrbitDrawingSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new OrbitDrawingSettingsSingleton
                {
                    OrbitSamplesCount = authoring.orbitSamplesCount,
                });
            }
        }
    }
}