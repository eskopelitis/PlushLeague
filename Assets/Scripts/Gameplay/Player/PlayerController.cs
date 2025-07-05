using UnityEngine;

namespace PlushLeague.Gameplay.Player
{
    /// <summary>
    /// Main controller for player movement, stamina, and basic actions
    /// Handles 2D movement with sprinting mechanics and stamina management
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float sprintMultiplier = 1.8f;
        [SerializeField] private Rect boundaryRect = new Rect(-10, -5, 20, 10);
        
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100.0f;
        [SerializeField] private float sprintStaminaCost = 35.0f;
        [SerializeField] private float staminaRegenRate = 20.0f;
        [SerializeField] private float staminaRegenDelay = 1.5f;
        
        [Header("Ball Interaction")]
        [SerializeField] private float kickForce = 10f;
        [SerializeField] private float possessionRange = 1.5f;
        [SerializeField] private LayerMask ballLayerMask = 1;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // Runtime values
        private float currentSpeed;
        private float currentStamina;
        private bool isSprinting = false;
        private bool isStunned = false;
        private float staminaRegenTimer = 0f;
        private bool canRegenStamina = true;
        
        // Components
        private Rigidbody2D rb;
        private Animator animator;
        
        // Ball interaction
        private PlushLeague.Gameplay.Ball.BallController nearbyBall;
        private bool hasBall = false;
        public bool canReclaimBall = true;
        private Coroutine reclaimDelayCoroutine;
        
        // Input
        private Vector2 movementInput;
        private bool sprintButtonHeld = false;
        private bool actionButtonPressed = false;
        private bool superpowerButtonPressed = false;
        private bool inputEnabled = true;
        
        // Events
        public System.Action<float, float> OnStaminaChanged; // current, max
        public System.Action<bool> OnSprintStateChanged; // is sprinting
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Initialize values
            currentStamina = maxStamina;
            currentSpeed = moveSpeed;
        }
        
        private void Start()
        {
            // Notify UI of initial stamina
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        private void Update()
        {
            HandleStaminaRegeneration();
            UpdateAnimations();
        }
        
        private void FixedUpdate()
        {
            if (!isStunned)
            {
                Move(movementInput);
            }
            else
            {
                // Stunned players can't move
                rb.linearVelocity = Vector2.zero;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set movement input from input system (typically from virtual joystick)
        /// </summary>
        /// <param name="input">Normalized movement vector</param>
        public void SetMovementInput(Vector2 input)
        {
            movementInput = input;
        }
        
        /// <summary>
        /// Set sprint button state from input system
        /// </summary>
        /// <param name="isHeld">Whether sprint button is currently held</param>
        public void SetSprintInput(bool isHeld)
        {
            sprintButtonHeld = isHeld;
            HandleSprinting(isHeld);
        }
        
        /// <summary>
        /// Set action button state from input system (for kicking, tackling, etc.)
        /// </summary>
        /// <param name="isPressed">Whether action button was pressed this frame</param>
        public void SetActionInput(bool isPressed)
        {
            if (isPressed && !actionButtonPressed)
            {
                actionButtonPressed = true;
                HandleActionInput();
            }
            else if (!isPressed)
            {
                actionButtonPressed = false;
            }
        }
        
        /// <summary>
        /// Set superpower button state from input system (Y button)
        /// </summary>
        /// <param name="isPressed">Whether superpower button was pressed this frame</param>
        public void SetSuperpowerInput(bool isPressed)
        {
            if (isPressed && !superpowerButtonPressed)
            {
                superpowerButtonPressed = true;
                HandleSuperpowerInput();
            }
            else if (!isPressed)
            {
                superpowerButtonPressed = false;
            }
        }
        
        /// <summary>
        /// Stun the player for a specified duration (e.g., from being tackled)
        /// </summary>
        /// <param name="duration">Stun duration in seconds</param>
        public void Stun(float duration)
        {
            if (isStunned) return; // Already stunned
            
            StartCoroutine(StunCoroutine(duration));
        }
        
        /// <summary>
        /// Get current stamina value
        /// </summary>
        /// <returns>Current stamina amount</returns>
        public float GetCurrentStamina()
        {
            return currentStamina;
        }
        
        /// <summary>
        /// Get maximum stamina value
        /// </summary>
        /// <returns>Maximum stamina amount</returns>
        public float GetMaxStamina()
        {
            return maxStamina;
        }
        
        /// <summary>
        /// Get stamina as percentage (0-1)
        /// </summary>
        /// <returns>Stamina percentage</returns>
        public float GetStaminaPercentage()
        {
            return maxStamina > 0 ? currentStamina / maxStamina : 0f;
        }
        
        /// <summary>
        /// Check if player can sprint (has stamina and not stunned)
        /// </summary>
        public bool CanSprint()
        {
            return currentStamina > 0 && !isStunned;
        }
        
        /// <summary>
        /// Get the current max speed
        /// </summary>
        public float GetMaxSpeed()
        {
            return moveSpeed;
        }
        
        /// <summary>
        /// Set the max speed (used by superpowers like Ultra Dash)
        /// </summary>
        /// <param name="newSpeed">New max speed value</param>
        public void SetMaxSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
            
            // Update current speed if not sprinting
            if (!isSprinting)
            {
                currentSpeed = moveSpeed;
            }
        }
        
        /// <summary>
        /// Set whether player can reclaim ball (used for chip kick delay)
        /// </summary>
        /// <param name="canReclaim">Whether player can reclaim ball</param>
        public void SetCanReclaimBall(bool canReclaim)
        {
            canReclaimBall = canReclaim;
        }
        
        /// <summary>
        /// Start ball reclaim delay
        /// </summary>
        /// <param name="delay">Delay duration in seconds</param>
        public void StartReclaimDelay(float delay)
        {
            if (reclaimDelayCoroutine != null)
            {
                StopCoroutine(reclaimDelayCoroutine);
            }
            
            reclaimDelayCoroutine = StartCoroutine(ReclaimDelayCoroutine(delay));
        }
        
        /// <summary>
        /// Coroutine to handle ball reclaim delay
        /// </summary>
        private System.Collections.IEnumerator ReclaimDelayCoroutine(float delay)
        {
            canReclaimBall = false;
            yield return new WaitForSeconds(delay);
            canReclaimBall = true;
            reclaimDelayCoroutine = null;
        }
        
        /// <summary>
        /// Enable or disable player input (used by MatchManager for match control)
        /// </summary>
        /// <param name="enabled">Whether input should be enabled</param>
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            
            if (!enabled)
            {
                // Clear current input when disabled
                movementInput = Vector2.zero;
                sprintButtonHeld = false;
                actionButtonPressed = false;
                superpowerButtonPressed = false;
            }
        }
        
        /// <summary>
        /// Check if player is currently stunned
        /// </summary>
        /// <returns>True if player is stunned</returns>
        public bool IsStunned()
        {
            return isStunned;
        }
        
        #endregion
        
        #region Movement
        
        /// <summary>
        /// Handle player movement based on input
        /// Called every physics tick (FixedUpdate)
        /// </summary>
        /// <param name="input">Movement input vector</param>
        private void Move(Vector2 input)
        {
            // Calculate desired velocity
            Vector2 desiredVelocity = input.normalized * currentSpeed;
            
            // Apply velocity
            rb.linearVelocity = desiredVelocity;
            
            // Clamp position to boundary
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, boundaryRect.xMin, boundaryRect.xMax);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, boundaryRect.yMin, boundaryRect.yMax);
            transform.position = clampedPosition;
            
            // Update facing direction
            if (input.magnitude > 0.1f)
            {
                // Face movement direction
                float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
        }
        
        #endregion
        
        #region Sprinting & Stamina
        
        /// <summary>
        /// Handle sprinting state and stamina consumption
        /// </summary>
        /// <param name="sprintButtonHeld">Whether sprint button is currently held</param>
        private void HandleSprinting(bool sprintButtonHeld)
        {
            bool wasSprinting = isSprinting;
            
            // Can we sprint?
            bool canSprint = sprintButtonHeld && currentStamina > 0 && !isStunned;
            
            if (canSprint)
            {
                // Start/continue sprinting
                isSprinting = true;
                currentSpeed = moveSpeed * sprintMultiplier;
                
                // Consume stamina
                currentStamina -= sprintStaminaCost * Time.deltaTime;
                currentStamina = Mathf.Max(0, currentStamina);
                
                // Stop stamina regeneration
                canRegenStamina = false;
                staminaRegenTimer = 0f;
                
                // Force stop sprinting if stamina depleted
                if (currentStamina <= 0)
                {
                    isSprinting = false;
                    currentSpeed = moveSpeed;
                    StartStaminaRegenDelay();
                }
            }
            else
            {
                // Not sprinting
                if (isSprinting)
                {
                    // Just stopped sprinting
                    isSprinting = false;
                    currentSpeed = moveSpeed;
                    StartStaminaRegenDelay();
                }
            }
            
            // Notify if sprint state changed
            if (wasSprinting != isSprinting)
            {
                OnSprintStateChanged?.Invoke(isSprinting);
            }
            
            // Always notify stamina change
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Start the stamina regeneration delay timer
        /// </summary>
        private void StartStaminaRegenDelay()
        {
            canRegenStamina = false;
            staminaRegenTimer = staminaRegenDelay;
        }
        
        /// <summary>
        /// Handle stamina regeneration logic
        /// </summary>
        private void HandleStaminaRegeneration()
        {
            if (!canRegenStamina)
            {
                // Count down regen delay
                staminaRegenTimer -= Time.deltaTime;
                if (staminaRegenTimer <= 0f)
                {
                    canRegenStamina = true;
                }
            }
            else if (!isSprinting && currentStamina < maxStamina)
            {
                // Regenerate stamina
                float oldStamina = currentStamina;
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                
                // Notify if stamina changed
                if (oldStamina != currentStamina)
                {
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                }
            }
        }
        
        #endregion
        
        #region Stun System
        
        /// <summary>
        /// Coroutine to handle stun duration
        /// </summary>
        private System.Collections.IEnumerator StunCoroutine(float duration)
        {
            isStunned = true;
            
            // Force stop sprinting when stunned
            if (isSprinting)
            {
                isSprinting = false;
                currentSpeed = moveSpeed;
                OnSprintStateChanged?.Invoke(false);
                StartStaminaRegenDelay();
            }
            
            // Wait for stun duration
            yield return new WaitForSeconds(duration);
            
            isStunned = false;
        }
        
        #endregion
        
        #region Animation
        
        /// <summary>
        /// Update animation parameters based on current state
        /// </summary>
        private void UpdateAnimations()
        {
            if (animator == null) return;
            
            // Set movement speed for blend tree
            float animSpeed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", animSpeed);
            
            // Set sprinting state
            animator.SetBool("IsSprinting", isSprinting);
            
            // Set stunned state
            animator.SetBool("IsStunned", isStunned);
        }
        
        #endregion
        
        #region Ball Interaction
        
        /// <summary>
        /// Handle action input (kicking, tackling, etc.)
        /// </summary>
        private void HandleActionInput()
        {
            if (isStunned) return;
            
            // Try to kick ball if we have it or it's nearby
            TryKickBall();
        }
        
        /// <summary>
        /// Handle superpower input (Y button)
        /// </summary>
        private void HandleSuperpowerInput()
        {
            if (isStunned) return;
            
            // Try to activate superpower using a generic component approach
            var powerupController = GetComponent("PowerupController");
            if (powerupController != null)
            {
                // Use reflection to call TryActivatePowerup method
                var method = powerupController.GetType().GetMethod("TryActivatePowerup");
                if (method != null)
                {
                    bool success = (bool)method.Invoke(powerupController, null);
                    if (!success)
                    {
                        // Could add feedback here (sound, UI indicator, etc.)
                        UnityEngine.Debug.Log($"Player {name} cannot activate superpower right now");
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Player {name} has no PowerupController component");
            }
        }
        
        /// <summary>
        /// Attempt to kick the ball
        /// </summary>
        private void TryKickBall()
        {
            var ball = FindNearbyBall();
            if (ball == null) return;
            
            // Calculate kick direction (based on player facing or movement direction)
            Vector2 kickDirection = GetKickDirection();
            Vector2 kickForceVector = kickDirection * kickForce;
            
            // Apply kick force to ball
            ball.ApplyForce(kickForceVector);
            
            UnityEngine.Debug.Log($"Player {name} kicked ball with force {kickForceVector}");
        }
        
        /// <summary>
        /// Find nearby ball for interaction
        /// </summary>
        /// <returns>Nearby ball controller or null</returns>
        private PlushLeague.Gameplay.Ball.BallController FindNearbyBall()
        {
            // Find ball in range
            var ballCollider = Physics2D.OverlapCircle(transform.position, possessionRange, ballLayerMask);
            if (ballCollider != null)
            {
                return ballCollider.GetComponent<PlushLeague.Gameplay.Ball.BallController>();
            }
            return null;
        }
        
        /// <summary>
        /// Calculate kick direction based on player state
        /// </summary>
        /// <returns>Normalized kick direction</returns>
        private Vector2 GetKickDirection()
        {
            // Use movement direction if moving, otherwise use facing direction
            if (movementInput.magnitude > 0.1f)
            {
                return movementInput.normalized;
            }
            else
            {
                // Use player's facing direction (up vector in 2D)
                return transform.up;
            }
        }
        
        /// <summary>
        /// Check if player currently has possession of the ball
        /// </summary>
        /// <returns>True if player has the ball</returns>
        public bool HasBall()
        {
            var ball = FindNearbyBall();
            bool currentlyHasBall = ball != null && ball.IsPossessed && ball.PossessingPlayer == this;
            
            // Update cached value
            hasBall = currentlyHasBall;
            
            return currentlyHasBall;
        }
        
        /// <summary>
        /// Get nearby ball (for UI feedback)
        /// </summary>
        /// <returns>Nearby ball controller or null</returns>
        public PlushLeague.Gameplay.Ball.BallController GetNearbyBall()
        {
            return nearbyBall;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (!showDebugInfo) return;
            
            // Draw boundary rect
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boundaryRect.center, boundaryRect.size);
            
            // Draw stamina bar above player
            if (Application.isPlaying)
            {
                Vector3 barPosition = transform.position + Vector3.up * 2f;
                float barWidth = 2f;
                float barHeight = 0.2f;
                
                // Background
                Gizmos.color = Color.red;
                Gizmos.DrawCube(barPosition, new Vector3(barWidth, barHeight, 0));
                
                // Foreground
                Gizmos.color = Color.green;
                float staminaPercent = currentStamina / maxStamina;
                Gizmos.DrawCube(barPosition - Vector3.right * barWidth * (1f - staminaPercent) * 0.5f, 
                    new Vector3(barWidth * staminaPercent, barHeight, 0));
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Stamina: {currentStamina:F1}/{maxStamina}");
            GUILayout.Label($"Speed: {currentSpeed:F1}");
            GUILayout.Label($"Sprinting: {isSprinting}");
            GUILayout.Label($"Stunned: {isStunned}");
            GUILayout.Label($"Can Regen: {canRegenStamina}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
