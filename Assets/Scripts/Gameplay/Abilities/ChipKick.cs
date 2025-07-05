using UnityEngine;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Chip Kick ability - launches ball in an arc over opponents' heads
    /// Can be used for high shots or passes over defenders
    /// </summary>
    [CreateAssetMenu(fileName = "ChipKick", menuName = "Plush League/Abilities/Chip Kick")]
    public class ChipKick : BaseAbility
    {
        [Header("Chip Kick Settings")]
        [SerializeField] private float chipForce = 15.0f;
        [SerializeField] private float chipUpwardForce = 10.0f;
        [SerializeField] private float reclaimDelay = 0.5f;
        [SerializeField] private float maxChipDistance = 8f;
        
        [Header("Arc Simulation (2D Mode)")]
        [SerializeField] private bool use2DArcSimulation = true;
        [SerializeField] private float arcHeight = 3f;
        [SerializeField] private float arcDuration = 1.5f;
        [SerializeField] private AnimationCurve arcCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject chipTrailPrefab;
        [SerializeField] private ParticleSystem chipEffect;
        [SerializeField] private Color chipTrailColor = Color.yellow;
        
        public override bool CanUse(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Chip kick specifically requires ball possession
            if (!player.HasBall())
            {
                return false;
            }
            
            return base.CanUse(player);
        }
        
        protected override bool ExecuteAbility(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            // Find the ball
            var ball = FindPlayerBall(player);
            if (ball == null)
            {
                UnityEngine.Debug.LogWarning("ChipKick: No ball found for player");
                return false;
            }
            
            // Calculate kick direction
            Vector2 kickDirection = CalculateKickDirection(player, input);
            
            // Execute the chip kick
            if (use2DArcSimulation)
            {
                ExecuteChipKick2D(player, ball, kickDirection);
            }
            else
            {
                ExecuteChipKick3D(player, ball, kickDirection);
            }
            
            // Start reclaim delay for the player
            StartReclaimDelay(player);
            
            return true;
        }
        
        /// <summary>
        /// Execute chip kick using 2D arc simulation
        /// </summary>
        private void ExecuteChipKick2D(PlushLeague.Gameplay.Player.PlayerController player, 
            PlushLeague.Gameplay.Ball.BallController ball, Vector2 direction)
        {
            // Detach ball from player
            ball.Detach();
            
            // Calculate target position
            Vector2 startPos = ball.Position;
            Vector2 targetPos = startPos + direction * chipForce;
            
            // Start arc coroutine
            player.StartCoroutine(SimulateChipArc(ball, startPos, targetPos, arcHeight, arcDuration));
            
            // Apply visual effects
            CreateChipEffects(ball, direction);
        }
        
        /// <summary>
        /// Execute chip kick using 3D physics
        /// </summary>
        private void ExecuteChipKick3D(PlushLeague.Gameplay.Player.PlayerController player,
            PlushLeague.Gameplay.Ball.BallController ball, Vector2 direction)
        {
            // Calculate 3D force vector
            Vector3 horizontalForce = new Vector3(direction.x, 0, direction.y) * chipForce;
            Vector3 upwardForce = Vector3.up * chipUpwardForce;
            Vector3 totalForce = horizontalForce + upwardForce;
            
            // Detach and apply force
            ball.Detach(totalForce);
            
            // Apply visual effects
            CreateChipEffects(ball, direction);
        }
        
        /// <summary>
        /// Simulate 2D arc movement for the ball
        /// </summary>
        private System.Collections.IEnumerator SimulateChipArc(PlushLeague.Gameplay.Ball.BallController ball, 
            Vector2 startPos, Vector2 targetPos, float height, float duration)
        {
            float elapsed = 0f;
            Vector2 lastPosition = startPos;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Calculate horizontal position
                Vector2 horizontalPos = Vector2.Lerp(startPos, targetPos, progress);
                
                // Calculate vertical offset using arc curve
                float heightOffset = arcCurve.Evaluate(progress) * height;
                
                // Apply position (this assumes ball has a visual height offset)
                Vector2 newPosition = horizontalPos;
                ball.transform.position = new Vector3(newPosition.x, newPosition.y + heightOffset, 0);
                
                // Calculate velocity for physics simulation
                Vector2 velocity = (newPosition - lastPosition) / Time.deltaTime;
                if (ball.GetComponent<Rigidbody2D>() != null)
                {
                    ball.GetComponent<Rigidbody2D>().linearVelocity = velocity;
                }
                
                lastPosition = newPosition;
                yield return null;
            }
            
            // Ensure ball lands at target position
            ball.transform.position = new Vector3(targetPos.x, targetPos.y, 0);
            
            // Resume normal physics
            if (ball.GetComponent<Rigidbody2D>() != null)
            {
                ball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Calculate kick direction based on player input and facing
        /// </summary>
        private Vector2 CalculateKickDirection(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            Vector2 direction;
            
            // Use input direction if provided and significant
            if (input.magnitude > 0.1f)
            {
                direction = input.normalized;
            }
            else
            {
                // Use player's facing direction (transform.up in 2D)
                direction = player.transform.up;
            }
            
            return direction;
        }
        
        /// <summary>
        /// Find ball that player currently possesses
        /// </summary>
        private PlushLeague.Gameplay.Ball.BallController FindPlayerBall(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // This could be improved with a direct reference system
            var ballManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager?.CurrentBall != null && 
                ballManager.CurrentBall.IsPossessed && 
                ballManager.CurrentBall.PossessingPlayer == player)
            {
                return ballManager.CurrentBall;
            }
            return null;
        }
        
        /// <summary>
        /// Start the reclaim delay period for the player
        /// </summary>
        private void StartReclaimDelay(PlushLeague.Gameplay.Player.PlayerController player)
        {
            player.StartCoroutine(ReclaimDelayCoroutine(player));
        }
        
        /// <summary>
        /// Coroutine to handle reclaim delay
        /// </summary>
        private System.Collections.IEnumerator ReclaimDelayCoroutine(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Set flag to prevent immediate ball reclaim
            // Note: This would require adding a SetCanReclaimBall method to PlayerController
            // player.SetCanReclaimBall(false);
            
            yield return new WaitForSeconds(reclaimDelay);
            
            // Re-enable ball reclaim
            // player.SetCanReclaimBall(true);
            
            UnityEngine.Debug.Log($"Player {player.name} can now reclaim ball after chip kick");
        }
        
        /// <summary>
        /// Create visual effects for chip kick
        /// </summary>
        private void CreateChipEffects(PlushLeague.Gameplay.Ball.BallController ball, Vector2 direction)
        {
            // Create trail effect
            if (chipTrailPrefab != null)
            {
                GameObject trail = Object.Instantiate(chipTrailPrefab, ball.transform.position, Quaternion.identity);
                trail.transform.SetParent(ball.transform);
                
                // Auto-destroy trail after duration
                Object.Destroy(trail, arcDuration + 1f);
            }
            
            // Play particle effect
            if (chipEffect != null)
            {
                var effect = Object.Instantiate(chipEffect, ball.transform.position, Quaternion.identity);
                effect.Play();
                Object.Destroy(effect.gameObject, 2f);
            }
            
            // Modify ball's visual effects component if available
            var ballEffects = ball.GetComponent<PlushLeague.Gameplay.Ball.BallVisualEffects>();
            if (ballEffects != null)
            {
                ballEffects.TriggerEffect(PlushLeague.Gameplay.Ball.BallVisualEffects.EffectType.Kick);
            }
        }
        
        protected override void OnAbilityUsed(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            base.OnAbilityUsed(player, input);
            
            // Trigger player animation
            var animController = player.GetComponent<PlushLeague.Gameplay.Player.PlayerAnimationController>();
            if (animController != null)
            {
                // This would require adding a kick animation trigger
                // animController.TriggerKick();
            }
            
            UnityEngine.Debug.Log($"Player {player.name} performed chip kick in direction {input}");
        }
        
        /// <summary>
        /// Get chip kick preview (for UI or AI)
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="direction">Kick direction</param>
        /// <returns>Estimated landing position</returns>
        public Vector2 GetChipLandingPosition(Vector2 startPos, Vector2 direction)
        {
            return startPos + direction.normalized * Mathf.Min(chipForce, maxChipDistance);
        }
        
        /// <summary>
        /// Calculate if chip kick can reach target position
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="targetPos">Target position</param>
        /// <returns>True if target is within chip range</returns>
        public bool CanReachTarget(Vector2 startPos, Vector2 targetPos)
        {
            float distance = Vector2.Distance(startPos, targetPos);
            return distance <= maxChipDistance;
        }
    }
}
