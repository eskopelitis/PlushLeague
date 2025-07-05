using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Slide Tackle ability - lunges forward to steal ball or knock it away
    /// Primary defensive tool for contesting possession
    /// </summary>
    [CreateAssetMenu(fileName = "SlideTackle", menuName = "Plush League/Abilities/Slide Tackle")]
    public class SlideTackle : BaseAbility
    {
        [Header("Slide Tackle Settings")]
        [SerializeField] private float tackleDistance = 2.5f;
        [SerializeField] private float tackleSpeed = 20.0f;
        [SerializeField] private float tackleWidth = 1.0f;
        [SerializeField] private float tackleStunDuration = 0.75f;
        [SerializeField] private float missStunDuration = 0.5f;
        [SerializeField] private float slideDuration = 0.2f;
        
        [Header("Hit Detection")]
        [SerializeField] private LayerMask ballLayerMask = 1;
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private bool allowBodyChecks = false;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject slideTrailPrefab;
        [SerializeField] private ParticleSystem dustEffect;
        [SerializeField] private ParticleSystem stealEffect;
        [SerializeField] private Color slideTrailColor = Color.brown;
        
        [Header("Physics")]
        [SerializeField] private bool usePhysicsImpulse = true;
        [SerializeField] private float slideForceMultiplier = 1.5f;
        [SerializeField] private PhysicsMaterial2D slideMaterial;
        
        public override bool CanUse(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Can't tackle if already stunned or possessing ball (configurable)
            if (player.IsStunned())
            {
                return false;
            }
            
            // Optional: prevent tackling when you have the ball
            if (requiresBallPossession == false && player.HasBall())
            {
                return false; // Can't tackle when you have the ball
            }
            
            return base.CanUse(player);
        }
        
        protected override bool ExecuteAbility(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            // Get tackle direction
            Vector2 tackleDirection = CalculateTackleDirection(player, input);
            
            // Start slide tackle coroutine
            player.StartCoroutine(ExecuteSlideTackle(player, tackleDirection));
            
            return true;
        }
        
        /// <summary>
        /// Execute the slide tackle sequence
        /// </summary>
        private IEnumerator ExecuteSlideTackle(PlushLeague.Gameplay.Player.PlayerController player, Vector2 direction)
        {
            // Disable player input during slide
            player.SetInputEnabled(false);
            
            // Store original physics material
            var playerCollider = player.GetComponent<Collider2D>();
            var originalMaterial = playerCollider?.sharedMaterial;
            
            // Apply slide physics material for reduced friction
            if (playerCollider != null && slideMaterial != null)
            {
                playerCollider.sharedMaterial = slideMaterial;
            }
            
            // Get starting position
            Vector2 startPosition = player.transform.position;
            Vector2 targetPosition = startPosition + direction * tackleDistance;
            
            // Perform hit detection immediately
            TackleResult result = PerformTackleHitDetection(player, startPosition, direction);
            
            // Apply slide movement
            if (usePhysicsImpulse)
            {
                ApplySlideImpulse(player, direction);
            }
            else
            {
                yield return player.StartCoroutine(AnimateSlideMovement(player, startPosition, targetPosition));
            }
            
            // Create visual effects
            CreateSlideEffects(player, direction, result);
            
            // Wait for slide to complete
            yield return new WaitForSeconds(slideDuration);
            
            // Process tackle result
            yield return player.StartCoroutine(ProcessTackleResult(player, result));
            
            // Restore physics material
            if (playerCollider != null)
            {
                playerCollider.sharedMaterial = originalMaterial;
            }
            
            // Re-enable input after recovery
            player.SetInputEnabled(true);
        }
        
        /// <summary>
        /// Calculate tackle direction based on player state
        /// </summary>
        private Vector2 CalculateTackleDirection(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            // Use input direction if provided
            if (input.magnitude > 0.1f)
            {
                return input.normalized;
            }
            
            // Use player's facing direction
            return player.transform.up;
        }
        
        /// <summary>
        /// Perform hit detection for the tackle
        /// </summary>
        private TackleResult PerformTackleHitDetection(PlushLeague.Gameplay.Player.PlayerController tackler, Vector2 startPos, Vector2 direction)
        {
            var result = new TackleResult();
            
            // Create box for hit detection
            Vector2 boxSize = new Vector2(tackleWidth, tackleDistance);
            Vector2 boxCenter = startPos + direction * (tackleDistance * 0.5f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Check for ball hits
            Collider2D ballHit = Physics2D.OverlapBox(boxCenter, boxSize, angle, ballLayerMask);
            if (ballHit != null)
            {
                var ballController = ballHit.GetComponent<PlushLeague.Gameplay.Ball.BallController>();
                if (ballController != null)
                {
                    result.hitBall = true;
                    result.ballController = ballController;
                    
                    // Check if ball is possessed
                    if (ballController.IsPossessed)
                    {
                        result.hitPlayer = true;
                        result.playerController = ballController.PossessingPlayer;
                        result.stolenBall = true;
                    }
                }
            }
            
            // Check for player hits (if no ball hit or allowing body checks)
            if (!result.hitBall || allowBodyChecks)
            {
                Collider2D[] playerHits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, playerLayerMask);
                foreach (var hit in playerHits)
                {
                    var playerController = hit.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
                    if (playerController != null && playerController != tackler)
                    {
                        result.hitPlayer = true;
                        result.playerController = playerController;
                        
                        // Check if this player has the ball
                        if (playerController.HasBall() && !result.stolenBall)
                        {
                            result.stolenBall = true;
                            // Find the ball this player has
                            var ballManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
                            if (ballManager?.CurrentBall != null && ballManager.CurrentBall.PossessingPlayer == playerController)
                            {
                                result.ballController = ballManager.CurrentBall;
                                result.hitBall = true;
                            }
                        }
                        break; // Only hit first player
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Apply physics impulse for slide movement
        /// </summary>
        private void ApplySlideImpulse(PlushLeague.Gameplay.Player.PlayerController player, Vector2 direction)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float force = tackleSpeed * slideForceMultiplier;
                rb.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }
        
        /// <summary>
        /// Animate slide movement kinematically
        /// </summary>
        private IEnumerator AnimateSlideMovement(PlushLeague.Gameplay.Player.PlayerController player, Vector2 startPos, Vector2 targetPos)
        {
            float elapsed = 0f;
            var rb = player.GetComponent<Rigidbody2D>();
            
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / slideDuration;
                
                Vector2 currentPos = Vector2.Lerp(startPos, targetPos, progress);
                
                if (rb != null)
                {
                    rb.MovePosition(currentPos);
                }
                else
                {
                    player.transform.position = currentPos;
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Process the results of the tackle
        /// </summary>
        private IEnumerator ProcessTackleResult(PlushLeague.Gameplay.Player.PlayerController tackler, TackleResult result)
        {
            if (result.stolenBall && result.ballController != null)
            {
                // Successful steal!
                HandleSuccessfulSteal(tackler, result);
            }
            else if (result.hitBall && !result.ballController.IsPossessed)
            {
                // Hit free ball - gain possession
                result.ballController.AttachToPlayer(tackler);
                UnityEngine.Debug.Log($"{tackler.name} gained possession via tackle");
            }
            else if (result.hitPlayer && allowBodyChecks)
            {
                // Body check (if enabled)
                HandleBodyCheck(tackler, result.playerController);
                yield return new WaitForSeconds(missStunDuration);
            }
            else
            {
                // Missed tackle
                HandleMissedTackle(tackler);
                yield return new WaitForSeconds(missStunDuration);
            }
        }
        
        /// <summary>
        /// Handle successful ball steal
        /// </summary>
        private void HandleSuccessfulSteal(PlushLeague.Gameplay.Player.PlayerController tackler, TackleResult result)
        {
            // Detach ball from opponent
            result.ballController.Detach();
            
            // Stun the opponent
            result.playerController.Stun(tackleStunDuration);
            
            // Give ball to tackler
            result.ballController.AttachToPlayer(tackler);
            
            // Trigger visual effects
            if (stealEffect != null)
            {
                var effect = Object.Instantiate(stealEffect, result.playerController.transform.position, Quaternion.identity);
                effect.Play();
                Object.Destroy(effect.gameObject, 2f);
            }
            
            UnityEngine.Debug.Log($"{tackler.name} stole ball from {result.playerController.name} via tackle!");
        }
        
        /// <summary>
        /// Handle body check (non-ball carrier tackle)
        /// </summary>
        private void HandleBodyCheck(PlushLeague.Gameplay.Player.PlayerController tackler, PlushLeague.Gameplay.Player.PlayerController target)
        {
            // Stun both players briefly
            target.Stun(tackleStunDuration * 0.5f);
            tackler.Stun(missStunDuration);
            
            UnityEngine.Debug.Log($"{tackler.name} body-checked {target.name}");
        }
        
        /// <summary>
        /// Handle missed tackle
        /// </summary>
        private void HandleMissedTackle(PlushLeague.Gameplay.Player.PlayerController tackler)
        {
            // Self-stun for recovery
            tackler.Stun(missStunDuration);
            
            UnityEngine.Debug.Log($"{tackler.name} missed tackle and is recovering");
        }
        
        /// <summary>
        /// Create visual effects for slide tackle
        /// </summary>
        private void CreateSlideEffects(PlushLeague.Gameplay.Player.PlayerController player, Vector2 direction, TackleResult result)
        {
            // Create slide trail
            if (slideTrailPrefab != null)
            {
                GameObject trail = Object.Instantiate(slideTrailPrefab, player.transform.position, Quaternion.identity);
                Object.Destroy(trail, slideDuration + 1f);
            }
            
            // Create dust effect
            if (dustEffect != null)
            {
                var dust = Object.Instantiate(dustEffect, player.transform.position, Quaternion.identity);
                dust.Play();
                Object.Destroy(dust.gameObject, 1f);
            }
        }
        
        protected override void OnAbilityUsed(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            base.OnAbilityUsed(player, input);
            
            // Trigger slide animation
            var animController = player.GetComponent<PlushLeague.Gameplay.Player.PlayerAnimationController>();
            if (animController != null)
            {
                animController.TriggerSlide();
            }
            
            UnityEngine.Debug.Log($"Player {player.name} performed slide tackle");
        }
        
        /// <summary>
        /// Get tackle range preview (for AI or UI)
        /// </summary>
        public Vector2 GetTackleRange()
        {
            return new Vector2(tackleWidth, tackleDistance);
        }
        
        /// <summary>
        /// Check if tackle would hit target
        /// </summary>
        public bool WouldHitTarget(Vector2 tacklerPos, Vector2 direction, Vector2 targetPos)
        {
            Vector2 boxCenter = tacklerPos + direction * (tackleDistance * 0.5f);
            Vector2 boxSize = new Vector2(tackleWidth, tackleDistance);
            
            // Simple box contains point check
            Vector2 localPoint = targetPos - boxCenter;
            return Mathf.Abs(localPoint.x) <= boxSize.x * 0.5f && Mathf.Abs(localPoint.y) <= boxSize.y * 0.5f;
        }
        
        /// <summary>
        /// Data structure for tackle hit detection results
        /// </summary>
        private class TackleResult
        {
            public bool hitBall = false;
            public bool hitPlayer = false;
            public bool stolenBall = false;
            public PlushLeague.Gameplay.Ball.BallController ballController = null;
            public PlushLeague.Gameplay.Player.PlayerController playerController = null;
        }
    }
}
