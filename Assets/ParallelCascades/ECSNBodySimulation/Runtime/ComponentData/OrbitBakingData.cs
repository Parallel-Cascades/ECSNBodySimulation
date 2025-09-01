using Unity.Entities;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    [BakingType]
    public struct OrbitBakingData : IComponentData
    {
        public OrbitData Value;
    }
    
    public struct OrbitData : IComponentData
    {
        public Entity PrimaryBody;
        
        public float3 OrbitUp;
    }
}