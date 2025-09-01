using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.Blobs;
using Unity.Entities;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.ComponentData
{
    // Internal Buffer capacity set to 0 since this will always be very large - 1000s of elements
    // This way the buffer gets stored outside the entity chunk (we only use this for an editor system anyway)
    // and we avoid bloating the chunk size for entities that have this component
    // NOTE: A DynamicBuffer will get stored outside the chunk anyway if it exceeds 16KB in size, which this will do with 
    // 1333+ elements either way. So setting to 0 just avoids having to think about it.
    [InternalBufferCapacity(0)]
    public struct FutureOrbitBufferElement : IBufferElementData
    {
        public float3 Position;
    }

    public struct OrbitLineColor : IComponentData
    {
        public BlobAssetReference<GradientBlobData> GradientBlob;
    }
}