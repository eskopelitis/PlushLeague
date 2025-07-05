using UnityEngine;

namespace PlushLeague.Gameplay.Goal
{
    /// <summary>
    /// Trigger component for detecting when a goal is scored
    /// Placed on goal colliders to detect ball entry
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GoalTrigger : MonoBehaviour
    {
        [Header("Goal Settings")]
        [SerializeField] private int teamId = 1; // Which team this goal belongs to (1 or 2)
        [SerializeField] private string ballTag = "Ball";
        [SerializeField] private bool debugMode = true;
        
        [Header("Audio/Visual")]
        [SerializeField] private AudioClip goalScoredSFX;
        [SerializeField] private GameObject goalEffectPrefab;
        [SerializeField] private float effectDuration = 3f;
        
        // Events
        public System.Action<int> OnGoalScored; // teamId that scored (opposite of this goal's team)
        
        // Components
        private AudioSource audioSource;
        private Collider goalCollider;
        
        private void Awake()
        {
            goalCollider = GetComponent<Collider>();
            goalCollider.isTrigger = true; // Ensure it's a trigger
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's the ball
            if (!other.CompareTag(ballTag)) return;
            
            // Check if ball has enough velocity (prevent accidental goals)
            var ballRb = other.GetComponent<Rigidbody>();
            if (ballRb != null && ballRb.linearVelocity.magnitude < 2f) return;
            
            // Determine which team scored
            int scoringTeam = (teamId == 1) ? 2 : 1; // Opposite team scored
            
            if (debugMode)
            {
                UnityEngine.Debug.Log($"GOAL! Team {scoringTeam} scored in Team {teamId}'s goal!");
            }
            
            // Play effects
            PlayGoalEffects();
            
            // Notify listeners
            OnGoalScored?.Invoke(scoringTeam);
        }
        
        /// <summary>
        /// Play visual and audio effects for the goal
        /// </summary>
        private void PlayGoalEffects()
        {
            // Play goal sound
            if (goalScoredSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(goalScoredSFX);
            }
            
            // Create goal effect
            if (goalEffectPrefab != null)
            {
                GameObject effect = Instantiate(goalEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, effectDuration);
            }
        }
        
        /// <summary>
        /// Get the team ID this goal belongs to
        /// </summary>
        public int GetTeamId()
        {
            return teamId;
        }
        
        /// <summary>
        /// Set the team ID this goal belongs to
        /// </summary>
        public void SetTeamId(int id)
        {
            teamId = id;
        }
        
        private void OnDrawGizmos()
        {
            if (!debugMode) return;
            
            // Draw goal area
            Gizmos.color = teamId == 1 ? Color.blue : Color.red;
            
            if (goalCollider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                if (goalCollider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (goalCollider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
            
            // Draw team label
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(labelPos, $"Team {teamId} Goal");
        }
    }
}
