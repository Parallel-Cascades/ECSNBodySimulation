using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class RotatingBodyAuthoring : MonoBehaviour
    {
        [SerializeField] private float3 m_rotationSpeedPerAxis;
        
        private class RotatingBodyAuthoringBaker : Baker<RotatingBodyAuthoring>
        {
            public override void Bake(RotatingBodyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new RotationOnAxisData
                {
                    Value = authoring.m_rotationSpeedPerAxis
                });
            }
        }
    }
}