using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Ultra Dash: Player dashes at 3x speed for 4 seconds, can phase through other players
    /// </summary>
    [CreateAssetMenu(fileName = "UltraDash", menuName = "Plush League/Superpowers/Ultra Dash")]
    public class UltraDashPowerup : BasePowerup
    {
        [Header("Ultra Dash Settings")]
        [SerializeField] private float speedMultiplier = 3f;
        [SerializeField] private float dashDuration = 4f;
        [SerializeField] private bool canPhaseThrough = true;
        [SerializeField] private Color dashTrailColor = Color.yellow;
        [SerializeField] private GameObject dashEffect;
        [SerializeField] private GameObject speedLinesEffect;
        
        public override bool Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!CanActivate(user)) return false;
            
            // Apply ultra dash effect
            var dashComponent = user.GetComponent<UltraDashEffect>();
            if (dashComponent == null)
            {
                dashComponent = user.gameObject.AddComponent<UltraDashEffect>();
            }
            
            dashComponent.Initialize(this, dashDuration, speedMultiplier, canPhaseThrough);
            
            return true;
        }
        
        public override void OnEffectEnd(PlushLeague.Gameplay.Player.PlayerController user)
        {
            var dashComponent = user.GetComponent<UltraDashEffect>();
            if (dashComponent != null)
            {
                dashComponent.RemoveEffect();
            }
        }
        
        public Color GetDashTrailColor() => dashTrailColor;
        public GameObject GetDashEffect() => dashEffect;
        public GameObject GetSpeedLinesEffect() => speedLinesEffect;
    }
    
    /// <summary>
    /// Component that handles the ultra dash effect on a player
    /// </summary>
    public class UltraDashEffect : MonoBehaviour
    {
        private UltraDashPowerup powerup;
        private float remainingTime;
        private float speedMultiplier;
        private bool canPhaseThrough;
        private bool isActive = true;
        
        // Original values to restore
        private float originalMaxSpeed;
        private LayerMask originalPlayerLayerMask;
        
        // Visual effects
        private TrailRenderer trailRenderer;
        private GameObject dashEffectObject;
        private GameObject speedLinesObject;
        private Renderer[] renderers;
        private Color[] originalColors;
        
        // Components
        private PlushLeague.Gameplay.Player.PlayerMovement playerMovement;
        private Collider playerCollider;
        
        public void Initialize(UltraDashPowerup dashPowerup, float duration, float speedMult, bool phaseThrough)
        {
            powerup = dashPowerup;
            remainingTime = duration;
            speedMultiplier = speedMult;
            canPhaseThrough = phaseThrough;
            
            // Get components
            playerMovement = GetComponent<PlushLeague.Gameplay.Player.PlayerMovement>();
            playerCollider = GetComponent<Collider>();
            
            ApplyDashEffect();
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
            
            // Update visual effects
            UpdateVisualEffects();
        }
        
        private void ApplyDashEffect()
        {
            // Boost movement speed
            var playerController = GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (playerController != null)
            {
                originalMaxSpeed = playerController.GetMaxSpeed();
                playerController.SetMaxSpeed(originalMaxSpeed * speedMultiplier);
            }
            
            // Enable phasing through players
            if (canPhaseThrough && playerCollider != null)
            {
                // Change collision layer to avoid player collisions
                int originalLayer = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("NoPlayerCollision"); // Assuming this layer exists
                
                // If layer doesn't exist, we'll disable collision detection with other players
                if (gameObject.layer == originalLayer)
                {                // Fallback: ignore collisions with other players manually
                var allPlayers = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
                    foreach (var player in allPlayers)
                    {
                        if (player != GetComponent<PlushLeague.Gameplay.Player.PlayerController>())
                        {
                            var otherCollider = player.GetComponent<Collider>();
                            if (otherCollider != null)
                            {
                                Physics.IgnoreCollision(playerCollider, otherCollider, true);
                            }
                        }
                    }
                }
            }
            
            // Apply visual effects
            ApplyVisualEffects();
        }
        
        private void ApplyVisualEffects()
        {
            // Add trail renderer
            trailRenderer = gameObject.GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }
            
            trailRenderer.enabled = true;
            trailRenderer.time = 0.3f;
            trailRenderer.startWidth = 0.5f;
            trailRenderer.endWidth = 0.1f;
            trailRenderer.startColor = powerup.GetDashTrailColor();
            trailRenderer.endColor = new Color(powerup.GetDashTrailColor().r, powerup.GetDashTrailColor().g, powerup.GetDashTrailColor().b, 0f);
            
            // Create trail material
            if (trailRenderer.material == null)
            {
                trailRenderer.material = CreateTrailMaterial();
            }
            
            // Create dash effect
            if (powerup.GetDashEffect() != null)
            {
                dashEffectObject = Instantiate(powerup.GetDashEffect(), transform.position, transform.rotation, transform);
            }
            
            // Create speed lines effect
            if (powerup.GetSpeedLinesEffect() != null)
            {
                speedLinesObject = Instantiate(powerup.GetSpeedLinesEffect(), transform.position, transform.rotation, transform);
            }
            
            // Apply speed aura to player
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    originalColors[i] = renderers[i].material.color;
                    
                    // Apply dash color tint
                    Color dashColor = Color.Lerp(originalColors[i], powerup.GetDashTrailColor(), 0.3f);
                    renderers[i].material.color = dashColor;
                }
            }
        }
        
        private void UpdateVisualEffects()
        {
            // Make player slightly transparent and glowing when phasing
            if (canPhaseThrough && renderers != null)
            {
                float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * 10f); // Pulsing effect
                
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material != null)
                    {
                        Color currentColor = renderers[i].material.color;
                        currentColor.a = alpha;
                        renderers[i].material.color = currentColor;
                    }
                }
            }
        }
        
        private Material CreateTrailMaterial()
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = powerup.GetDashTrailColor();
            return material;
        }
        
        public void RemoveEffect()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Restore original movement speed
            var playerController = GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (playerController != null)
            {
                playerController.SetMaxSpeed(originalMaxSpeed);
            }
            
            // Restore collision detection
            if (canPhaseThrough)
            {
                // Re-enable collisions with other players
                var allPlayers = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
                foreach (var player in allPlayers)
                {
                    if (player != GetComponent<PlushLeague.Gameplay.Player.PlayerController>())
                    {
                        var otherCollider = player.GetComponent<Collider>();
                        if (otherCollider != null && playerCollider != null)
                        {
                            Physics.IgnoreCollision(playerCollider, otherCollider, false);
                        }
                    }
                }
            }
            
            // Remove visual effects
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
                Destroy(trailRenderer);
            }
            
            if (dashEffectObject != null)
            {
                Destroy(dashEffectObject);
            }
            
            if (speedLinesObject != null)
            {
                Destroy(speedLinesObject);
            }
            
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
            
            // Remove this component
            Destroy(this);
        }
        
        private void OnDestroy()
        {
            RemoveEffect();
        }
    }
}
