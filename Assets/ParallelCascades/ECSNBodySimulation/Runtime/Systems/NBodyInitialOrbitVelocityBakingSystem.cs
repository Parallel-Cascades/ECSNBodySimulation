using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// This system and the Baking System below it initialize the starting velocities for a satellite to have a circular orbit around a primary body.
    /// There are two different systems since during baking, you want to recalculate all initial velocities - when you change a primary body's mass or position, you want all its satellites to update.
    /// At runtime, this system is used instead, and it only updates satellites when their OrbitData component changes.
    /// This does not account for any additional bodies in the scene, so the actual orbit will differ from a perfect circular one, but if you wanted perfect circular orbits, you wouldn't use an N-body simulation.
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [WorldSystemFilter( WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(SpawnSatellitesOnInitializeSystem))]
    public partial struct NBodyInitialOrbitVelocityRuntimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NBodySimulationSettingsSingleton>();
            state.RequireForUpdate<OrbitData>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new InitialVelocityJob
            {
                GravitationalConstant = SystemAPI.GetSingleton<NBodySimulationSettingsSingleton>().GravitationalConstant,
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                PhysicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true)
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            // Need to add the primary body's velocity to the satellite's velocity so that if the primary body is moving, the satellite will still orbit around it correctly.
            // And we do this after the initial round of calculation
            // Note: This can create a race condition if we have many nested levels of satellites
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(true);

            foreach (var (velocity,orbitData) in SystemAPI.Query<RefRW<PhysicsVelocity>,RefRO<OrbitData>>().WithChangeFilter<OrbitData>())
            {
                velocity.ValueRW.Linear += velocityLookup[orbitData.ValueRO.PrimaryBody].Linear;
            }
        }
        
        [WithChangeFilter(typeof(OrbitData))]
        [BurstCompile]
        private partial struct InitialVelocityJob : IJobEntity
        {
            [ReadOnly] public float GravitationalConstant;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<PhysicsMass> PhysicsMassLookup;
            
            private void Execute(
                in OrbitData orbitData,
                in LocalTransform transform,
                ref PhysicsVelocity velocity)
            {
                OrbitMechanicsUtility.CalculateCircularOrbitVelocityUpVector( 
                    transform.Position,
                    LocalTransformLookup[orbitData.PrimaryBody].Position,
                    PhysicsMassLookup[orbitData.PrimaryBody].InverseMass,
                    orbitData.OrbitUp,
                    GravitationalConstant,
                    ref velocity);
            }
        }
    }
    
    /// <summary>
    /// This baking system initializes the starting velocities for a satellite to have a circular orbit around a primary body.
    /// This does not account for any additional bodies in the scene, so the actual orbit will differ from a perfect circular one.
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [WorldSystemFilter( WorldSystemFilterFlags.BakingSystem)]
    public partial struct NBodyInitialOrbitVelocityBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NBodySimulationSettingsSingleton>();
            state.RequireForUpdate<OrbitBakingData>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new InitialVelocityJob
            {
                GravitationalConstant = SystemAPI.GetSingleton<NBodySimulationSettingsSingleton>().GravitationalConstant,
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                PhysicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true)
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            // Need to add the primary body's velocity to the satellite's velocity so that if the primary body is moving, the satellite will still orbit around it correctly.
            // And we do this after the initial round of calculation
            // Note: This can create a race condition if we have many nested levels of satellites
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(true);

            foreach (var (velocity,orbitBakingData) in SystemAPI.Query<RefRW<PhysicsVelocity>,RefRO<OrbitBakingData>>())
            {
                velocity.ValueRW.Linear += velocityLookup[orbitBakingData.ValueRO.Value.PrimaryBody].Linear;
            }
        }
        
        [BurstCompile]
        private partial struct InitialVelocityJob : IJobEntity
        {
            [ReadOnly] public float GravitationalConstant;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<PhysicsMass> PhysicsMassLookup;
            
            private void Execute(
                in OrbitBakingData bakingData,
                in LocalTransform transform,
                ref PhysicsVelocity velocity)
            {
                var primaryBody = bakingData.Value.PrimaryBody;
                OrbitMechanicsUtility.CalculateCircularOrbitVelocityUpVector( 
                    transform.Position,
                    LocalTransformLookup[primaryBody].Position,
                    PhysicsMassLookup[primaryBody].InverseMass,
                    bakingData.Value.OrbitUp,
                    GravitationalConstant,
                    ref velocity);
            }
        }
    }
    
}