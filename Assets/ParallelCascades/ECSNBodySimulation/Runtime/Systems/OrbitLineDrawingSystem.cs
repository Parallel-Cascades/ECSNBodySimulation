using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData.Blobs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    /// <summary>
    /// This system runs in the editor and persists when play mode is entered. For debugging/gizmos visualisation.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [RequireMatchingQueriesForUpdate]
    public partial struct OrbitLineDrawingSystem : ISystem
    {
        private const int k_NumArrows = 10;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrbitDrawingSettingsSingleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            OrbitDrawingSettingsSingleton orbitDrawingSettings =
                SystemAPI.GetSingleton<OrbitDrawingSettingsSingleton>();

            foreach (var (colorData,elements) in SystemAPI.Query<RefRO<OrbitLineColor>, DynamicBuffer<FutureOrbitBufferElement>>())
            {
                int stepSize = (int)elements.Length / k_NumArrows;
                if(stepSize < 1) stepSize = 1;
                
                for (int i = 0; i < elements.Length-1; i++)
                {
                    var t = (float)i / orbitDrawingSettings.OrbitSamplesCount;

                    Color sampleColor = Evaluate(t, colorData.ValueRO.GradientBlob);
                    Debug.DrawLine(elements[i].Position, elements[i + 1].Position, sampleColor);
                    
                    if (i % stepSize == 0)
                    {
                        float3 dir = elements[i + 1].Position - elements[i].Position;
                        DrawArrowHead(elements[i].Position, dir, 4f, sampleColor);
                    }
                }
            }
        }

        // Draw two short lines at ±45° from the travel direction to form an arrowhead.
        static void DrawArrowHead(float3 position, float3 direction, float size, Color color)
        {
            float3 up = new float3(0, 1, 0);
            // if (math.abs(math.dot(direction, up)) > 0.99f) up = new float3(0, 0, 1);

            float3 right = math.cross(direction, up);

            // Lines are 45° from -d (forming a V pointing along d)
            float3 leftDir = math.normalizesafe(-direction + right);
            float3 rightDir = math.normalizesafe(-direction - right);

            Debug.DrawLine(position, position + leftDir * size, color);
            Debug.DrawLine(position, position + rightDir * size, color);
        }

        static Color Evaluate(float time, BlobAssetReference<GradientBlobData> gradientBlob)
        {
            // normalize t (when t exceeds the curve time, repeat it)
            time -= math.floor(time);
            
            // Find index and interpolation value in the array (we need to get two colors to interpolate between, and the interpolation value)
            float sampleT = time * gradientBlob.Value.KeyCount;
            var sampleTFloor = math.floor(sampleT);

            float interpolation = sampleT - sampleTFloor;
            var index = (int) sampleTFloor;
            
            return Color.Lerp(gradientBlob.Value.Colors[index], gradientBlob.Value.Colors[index + 1], interpolation);
        }
    }
}