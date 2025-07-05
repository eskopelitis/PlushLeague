using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Curve Boost: Player's next shot will curve toward the goal automatically
    /// Can be activated without the ball, but effect only triggers on next shot
    /// </summary>
    [CreateAssetMenu(fileName = "CurveBoost", menuName = "Plush League/Superpowers/Curve Boost")]
    public class CurveBoostPowerup : BasePowerup
    {
        [Header("Curve Boost Settings")]
        [SerializeField] private float curveStrength = 15f;
        [SerializeField] private float effectDuration = 30f; // How long the boost lasts if not used
        [SerializeField] private Color boostAuraColor = Color.green;
        [SerializeField] private GameObject chargeEffect;
        [SerializeField] private GameObject shotCurveEffect;
        
        public override bool Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!CanActivate(user)) return false;
            
            // Apply curve boost effect to player
            var curveComponent = user.GetComponent<CurveBoostEffect>();
            if (curveComponent == null)
            {
                curveComponent = user.gameObject.AddComponent<CurveBoostEffect>();
            }
            
            curveComponent.Initialize(this, effectDuration);
            
            // Play activation effects
            PlayActivationEffects(user);
            
            return true;
        }
        
        public override void OnEffectEnd(PlushLeague.Gameplay.Player.PlayerController user)
        {
            // Remove curve boost component if it still exists
            var curveComponent = user.GetComponent<CurveBoostEffect>();
            if (curveComponent != null)
            {
                curveComponent.RemoveEffect();
            }
        }
        
        private void PlayActivationEffects(PlushLeague.Gameplay.Player.PlayerController user)
        {
            // Create charge effect around player
            if (chargeEffect != null)
            {
                GameObject effect = Instantiate(chargeEffect, user.transform.position, user.transform.rotation, user.transform);
                
                // The effect will be destroyed when the boost is used or expires
                var curveComponent = user.GetComponent<CurveBoostEffect>();
                if (curveComponent != null)
                {
                    curveComponent.SetChargeEffect(effect);
                }
            }
        }
        
        /// <summary>
        /// Apply curve to a ball shot by the boosted player
        /// </summary>
        public void ApplyCurveToShot(PlushLeague.Gameplay.Ball.BallController ball, PlushLeague.Gameplay.Player.PlayerController shooter)
        {
            var curveComponent = ball.gameObject.GetComponent<CurveBallEffect>();
            if (curveComponent == null)
            {
                curveComponent = ball.gameObject.AddComponent<CurveBallEffect>();
            }
            
            curveComponent.Initialize(curveStrength, shooter, shotCurveEffect);
        }
    }
    
    /// <summary>
    /// Component that tracks curve boost effect on a player
    /// </summary>
    public class CurveBoostEffect : MonoBehaviour
    {
        private CurveBoostPowerup powerup;
        private float remainingTime;
        private GameObject chargeEffectObject;
        private bool isActive = true;
        
        // Visual effects
        private Renderer[] renderers;
        private Color[] originalColors;
        
        public void Initialize(CurveBoostPowerup curvePowerup, float duration)
        {
            powerup = curvePowerup;
            remainingTime = duration;
            
            ApplyVisualEffect();
            
            // Listen for player shooting
            var playerController = GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (playerController != null)
            {
                // We'll check for shots in Update since we don't have a shot event system
            }
        }
        
        public void SetChargeEffect(GameObject effect)
        {
            chargeEffectObject = effect;
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            // Check if effect expired
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                RemoveEffect();
                return;
            }
            
            // Check if player shot the ball
            CheckForShot();
        }
        
        private void CheckForShot()
        {
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager == null) return;
            
            var ballController = ballManager.GetBallController();
            if (ballController == null) return;
            
            var ballRb = ballController.GetComponent<Rigidbody>();
            if (ballRb == null) return;
            
            // Check if ball is moving fast (indicating a shot was made)
            // and if this player was the last to have the ball
            if (ballRb.linearVelocity.magnitude > 10f && ballManager.GetLastBallCarrier() == GetComponent<PlushLeague.Gameplay.Player.PlayerController>())
            {
                // Check if ball doesn't already have curve effect (to avoid double application)
                if (ballController.GetComponent<CurveBallEffect>() == null)
                {
                    // Apply curve to the shot
                    powerup.ApplyCurveToShot(ballController, GetComponent<PlushLeague.Gameplay.Player.PlayerController>());
                    
                    // Remove this effect (one-time use)
                    RemoveEffect();
                }
            }
        }
        
        private void ApplyVisualEffect()
        {
            // Apply aura effect to player
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    originalColors[i] = renderers[i].material.color;
                    
                    // Apply boost color tint
                    Color boostedColor = Color.Lerp(originalColors[i], Color.green, 0.3f);
                    renderers[i].material.color = boostedColor;
                }
            }
        }
        
        public void RemoveEffect()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Restore original colors
            if (renderers != null && originalColors != null)
            {
                for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].material != null)
                    {
                        renderers[i].material.color = originalColors[i];
                    }
                }
            }
            
            // Remove charge effect
            if (chargeEffectObject != null)
            {
                Destroy(chargeEffectObject);
            }
            
            // Remove this component
            Destroy(this);
        }
        
        private void OnDestroy()
        {
            RemoveEffect();
        }
    }
    
    /// <summary>
    /// Component that applies curve effect to a ball in flight
    /// </summary>
    public class CurveBallEffect : MonoBehaviour
    {
        private float curveStrength;
        private PlushLeague.Gameplay.Player.PlayerController shooter;
        private GameObject curveEffectObject;
        private Vector3 targetGoalPosition;
        private Rigidbody ballRb;
        private bool isActive = true;
        
        public void Initialize(float strength, PlushLeague.Gameplay.Player.PlayerController shooterPlayer, GameObject curveEffect)
        {
            curveStrength = strength;
            shooter = shooterPlayer;
            ballRb = GetComponent<Rigidbody>();
            
            // Find target goal (opposite to shooter's goal)
            FindTargetGoal();
            
            // Create curve effect
            if (curveEffect != null)
            {
                curveEffectObject = Instantiate(curveEffect, transform.position, transform.rotation, transform);
            }
        }
        
        private void FindTargetGoal()
        {
            // Find all goals and determine which one to target
            var goals = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            
            foreach (var goal in goals)
            {
                if (goal.CompareTag("Goal"))
                {
                    // Simple logic: target the goal furthest from the shooter
                    float distance = Vector3.Distance(shooter.transform.position, goal.transform.position);
                    if (distance > 10f) // Assume goals are at least 10 units apart
                    {
                        targetGoalPosition = goal.transform.position;
                        break;
                    }
                }
            }
            
            // Fallback: if no goal found, target forward direction
            if (targetGoalPosition == Vector3.zero)
            {
                targetGoalPosition = transform.position + transform.forward * 50f;
            }
        }
        
        private void FixedUpdate()
        {
            if (!isActive || ballRb == null) return;
            
            // Only apply curve while ball is moving at significant speed
            if (ballRb.linearVelocity.magnitude < 2f)
            {
                RemoveEffect();
                return;
            }
            
            // Calculate direction to goal
            Vector3 directionToGoal = (targetGoalPosition - transform.position).normalized;
            
            // Apply curve force perpendicular to current velocity
            Vector3 currentDirection = ballRb.linearVelocity.normalized;
            Vector3 curveDirection = Vector3.Cross(Vector3.up, currentDirection).normalized;
            
            // Determine which direction to curve based on goal position
            float dot = Vector3.Dot(curveDirection, directionToGoal);
            if (dot < 0)
            {
                curveDirection = -curveDirection;
            }
            
            // Apply curve force
            ballRb.AddForce(curveDirection * curveStrength * Time.fixedDeltaTime, ForceMode.Force);
            
            // Also apply slight attraction to goal
            Vector3 goalAttraction = directionToGoal * (curveStrength * 0.3f);
            ballRb.AddForce(goalAttraction * Time.fixedDeltaTime, ForceMode.Force);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Remove effect when ball hits something significant
            if (other.CompareTag("Goal") || other.CompareTag("Player") || other.CompareTag("Ground"))
            {
                RemoveEffect();
            }
        }
        
        private void RemoveEffect()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Remove curve effect object
            if (curveEffectObject != null)
            {
                Destroy(curveEffectObject);
            }
            
            // Remove this component
            Destroy(this);
        }
        
        private void OnDestroy()
        {
            RemoveEffect();
        }
    }
}
