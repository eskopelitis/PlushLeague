using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Goal
{
    /// <summary>
    /// Manages goalie save system - coordinates shot detection, timing UI, and save outcomes
    /// Handles the complete goalie mini-game flow from shot detection to result processing
    /// </summary>
    public class GoalieSaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private float perfectSaveStaminaBonus = 50f;
        [SerializeField] private float blockReflectionSpeed = 0.7f;
        [SerializeField] private float blockUpwardAngle = 30f;
        [SerializeField] private LayerMask goalieLayerMask = 1;
        
        [Header("Animation")]
        [SerializeField] private float saveAnimationDuration = 1.0f;
        [SerializeField] private bool useSlowMotionEffect = true;
        [SerializeField] private float slowMotionScale = 0.3f;
        [SerializeField] private float slowMotionDuration = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem perfectSaveEffect;
        [SerializeField] private ParticleSystem blockSaveEffect;
        [SerializeField] private ParticleSystem failEffect;
        [SerializeField] private GameObject saveGlowEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip perfectSaveSound;
        [SerializeField] private AudioClip blockSaveSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioClip crowdCheerSound;
        [SerializeField] private AudioSource audioSource;
        
        // Components
        private PlushLeague.UI.HUD.GoalieTimingBar timingBar;
        private PlushLeague.Gameplay.Ball.BallController ballController;
        
        // Current save state
        private bool saveInProgress = false;
        private PlushLeague.Gameplay.Player.PlayerController currentGoalie;
        private Vector3 shotStartPosition;
        private Vector2 shotVelocity;
        private float saveWindow;
        
        // Events
        public System.Action<PlushLeague.UI.HUD.GoalieTimingBar.SaveResult, PlushLeague.Gameplay.Player.PlayerController> OnSaveCompleted;
        
        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            // Find timing bar UI
            timingBar = Object.FindFirstObjectByType<PlushLeague.UI.HUD.GoalieTimingBar>();
            if (timingBar == null)
            {
                UnityEngine.Debug.LogWarning("GoalieSaveManager: No timing bar UI found");
            }
            
            // Find ball controller
            var ballManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager != null)
            {
                ballController = ballManager.CurrentBall;
            }
            
            if (ballController == null)
            {
                ballController = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
            }
            
            if (ballController == null)
            {
                UnityEngine.Debug.LogWarning("GoalieSaveManager: No ball controller found");
            }
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }
        
        /// <summary>
        /// Trigger save opportunity for detected shot
        /// Called by ShotDetector when shot on goal is detected
        /// </summary>
        public void TriggerSaveOpportunity(Vector3 ballPosition, Vector2 ballVelocity, float window)
        {
            if (saveInProgress) return;
            
            shotStartPosition = ballPosition;
            shotVelocity = ballVelocity;
            saveWindow = window;
            
            // Find designated goalie
            currentGoalie = FindDesignatedGoalie();
            if (currentGoalie == null)
            {
                UnityEngine.Debug.LogWarning("GoalieSaveManager: No goalie found for save opportunity");
                return;
            }
            
            saveInProgress = true;
            
            UnityEngine.Debug.Log($"Save opportunity triggered for {currentGoalie.name} - Window: {window:F2}s");
            
            // Show timing bar
            if (timingBar != null)
            {
                timingBar.ShowTimingBar(window, OnTimingBarResult);
            }
            
            // Play goalie save animation prep
            TriggerGoaliePrepAnimation();
        }
        
        /// <summary>
        /// Find the designated goalie for this save
        /// </summary>
        private PlushLeague.Gameplay.Player.PlayerController FindDesignatedGoalie()
        {
            // For now, find closest player to goal
            // In future, this could check for assigned goalie roles
            var shotDetector = Object.FindFirstObjectByType<ShotDetector>();
            if (shotDetector != null)
            {
                return shotDetector.FindClosestGoalie();
            }
            
            // Fallback: find any nearby player
            var players = Object.FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            PlushLeague.Gameplay.Player.PlayerController closest = null;
            float closestDist = float.MaxValue;
            
            foreach (var player in players)
            {
                if (player == null) continue;
                
                float dist = Vector3.Distance(player.transform.position, shotStartPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = player;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Handle result from timing bar
        /// </summary>
        private void OnTimingBarResult(PlushLeague.UI.HUD.GoalieTimingBar.SaveResult result)
        {
            if (!saveInProgress) return;
            
            UnityEngine.Debug.Log($"Goalie save result: {result}");
            
            // Process save result
            ProcessSaveResult(result);
            
            // Complete save
            CompleteSave(result);
        }
        
        /// <summary>
        /// Process the save result and apply effects to ball and goalie
        /// </summary>
        private void ProcessSaveResult(PlushLeague.UI.HUD.GoalieTimingBar.SaveResult result)
        {
            if (ballController == null || currentGoalie == null) return;
            
            switch (result)
            {
                case PlushLeague.UI.HUD.GoalieTimingBar.SaveResult.Perfect:
                    HandlePerfectSave();
                    break;
                    
                case PlushLeague.UI.HUD.GoalieTimingBar.SaveResult.Good:
                    HandleGoodSave();
                    break;
                    
                case PlushLeague.UI.HUD.GoalieTimingBar.SaveResult.Fail:
                    HandleFailedSave();
                    break;
            }
        }
        
        /// <summary>
        /// Handle perfect save (green zone)
        /// </summary>
        private void HandlePerfectSave()
        {
            UnityEngine.Debug.Log("Perfect save executed!");
            
            // Stop ball and give to goalie
            ballController.SetPosition(currentGoalie.transform.position + Vector3.up * 0.5f, true);
            ballController.AttachToPlayer(currentGoalie);
            
            // Award stamina bonus
            // Note: This would require extending PlayerController with stamina modification
            // For now, we'll just log it
            UnityEngine.Debug.Log($"Goalie {currentGoalie.name} awarded {perfectSaveStaminaBonus} stamina");
            
            // Trigger animations and effects
            TriggerGoalieAnimation("PerfectSave");
            CreateVisualEffect(perfectSaveEffect, currentGoalie.transform.position);
            PlaySound(perfectSaveSound);
            PlaySound(crowdCheerSound);
            
            // Slow motion effect
            if (useSlowMotionEffect)
            {
                StartCoroutine(SlowMotionCoroutine());
            }
            
            // Glow effect on ball
            if (saveGlowEffect != null)
            {
                var glow = Instantiate(saveGlowEffect, ballController.transform.position, Quaternion.identity);
                glow.transform.SetParent(ballController.transform);
                Destroy(glow, 2f);
            }
        }
        
        /// <summary>
        /// Handle good save (yellow zone) - deflection
        /// </summary>
        private void HandleGoodSave()
        {
            UnityEngine.Debug.Log("Good save - ball deflected");
            
            // Calculate deflection angle
            Vector2 deflectionDirection = CalculateDeflectionDirection();
            float deflectionSpeed = shotVelocity.magnitude * blockReflectionSpeed;
            
            // Apply deflection to ball
            ballController.Detach();
            ballController.ApplyForce(deflectionDirection * deflectionSpeed);
            
            // Trigger animations and effects
            TriggerGoalieAnimation("BlockSave");
            CreateVisualEffect(blockSaveEffect, currentGoalie.transform.position);
            PlaySound(blockSaveSound);
        }
        
        /// <summary>
        /// Handle failed save (red zone or no input)
        /// </summary>
        private void HandleFailedSave()
        {
            UnityEngine.Debug.Log("Save failed - goal likely scored");
            
            // Ball continues on original path (goal scored)
            // No modification to ball trajectory
            
            // Trigger failure animation
            TriggerGoalieAnimation("FailSave");
            CreateVisualEffect(failEffect, currentGoalie.transform.position);
            PlaySound(failSound);
            
            // Stun goalie briefly from failed dive
            currentGoalie.Stun(0.5f);
        }
        
        /// <summary>
        /// Calculate ball deflection direction for blocked saves
        /// </summary>
        private Vector2 CalculateDeflectionDirection()
        {
            // Reflect velocity away from goal with upward angle
            Vector2 awayFromGoal = -shotVelocity.normalized;
            
            // Add upward component
            float upwardComponent = Mathf.Sin(blockUpwardAngle * Mathf.Deg2Rad);
            float forwardComponent = Mathf.Cos(blockUpwardAngle * Mathf.Deg2Rad);
            
            Vector2 deflection = new Vector2(
                awayFromGoal.x * forwardComponent,
                awayFromGoal.y * forwardComponent + upwardComponent
            );
            
            return deflection.normalized;
        }
        
        /// <summary>
        /// Trigger goalie animation
        /// </summary>
        private void TriggerGoalieAnimation(string animationType)
        {
            if (currentGoalie == null) return;
            
            var animController = currentGoalie.GetComponent<PlushLeague.Gameplay.Player.PlayerAnimationController>();
            if (animController != null)
            {
                // These would need to be added to PlayerAnimationController
                switch (animationType)
                {
                    case "PerfectSave":
                        // animController.TriggerPerfectSave();
                        UnityEngine.Debug.Log("Triggering perfect save animation");
                        break;
                    case "BlockSave":
                        // animController.TriggerBlockSave();
                        UnityEngine.Debug.Log("Triggering block save animation");
                        break;
                    case "FailSave":
                        // animController.TriggerFailSave();
                        UnityEngine.Debug.Log("Triggering fail save animation");
                        break;
                }
            }
            
            // Start animation duration coroutine
            StartCoroutine(WaitForAnimationComplete(animationType));
        }
        
        /// <summary>
        /// Wait for animation to complete and handle cleanup
        /// </summary>
        private System.Collections.IEnumerator WaitForAnimationComplete(string animationType)
        {
            yield return new WaitForSeconds(saveAnimationDuration);
            
            UnityEngine.Debug.Log($"Goalie {animationType} animation completed after {saveAnimationDuration} seconds");
            
            // Reset any animation states if needed
            // Could trigger animation end events here
        }
        
        /// <summary>
        /// Trigger goalie preparation animation when save opportunity starts
        /// </summary>
        private void TriggerGoaliePrepAnimation()
        {
            if (currentGoalie == null) return;
            
            var animController = currentGoalie.GetComponent<PlushLeague.Gameplay.Player.PlayerAnimationController>();
            if (animController != null)
            {
                // This would need to be added to PlayerAnimationController
                // animController.TriggerGoaliePrep();
                UnityEngine.Debug.Log("Triggering goalie prep animation");
            }
        }
        
        /// <summary>
        /// Create visual effect at position
        /// </summary>
        private void CreateVisualEffect(ParticleSystem effectPrefab, Vector3 position)
        {
            if (effectPrefab != null)
            {
                var effect = Instantiate(effectPrefab, position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, 3f);
            }
        }
        
        /// <summary>
        /// Play audio clip
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        /// <summary>
        /// Slow motion effect coroutine
        /// </summary>
        private IEnumerator SlowMotionCoroutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = slowMotionScale;
            
            yield return new WaitForSecondsRealtime(slowMotionDuration);
            
            Time.timeScale = originalTimeScale;
        }
        
        /// <summary>
        /// Complete save and cleanup
        /// </summary>
        private void CompleteSave(PlushLeague.UI.HUD.GoalieTimingBar.SaveResult result)
        {
            // Fire completion event
            OnSaveCompleted?.Invoke(result, currentGoalie);
            
            // Cleanup state
            saveInProgress = false;
            currentGoalie = null;
            
            UnityEngine.Debug.Log("Goalie save completed and cleaned up");
        }
        
        /// <summary>
        /// Force trigger save input (for external input systems)
        /// </summary>
        public void TriggerSaveInput()
        {
            if (timingBar != null && timingBar.IsActive)
            {
                timingBar.TriggerSaveInput();
            }
        }
        
        /// <summary>
        /// Check if save is currently in progress
        /// </summary>
        public bool IsSaveInProgress => saveInProgress;
        
        /// <summary>
        /// Get current goalie (if save in progress)
        /// </summary>
        public PlushLeague.Gameplay.Player.PlayerController CurrentGoalie => currentGoalie;
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(270, 300, 250, 150));
            GUILayout.Label("=== Goalie Save Manager ===");
            GUILayout.Label($"Save in Progress: {saveInProgress}");
            GUILayout.Label($"Current Goalie: {(currentGoalie != null ? currentGoalie.name : "None")}");
            GUILayout.Label($"Save Window: {saveWindow:F2}s");
            
            if (saveInProgress && timingBar != null)
            {
                GUILayout.Label($"Timing Progress: {timingBar.CurrentProgress:F2}");
            }
            
            if (ballController != null)
            {
                GUILayout.Label($"Ball Speed: {ballController.Velocity.magnitude:F1}");
            }
            
            if (GUILayout.Button("Test Perfect Save"))
            {
                HandlePerfectSave();
            }
            
            if (GUILayout.Button("Test Block Save"))
            {
                HandleGoodSave();
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
