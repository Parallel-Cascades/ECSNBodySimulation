using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    
    public enum OrbitDirection
    {
        CounterClockwise,
        Clockwise
    }
    
    public class NBodyOrbitAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform m_PrimaryBody;
        
        [SerializeField] private OrbitDirection m_OrbitDirection = OrbitDirection.CounterClockwise;

        [SerializeField] private bool m_AutomaticOrbitAxisUpVector = true;
        
        [Tooltip("If Automatic Orbit Axis Up Vector is disabled, you can directly set the value here and orbit direction preference will be disregarded.")]
        [SerializeField] private float3 m_OrbitAxisUpVector;

        private void OnValidate()
        {
            if (m_AutomaticOrbitAxisUpVector && m_PrimaryBody)
            {
                float3 radialVector = transform.position - m_PrimaryBody.position;

                bool clockwiseOrbit = m_OrbitDirection == OrbitDirection.Clockwise;
                m_OrbitAxisUpVector = clockwiseOrbit ? math.down() : math.up();
                if (math.cross(m_OrbitAxisUpVector, radialVector).Equals(float3.zero)) // solve NaN case when lying on Z axis.
                {
                    m_OrbitAxisUpVector = clockwiseOrbit ? math.right() : math.left();
                }
            }
        }

        private class NBodyOrbitAuthoringBaker : Baker<NBodyOrbitAuthoring>
        {
            public override void Bake(NBodyOrbitAuthoring authoring)
            {
                // Update baking data if transform or primary body transform is changed/moved
                DependsOn(authoring.transform);
                if (authoring.m_PrimaryBody == null) return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new OrbitBakingData
                {
                    Value = new OrbitData
                    {
                        PrimaryBody = GetEntity(authoring.m_PrimaryBody, TransformUsageFlags.Dynamic),
                        OrbitUp =  authoring.m_OrbitAxisUpVector
                    }
                });
            }
        }
    }
}