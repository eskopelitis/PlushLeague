using UnityEngine;

namespace PlushLeague.Gameplay.Player
{
    /// <summary>
    /// Handles player animation states and transitions
    /// Works with PlayerController to animate movement, sprinting, and actions
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float speedThreshold = 0.1f;
        [SerializeField] private float walkToRunThreshold = 2.5f;
        [SerializeField] private float animationSmoothTime = 0.1f;
        
        // Animation parameter names
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsWalkingParam = Animator.StringToHash("IsWalking");
        private static readonly int IsRunningParam = Animator.StringToHash("IsRunning");
        private static readonly int IsSprintingParam = Animator.StringToHash("IsSprinting");
        private static readonly int IsStunnedParam = Animator.StringToHash("IsStunned");
        private static readonly int TackleParam = Animator.StringToHash("Tackle");
        private static readonly int SlideParam = Animator.StringToHash("Slide");
        private static readonly int StumbleParam = Animator.StringToHash("Stumble");
        
        private Animator animator;
        private PlayerController playerController;
        private PlayerMovement playerMovement;
        
        // Animation state tracking
        private float currentAnimationSpeed;
        private bool isWalking;
        private bool isRunning;
        private bool isSprinting;
        private bool isStunned;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        private void Start()
        {
            // Subscribe to player controller events
            if (playerController != null)
            {
                playerController.OnSprintStateChanged += OnSprintStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerController != null)
            {
                playerController.OnSprintStateChanged -= OnSprintStateChanged;
            }
        }
        
        private void Update()
        {
            UpdateMovementAnimations();
        }
        
        /// <summary>
        /// Update movement-related animations based on current speed
        /// </summary>
        private void UpdateMovementAnimations()
        {
            if (playerMovement == null) return;
            
            float currentSpeed = playerMovement.CurrentSpeed;
            
            // Smooth the animation speed for blend trees
            currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, currentSpeed, 
                Time.deltaTime / animationSmoothTime);
            
            // Determine movement states
            bool newIsWalking = currentSpeed > speedThreshold && currentSpeed <= walkToRunThreshold;
            bool newIsRunning = currentSpeed > walkToRunThreshold && !isSprinting;
            
            // Update states if they changed
            if (newIsWalking != isWalking)
            {
                isWalking = newIsWalking;
                animator.SetBool(IsWalkingParam, isWalking);
            }
            
            if (newIsRunning != isRunning)
            {
                isRunning = newIsRunning;
                animator.SetBool(IsRunningParam, isRunning);
            }
            
            // Set speed parameter for blend trees
            animator.SetFloat(SpeedParam, currentAnimationSpeed);
        }
        
        /// <summary>
        /// Called when sprint state changes from PlayerController
        /// </summary>
        /// <param name="sprinting">Whether player is now sprinting</param>
        private void OnSprintStateChanged(bool sprinting)
        {
            isSprinting = sprinting;
            animator.SetBool(IsSprintingParam, isSprinting);
        }
        
        /// <summary>
        /// Set stunned animation state
        /// </summary>
        /// <param name="stunned">Whether player is stunned</param>
        public void SetStunned(bool stunned)
        {
            isStunned = stunned;
            animator.SetBool(IsStunnedParam, isStunned);
        }
        
        /// <summary>
        /// Trigger tackle animation
        /// </summary>
        public void TriggerTackle()
        {
            animator.SetTrigger(TackleParam);
        }
        
        /// <summary>
        /// Trigger slide animation
        /// </summary>
        public void TriggerSlide()
        {
            animator.SetTrigger(SlideParam);
        }
        
        /// <summary>
        /// Trigger stumble animation (when tackled)
        /// </summary>
        public void TriggerStumble()
        {
            animator.SetTrigger(StumbleParam);
        }
        
        /// <summary>
        /// Get current animation state info for debugging
        /// </summary>
        public AnimatorStateInfo GetCurrentAnimationState()
        {
            return animator.GetCurrentAnimatorStateInfo(0);
        }
        
        /// <summary>
        /// Check if currently playing a specific animation
        /// </summary>
        /// <param name="stateName">Name of the animation state</param>
        /// <returns>True if currently playing that animation</returns>
        public bool IsPlayingAnimation(string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }
        
        /// <summary>
        /// Force an immediate animation state (for special cases)
        /// </summary>
        /// <param name="stateName">Name of the state to play</param>
        /// <param name="layer">Animator layer (default 0)</param>
        public void PlayAnimation(string stateName, int layer = 0)
        {
            animator.Play(stateName, layer);
        }
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            // Debug info (only show if PlayerController debug is enabled)
            if (playerController != null)
            {
                GUILayout.BeginArea(new Rect(220, 10, 200, 150));
                GUILayout.Label("=== Animation Debug ===");
                GUILayout.Label($"Anim Speed: {currentAnimationSpeed:F2}");
                GUILayout.Label($"Walking: {isWalking}");
                GUILayout.Label($"Running: {isRunning}");
                GUILayout.Label($"Sprinting: {isSprinting}");
                GUILayout.Label($"Stunned: {isStunned}");
                
                var stateInfo = GetCurrentAnimationState();
                GUILayout.Label($"Current State: {stateInfo.shortNameHash}");
                GUILayout.Label($"Normalized Time: {stateInfo.normalizedTime:F2}");
                GUILayout.EndArea();
            }
        }
        
        #endregion
    }
}
