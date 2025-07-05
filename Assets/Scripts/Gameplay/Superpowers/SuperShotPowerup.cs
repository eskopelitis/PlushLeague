using UnityEngine;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Super Shot: Unleashes a powerful shot that moves 3x faster and cannot be blocked by goalies
    /// Only usable when the player has the ball
    /// </summary>
    [CreateAssetMenu(fileName = "SuperShot", menuName = "Plush League/Superpowers/Super Shot")]
    public class SuperShotPowerup : BasePowerup
    {
        [Header("Super Shot Settings")]
        [SerializeField] private float shotSpeedMultiplier = 3f;
        [SerializeField] private bool cannotBeBlocked = true;
        [SerializeField] private Color shotTrailColor = Color.red;
        [SerializeField] private GameObject shotEffect;
        
        public override bool CanActivate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!base.CanActivate(user)) return false;
            
            // Can only use Super Shot when player has the ball
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager != null)
            {
                return ballManager.GetBallCarrier() == user;
            }
            
            return false;
        }
        
        public override bool Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!CanActivate(user)) return false;
            
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager == null) return false;
            
            var ballController = ballManager.GetBallController();
            if (ballController == null) return false;
            
            // Calculate shot direction based on user's facing direction
            Vector3 shotDirection = user.transform.forward;
            
            // Apply super shot effect to the ball
            ApplySuperShotToBall(ballController, shotDirection, user);
            
            // Release the ball from player
            ballManager.ReleaseBall();
            
            // Apply visual effects
            PlaySuperShotEffects(user, ballController);
            
            return true;
        }
        
        private void ApplySuperShotToBall(PlushLeague.Gameplay.Ball.BallController ball, Vector3 direction, PlushLeague.Gameplay.Player.PlayerController shooter)
        {
            var ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb == null) return;
            
            // Calculate super shot force
            float baseForce = 25f; // Standard shot force
            float superShotForce = baseForce * shotSpeedMultiplier;
            
            // Apply the force
            ballRb.linearVelocity = Vector3.zero; // Reset current velocity
            ballRb.AddForce(direction * superShotForce, ForceMode.Impulse);
            
            // Mark ball as super shot (for goalie blocking logic)
            var superShotMarker = ball.gameObject.GetComponent<SuperShotMarker>();
            if (superShotMarker == null)
            {
                superShotMarker = ball.gameObject.AddComponent<SuperShotMarker>();
            }
            
            superShotMarker.Initialize(shooter, cannotBeBlocked, data.duration);
        }
        
        private void PlaySuperShotEffects(PlushLeague.Gameplay.Player.PlayerController user, PlushLeague.Gameplay.Ball.BallController ball)
        {
            // Create shot effect at ball position
            if (shotEffect != null)
            {
                GameObject effect = Instantiate(shotEffect, ball.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Add trail effect to ball
            var trailRenderer = ball.GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = ball.gameObject.AddComponent<TrailRenderer>();
                trailRenderer.time = 0.5f;
                trailRenderer.startWidth = 0.3f;
                trailRenderer.endWidth = 0.1f;
                trailRenderer.material = CreateTrailMaterial();
            }
            
            trailRenderer.enabled = true;
            trailRenderer.startColor = shotTrailColor;
            trailRenderer.endColor = new Color(shotTrailColor.r, shotTrailColor.g, shotTrailColor.b, 0f);
            
            // Disable trail after duration (start coroutine from the ball MonoBehaviour)
            ball.StartCoroutine(DisableTrailAfterDelay(trailRenderer, data.duration));
        }
        
        private Material CreateTrailMaterial()
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = shotTrailColor;
            return material;
        }
        
        private System.Collections.IEnumerator DisableTrailAfterDelay(TrailRenderer trail, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (trail != null)
            {
                trail.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Component that marks a ball as being a super shot
    /// Used by goalie save system to determine if shot can be blocked
    /// </summary>
    public class SuperShotMarker : MonoBehaviour
    {
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController shooter;
        [SerializeField] private bool cannotBeBlocked;
        [SerializeField] private float duration;
        
        private float remainingTime;
        
        public PlushLeague.Gameplay.Player.PlayerController Shooter => shooter;
        public bool CannotBeBlocked => cannotBeBlocked && remainingTime > 0;
        
        public void Initialize(PlushLeague.Gameplay.Player.PlayerController shooterPlayer, bool unblockable, float effectDuration)
        {
            shooter = shooterPlayer;
            cannotBeBlocked = unblockable;
            duration = effectDuration;
            remainingTime = duration;
        }
        
        private void Update()
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                
                if (remainingTime <= 0)
                {
                    // Effect expired, clean up
                    CleanupEffect();
                }
            }
        }
        
        private void CleanupEffect()
        {
            // Remove trail renderer if it exists
            var trail = GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.enabled = false;
                Destroy(trail);
            }
            
            // Remove this component
            Destroy(this);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // If ball hits goal, remove marker
            if (other.CompareTag("Goal"))
            {
                CleanupEffect();
            }
        }
    }
}
