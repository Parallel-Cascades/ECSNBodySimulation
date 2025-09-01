using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.Blobs
{
    public struct GradientBlobData
    {
        public BlobArray<Color> Colors;
        public int KeyCount;
    }
}