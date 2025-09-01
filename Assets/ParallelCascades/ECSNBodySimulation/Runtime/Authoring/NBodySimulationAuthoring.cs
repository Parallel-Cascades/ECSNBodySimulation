using ParallelCascades.Common.Runtime;
using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class NBodySimulationAuthoring : MonoBehaviour
    {
        [SerializeField] private float3 m_SimulationBounds = new float3(1000f, 1000f, 1000f);
        
        [SerializeField] private Transform m_SimulationCenter;
        
        [SerializeField] private float OutOfBoundsAcceleration = 5f;
        
        [SerializeField] private float m_GravityConstant = 0.1f;
        
        [HelpBox("The gravity simulation is built on top of the physics system. " +
                  "\nThis will set the fixed physics timestep used by the DOTS physics system. " +
                  "\nIt's also used by OrbitLinePositionCalculateSystem to calculate the orbits of bodies during baking, where we don't have access to the runtime fixed time step data.")]
        [Tooltip("Physics timestep is 1 / PhysicsTicksPerSecond")]
        [SerializeField] private int m_PhysicsTicksPerSecond = 60;
        private class NBodySimulationAuthoringBaker : Baker<NBodySimulationAuthoring>
        {
            public override void Bake(NBodySimulationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NBodySimulationSettingsSingleton
                {
                    GravitationalConstant = authoring.m_GravityConstant,
                    FixedDeltaTime = 1f / authoring.m_PhysicsTicksPerSecond,
                    SimulationBounds = authoring.m_SimulationBounds,
                    SimulationCenterEntity = authoring.m_SimulationCenter ? GetEntity(authoring.m_SimulationCenter, TransformUsageFlags.Dynamic) : entity,
                    OutOfBoundsAcceleration = authoring.OutOfBoundsAcceleration,
                });
            }
        }

        private void OnDrawGizmos()
        {
            // Draw the simulation bounds
            Gizmos.color = Color.yellow;
            if (m_SimulationCenter != null)
            {
                Gizmos.DrawWireCube(m_SimulationCenter.position, m_SimulationBounds * 2f);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, m_SimulationBounds * 2f);
            }
        }
    }
}