using UnityEngine;

namespace PlushLeague.Gameplay.Ball
{
    /// <summary>
    /// Controls ball physics, possession, and sticky dribble mechanics
    /// Handles ball attachment to players and physics when free
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class BallController : MonoBehaviour
    {
        [Header("Possession Settings")]
        [SerializeField] private Vector2 dribbleOffset = new Vector2(0, -0.5f);
        [SerializeField] private float followLerpSpeed = 25.0f;
        [SerializeField] private float possessionRadius = 1.0f;
        
        [Header("Physics Settings")]
        [SerializeField] private float freeBallDrag = 2.0f;
        [SerializeField] private float dribbleDrag = 0f;
        [SerializeField] private PhysicsMaterial2D ballPhysicsMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip ballPickupSound;
        [SerializeField] private AudioClip ballBounceSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // State
        private bool isPossessed = false;
        private PlushLeague.Gameplay.Player.PlayerController possessingPlayer = null;
        
        // Components
        private Rigidbody2D rb;
        private Collider2D ballCollider;
        
        // Runtime
        private Vector2 targetPosition;
        private bool wasKicked = false;
        private float kickCooldown = 0f;
        private const float KICK_COOLDOWN_TIME = 0.1f;
        
        // Events
        public System.Action<PlushLeague.Gameplay.Player.PlayerController> OnBallPossessed;
        public System.Action OnBallReleased;
        public System.Action<Vector2> OnBallKicked;
        
        // Properties
        public bool IsPossessed => isPossessed;
        public PlushLeague.Gameplay.Player.PlayerController PossessingPlayer => possessingPlayer;
        public Vector2 Position => transform.position;
        public Vector2 Velocity => rb.linearVelocity;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ballCollider = GetComponent<Collider2D>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            // Set initial physics material
            if (ballPhysicsMaterial != null)
                ballCollider.sharedMaterial = ballPhysicsMaterial;
        }
        
        private void Start()
        {
            // Initialize as free ball
            SetFreeBallPhysics();
        }
        
        private void Update()
        {
            HandleKickCooldown();
            
            if (isPossessed)
            {
                HandleStickyDribble();
            }
        }
        
        private void FixedUpdate()
        {
            // Additional physics handling if needed
        }
        
        #endregion
        
        #region Possession System
        
        /// <summary>
        /// Attach ball to a player for dribbling
        /// </summary>
        /// <param name="player">Player to attach ball to</param>
        public void AttachToPlayer(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (player == null || isPossessed) return;
            
            // Set possession state
            isPossessed = true;
            possessingPlayer = player;
            
            // Switch to kinematic physics
            SetDribblePhysics();
            
            // Play pickup sound
            PlaySound(ballPickupSound);
            
            // Notify listeners
            OnBallPossessed?.Invoke(player);
            
            UnityEngine.Debug.Log($"Ball attached to {player.name}");
        }
        
        /// <summary>
        /// Detach ball from current player
        /// </summary>
        /// <param name="applyForce">Force to apply when detaching (for kicks)</param>
        public void Detach(Vector2 applyForce = default)
        {
            if (!isPossessed) return;
            
            var previousPlayer = possessingPlayer;
            
            // Clear possession state
            isPossessed = false;
            possessingPlayer = null;
            
            // Switch back to dynamic physics
            SetFreeBallPhysics();
            
            // Apply force if specified
            if (applyForce != Vector2.zero)
            {
                rb.AddForce(applyForce, ForceMode2D.Impulse);
                wasKicked = true;
                kickCooldown = KICK_COOLDOWN_TIME;
                OnBallKicked?.Invoke(applyForce);
            }
            
            // Notify listeners
            OnBallReleased?.Invoke();
            
            UnityEngine.Debug.Log($"Ball detached from {previousPlayer?.name ?? "unknown"}");
        }
        
        /// <summary>
        /// Steal ball from current player and give to new player
        /// </summary>
        /// <param name="newPlayer">Player stealing the ball</param>
        public void StealBall(PlushLeague.Gameplay.Player.PlayerController newPlayer)
        {
            if (!isPossessed || newPlayer == possessingPlayer) return;
            
            var previousPlayer = possessingPlayer;
            
            // Detach from current player
            Detach();
            
            // Immediately attach to new player
            AttachToPlayer(newPlayer);
            
            UnityEngine.Debug.Log($"Ball stolen from {previousPlayer?.name ?? "unknown"} by {newPlayer.name}");
        }
        
        #endregion
        
        #region Sticky Dribble
        
        /// <summary>
        /// Handle ball following player during dribble
        /// </summary>
        private void HandleStickyDribble()
        {
            if (!isPossessed || possessingPlayer == null) return;
            
            // Calculate target position based on player position and facing direction
            Vector3 playerPosition = possessingPlayer.transform.position;
            Vector3 playerForward = possessingPlayer.transform.up; // Assuming player faces "up" direction
            
            // Calculate dribble position
            Vector3 offsetPosition = playerPosition + 
                (playerForward * dribbleOffset.y) + 
                (possessingPlayer.transform.right * dribbleOffset.x);
            
            targetPosition = offsetPosition;
            
            // Smoothly move ball to target position
            Vector2 currentPosition = transform.position;
            Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, 
                followLerpSpeed * Time.deltaTime);
            
            // Apply position (kinematic rigidbody)
            rb.MovePosition(newPosition);
        }
        
        #endregion
        
        #region Physics Management
        
        /// <summary>
        /// Set physics for when ball is free (dynamic)
        /// </summary>
        private void SetFreeBallPhysics()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            
            // Use different drag if player is nearby (dribbling context)
            bool isPlayerNearby = CheckNearbyPlayers();
            float dragValue = isPlayerNearby ? dribbleDrag : freeBallDrag;
            
            rb.linearDamping = dragValue;
            rb.angularDamping = dragValue;
            
            // Enable collision detection
            ballCollider.enabled = true;
        }
        
        /// <summary>
        /// Set physics for when ball is being dribbled (kinematic)
        /// </summary>
        private void SetDribblePhysics()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            // Keep collision detection enabled for steal attempts
            ballCollider.enabled = true;
        }
        
        #endregion
        
        #region Collision Detection
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            HandlePlayerCollision(other);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Handle bouncing sound
            if (!isPossessed && collision.relativeVelocity.magnitude > 2f)
            {
                PlaySound(ballBounceSound);
            }
            
            // Check for player collision
            HandlePlayerCollision(collision.collider);
        }
        
        /// <summary>
        /// Handle collision with players for possession
        /// </summary>
        /// <param name="other">Collider that hit the ball</param>
        private void HandlePlayerCollision(Collider2D other)
        {
            // Skip during kick cooldown to prevent immediate re-possession
            if (wasKicked && kickCooldown > 0) return;
            
            var player = other.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (player == null) return;
            
            // Check if player can reclaim ball (chip kick delay)
            if (!player.canReclaimBall) return;
            
            // Check distance for possession
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > possessionRadius) return;
            
            if (!isPossessed)
            {
                // Free ball - player picks it up
                AttachToPlayer(player);
            }
            else if (possessingPlayer != player)
            {
                // Ball is possessed by different player - attempt steal
                // This could be expanded with tackle mechanics
                StealBall(player);
            }
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Handle kick cooldown timer
        /// </summary>
        private void HandleKickCooldown()
        {
            if (wasKicked && kickCooldown > 0)
            {
                kickCooldown -= Time.deltaTime;
                if (kickCooldown <= 0)
                {
                    wasKicked = false;
                }
            }
        }
        
        /// <summary>
        /// Play audio clip if available
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        /// <summary>
        /// Force ball to a specific position (for resets, goals, etc.)
        /// </summary>
        /// <param name="position">Position to place ball</param>
        /// <param name="clearPossession">Whether to clear current possession</param>
        public void SetPosition(Vector2 position, bool clearPossession = true)
        {
            if (clearPossession && isPossessed)
            {
                Detach();
            }
            
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        /// <summary>
        /// Apply force to ball (for external kicks, explosions, etc.)
        /// </summary>
        /// <param name="force">Force vector to apply</param>
        /// <param name="forceMode">Type of force to apply</param>
        public void ApplyForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
        {
            if (isPossessed)
            {
                Detach(force);
            }
            else
            {
                rb.AddForce(force, forceMode);
            }
        }
        
        /// <summary>
        /// Check if any players are within dribbling range
        /// </summary>
        private bool CheckNearbyPlayers()
        {
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player != null && Vector2.Distance(transform.position, player.transform.position) <= possessionRadius * 1.5f)
                {
                    return true;
                }
            }
            return false;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (!showDebugInfo) return;
            
            // Draw possession radius
            Gizmos.color = isPossessed ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, possessionRadius);
            
            // Draw dribble target position when possessed
            if (isPossessed && Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(targetPosition, 0.2f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            
            // Draw velocity vector
            if (Application.isPlaying && rb != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rb.linearVelocity);
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(640, 10, 200, 120));
            GUILayout.Label("=== Ball Debug ===");
            GUILayout.Label($"Possessed: {isPossessed}");
            GUILayout.Label($"Owner: {possessingPlayer?.name ?? "None"}");
            GUILayout.Label($"Velocity: {rb.linearVelocity.magnitude:F1}");
            GUILayout.Label($"Position: {transform.position.ToString("F1")}");
            GUILayout.Label($"Kick Cooldown: {kickCooldown:F2}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
