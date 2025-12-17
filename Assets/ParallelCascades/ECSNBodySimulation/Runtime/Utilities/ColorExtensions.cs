using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Utilities
{
    public static class ColorExtensions
    {
        public static float4 ToFloat4(this Color c)
        {
            return new float4(c.r,c.g,c.b,c.a);
        }
    }
}