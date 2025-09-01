using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    public class DisableRotation : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}