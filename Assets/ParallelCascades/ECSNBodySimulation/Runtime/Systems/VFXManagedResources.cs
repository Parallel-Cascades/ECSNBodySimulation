using UnityEngine;
using UnityEngine.VFX;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    public class VFXManagedResources : MonoBehaviour
    {
        public VisualEffect ExplosionsGraph;

        private void Awake()
        {
            VFXReferences.ExplosionsGraph = ExplosionsGraph;
        }
    }
}