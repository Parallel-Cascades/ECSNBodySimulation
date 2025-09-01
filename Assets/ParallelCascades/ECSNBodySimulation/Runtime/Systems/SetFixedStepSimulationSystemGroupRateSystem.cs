using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// This system exists to ensure that we use the same fixed time step in NBodySimulation, DOTS Physics, and during
    /// the baking simulation to visualise orbits.
    /// The EntityQuery with change filter makes sure this only updates the timestep whenever the NBodySimulationSettingsSingleton
    /// is changed.
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SetFixedStepSimulationSystemGroupRateSystem : ISystem
    {
        private EntityQuery m_NBodySimulationSettingsQuery;
        
        public void OnCreate(ref SystemState state)
        {
            m_NBodySimulationSettingsQuery = SystemAPI.QueryBuilder().WithAll<NBodySimulationSettingsSingleton>().Build();
            m_NBodySimulationSettingsQuery.AddChangedVersionFilter(ComponentType.ReadOnly<NBodySimulationSettingsSingleton>());
            state.RequireForUpdate(m_NBodySimulationSettingsQuery);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // Require matching queries for update doesn't catch EntityQueries with change filters, so we need this guard.
            if(m_NBodySimulationSettingsQuery.IsEmpty)
                return;
            
            var singleton = m_NBodySimulationSettingsQuery.GetSingleton<NBodySimulationSettingsSingleton>();
            
            var rate = new RateUtils.FixedRateCatchUpManager(singleton.FixedDeltaTime);
            var fixedStepSimulationSystemGroup = state.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            fixedStepSimulationSystemGroup.RateManager = rate;
        }
        
    }
}