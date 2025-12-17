using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.MaterialProperties;
using ParallelCascades.ECSNBodySimulation.Runtime.Utilities;
using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class URPPlanetShaderMaterialPropertiesAuthoring : MonoBehaviour
    {
        [SerializeField] private float m_saturation = 1;
        [SerializeField] private Color m_colorA = Color.white;
        [SerializeField] private Color m_colorB = Color.black;
        [SerializeField] private Color m_fresnelColor = Color.white;
        
        private class URPPlanetShaderMaterialPropertiesAuthoringBaker : Baker<
            URPPlanetShaderMaterialPropertiesAuthoring>
        {
            public override void Bake(URPPlanetShaderMaterialPropertiesAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                
                AddComponent(entity, new URPMaterialPropertySaturation
                {
                    Value = authoring.m_saturation
                });
                
                AddComponent(entity, new URPMaterialPropertyFresnelColor
                {
                    Value = authoring.m_fresnelColor.ToFloat4()
                });
                
                AddComponent(entity, new URPMaterialPropertyColorA
                {
                    Value = authoring.m_colorA.ToFloat4()
                });
                
                AddComponent(entity, new URPMaterialPropertyColorB
                {
                    Value = authoring.m_colorB.ToFloat4()
                });
            }
        }
        
    }
    
}