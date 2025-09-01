using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ParallelCascades.Common.Runtime
{
    public class FlyCamera : MonoBehaviour
    {
        public float MaxVAngle = 89;
        public float MinVAngle = -89;
    
        public float FlyRotationSpeed = 1.5f;
        public float FlyRotationSharpness = 999999;
        public float FlyMoveSharpness = 10;
        public float FlyMaxMoveSpeed = 35;
        public float FlySprintSpeedBoost = 5;

        private Vector3 m_CurrentMoveVelocity;
        private float m_PitchAngle;
        private Vector3 m_PlanarForward = Vector3.forward;

        private bool m_IgnoreInput;

        private Camera m_Camera;
        
        private struct CameraInputs
        {
            public Vector3 Move;
            public Vector2 Look;
            public bool Sprint;
        }
        
        void OnEnable()
        {
            m_Camera = Camera.main;
            if (m_Camera == null) return;
            
            // Initialize yaw purely from euler Y (simpler, avoids near-vertical branch)
            Vector3 planar = Quaternion.Euler(0f, m_Camera.transform.eulerAngles.y, 0f) * Vector3.forward;
            m_PlanarForward = planar.normalized;

            // Initialize pitch relative to the planar forward
            Vector3 forward = m_Camera.transform.forward;
            Vector3 right = Vector3.Cross(Vector3.up, m_PlanarForward).normalized;
            float pitch = Vector3.SignedAngle(m_PlanarForward, forward, right);
            m_PitchAngle = Mathf.Clamp(pitch, MinVAngle, MaxVAngle);
        }
        
        void Update()
        {
            CameraInputs cameraInputs = new CameraInputs();

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            
            if (keyboard == null || mouse == null)
            {
                return;
            }
            
            cameraInputs.Move = new Vector3(
                (keyboard.dKey.isPressed ? 1f : 0f) + (keyboard.aKey.isPressed ? -1f : 0f),
                (keyboard.eKey.isPressed ? 1f : 0f) + (keyboard.qKey.isPressed ? -1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) + (keyboard.sKey.isPressed ? -1f : 0f));
            cameraInputs.Look = new Vector2(
                mouse.delta.x.ReadValue() * 0.05f,
                mouse.delta.y.ReadValue() * 0.05f);
            cameraInputs.Sprint = keyboard.leftShiftKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            cameraInputs.Move = new Vector3(
                (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f),
                (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f),
                (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f));
            cameraInputs.Look = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y"));
            cameraInputs.Sprint = Input.GetKey(KeyCode.LeftShift);
#endif

            cameraInputs.Move = cameraInputs.Move *
                                Mathf.Clamp(Vector3.Magnitude(cameraInputs.Move),0,1); // Clamp move inputs magnitude

            if (m_IgnoreInput)
            { 
                return;
            }
        
            // Yaw
            float yawAngleChange = cameraInputs.Look.x * FlyRotationSpeed;
            Quaternion yawRotation = Quaternion.Euler(Vector3.up *  yawAngleChange);

            m_PlanarForward = yawRotation * m_PlanarForward;

            // Pitch
            m_PitchAngle += -cameraInputs.Look.y * FlyRotationSpeed;
            m_PitchAngle = Mathf.Clamp(m_PitchAngle, MinVAngle,
                MaxVAngle);
            Quaternion pitchRotation = Quaternion.Euler(Vector3.right *  m_PitchAngle);

            // Final rotation
            Quaternion targetRotation =
                Quaternion.LookRotation(m_PlanarForward, Vector3.up) * pitchRotation;
        
            float deltaTime = Time.deltaTime;
        
            var cameraTransform = m_Camera.transform;
        
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation,
                GetSharpnessInterpolant(FlyRotationSharpness, deltaTime));

            // Move
            Vector3 worldMoveInputs = cameraTransform.rotation * cameraInputs.Move;
            float finalMaxSpeed = FlyMaxMoveSpeed;
            if (cameraInputs.Sprint)
            {
                finalMaxSpeed *= FlySprintSpeedBoost;
            }

            m_CurrentMoveVelocity = Vector3.Lerp(m_CurrentMoveVelocity,
                worldMoveInputs * finalMaxSpeed,
                GetSharpnessInterpolant(FlyMoveSharpness, deltaTime));
            cameraTransform.position += m_CurrentMoveVelocity * deltaTime;
        }
        
        private static float GetSharpnessInterpolant(float sharpness, float dt)
        {
            return Mathf.Clamp(1f - Mathf.Exp(-sharpness * dt),0,1);
        }
    }
}