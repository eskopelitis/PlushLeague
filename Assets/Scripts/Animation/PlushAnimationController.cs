using UnityEngine;
using System.Collections;

namespace PlushLeague.Animation
{
    /// <summary>
    /// Animation controller for plush characters with toy-like movement and behavior.
    /// Handles idle bounce, celebration, tackle flop, and other plush-themed animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlushAnimationController : MonoBehaviour
    {
        [Header("Animation References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform plushModel;
        
        [Header("Idle Animation")]
        [SerializeField] private bool enableIdleBounce = true;
        [SerializeField] private float bounceHeight = 0.1f;
        [SerializeField] private float bounceSpeed = 2f;
        [SerializeField] private float bounceRandomness = 0.3f;
        
        [Header("Movement Animation")]
        [SerializeField] private bool enableMovementBounce = true;
        [SerializeField] private float movementBounceIntensity = 0.15f;
        [SerializeField] private float movementBounceSpeed = 8f;
        
        [Header("Celebration Animation")]
        [SerializeField] private float celebrationDuration = 2f;
        [SerializeField] private float celebrationBounceHeight = 0.3f;
        [SerializeField] private float celebrationSpeed = 4f;
        
        [Header("Tackle Animation")]
        [SerializeField] private float tackleFlopDuration = 1f;
        [SerializeField] private float tackleRecoveryDuration = 0.5f;
        [SerializeField] private Vector3 tackleFlopRotation = new Vector3(0, 0, 90);
        
        [Header("Squash and Stretch")]
        [SerializeField] private bool enableSquashStretch = true;
        [SerializeField] private float squashStretchIntensity = 0.2f;
        [SerializeField] private float squashStretchSpeed = 5f;
        
        // Animation state
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool isAnimating = false;
        private bool isCelebrating = false;
        private bool isTackled = false;
        private float animationTimer = 0f;
        
        // Movement tracking
        private Vector3 lastPosition;
        private float currentSpeed = 0f;
        private bool isMoving = false;
        
        // Animation parameters (for Animator)
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
        private static readonly int CelebrateParam = Animator.StringToHash("Celebrate");
        private static readonly int TackleParam = Animator.StringToHash("Tackle");
        private static readonly int IdleParam = Animator.StringToHash("Idle");
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
                
            if (plushModel == null)
                plushModel = transform;
                
            // Store original transform values
            originalScale = plushModel.localScale;
            originalPosition = plushModel.localPosition;
            originalRotation = plushModel.localRotation;
            lastPosition = transform.position;
        }
        
        private void Update()
        {
            UpdateMovementTracking();
            UpdateAnimations();
            UpdateAnimatorParameters();
        }
        
        #endregion
        
        #region Animation Control
        
        /// <summary>
        /// Play a specific animation by name
        /// </summary>
        public void AnimatePlush(string animationName)
        {
            switch (animationName.ToLower())
            {
                case "idle":
                    StartIdleAnimation();
                    break;
                case "celebrate":
                case "celebration":
                    StartCelebrationAnimation();
                    break;
                case "tackle":
                case "tackled":
                    StartTackleAnimation();
                    break;
                case "bounce":
                    StartBounceAnimation();
                    break;
                case "squash":
                    StartSquashAnimation();
                    break;
                case "stretch":
                    StartStretchAnimation();
                    break;
                default:
                    UnityEngine.Debug.LogWarning($"Animation '{animationName}' not found");
                    break;
            }
        }
        
        /// <summary>
        /// Start idle bounce animation
        /// </summary>
        public void StartIdleAnimation()
        {
            if (!enableIdleBounce || isAnimating) return;
            
            StartCoroutine(IdleBounceCoroutine());
        }
        
        /// <summary>
        /// Start celebration animation
        /// </summary>
        public void StartCelebrationAnimation()
        {
            if (isCelebrating) return;
            
            StartCoroutine(CelebrationCoroutine());
        }
        
        /// <summary>
        /// Start tackle flop animation
        /// </summary>
        public void StartTackleAnimation()
        {
            if (isTackled) return;
            
            StartCoroutine(TackleCoroutine());
        }
        
        /// <summary>
        /// Start bounce animation
        /// </summary>
        public void StartBounceAnimation()
        {
            StartCoroutine(BounceCoroutine());
        }
        
        /// <summary>
        /// Start squash animation
        /// </summary>
        public void StartSquashAnimation()
        {
            StartCoroutine(SquashCoroutine());
        }
        
        /// <summary>
        /// Start stretch animation
        /// </summary>
        public void StartStretchAnimation()
        {
            StartCoroutine(StretchCoroutine());
        }
        
        /// <summary>
        /// Stop all animations and return to normal
        /// </summary>
        public void StopAllAnimations()
        {
            StopAllCoroutines();
            
            // Reset to original state
            plushModel.localScale = originalScale;
            plushModel.localPosition = originalPosition;
            plushModel.localRotation = originalRotation;
            
            isAnimating = false;
            isCelebrating = false;
            isTackled = false;
            animationTimer = 0f;
        }
        
        #endregion
        
        #region Animation Coroutines
        
        /// <summary>
        /// Continuous idle bounce animation
        /// </summary>
        private IEnumerator IdleBounceCoroutine()
        {
            while (enableIdleBounce && !isMoving && !isCelebrating && !isTackled)
            {
                float randomOffset = Random.Range(-bounceRandomness, bounceRandomness);
                float bounce = Mathf.Sin(Time.time * bounceSpeed + randomOffset) * bounceHeight;
                
                Vector3 bouncePosition = originalPosition + Vector3.up * bounce;
                plushModel.localPosition = bouncePosition;
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Celebration animation with energetic bouncing
        /// </summary>
        private IEnumerator CelebrationCoroutine()
        {
            isCelebrating = true;
            isAnimating = true;
            
            // Set animator parameter
            if (animator != null)
            {
                animator.SetBool(CelebrateParam, true);
            }
            
            float celebrationTimer = 0f;
            
            while (celebrationTimer < celebrationDuration)
            {
                celebrationTimer += Time.deltaTime;
                
                // Energetic bouncing
                float bounce = Mathf.Sin(celebrationTimer * celebrationSpeed) * celebrationBounceHeight;
                Vector3 bouncePosition = originalPosition + Vector3.up * bounce;
                plushModel.localPosition = bouncePosition;
                
                // Slight rotation for excitement
                float rotationAngle = Mathf.Sin(celebrationTimer * celebrationSpeed * 2f) * 15f;
                plushModel.localRotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
                
                // Squash and stretch
                if (enableSquashStretch)
                {
                    float squash = 1f + Mathf.Sin(celebrationTimer * squashStretchSpeed * 1.5f) * squashStretchIntensity;
                    plushModel.localScale = new Vector3(originalScale.x * squash, originalScale.y / squash, originalScale.z * squash);
                }
                
                yield return null;
            }
            
            // Return to normal
            plushModel.localPosition = originalPosition;
            plushModel.localRotation = originalRotation;
            plushModel.localScale = originalScale;
            
            if (animator != null)
            {
                animator.SetBool(CelebrateParam, false);
            }
            
            isCelebrating = false;
            isAnimating = false;
        }
        
        /// <summary>
        /// Tackle flop animation
        /// </summary>
        private IEnumerator TackleCoroutine()
        {
            isTackled = true;
            isAnimating = true;
            
            // Set animator parameter
            if (animator != null)
            {
                animator.SetBool(TackleParam, true);
            }
            
            // Flop animation
            float flopTimer = 0f;
            Quaternion targetRotation = originalRotation * Quaternion.Euler(tackleFlopRotation);
            
            while (flopTimer < tackleFlopDuration)
            {
                flopTimer += Time.deltaTime;
                float progress = flopTimer / tackleFlopDuration;
                
                // Rotate to flop position
                plushModel.localRotation = Quaternion.Slerp(originalRotation, targetRotation, progress);
                
                // Squash effect
                if (enableSquashStretch)
                {
                    float squash = 1f - (progress * squashStretchIntensity);
                    plushModel.localScale = new Vector3(originalScale.x / squash, originalScale.y * squash, originalScale.z);
                }
                
                yield return null;
            }
            
            // Recovery animation
            float recoveryTimer = 0f;
            while (recoveryTimer < tackleRecoveryDuration)
            {
                recoveryTimer += Time.deltaTime;
                float progress = recoveryTimer / tackleRecoveryDuration;
                
                // Return to normal rotation
                plushModel.localRotation = Quaternion.Slerp(targetRotation, originalRotation, progress);
                
                // Return to normal scale
                if (enableSquashStretch)
                {
                    float squash = 1f - ((1f - progress) * squashStretchIntensity);
                    plushModel.localScale = Vector3.Lerp(
                        new Vector3(originalScale.x / squash, originalScale.y * squash, originalScale.z),
                        originalScale,
                        progress
                    );
                }
                
                yield return null;
            }
            
            // Return to normal
            plushModel.localRotation = originalRotation;
            plushModel.localScale = originalScale;
            
            if (animator != null)
            {
                animator.SetBool(TackleParam, false);
            }
            
            isTackled = false;
            isAnimating = false;
        }
        
        /// <summary>
        /// Simple bounce animation
        /// </summary>
        private IEnumerator BounceCoroutine()
        {
            isAnimating = true;
            
            float bounceTimer = 0f;
            float bounceDuration = 0.5f;
            
            while (bounceTimer < bounceDuration)
            {
                bounceTimer += Time.deltaTime;
                float progress = bounceTimer / bounceDuration;
                
                // Bounce up and down
                float bounce = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
                plushModel.localPosition = originalPosition + Vector3.up * bounce;
                
                yield return null;
            }
            
            plushModel.localPosition = originalPosition;
            isAnimating = false;
        }
        
        /// <summary>
        /// Squash animation
        /// </summary>
        private IEnumerator SquashCoroutine()
        {
            isAnimating = true;
            
            float squashTimer = 0f;
            float squashDuration = 0.3f;
            
            while (squashTimer < squashDuration)
            {
                squashTimer += Time.deltaTime;
                float progress = squashTimer / squashDuration;
                
                // Squash down
                float squash = 1f - (Mathf.Sin(progress * Mathf.PI) * squashStretchIntensity);
                plushModel.localScale = new Vector3(originalScale.x / squash, originalScale.y * squash, originalScale.z);
                
                yield return null;
            }
            
            plushModel.localScale = originalScale;
            isAnimating = false;
        }
        
        /// <summary>
        /// Stretch animation
        /// </summary>
        private IEnumerator StretchCoroutine()
        {
            isAnimating = true;
            
            float stretchTimer = 0f;
            float stretchDuration = 0.3f;
            
            while (stretchTimer < stretchDuration)
            {
                stretchTimer += Time.deltaTime;
                float progress = stretchTimer / stretchDuration;
                
                // Stretch up
                float stretch = 1f + (Mathf.Sin(progress * Mathf.PI) * squashStretchIntensity);
                plushModel.localScale = new Vector3(originalScale.x / stretch, originalScale.y * stretch, originalScale.z);
                
                yield return null;
            }
            
            plushModel.localScale = originalScale;
            isAnimating = false;
        }
        
        #endregion
        
        #region Movement Tracking
        
        /// <summary>
        /// Update movement tracking for animation
        /// </summary>
        private void UpdateMovementTracking()
        {
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - lastPosition;
            currentSpeed = movement.magnitude / Time.deltaTime;
            
            isMoving = currentSpeed > 0.1f;
            lastPosition = currentPosition;
        }
        
        /// <summary>
        /// Update all animations based on current state
        /// </summary>
        private void UpdateAnimations()
        {
            if (isMoving && enableMovementBounce && !isCelebrating && !isTackled)
            {
                // Movement bounce
                float bounce = Mathf.Sin(Time.time * movementBounceSpeed) * movementBounceIntensity;
                plushModel.localPosition = originalPosition + Vector3.up * bounce;
            }
            else if (!isAnimating && enableIdleBounce && !isCelebrating && !isTackled)
            {
                // Start idle bounce if not already running
                StartCoroutine(IdleBounceCoroutine());
            }
        }
        
        /// <summary>
        /// Update Animator parameters
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;
            
            animator.SetFloat(SpeedParam, currentSpeed);
            animator.SetBool(IsMovingParam, isMoving);
            animator.SetBool(IdleParam, !isMoving && !isCelebrating && !isTackled);
        }
        
        #endregion
        
        #region Public Utility Methods
        
        /// <summary>
        /// Check if currently animating
        /// </summary>
        public bool IsAnimating()
        {
            return isAnimating;
        }
        
        /// <summary>
        /// Check if currently celebrating
        /// </summary>
        public bool IsCelebrating()
        {
            return isCelebrating;
        }
        
        /// <summary>
        /// Check if currently tackled
        /// </summary>
        public bool IsTackled()
        {
            return isTackled;
        }
        
        /// <summary>
        /// Set animation speed multiplier
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
            }
        }
        
        /// <summary>
        /// Enable/disable specific animation features
        /// </summary>
        public void SetAnimationFeatures(bool idleBounce, bool movementBounce, bool squashStretch)
        {
            enableIdleBounce = idleBounce;
            enableMovementBounce = movementBounce;
            enableSquashStretch = squashStretch;
        }
        
        #endregion
    }
}
