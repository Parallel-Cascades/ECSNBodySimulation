using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// Performs N-body gravity simulation at runtime by calculating the gravitational forces between celestial bodies.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    public partial struct NBodyGravitySystem : ISystem
    {
        private EntityQuery m_GravityContributingBodiesQuery;
        private EntityQuery m_StaticGravityContributingBodiesQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsVelocity>();
            state.RequireForUpdate<NBodySimulationSettingsSingleton>();

            m_GravityContributingBodiesQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, PhysicsMass, NBodyEntity>().WithNone<NBodyDoNotContributeToGravityTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transforms =
                m_GravityContributingBodiesQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var masses =
                m_GravityContributingBodiesQuery.ToComponentDataArray<PhysicsMass>(Allocator.TempJob);

            NBodySimulationSettingsSingleton universeSingletonData = SystemAPI.GetSingleton<NBodySimulationSettingsSingleton>();

            state.Dependency = new VelocityUpdateJob
            {
                GravitationalConstant = universeSingletonData.GravitationalConstant,
                DeltaTime = SystemAPI.Time.DeltaTime, // This system is part of the FixedRateSimulationSystemGroup, so this returns a fixed time step.
                Transforms = transforms,
                Masses = masses
            }.ScheduleParallel(state.Dependency);

            transforms.Dispose(state.Dependency);
            masses.Dispose(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(NBodyEntity))]
        [WithNone(typeof(NBodyDoNotReceiveGravityTag))]
        public partial struct VelocityUpdateJob : IJobEntity
        {
            public float GravitationalConstant;
            public float DeltaTime; // This is a physics system, so should be independent of framerate
            [ReadOnly] public NativeArray<LocalTransform> Transforms;
            [ReadOnly] public NativeArray<PhysicsMass> Masses;
            
            private void Execute(in LocalTransform transform, in PhysicsMass mass, ref PhysicsVelocity velocity)
            {
                float3 forces = float3.zero;
                float inverseMassThisBody = mass.InverseMass;
                for (int i = 0; i < Transforms.Length; i++)
                {
                    if (Transforms[i].Position.Equals(transform.Position)) continue;
                    
                    float inverseMassOtherBody = Masses[i].InverseMass;

                    float squaredDistance = math.lengthsq(Transforms[i].Position - transform.Position);
                    float3 forceDir = math.normalize(Transforms[i].Position - transform.Position);
                    forces += forceDir * GravitationalConstant / (squaredDistance * inverseMassThisBody * inverseMassOtherBody);
                }
                
                velocity.Linear += (forces * inverseMassThisBody) * DeltaTime;
            }
        }
    }
}