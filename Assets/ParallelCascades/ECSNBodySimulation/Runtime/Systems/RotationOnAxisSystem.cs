using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct RotationOnAxisSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotationOnAxisData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RotationOnAxisJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        public partial struct RotationOnAxisJob : IJobEntity
        {
            public float DeltaTime;
            
            [BurstCompile]
            public void Execute(ref LocalTransform localTransform, in RotationOnAxisData rotationOnAxisData)
            {
                float3 eulerRadians = math.radians(rotationOnAxisData.Value * DeltaTime);
            
                quaternion rotation = quaternion.EulerXYZ(eulerRadians);
            
                localTransform.Rotation = math.mul(localTransform.Rotation, rotation);
            }
        }
    }
}