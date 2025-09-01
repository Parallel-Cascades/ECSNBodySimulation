using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    /// <summary>
    /// Use boolean flags to determine what components get added to the entity, changing how it affects and gets
    /// affected by the gravity system.
    /// </summary>
    public class NBodyAuthoring : MonoBehaviour
    {
        [SerializeField] private bool m_DoNotReceiveGravity;
        [SerializeField] private bool m_DoNotContributeToGravity;
        
        public class NBodyAuthoringBaker : Baker<NBodyAuthoring>
        {
            public override void Bake(NBodyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new NBodyEntity());

                if (authoring.m_DoNotReceiveGravity)
                {
                    AddComponent<NBodyDoNotReceiveGravityTag>(entity);
                }
                
                if (authoring.m_DoNotContributeToGravity)
                {
                    AddComponent<NBodyDoNotContributeToGravityTag>(entity);
                }
            }
        }
    }
}