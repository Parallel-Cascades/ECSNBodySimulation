using Unity.Collections;
using Unity.Mathematics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    public static class RandomUtilities
    {
        /// <summary>
        /// This function will generate positions at 0 if it fails to find a candidate - if the space is too small or
        /// the iteration count is too low, or the count of points is too high.
        /// </summary>
        /// <param name="rotation">Rotation quaternion for the orbit axis.</param>
         public static void GenerateOrbitPointsBestCandidate(
            float3 center,
            quaternion rotation,
            float innerRadius,
            float outerRadius,
            float maxPitchAngle,
            float2 scaleRange,
            ref NativeArray<float> scales,
            ref NativeArray<float3> positions,
            int seed = 0,
            int samplesPerIteration = 30)
        {
            var count = positions.Length;
            var random = Unity.Mathematics.Random.CreateFromIndex((uint)seed);

            var spheres = new NativeArray<float4>(count, Allocator.Temp); // (x, y, z, r)

            for (int i = 0; i < count; i++)
            {
                float maxSampleDistance = random.NextFloat(scaleRange.x, scaleRange.y);
                float bestDistance = 0f;
                float3 bestSample = float3.zero;

                for (int j = 0; j < samplesPerIteration; j++)
                {
                    float2 yawDir = random.NextFloat2Direction();
                    float3 dir2D = new float3(yawDir.x, 0f, yawDir.y);
                    
                    dir2D = math.mul(rotation, dir2D);

                    float pitchDeg = random.NextFloat(-maxPitchAngle, maxPitchAngle);
                    float3 upVector = math.mul(rotation, math.up());

                    float3 rightAxis =math.normalize(math.cross(upVector, dir2D));
                    quaternion qPitch = quaternion.AxisAngle(rightAxis, math.radians(pitchDeg));
                    float3 dir3d = math.mul(qPitch, dir2D);

                    float distance = random.NextFloat(innerRadius, outerRadius);

                    float3 samplePos = center + dir3d * distance;
                    
                    float minDistance =  maxSampleDistance;
                    
                    for(int k = 0; k < i; k++)
                    {
                        float3 c = spheres[k].xyz;
                        float r = spheres[k].w;
                        float3 d = samplePos - c;
                        float dSq = math.dot(d, d);
                        
                        if (dSq < r * r)
                        {
                            minDistance = 0f; // Inside an existing sphere.
                            break;
                        }

                        float dLen = math.sqrt(dSq) - r;
                        if (dLen < minDistance) minDistance = dLen;
                    }
                    
                    if(minDistance > bestDistance)
                    {
                        bestDistance = minDistance;
                        bestSample = samplePos;
                    }
                }
                
                spheres[i] = new float4(bestSample.x, bestSample.y, bestSample.z, bestDistance);
                positions[i] = bestSample;
                scales[i] = bestDistance;
            }
            
            spheres.Dispose();
        }
    }
}