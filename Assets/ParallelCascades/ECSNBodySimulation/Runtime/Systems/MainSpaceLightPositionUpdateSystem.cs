using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// This system executes in the Editor and at runtime to update the global shader variable for a custom lighting solution,
    /// so that a single star can illuminate a star system in all directions, which a point light cannot do.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct MainSpaceLightPositionUpdateSystem : ISystem
    {
        private static readonly int s_mainSpaceLightPosition = Shader.PropertyToID("_Main_Space_Light_Position");
        
        private EntityQuery m_MainSpaceLightSingletonQuery;

        public void OnCreate(ref SystemState state)
        {
            m_MainSpaceLightSingletonQuery =
                SystemAPI.QueryBuilder().WithAll<MainSpaceLightSingletonTag, LocalTransform>().Build();
            state.RequireForUpdate(m_MainSpaceLightSingletonQuery);
        }

        
        public void OnUpdate(ref SystemState state)
        {
            var singleton = m_MainSpaceLightSingletonQuery.GetSingleton<LocalTransform>();
            Vector3 positionToSet = singleton.Position;
            Shader.SetGlobalVector(s_mainSpaceLightPosition, positionToSet);
        }
    }
}