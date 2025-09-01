using Unity.Entities;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    public struct NBodyEntity : IComponentData
    {
        
    }

    /// <summary>
    /// xample: Static Sun that affects others with gravity but remains stationary.
    /// </summary>
    public struct NBodyDoNotReceiveGravityTag : IComponentData
    {
        
    }
    
    /// <summary>
    /// Example: Small satellites or space stations that are affected by gravity but do not exert gravitational forces on other bodies.
    /// </summary>
    public struct NBodyDoNotContributeToGravityTag : IComponentData
    {
        
    }
}