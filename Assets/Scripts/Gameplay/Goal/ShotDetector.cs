using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Goal
{
    /// <summary>
    /// Detects shots on goal and triggers goalie save opportunities
    /// Monitors ball trajectory and speed to identify save-worthy shots
    /// </summary>
    public class ShotDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float minimumShotSpeed = 3.0f;
        [SerializeField] private float detectionRadius = 5.0f;
        [SerializeField] private float goalWidth = 3.0f;
        [SerializeField] private LayerMask ballLayerMask = 1;
        [SerializeField] private LayerMask playerLayerMask = 1;
        
        [Header("Goal Position")]
        [SerializeField] private Transform goalCenter;
        [SerializeField] private Vector2 goalDirection = Vector2.up; // Direction ball must travel to score
        
        [Header("Timing")]
        [SerializeField] private float standardSaveWindow = 0.5f;
        [SerializeField] private float maxSaveDistance = 8.0f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        
        // Components
        private PlushLeague.Gameplay.Ball.BallController ballController;
        private GoalieSaveManager saveManager;
        
        // State
        private bool shotInProgress = false;
        private float lastShotTime = 0f;
        private const float SHOT_COOLDOWN = 1.0f; // Prevent multiple triggers for same shot
        
        // Events
        public System.Action<Vector3, Vector3, float> OnShotDetected; // ball position, velocity, save window
        
        private void Start()
        {
            InitializeComponents();
        }
        
        private void Update()
        {
            if (ballController != null && !shotInProgress)
            {
                CheckForShot();
            }
        }
        
        private void InitializeComponents()
        {
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
            
            // Find save manager
            saveManager = Object.FindFirstObjectByType<GoalieSaveManager>();
            
            if (ballController == null)
            {
                UnityEngine.Debug.LogWarning("ShotDetector: No ball controller found");
            }
            
            if (saveManager == null)
            {
                UnityEngine.Debug.LogWarning("ShotDetector: No save manager found");
            }
        }
        
        /// <summary>
        /// Check if ball trajectory constitutes a shot on goal
        /// </summary>
        private void CheckForShot()
        {
            if (ballController == null || Time.time - lastShotTime < SHOT_COOLDOWN) return;
            
            Vector3 ballPosition = ballController.Position;
            Vector2 ballVelocity = ballController.Velocity;
            
            // Check if ball is moving fast enough
            if (ballVelocity.magnitude < minimumShotSpeed) return;
            
            // Check if ball is within detection range
            float distanceToGoal = Vector3.Distance(ballPosition, goalCenter.position);
            if (distanceToGoal > detectionRadius) return;
            
            // Check if ball is moving toward goal
            Vector2 ballToGoal = (goalCenter.position - ballPosition).normalized;
            float dot = Vector2.Dot(ballVelocity.normalized, ballToGoal);
            
            if (dot < 0.7f) return; // Must be reasonably aimed at goal
            
            // Check if trajectory intersects goal area
            if (!WillBallHitGoal(ballPosition, ballVelocity)) return;
            
            // Valid shot detected!
            TriggerShotEvent(ballPosition, ballVelocity);
        }
        
        /// <summary>
        /// Calculate if ball trajectory will intersect goal
        /// </summary>
        private bool WillBallHitGoal(Vector3 ballPos, Vector2 ballVel)
        {
            if (ballVel.magnitude < 0.1f) return false;
            
            // Project ball path to goal line
            Vector3 goalPos = goalCenter.position;
            
            // Simple trajectory intersection check
            // This assumes 2D movement - can be enhanced for 3D arcs
            float timeToGoal = Vector3.Distance(ballPos, goalPos) / ballVel.magnitude;
            Vector3 projectedPosition = ballPos + (Vector3)ballVel * timeToGoal;
            
            // Check if projected position is within goal bounds
            float distanceFromGoalCenter = Vector3.Distance(projectedPosition, goalPos);
            return distanceFromGoalCenter <= goalWidth * 0.5f;
        }
        
        /// <summary>
        /// Trigger shot event and initiate save opportunity
        /// </summary>
        private void TriggerShotEvent(Vector3 ballPos, Vector2 ballVel)
        {
            shotInProgress = true;
            lastShotTime = Time.time;
            
            // Calculate save window based on distance and speed
            float distanceToGoal = Vector3.Distance(ballPos, goalCenter.position);
            float timeToGoal = distanceToGoal / ballVel.magnitude;
            float saveWindow = Mathf.Min(standardSaveWindow, timeToGoal * 0.8f);
            
            UnityEngine.Debug.Log($"Shot detected! Distance: {distanceToGoal:F1}, Speed: {ballVel.magnitude:F1}, Save window: {saveWindow:F1}s");
            
            // Notify save manager
            if (saveManager != null)
            {
                saveManager.TriggerSaveOpportunity(ballPos, ballVel, saveWindow);
            }
            
            // Fire event
            OnShotDetected?.Invoke(ballPos, ballVel, saveWindow);
            
            // Start cooldown coroutine
            StartCoroutine(ShotCooldownCoroutine());
        }
        
        /// <summary>
        /// Cooldown after shot to prevent multiple triggers
        /// </summary>
        private IEnumerator ShotCooldownCoroutine()
        {
            yield return new WaitForSeconds(SHOT_COOLDOWN);
            shotInProgress = false;
        }
        
        /// <summary>
        /// Set goal position and direction manually
        /// </summary>
        public void SetGoal(Transform goal, Vector2 direction)
        {
            goalCenter = goal;
            goalDirection = direction.normalized;
        }
        
        /// <summary>
        /// Get the closest potential goalie to this goal
        /// </summary>
        public PlushLeague.Gameplay.Player.PlayerController FindClosestGoalie()
        {
            var players = Object.FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            PlushLeague.Gameplay.Player.PlayerController closestPlayer = null;
            float closestDistance = float.MaxValue;
            
            foreach (var player in players)
            {
                if (player == null) continue;
                
                float distance = Vector3.Distance(player.transform.position, goalCenter.position);
                if (distance < closestDistance && distance < maxSaveDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
            
            return closestPlayer;
        }
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || goalCenter == null) return;
            
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(goalCenter.position, detectionRadius);
            
            // Draw goal area
            Gizmos.color = Color.red;
            Vector3 goalLeft = goalCenter.position + Vector3.right * (goalWidth * 0.5f);
            Vector3 goalRight = goalCenter.position - Vector3.right * (goalWidth * 0.5f);
            Gizmos.DrawLine(goalLeft, goalRight);
            
            // Draw goal direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(goalCenter.position, goalDirection * 2f);
            
            // Draw max save distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalCenter.position, maxSaveDistance);
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying || !showDebugGizmos) return;
            
            GUILayout.BeginArea(new Rect(10, 300, 250, 120));
            GUILayout.Label("=== Shot Detector Debug ===");
            GUILayout.Label($"Shot in Progress: {shotInProgress}");
            GUILayout.Label($"Last Shot: {Time.time - lastShotTime:F1}s ago");
            
            if (ballController != null)
            {
                GUILayout.Label($"Ball Speed: {ballController.Velocity.magnitude:F1}");
                float distToGoal = Vector3.Distance(ballController.Position, goalCenter.position);
                GUILayout.Label($"Distance to Goal: {distToGoal:F1}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
