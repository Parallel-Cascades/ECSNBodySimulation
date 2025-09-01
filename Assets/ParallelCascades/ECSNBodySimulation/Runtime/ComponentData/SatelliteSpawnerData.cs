using Unity.Entities;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    /// <summary>
    /// Component is IEnableable:
    /// After spawning satellites, the system disables it.
    /// If you want to respawn at runtime - re-enable it.
    /// </summary>
    public struct SatelliteSpawnerData : IComponentData, IEnableableComponent
    {
        public int Count;
        public float3 PrimaryBodyPosition;

        public quaternion OrbitAxisRotation;

        public float InnerRadius;

        public float OuterRadius;

        public float PitchMaxAngle;

        public float2 ScaleRange;
        
        public Entity SatellitePrefab;
        
        public int RandomSeedOffset;
        
        public OrbitData OrbitData;
        
        public int GetRandomSeed()
        {
            var seed = RandomSeedOffset;
            seed = (seed * 397) ^ Count;
            seed = (seed * 397) ^ (int)math.csum(PrimaryBodyPosition);
            seed = (seed * 397) ^ (int)InnerRadius;
            seed = (seed * 397) ^ (int)OuterRadius;
            return seed;
        }
    }
}