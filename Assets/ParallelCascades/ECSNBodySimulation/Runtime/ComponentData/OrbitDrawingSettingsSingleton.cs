using Unity.Entities;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    public struct OrbitDrawingSettingsSingleton : IComponentData
    {
        public int OrbitSamplesCount;
    }
}