using ParallelCascades.ECSNBodySimulation.Runtime.ComponentData;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Authoring
{
    public class SatelliteSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject m_SatellitePrefab;
        [SerializeField] private int m_SatelliteGenerationSeed = 1234;
        [SerializeField] private int m_SatelliteCount;

        [SerializeField] private float m_InnerRadius = 2f;
        [SerializeField] private float m_OuterRadius = 5f;
        [SerializeField, Range(0,90)] private float m_MaxPitchAngle = 0f;
        
        [SerializeField] private Vector2 m_ScaleRange = Vector2.one;
        
        [SerializeField] private Vector3 m_OrbitAxisRotation;
        
        [SerializeField] private OrbitDirection m_OrbitDirection = OrbitDirection.CounterClockwise;
        
        private class SatelliteSpawnerAuthoringBaker : Baker<SatelliteSpawnerAuthoring>
        {
            public override void Bake(SatelliteSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                if( authoring.m_SatellitePrefab == null)
                {
                    return;
                }
                
                float3 upVector = authoring.m_OrbitDirection == OrbitDirection.CounterClockwise ? new float3(0, -1, 0) : new float3(0, 1, 0);
                quaternion rotation = quaternion.EulerXYZ(math.radians(authoring.m_OrbitAxisRotation));
                float3 rotatedUpVector = math.mul(rotation, upVector);
                AddComponent(entity, new SatelliteSpawnerData
                {
                    Count = authoring.m_SatelliteCount,
                    PrimaryBodyPosition = authoring.transform.position,
                    SatellitePrefab = GetEntity(authoring.m_SatellitePrefab, TransformUsageFlags.Dynamic),
                    OrbitAxisRotation = rotation,
                    InnerRadius = authoring.m_InnerRadius,
                    OuterRadius = authoring.m_OuterRadius,
                    PitchMaxAngle = authoring.m_MaxPitchAngle,
                    ScaleRange = authoring.m_ScaleRange,
                    RandomSeedOffset = authoring.m_SatelliteGenerationSeed,
                    OrbitData = new OrbitData
                    {
                        PrimaryBody = entity,
                        OrbitUp = rotatedUpVector
                    }
                });
            }
        }

        private void OnDrawGizmos()
        {
            void DrawArrowHead(float3 position, float3 direction, float size)
            {
                float3 up = new float3(0, 1, 0);
                // if (math.abs(math.dot(direction, up)) > 0.99f) up = new float3(0, 0, 1);

                float3 right = math.cross(direction, up);

                // Lines are 45° from -d (forming a V pointing along d)
                float3 leftDir = math.normalizesafe(-direction + right);
                float3 rightDir = math.normalizesafe(-direction - right);

                Gizmos.DrawLine(position, position + leftDir * size);
                Gizmos.DrawLine(position, position + rightDir * size);
            }
            
            void DrawMinMaxCircle(float2 radiusMinMax, float3 upVector)
            {
                var center = transform.position;
                int segments = 64;
                float minRadius = radiusMinMax.x;
                float maxRadius = radiusMinMax.y;
            
                Vector3[] minCircle = new Vector3[segments + 1];
                Vector3[] maxCircle = new Vector3[segments + 1];
                
            
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float x = Mathf.Cos(angle);
                    float y = Mathf.Sin(angle);
                    minCircle[i] = center + new Vector3(x, 0, y) * minRadius;
                    maxCircle[i] = center + new Vector3(x, 0, y) * maxRadius;
                
                    // Rotate points to align with upVector
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, upVector);
                    minCircle[i] = rotation * (minCircle[i] - center) + center;
                    maxCircle[i] = rotation * (maxCircle[i] - center) + center;
                }

                if (m_OrbitDirection == OrbitDirection.Clockwise)
                {
                    for (int i = 0; i < 64; i += 8)
                    {
                        float3 direction = maxCircle[i+1] - maxCircle[i];
                        DrawArrowHead(maxCircle[i],direction, 4f);
                    }
                }
                else
                {
                    for (int i = 0; i < 64; i += 8)
                    {
                        float3 direction = maxCircle[i] - maxCircle[i+1];
                        DrawArrowHead(maxCircle[i],direction, 4f);
                    }
                }

                Gizmos.DrawLineStrip(minCircle, true);
                Gizmos.DrawLineStrip(maxCircle, true);
            }

            Gizmos.color = Color.yellow;
            Vector3 upVector = Quaternion.Euler(m_OrbitAxisRotation) * Vector3.up;
            Gizmos.DrawLine(transform.position, transform.position+upVector);
            Gizmos.DrawSphere(transform.position+upVector, 0.1f);
            
            DrawMinMaxCircle(new float2(m_InnerRadius, m_OuterRadius), upVector);
        }
    }
}