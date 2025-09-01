using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct NBodyOutOfBoundsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NBodySimulationSettingsSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var settings = SystemAPI.GetSingleton<NBodySimulationSettingsSingleton>();

            var job = new ReturnInBoundsJob
            {
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                SimulationCenterEntity = settings.SimulationCenterEntity,
                SimulationBounds = settings.SimulationBounds,
                OutOfBoundsAcceleration = settings.OutOfBoundsAcceleration,
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ReturnInBoundsJob : IJobEntity
        {

            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            
            public Entity SimulationCenterEntity;
            public float3 SimulationBounds;

            // new: constant acceleration and deltaTime passed from system
            public float OutOfBoundsAcceleration;
            public float DeltaTime;
            
            private bool BodyOutOfBounds(float3 position)
            {
                var centerTransformPos = LocalTransformLookup[SimulationCenterEntity].Position;
                // compute position relative to the simulation center
                var rel = position - centerTransformPos;
                return math.abs(rel.x) >  SimulationBounds.x ||
                       math.abs(rel.y) > SimulationBounds.y ||
                       math.abs(rel.z) > SimulationBounds.z;
            }
            
            [BurstCompile]
            public void Execute(in LocalTransform localTransform, ref PhysicsVelocity physicsVelocity)
            {
                // check that localTransform is within bounds
                if (BodyOutOfBounds(localTransform.Position))
                {
                    // Apply acceleration toward the center body
                    var centerPos = LocalTransformLookup[SimulationCenterEntity].Position;
                    var toCenter = centerPos - localTransform.Position;
                    var dir = math.normalizesafe(toCenter); // safe normalize
                    // increment velocity by a constant acceleration toward center (scaled by deltaTime)
                    physicsVelocity.Linear += dir * OutOfBoundsAcceleration * DeltaTime;
                }
            }
        }

    }
}