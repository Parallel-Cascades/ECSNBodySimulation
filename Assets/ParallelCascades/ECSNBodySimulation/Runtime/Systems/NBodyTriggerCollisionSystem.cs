using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// When two NBody entities collide, the one with the smaller mass gets destroyed.
    /// All NBody entities must have a RigidBody component with IsTrigger = true to register trigger events.
    /// This system ensures collisions are handled, preventing bodies in an NBody simulation from flying away at high speeds when very close together.
    /// </summary>
    /// <remarks>
    /// For bodies with child entities (e.g., mesh, VFX, or other entities that should be destroyed with the parent),
    /// add the LinkedEntityGroupAuthoring component to the parent entity.
    /// </remarks>
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct NBodyTriggerCollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<NBodyEntity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            VFXExplosionsSingleton vfxExplosionsSingleton = SystemAPI.GetSingletonRW <VFXExplosionsSingleton>().ValueRW;
            
            var triggerJob = new NBodyTriggerCollideJob()
            {
                NBodyEntityLookup = SystemAPI.GetComponentLookup<NBodyEntity>(true),
                PhysicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true),
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                ExplosionsManager = vfxExplosionsSingleton.Manager,
                PhysicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
                ECB = new EntityCommandBuffer(Allocator.TempJob)
            };
            
            state.Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();

            triggerJob.ECB.Playback(state.EntityManager);
            triggerJob.ECB.Dispose();
        }

        [BurstCompile]
        struct NBodyTriggerCollideJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<NBodyEntity> NBodyEntityLookup;
            [ReadOnly] public ComponentLookup<PhysicsMass> PhysicsMassLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;

            public EntityCommandBuffer ECB;
            
            public VFXManager<VFXExplosionRequest> ExplosionsManager;
            
            private bool ValidCollisionEntity(Entity e)
            {
                return NBodyEntityLookup.HasComponent(e) && PhysicsMassLookup.HasComponent(e) && PhysicsVelocityLookup.HasComponent(e);
            }
            
            private void ApplyImpactForce(
                Entity impactingEntity,
                Entity impactedEntity)
            {
                PhysicsVelocity impactingVelocity = PhysicsVelocityLookup[impactingEntity];
                PhysicsMass impactingMass = PhysicsMassLookup[impactingEntity];
                PhysicsVelocity impactedVelocity = PhysicsVelocityLookup[impactedEntity];
                PhysicsMass impactedMass = PhysicsMassLookup[impactedEntity];

                float3 momentum =  impactingVelocity.Linear / impactingMass.InverseMass;
                impactedVelocity.ApplyLinearImpulse(impactedMass, momentum);
                PhysicsVelocityLookup[impactedEntity] = impactedVelocity;
            }

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                if (ValidCollisionEntity(entityA) && ValidCollisionEntity(entityB))
                {
                    var entityAMass = PhysicsMassLookup[entityA];
                    var entityBMass = PhysicsMassLookup[entityB];

                    // entity with smaller mass gets destroyed
                    if (entityAMass.InverseMass > entityBMass.InverseMass)
                    {
                        ECB.DestroyEntity(entityA);
                        
                        // Apply impact of entityA to entityB
                        ApplyImpactForce(entityA, entityB);

                        ExplosionsManager.AddRequest(new VFXExplosionRequest
                        {
                            Position = LocalTransformLookup[entityA].Position,
                            Scale = LocalTransformLookup[entityA].Scale,
                            Color = new float3(1,1,0)
                        });
                    }
                    else
                    {
                        ECB.DestroyEntity(entityB);
                        
                        // Apply impact of entityB to entityA
                        ApplyImpactForce(entityB, entityA);
                        
                        ExplosionsManager.AddRequest(new VFXExplosionRequest
                        {
                            Position = LocalTransformLookup[entityB].Position,
                            Scale = LocalTransformLookup[entityB].Scale,
                            Color = new float3(1,1,0)
                        });
                    }
                }
            }
        }
    }
}