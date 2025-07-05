using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Freeze Zone: Creates a large area that freezes any opposing players who enter it for 8 seconds
    /// Zone lasts for 10 seconds
    /// </summary>
    [CreateAssetMenu(fileName = "FreezeZone", menuName = "Plush League/Superpowers/Freeze Zone")]
    public class FreezeZonePowerup : BasePowerup
    {
        [Header("Freeze Zone Settings")]
        [SerializeField] private float zoneRadius = 10f;
        [SerializeField] private float zoneDuration = 10f;
        [SerializeField] private float freezeDuration = 8f;
        [SerializeField] private Color zoneColor = Color.cyan;
        [SerializeField] private GameObject zoneEffect;
        [SerializeField] private GameObject freezeEffect;
        
        public override bool Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!CanActivate(user)) return false;
            
            // Create freeze zone at player's position
            CreateFreezeZone(user.transform.position, user);
            
            return true;
        }
        
        private void CreateFreezeZone(Vector3 position, PlushLeague.Gameplay.Player.PlayerController creator)
        {
            // Create zone object
            GameObject zoneObject = new GameObject("FreezeZone");
            zoneObject.transform.position = position;
            
            // Add freeze zone component
            var freezeZone = zoneObject.AddComponent<FreezeZoneArea>();
            freezeZone.Initialize(this, creator, zoneRadius, zoneDuration, freezeDuration);
            
            // Add trigger collider
            var collider = zoneObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = zoneRadius;
            
            // Create visual effect
            if (zoneEffect != null)
            {
                GameObject effect = Instantiate(zoneEffect, position, Quaternion.identity, zoneObject.transform);
                effect.transform.localScale = Vector3.one * (zoneRadius / 5f); // Scale to match radius
            }
        }
        
        public Color GetZoneColor() => zoneColor;
        public GameObject GetFreezeEffect() => freezeEffect;
        public float GetFreezeDuration() => freezeDuration;
    }
    
    /// <summary>
    /// Component that manages the freeze zone area
    /// </summary>
    public class FreezeZoneArea : MonoBehaviour
    {
        private FreezeZonePowerup powerup;
        private PlushLeague.Gameplay.Player.PlayerController creator;
        private float zoneRadius;
        private float remainingTime;
        private float freezeDuration;
        private bool isActive = true;
        
        // Track players in zone
        private HashSet<PlushLeague.Gameplay.Player.PlayerController> playersInZone = new HashSet<PlushLeague.Gameplay.Player.PlayerController>();
        private Dictionary<PlushLeague.Gameplay.Player.PlayerController, Coroutine> freezeCoroutines = new Dictionary<PlushLeague.Gameplay.Player.PlayerController, Coroutine>();
        
        public void Initialize(FreezeZonePowerup freezePowerup, PlushLeague.Gameplay.Player.PlayerController zoneCreator, float radius, float zoneDuration, float playerFreezeDuration)
        {
            powerup = freezePowerup;
            creator = zoneCreator;
            zoneRadius = radius;
            remainingTime = zoneDuration;
            freezeDuration = playerFreezeDuration;
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            // Check if zone expired
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                DestroyZone();
                return;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            var player = other.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (player != null && player != creator)
            {
                // Player entered zone
                playersInZone.Add(player);
                FreezePlayer(player);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!isActive) return;
            
            var player = other.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (player != null && playersInZone.Contains(player))
            {
                // Player left zone
                playersInZone.Remove(player);
                
                // Don't unfreeze immediately - let freeze duration run its course
                // This makes the power more strategic
            }
        }
        
        private void FreezePlayer(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // If player is already frozen, restart freeze duration
            if (freezeCoroutines.ContainsKey(player))
            {
                StopCoroutine(freezeCoroutines[player]);
                freezeCoroutines.Remove(player);
            }
            
            // Start freeze effect
            freezeCoroutines[player] = StartCoroutine(FreezePlayerCoroutine(player));
        }
        
        private IEnumerator FreezePlayerCoroutine(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Apply freeze effect
            ApplyFreezeToPlayer(player);
            
            // Wait for freeze duration
            yield return new WaitForSeconds(freezeDuration);
            
            // Remove freeze effect
            RemoveFreezeFromPlayer(player);
            
            // Remove from tracking
            if (freezeCoroutines.ContainsKey(player))
            {
                freezeCoroutines.Remove(player);
            }
        }
        
        private void ApplyFreezeToPlayer(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Disable player input
            player.SetInputEnabled(false);
            
            // Stop player movement
            var playerMovement = player.GetComponent<PlushLeague.Gameplay.Player.PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.StopMovement();
            }
            
            // Add freeze effect component
            var freezeComponent = player.gameObject.GetComponent<FreezeZoneEffect>();
            if (freezeComponent == null)
            {
                freezeComponent = player.gameObject.AddComponent<FreezeZoneEffect>();
            }
            
            freezeComponent.ApplyFreezeEffect(powerup.GetZoneColor(), powerup.GetFreezeEffect());
        }
        
        private void RemoveFreezeFromPlayer(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (player == null) return;
            
            // Re-enable player input
            player.SetInputEnabled(true);
            
            // Remove freeze effect
            var freezeComponent = player.GetComponent<FreezeZoneEffect>();
            if (freezeComponent != null)
            {
                freezeComponent.RemoveFreezeEffect();
                Destroy(freezeComponent);
            }
        }
        
        private void DestroyZone()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Unfreeze all players still in zone
            foreach (var player in playersInZone)
            {
                if (player != null)
                {
                    RemoveFreezeFromPlayer(player);
                }
            }
            
            // Stop all freeze coroutines
            foreach (var coroutine in freezeCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            
            // Clear tracking
            playersInZone.Clear();
            freezeCoroutines.Clear();
            
            // Destroy zone object
            Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            DestroyZone();
        }
        
        // Visualize zone in editor
        private void OnDrawGizmosSelected()
        {
            if (isActive)
            {
                Gizmos.color = powerup?.GetZoneColor() ?? Color.cyan;
                Gizmos.DrawWireSphere(transform.position, zoneRadius);
            }
        }
    }
    
    /// <summary>
    /// Component that handles the visual freeze effect on players in freeze zone
    /// </summary>
    public class FreezeZoneEffect : MonoBehaviour
    {
        private Color originalColor;
        private Renderer[] renderers;
        private GameObject freezeEffectObject;
        
        public void ApplyFreezeEffect(Color freezeColor, GameObject freezeEffectPrefab)
        {
            // Store original colors and apply freeze color
            renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    // Store original color (assuming main color property)
                    originalColor = renderer.material.color;
                    
                    // Apply freeze tint
                    Color tintedColor = Color.Lerp(originalColor, freezeColor, 0.7f);
                    renderer.material.color = tintedColor;
                }
            }
            
            // Instantiate freeze effect
            if (freezeEffectPrefab != null)
            {
                freezeEffectObject = Instantiate(freezeEffectPrefab, transform.position, transform.rotation, transform);
            }
            
            // Add pulsing effect
            StartCoroutine(PulsingEffect());
        }
        
        private IEnumerator PulsingEffect()
        {
            while (gameObject != null && this != null)
            {
                // Pulse between normal and more intense freeze color
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);
                
                if (renderers != null)
                {
                    foreach (var renderer in renderers)
                    {
                        if (renderer != null && renderer.material != null)
                        {
                            Color baseColor = Color.Lerp(originalColor, Color.cyan, 0.7f);
                            Color pulseColor = Color.Lerp(baseColor, Color.white, pulse * 0.3f);
                            renderer.material.color = pulseColor;
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        public void RemoveFreezeEffect()
        {
            StopAllCoroutines();
            
            // Restore original colors
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = originalColor;
                    }
                }
            }
            
            // Remove freeze effect object
            if (freezeEffectObject != null)
            {
                Destroy(freezeEffectObject);
            }
        }
        
        private void OnDestroy()
        {
            RemoveFreezeEffect();
        }
    }
}
