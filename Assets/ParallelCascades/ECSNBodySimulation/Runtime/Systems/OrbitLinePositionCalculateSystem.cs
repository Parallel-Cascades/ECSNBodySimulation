using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// This system runs after the initial orbit velocity baking system, and calculates future positions stored in a dynamic buffer.
    /// that is then used by the Editor system OrbitLineDrawingSystem to visualise the orbit lines in the editor.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateAfter(typeof(ECSNBodySimulation.Runtime.Systems.NBodyInitialOrbitVelocityBakingSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial struct OrbitLinePositionCalculationSystem : ISystem
    {
        private EntityQuery m_AllNBodyEntitiesQuery;
        private EntityQuery m_BodiesReceivingGravityQuery;
        private EntityQuery m_BodiesContributingGravityQuery;
        
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NBodySimulationSettingsSingleton>();
            state.RequireForUpdate<OrbitDrawingSettingsSingleton>();
            state.RequireForUpdate<FutureOrbitBufferElement>();
            
            m_AllNBodyEntitiesQuery = SystemAPI.QueryBuilder()
                .WithAll<NBodyEntity,LocalTransform, PhysicsMass, PhysicsVelocity>().Build();
            m_BodiesReceivingGravityQuery = SystemAPI.QueryBuilder()
                .WithAll<NBodyEntity,LocalTransform, PhysicsMass, PhysicsVelocity>().WithNone<NBodyDoNotReceiveGravityTag>()
                .Build();
            m_BodiesContributingGravityQuery = SystemAPI.QueryBuilder()
                .WithAll<NBodyEntity,LocalTransform, PhysicsMass, PhysicsVelocity>().WithNone<NBodyDoNotContributeToGravityTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformsArray = m_AllNBodyEntitiesQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            NativeArray<float3> positions = new NativeArray<float3>(transformsArray.Length, Allocator.TempJob);
            for (int i = 0; i < transformsArray.Length; i++)
            {
                positions[i] = transformsArray[i].Position;
            }
            
            var physicsMassArray = m_AllNBodyEntitiesQuery.ToComponentDataArray<PhysicsMass>(Allocator.Temp);
            NativeArray<float> inverseMasses = new NativeArray<float>(physicsMassArray.Length, Allocator.TempJob);
            for (int i = 0; i < physicsMassArray.Length; i++)
            {
                inverseMasses[i] = physicsMassArray[i].InverseMass;
            }
            
            var velocitiesArray = m_AllNBodyEntitiesQuery.ToComponentDataArray<PhysicsVelocity>(Allocator.Temp);
            NativeArray<float3> velocities = new NativeArray<float3>(velocitiesArray.Length, Allocator.TempJob);
            for (int i = 0; i < velocitiesArray.Length; i++)
            {
                velocities[i] = velocitiesArray[i].Linear;
            }
            
            var entitiesArray = m_AllNBodyEntitiesQuery.ToEntityArray(Allocator.TempJob);
            
            NativeList<int> gravityContributingBodyIndices = new NativeList<int>(Allocator.TempJob);
            NativeList<int> gravityReceivingBodyIndices = new NativeList<int>(Allocator.TempJob);
            for (var i = 0; i < entitiesArray.Length; i++)
            {
                var entity = entitiesArray[i];
                if (m_BodiesContributingGravityQuery.Matches(entity))
                {
                    gravityContributingBodyIndices.Add(i);
                }
                if (m_BodiesReceivingGravityQuery.Matches(entity))
                {
                    gravityReceivingBodyIndices.Add(i);
                }
            }

            var newPositions = new NativeArray<float3>(positions.Length, Allocator.TempJob);
            var newVelocities = new NativeArray<float3>(velocities.Length, Allocator.TempJob);
            
            // clear buffers
            foreach(var (localTransform,orbitBuffer) in SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<FutureOrbitBufferElement>>())
            {
                orbitBuffer.Clear();
                
                // Add the initial position to the orbit buffer
                orbitBuffer.Add(new FutureOrbitBufferElement
                {
                    Position = localTransform.ValueRO.Position,
                });
            }

            var orbitDrawingSettings = SystemAPI.GetSingleton<OrbitDrawingSettingsSingleton>();
            var simulationStepCount = orbitDrawingSettings.OrbitSamplesCount;

            var nBodySimSettings = SystemAPI.GetSingleton<NBodySimulationSettingsSingleton>();
            for (int step = 0; step < simulationStepCount; step++)
            {
                var job = new OrbitPositionCalculateTwoJob()
                {
                    Positions = positions,
                    InverseMasses = inverseMasses,
                    Velocities = velocities,
                    GravityContributingBodyIndices = gravityContributingBodyIndices,
                    GravityReceivingBodyIndices = gravityReceivingBodyIndices,
                    NewPositions = newPositions,
                    NewVelocities = newVelocities,
                    GravitationConstant = nBodySimSettings.GravitationalConstant,
                    DeltaTime = nBodySimSettings.FixedDeltaTime,
                };
                var calculateJobHandle = job.ScheduleParallelByRef(positions.Length, 64, default);
                calculateJobHandle.Complete();
                
                for (int i = 0; i < positions.Length; i++)
                {
                    if (SystemAPI.HasBuffer<FutureOrbitBufferElement>(entitiesArray[i]))
                    {
                        var orbitBuffer = SystemAPI.GetBuffer<FutureOrbitBufferElement>(entitiesArray[i]);
                        // Add the new position to the orbit buffer
                        orbitBuffer.Add(new FutureOrbitBufferElement
                        {
                            Position = newPositions[i]
                        });
                    }
                }

                // Copy newPositions and newVelocities to positions and velocities for the next step
                NativeArray<float3>.Copy(newPositions, positions);
                NativeArray<float3>.Copy(newVelocities, velocities);
            }
            
            positions.Dispose();
            inverseMasses.Dispose();
            velocities.Dispose();
            entitiesArray.Dispose();
            gravityContributingBodyIndices.Dispose();
            gravityReceivingBodyIndices.Dispose();
            newPositions.Dispose();
            newVelocities.Dispose();
        }

        [BurstCompile]
        private struct OrbitPositionCalculateJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<float3> Positions;
            [ReadOnly]
            public NativeArray<float> InverseMasses;
            [ReadOnly]
            public NativeArray<float3> Velocities;
            
            [WriteOnly]
            public NativeArray<float3> NewPositions;
            [WriteOnly]
            public NativeArray<float3> NewVelocities;

            public float GravitationConstant;
            public float DeltaTime; // Should be equal or very close the delta time used by gravity system for correct prediction
            
            public void Execute(int currentBodyIndex)
            {
                float3 forces = float3.zero;
                for(int otherBodyIndex = 0; otherBodyIndex < Positions.Length; otherBodyIndex++)
                {
                    if (currentBodyIndex == otherBodyIndex) continue; // Skip self

                    float squaredDistance = math.lengthsq(Positions[otherBodyIndex] - Positions[currentBodyIndex]);
                    float3 forceDir = math.normalize(Positions[otherBodyIndex] - Positions[currentBodyIndex]);
                    forces += forceDir * GravitationConstant / (squaredDistance * InverseMasses[currentBodyIndex] * InverseMasses[otherBodyIndex]);
                }

                float3 newVelocity = Velocities[currentBodyIndex] + forces * InverseMasses[currentBodyIndex] * DeltaTime;
                NewVelocities[currentBodyIndex] = newVelocity;
                NewPositions[currentBodyIndex] = Positions[currentBodyIndex] + newVelocity * DeltaTime;
            }
        }
        
        [BurstCompile]
        private struct OrbitPositionCalculateTwoJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<float3> Positions;
            
            [ReadOnly]
            public NativeList<int> GravityContributingBodyIndices;
            
            [ReadOnly]
            public NativeList<int> GravityReceivingBodyIndices;
            
            [ReadOnly]
            public NativeArray<float> InverseMasses;
            [ReadOnly]
            public NativeArray<float3> Velocities;
            
            [WriteOnly]
            public NativeArray<float3> NewPositions;
            [WriteOnly]
            public NativeArray<float3> NewVelocities;

            public float GravitationConstant;
            public float DeltaTime; // Should be equal or very close the delta time used by gravity system for correct prediction
            
            public void Execute(int currentBodyIndex)
            {
                if (!GravityReceivingBodyIndices.Contains(currentBodyIndex))
                {
                    NewVelocities[currentBodyIndex] = Velocities[currentBodyIndex];
                    NewPositions[currentBodyIndex] = Positions[currentBodyIndex];
                    return;
                }
                
                float3 forces = float3.zero;
                for(int i = 0; i < GravityContributingBodyIndices.Length; i++)
                {
                    int otherBodyIndex = GravityContributingBodyIndices[i];
                    if (currentBodyIndex == otherBodyIndex) continue; // Skip self

                    float squaredDistance = math.lengthsq(Positions[otherBodyIndex] - Positions[currentBodyIndex]);
                    float3 forceDir = math.normalize(Positions[otherBodyIndex] - Positions[currentBodyIndex]);
                    forces += forceDir * GravitationConstant / (squaredDistance * InverseMasses[currentBodyIndex] * InverseMasses[otherBodyIndex]);
                }

                float3 newVelocity = Velocities[currentBodyIndex] + forces * InverseMasses[currentBodyIndex] * DeltaTime;
                NewVelocities[currentBodyIndex] = newVelocity;
                NewPositions[currentBodyIndex] = Positions[currentBodyIndex] + newVelocity * DeltaTime;
            }
        }
    }
}

