using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct SpawnSatellitesOnInitializeSystem : ISystem
    {
        private EntityQuery m_SpawnersQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_SpawnersQuery = SystemAPI.QueryBuilder().WithAll<SatelliteSpawnerData>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var spawnersArray = m_SpawnersQuery.ToEntityArray(Allocator.TempJob);

            foreach (var spawner in spawnersArray)
            {
                var spawnSettings = SystemAPI.GetComponent<SatelliteSpawnerData>(spawner);
                
                int satelliteCount = spawnSettings.Count;
                
                var prefabInstances = state.EntityManager.Instantiate(spawnSettings.SatellitePrefab, satelliteCount,Allocator.Temp);
                var scales = new NativeArray<float>(satelliteCount, Allocator.Temp);
                var positions = new NativeArray<float3>(satelliteCount, Allocator.Temp);
                
                RandomUtilities.GenerateOrbitPointsBestCandidate(
                    spawnSettings.PrimaryBodyPosition,
                    spawnSettings.OrbitAxisRotation,
                    spawnSettings.InnerRadius,
                    spawnSettings.OuterRadius,
                    spawnSettings.PitchMaxAngle,
                    spawnSettings.ScaleRange,
                    ref scales,
                    ref positions,
                    spawnSettings.GetRandomSeed());
                
                for (var i = 0; i < prefabInstances.Length; i++)
                {
                    var instance = prefabInstances[i];
                    state.EntityManager.AddComponentData(instance, spawnSettings.OrbitData);
                    
                    var scale = scales[i];

                    var physicsMass = SystemAPI.GetComponentRW<PhysicsMass>(instance);
                    float volume = scale * scale * scale;
                    physicsMass.ValueRW.InverseMass /= volume; // Scale mass by volume
                    
                    if (SystemAPI.HasComponent<PlanetModelData>(instance))
                    {
                        var modelEntity = state.EntityManager.GetComponentData<PlanetModelData>(instance);
                        state.EntityManager.SetComponentData(modelEntity.ModelEntity, LocalTransform.FromScale(scale));
                        state.EntityManager.SetComponentData(instance, LocalTransform.FromPosition(positions[i]));
                    }
                    else
                    {
                        state.EntityManager.SetComponentData(instance, LocalTransform.FromPositionRotationScale(positions[i], quaternion.identity, scale));
                    }
                }
                
                // Don't spawn again
                state.EntityManager.SetComponentEnabled<SatelliteSpawnerData>(spawner,false);
            }
        }
    }
}