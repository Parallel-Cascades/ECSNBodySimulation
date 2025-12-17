using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.MaterialProperties
{
    [MaterialProperty("_Color_A")]
    public struct URPMaterialPropertyColorA : IComponentData
    {
        public float4 Value;
    }
    
    [MaterialProperty("_Color_B")]
    public struct URPMaterialPropertyColorB : IComponentData
    {
        public float4 Value;
    }
    
    [MaterialProperty("_Fresnel_Color")]
    public struct URPMaterialPropertyFresnelColor : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("_Saturation")]
    public struct URPMaterialPropertySaturation : IComponentData
    {
        public float Value;
    }
}