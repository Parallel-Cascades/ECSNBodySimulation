using Unity.Mathematics;
using Unity.Physics;

namespace ParallelCascades.ECSNBodySimulation.Runtime.Systems
{
    public static class OrbitMechanicsUtility
    {
        public static void CalculateCircularOrbitVelocityUpVector(float3 position, float3 primaryBodyPosition, float primaryBodyInverseMass, float3 orbitUp, float gravitationalConstant, ref PhysicsVelocity velocity)
        {
            float3 radialVector = position - primaryBodyPosition;

            if (math.cross(orbitUp,radialVector).Equals(0))
            {
                orbitUp = math.right();
            }

            float orbitalRadius = math.length(radialVector);

            var primaryBodyMassInverseMass = primaryBodyInverseMass;
            float centripetalForce = math.sqrt( gravitationalConstant  / (orbitalRadius*primaryBodyMassInverseMass));
            float3 tangentialDirection = math.normalize(math.cross(radialVector, orbitUp));
            velocity.Linear = centripetalForce * tangentialDirection;
        }
    }
}