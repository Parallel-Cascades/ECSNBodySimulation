using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class PlanetModelAuthoring : MonoBehaviour
    {
        [SerializeField]private Transform m_ModelTransform;
        private class PlanetModelAuthoringBaker : Baker<PlanetModelAuthoring>
        {
            public override void Bake(PlanetModelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ComponentData.PlanetModelData
                {
                    ModelEntity = GetEntity(authoring.m_ModelTransform, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}