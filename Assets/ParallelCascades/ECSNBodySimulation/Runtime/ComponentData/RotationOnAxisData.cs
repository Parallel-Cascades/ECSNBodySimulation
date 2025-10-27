using Unity.Entities;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    public struct RotationOnAxisData : IComponentData
    {
        public float3 Value;
    }
}