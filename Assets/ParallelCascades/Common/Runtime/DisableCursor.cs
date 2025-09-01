using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    public class DisableCursor : MonoBehaviour
    {
        private void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}