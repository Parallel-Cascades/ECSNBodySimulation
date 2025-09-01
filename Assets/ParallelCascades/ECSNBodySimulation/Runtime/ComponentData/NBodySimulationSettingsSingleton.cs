using Unity.Entities;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    public struct NBodySimulationSettingsSingleton : IComponentData
    {
        public float GravitationalConstant;
        public float FixedDeltaTime;
        public float3 SimulationBounds;
        
        public Entity SimulationCenterEntity;
        public float OutOfBoundsAcceleration;
    }
}