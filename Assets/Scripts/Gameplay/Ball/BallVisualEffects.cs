using UnityEngine;

namespace PlushLeague.Gameplay.Ball
{
    /// <summary>
    /// Handles visual effects for the ball (trails, possession indicators, etc.)
    /// </summary>
    public class BallVisualEffects : MonoBehaviour
    {
        [Header("Trail Effect")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private float trailSpeedThreshold = 5f;
        [SerializeField] private Gradient highSpeedTrailColor;
        [SerializeField] private Gradient normalTrailColor;
        
        [Header("Possession Effect")]
        [SerializeField] private ParticleSystem possessionEffect;
        [SerializeField] private GameObject possessionIndicator;
        [SerializeField] private Color[] teamColors = { Color.red, Color.blue, Color.green, Color.yellow };
        
        [Header("Bounce Effect")]
        [SerializeField] private ParticleSystem bounceEffect;
        [SerializeField] private AnimationCurve bounceScaleCurve;
        [SerializeField] private float bounceEffectDuration = 0.3f;
        
        [Header("Kick Effect")]
        [SerializeField] private ParticleSystem kickEffect;
        [SerializeField] private GameObject kickImpactPrefab;
        
        private BallController ballController;
        private Rigidbody2D rb;
        private bool isEffectActive;
        private float bounceTimer;
        private Vector3 originalScale;
        
        private void Awake()
        {
            ballController = GetComponent<BallController>();
            rb = GetComponent<Rigidbody2D>();
            originalScale = transform.localScale;
        }
        
        private void Start()
        {
            InitializeEffects();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void Update()
        {
            UpdateTrailEffect();
            UpdateBounceEffect();
            UpdatePossessionIndicator();
        }
        
        #region Initialization
        
        private void InitializeEffects()
        {
            // Initialize trail
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
                trailRenderer.colorGradient = normalTrailColor;
            }
            
            // Initialize possession indicator
            if (possessionIndicator != null)
            {
                possessionIndicator.SetActive(false);
            }
            
            // Initialize particle effects
            if (possessionEffect != null)
            {
                possessionEffect.Stop();
            }
            
            if (bounceEffect != null)
            {
                bounceEffect.Stop();
            }
            
            if (kickEffect != null)
            {
                kickEffect.Stop();
            }
        }
        
        private void SubscribeToEvents()
        {
            if (ballController != null)
            {
                ballController.OnBallPossessed += OnBallPossessed;
                ballController.OnBallReleased += OnBallReleased;
                ballController.OnBallKicked += OnBallKicked;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (ballController != null)
            {
                ballController.OnBallPossessed -= OnBallPossessed;
                ballController.OnBallReleased -= OnBallReleased;
                ballController.OnBallKicked -= OnBallKicked;
            }
        }
        
        #endregion
        
        #region Trail Effect
        
        private void UpdateTrailEffect()
        {
            if (trailRenderer == null || rb == null) return;
            
            float currentSpeed = rb.linearVelocity.magnitude;
            bool shouldShowTrail = currentSpeed > trailSpeedThreshold && !ballController.IsPossessed;
            
            if (shouldShowTrail && !trailRenderer.enabled)
            {
                trailRenderer.enabled = true;
                trailRenderer.Clear(); // Clear old trail
            }
            else if (!shouldShowTrail && trailRenderer.enabled)
            {
                trailRenderer.enabled = false;
            }
            
            // Update trail color based on speed
            if (trailRenderer.enabled)
            {
                float speedRatio = Mathf.Clamp01(currentSpeed / (trailSpeedThreshold * 2f));
                trailRenderer.colorGradient = Color.Lerp(normalTrailColor.colorKeys[0].color, 
                    highSpeedTrailColor.colorKeys[0].color, speedRatio) == normalTrailColor.colorKeys[0].color ? 
                    normalTrailColor : highSpeedTrailColor;
            }
        }
        
        #endregion
        
        #region Possession Effects
        
        private void OnBallPossessed(PlushLeague.Gameplay.Player.PlayerController player)
        {
            ShowPossessionEffect(player);
            StopTrailEffect();
        }
        
        private void OnBallReleased()
        {
            HidePossessionEffect();
        }
        
        private void ShowPossessionEffect(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Show possession indicator
            if (possessionIndicator != null)
            {
                possessionIndicator.SetActive(true);
                
                // Set team color (simple implementation - could be enhanced)
                var renderer = possessionIndicator.GetComponent<Renderer>();
                if (renderer != null && teamColors.Length > 0)
                {
                    int teamIndex = Mathf.Abs(player.GetInstanceID()) % teamColors.Length;
                    renderer.material.color = teamColors[teamIndex];
                }
            }
            
            // Play possession particle effect
            if (possessionEffect != null)
            {
                possessionEffect.Play();
            }
            
            // Quick scale bounce effect
            StartCoroutine(PossessionBounceEffect());
        }
        
        private void HidePossessionEffect()
        {
            if (possessionIndicator != null)
            {
                possessionIndicator.SetActive(false);
            }
            
            if (possessionEffect != null)
            {
                possessionEffect.Stop();
            }
        }
        
        private void UpdatePossessionIndicator()
        {
            if (possessionIndicator != null && possessionIndicator.activeInHierarchy)
            {
                // Rotate possession indicator for visual appeal
                possessionIndicator.transform.Rotate(0, 0, 90f * Time.deltaTime);
            }
        }
        
        private System.Collections.IEnumerator PossessionBounceEffect()
        {
            float duration = 0.2f;
            float elapsed = 0f;
            Vector3 startScale = originalScale;
            Vector3 bounceScale = originalScale * 1.2f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scaleMultiplier = Mathf.Lerp(1.2f, 1f, progress);
                transform.localScale = originalScale * scaleMultiplier;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        #endregion
        
        #region Bounce Effects
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!ballController.IsPossessed && rb.linearVelocity.magnitude > 2f)
            {
                TriggerBounceEffect(collision);
            }
        }
        
        private void TriggerBounceEffect(Collision2D collision)
        {
            // Play bounce particle effect
            if (bounceEffect != null)
            {
                bounceEffect.transform.position = collision.contacts[0].point;
                bounceEffect.Play();
            }
            
            // Start bounce animation
            bounceTimer = bounceEffectDuration;
        }
        
        private void UpdateBounceEffect()
        {
            if (bounceTimer > 0)
            {
                bounceTimer -= Time.deltaTime;
                float progress = 1f - (bounceTimer / bounceEffectDuration);
                float scaleMultiplier = bounceScaleCurve.Evaluate(progress);
                transform.localScale = originalScale * scaleMultiplier;
                
                if (bounceTimer <= 0)
                {
                    transform.localScale = originalScale;
                }
            }
        }
        
        #endregion
        
        #region Kick Effects
        
        private void OnBallKicked(Vector2 kickForce)
        {
            ShowKickEffect(kickForce);
        }
        
        private void ShowKickEffect(Vector2 kickForce)
        {
            // Play kick particle effect
            if (kickEffect != null)
            {
                var main = kickEffect.main;
                main.startSpeed = kickForce.magnitude * 0.5f;
                kickEffect.Play();
            }
            
            // Spawn kick impact effect
            if (kickImpactPrefab != null)
            {
                GameObject impact = Instantiate(kickImpactPrefab, transform.position, Quaternion.identity);
                Destroy(impact, 2f); // Auto-cleanup
            }
            
            // Camera shake could be triggered here
            // CameraShake.Instance?.Shake(kickForce.magnitude * 0.1f);
        }
        
        private void StopTrailEffect()
        {
            if (trailRenderer != null && trailRenderer.enabled)
            {
                trailRenderer.enabled = false;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Manually trigger a visual effect
        /// </summary>
        /// <param name="effectType">Type of effect to trigger</param>
        public void TriggerEffect(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Bounce:
                    bounceTimer = bounceEffectDuration;
                    break;
                case EffectType.Kick:
                    ShowKickEffect(Vector2.up * 5f); // Default kick
                    break;
                case EffectType.Possession:
                    if (ballController.PossessingPlayer != null)
                        ShowPossessionEffect(ballController.PossessingPlayer);
                    break;
            }
        }
        
        /// <summary>
        /// Set team color for possession indicator
        /// </summary>
        /// <param name="color">Team color</param>
        public void SetTeamColor(Color color)
        {
            if (possessionIndicator != null)
            {
                var renderer = possessionIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }
        
        #endregion
        
        public enum EffectType
        {
            Bounce,
            Kick,
            Possession
        }
    }
}
