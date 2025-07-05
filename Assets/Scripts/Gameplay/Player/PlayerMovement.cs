using UnityEngine;

namespace PlushLeague.Gameplay.Player
{
    /// <summary>
    /// Handles player movement input and physics
    /// Separated from PlayerController for better modularity
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Physics")]
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 15f;
        [SerializeField] private float velocityDamping = 0.9f;
        
        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 720f; // degrees per second
        [SerializeField] private bool faceMovementDirection = true;
        
        private Rigidbody2D rb;
        private Vector2 targetVelocity;
        private Vector2 currentVelocity;
        
        public Vector2 CurrentVelocity => currentVelocity;
        public float CurrentSpeed => currentVelocity.magnitude;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        private void FixedUpdate()
        {
            UpdateMovement();
            UpdateRotation();
        }
        
        /// <summary>
        /// Set the target velocity for movement
        /// </summary>
        /// <param name="velocity">Target velocity vector</param>
        public void SetTargetVelocity(Vector2 velocity)
        {
            targetVelocity = velocity;
        }
        
        /// <summary>
        /// Immediately stop all movement
        /// </summary>
        public void Stop()
        {
            targetVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
        
        /// <summary>
        /// Alias for Stop() - used by superpower effects
        /// </summary>
        public void StopMovement()
        {
            Stop();
        }
        
        /// <summary>
        /// Apply an impulse force (for tackles, ball hits, etc.)
        /// </summary>
        /// <param name="force">Force vector to apply</param>
        public void ApplyImpulse(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        
        private void UpdateMovement()
        {
            // Smoothly interpolate towards target velocity
            if (targetVelocity.magnitude > 0.1f)
            {
                // Accelerating
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                    acceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Decelerating
                currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                    deceleration * Time.fixedDeltaTime);
            }
            
            // Apply velocity damping for more responsive feel
            currentVelocity *= velocityDamping;
            
            // Set rigidbody velocity
            rb.linearVelocity = currentVelocity;
        }
        
        private void UpdateRotation()
        {
            if (!faceMovementDirection || currentVelocity.magnitude < 0.1f) return;
            
            // Calculate target rotation based on movement direction
            float targetAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
            
            // Smoothly rotate towards target
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 
                rotationSpeed * Time.fixedDeltaTime);
            
            transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.forward);
        }
    }
}
