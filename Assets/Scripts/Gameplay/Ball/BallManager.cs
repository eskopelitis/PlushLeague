using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Ball
{
    /// <summary>
    /// Manages ball lifecycle, spawning, resets, and game state interactions
    /// </summary>
    public class BallManager : MonoBehaviour
    {
        [Header("Ball Setup")]
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private Transform ballSpawnPoint;
        [SerializeField] private BallPhysicsConfig physicsConfig;
        
        [Header("Reset Settings")]
        [SerializeField] private float autoResetDelay = 3f;
        [SerializeField] private Vector2 fieldBounds = new Vector2(20f, 10f);
        [SerializeField] private bool enableOutOfBounds = true;
        
        [Header("Goal Settings")]
        [SerializeField] private Transform[] goalAreas;
        [SerializeField] private float goalDetectionRadius = 2f;
        
        private BallController currentBall;
        private Vector2 centerFieldPosition;
        private Coroutine resetCoroutine;
        
        // Ball carrier tracking for superpowers
        private PlushLeague.Gameplay.Player.PlayerController currentBallCarrier;
        private PlushLeague.Gameplay.Player.PlayerController lastBallCarrier;
        
        // Events
        public System.Action<BallController> OnBallSpawned;
        public System.Action OnBallReset;
        public System.Action<int> OnGoalScored; // team index
        
        public BallController CurrentBall => currentBall;
        public bool HasBall => currentBall != null;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeManager();
            SpawnBall();
        }
        
        private void Update()
        {
            if (currentBall != null)
            {
                CheckBallState();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeManager()
        {
            // Set center field position
            if (ballSpawnPoint != null)
            {
                centerFieldPosition = ballSpawnPoint.position;
            }
            else
            {
                centerFieldPosition = Vector2.zero;
            }
        }
        
        #endregion
        
        #region Ball Management
        
        /// <summary>
        /// Spawn a new ball at the center of the field
        /// </summary>
        public void SpawnBall()
        {
            if (ballPrefab == null)
            {
                UnityEngine.Debug.LogError("BallManager: Ball prefab not assigned!");
                return;
            }
            
            // Destroy existing ball
            if (currentBall != null)
            {
                DestroyBall();
            }
            
            // Instantiate new ball
            Vector3 spawnPosition = ballSpawnPoint != null ? ballSpawnPoint.position : centerFieldPosition;
            GameObject ballObject = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            currentBall = ballObject.GetComponent<BallController>();
            
            if (currentBall == null)
            {
                UnityEngine.Debug.LogError("BallManager: Ball prefab doesn't have BallController component!");
                DestroyImmediate(ballObject);
                return;
            }
            
            // Apply physics config if available
            if (physicsConfig != null)
            {
                ApplyPhysicsConfig();
            }
            
            // Subscribe to ball events
            SubscribeToBallEvents();
            
            // Notify listeners
            OnBallSpawned?.Invoke(currentBall);
            
            UnityEngine.Debug.Log("Ball spawned at center field");
        }
        
        /// <summary>
        /// Reset ball to center field
        /// </summary>
        public void ResetBall()
        {
            if (currentBall == null)
            {
                SpawnBall();
                return;
            }
            
            // Stop any pending reset
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
            
            // Reset ball position and state
            currentBall.SetPosition(centerFieldPosition, clearPossession: true);
            
            // Notify listeners
            OnBallReset?.Invoke();
            
            UnityEngine.Debug.Log("Ball reset to center field");
        }
        
        /// <summary>
        /// Destroy current ball
        /// </summary>
        public void DestroyBall()
        {
            if (currentBall != null)
            {
                UnsubscribeFromBallEvents();
                Destroy(currentBall.gameObject);
                currentBall = null;
            }
        }
        
        #endregion
        
        #region Ball State Monitoring
        
        private void CheckBallState()
        {
            CheckOutOfBounds();
            CheckGoalAreas();
        }
        
        private void CheckOutOfBounds()
        {
            if (!enableOutOfBounds || currentBall == null) return;
            
            Vector2 ballPosition = currentBall.Position;
            
            // Check if ball is outside field bounds
            if (Mathf.Abs(ballPosition.x) > fieldBounds.x * 0.5f || 
                Mathf.Abs(ballPosition.y) > fieldBounds.y * 0.5f)
            {
                // Start auto-reset timer if not already running
                if (resetCoroutine == null)
                {
                    resetCoroutine = StartCoroutine(AutoResetBall());
                }
            }
        }
        
        private void CheckGoalAreas()
        {
            if (goalAreas == null || goalAreas.Length == 0 || currentBall == null) return;
            
            Vector2 ballPosition = currentBall.Position;
            
            for (int i = 0; i < goalAreas.Length; i++)
            {
                if (goalAreas[i] == null) continue;
                
                float distance = Vector2.Distance(ballPosition, goalAreas[i].position);
                if (distance <= goalDetectionRadius)
                {
                    OnGoalScored?.Invoke(i);
                    
                    // Reset ball after goal
                    StartCoroutine(ResetAfterGoal());
                    break;
                }
            }
        }
        
        #endregion
        
        #region Configuration
        
        private void ApplyPhysicsConfig()
        {
            if (physicsConfig == null || currentBall == null) return;
            
            // This would typically involve setting up the ball's physics properties
            // based on the ScriptableObject configuration
            
            var rb = currentBall.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearDamping = physicsConfig.freeBallDrag;
            }
            
            // Apply other config settings...
            UnityEngine.Debug.Log("Physics config applied to ball");
        }
        
        #endregion
        
        #region Event Handling
        
        private void SubscribeToBallEvents()
        {
            if (currentBall != null)
            {
                currentBall.OnBallPossessed += OnBallPossessed;
                currentBall.OnBallReleased += OnBallReleased;
                currentBall.OnBallKicked += OnBallKicked;
            }
        }
        
        private void UnsubscribeFromBallEvents()
        {
            if (currentBall != null)
            {
                currentBall.OnBallPossessed -= OnBallPossessed;
                currentBall.OnBallReleased -= OnBallReleased;
                currentBall.OnBallKicked -= OnBallKicked;
            }
        }
        
        private void OnBallPossessed(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Cancel any pending resets when ball is possessed
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
            
            UnityEngine.Debug.Log($"Ball possessed by {player.name}");
        }
        
        private void OnBallReleased()
        {
            UnityEngine.Debug.Log("Ball released");
        }
        
        private void OnBallKicked(Vector2 kickForce)
        {
            UnityEngine.Debug.Log($"Ball kicked with force: {kickForce}");
        }
        
        #endregion
        
        #region Coroutines
        
        private IEnumerator AutoResetBall()
        {
            yield return new WaitForSeconds(autoResetDelay);
            
            if (currentBall != null)
            {
                ResetBall();
            }
            
            resetCoroutine = null;
        }
        
        private IEnumerator ResetAfterGoal()
        {
            // Wait a moment for goal celebration
            yield return new WaitForSeconds(2f);
            
            ResetBall();
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Force ball to a specific position
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="clearPossession">Whether to clear current possession</param>
        public void MoveBallTo(Vector2 position, bool clearPossession = true)
        {
            if (currentBall != null)
            {
                currentBall.SetPosition(position, clearPossession);
            }
        }
        
        /// <summary>
        /// Apply force to the ball
        /// </summary>
        /// <param name="force">Force vector</param>
        public void KickBall(Vector2 force)
        {
            if (currentBall != null)
            {
                currentBall.ApplyForce(force);
            }
        }
        
        /// <summary>
        /// Get distance from ball to a position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Distance to ball, or float.MaxValue if no ball</returns>
        public float GetDistanceToBall(Vector2 position)
        {
            if (currentBall == null) return float.MaxValue;
            return Vector2.Distance(position, currentBall.Position);
        }
        
        /// <summary>
        /// Check if ball is near a position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="radius">Check radius</param>
        /// <returns>True if ball is within radius</returns>
        public bool IsBallNear(Vector2 position, float radius)
        {
            return GetDistanceToBall(position) <= radius;
        }
        
        /// <summary>
        /// Set the current ball carrier
        /// </summary>
        /// <param name="player">Player who now has the ball</param>
        public void SetBallCarrier(PlushLeague.Gameplay.Player.PlayerController player)
        {
            lastBallCarrier = currentBallCarrier;
            currentBallCarrier = player;
        }
        
        /// <summary>
        /// Get the current ball carrier
        /// </summary>
        /// <returns>Current ball carrier or null</returns>
        public PlushLeague.Gameplay.Player.PlayerController GetBallCarrier()
        {
            return currentBallCarrier;
        }
        
        /// <summary>
        /// Get the last player who had the ball
        /// </summary>
        /// <returns>Last ball carrier or null</returns>
        public PlushLeague.Gameplay.Player.PlayerController GetLastBallCarrier()
        {
            return lastBallCarrier;
        }
        
        /// <summary>
        /// Release the ball from current carrier
        /// </summary>
        public void ReleaseBall()
        {
            if (currentBallCarrier != null)
            {
                lastBallCarrier = currentBallCarrier;
                currentBallCarrier = null;
            }
        }
        
        /// <summary>
        /// Get the ball controller
        /// </summary>
        /// <returns>Current ball controller or null</returns>
        public BallController GetBallController()
        {
            return currentBall;
        }
        
        /// <summary>
        /// Reset ball to a specific position (used by MatchManager for kickoffs)
        /// </summary>
        /// <param name="position">Position to reset ball to</param>
        public void ResetBallPosition(Vector3 position)
        {
            if (currentBall != null)
            {
                currentBall.SetPosition(position, clearPossession: true);
                
                // Stop any pending reset
                if (resetCoroutine != null)
                {
                    StopCoroutine(resetCoroutine);
                    resetCoroutine = null;
                }
                
                // Notify listeners
                OnBallReset?.Invoke();
                
                UnityEngine.Debug.Log($"Ball reset to position: {position}");
            }
            else
            {
                // If no ball exists, spawn one at the position
                centerFieldPosition = position;
                SpawnBall();
            }
        }
        
        /// <summary>
        /// Set ball frozen state (used by MatchManager for match pauses)
        /// </summary>
        /// <param name="frozen">Whether ball should be frozen</param>
        public void SetBallFrozen(bool frozen)
        {
            if (currentBall != null)
            {
                var rb = currentBall.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    if (frozen)
                    {
                        rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    }
                    else
                    {
                        rb.constraints = RigidbodyConstraints2D.None;
                    }
                }
            }
        }
        
        /// <summary>
        /// Get current ball position (used by MatchManager)
        /// </summary>
        /// <returns>Ball position or Vector3.zero if no ball</returns>
        public Vector3 GetBallPosition()
        {
            if (currentBall != null)
            {
                return currentBall.transform.position;
            }
            return Vector3.zero;
        }

        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            // Draw field bounds
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(fieldBounds.x, fieldBounds.y, 0));
            
            // Draw spawn point
            if (ballSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(ballSpawnPoint.position, 0.5f);
            }
            
            // Draw goal areas
            if (goalAreas != null)
            {
                Gizmos.color = Color.red;
                foreach (var goal in goalAreas)
                {
                    if (goal != null)
                    {
                        Gizmos.DrawWireSphere(goal.position, goalDetectionRadius);
                    }
                }
            }
        }
        
        #endregion
    }
}
