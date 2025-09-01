using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.Blobs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class OrbitLineAuthoring : MonoBehaviour
    {
        public Gradient colorGradient;
        
        private void Reset()
        {
            colorGradient = new Gradient()
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };
        }

        private class OrbitLineAuthoringBaker : Baker<OrbitLineAuthoring>
        {
            public override void Bake(OrbitLineAuthoring authoring)
            {
                // Re-bake whenever position is changed
                DependsOn(authoring.transform);
                
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddBuffer<FutureOrbitBufferElement>(entity);
                
                // Bake gradient to blob as it is a managed field otherwise
                var blobReference = CreateBlob(authoring.colorGradient, Allocator.Persistent);
                
                // Ownership of the BlobAsset is passed to the BlobAssetStore,
                // which will automatically manage the lifetime and deduplication of the BlobAsset.
                AddBlobAsset(ref blobReference, out _);
                
                AddComponent(entity, new OrbitLineColor
                {
                    GradientBlob = blobReference
                });
            }

            private BlobAssetReference<GradientBlobData> CreateBlob(Gradient gradient, Allocator blobAllocator, Allocator builderAllocator = Allocator.TempJob)
            {
                using (var blobBuilder = new BlobBuilder(builderAllocator))
                {
                    ref var root = ref blobBuilder.ConstructRoot<GradientBlobData>();
                    int keyCount = gradient.colorKeys.Length;
                    
                    root.KeyCount = keyCount; 
                    var colorsArray = blobBuilder.Allocate<Color>(ref root.Colors, keyCount + 1); // +1 for the last key which is a copy of the last color, since we always need to interpolate between two colors
                    for (int i = 0; i < keyCount; i++)
                    {
                        float time = gradient.colorKeys[i].time;
                        colorsArray[i] = gradient.Evaluate(time);
                    }
                    // Copy the last color to the end of the array to ensure we can interpolate between the last two colors
                    colorsArray[keyCount] = colorsArray[keyCount - 1];
                    
                    return blobBuilder.CreateBlobAssetReference<GradientBlobData>(blobAllocator);
                }
            }
        }
    }
}