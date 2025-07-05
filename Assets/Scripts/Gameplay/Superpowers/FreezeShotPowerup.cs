using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Freeze Shot: Temporarily freezes all opposing players for 3 seconds, then shoots the ball
    /// Only usable when the player has the ball
    /// </summary>
    [CreateAssetMenu(fileName = "FreezeShot", menuName = "Plush League/Superpowers/Freeze Shot")]
    public class FreezeShotPowerup : BasePowerup
    {
        [Header("Freeze Shot Settings")]
        [SerializeField] private float freezeDuration = 3f;
        [SerializeField] private float shotForce = 20f;
        [SerializeField] private Color freezeEffectColor = Color.cyan;
        [SerializeField] private GameObject freezeEffect;
        [SerializeField] private GameObject shotEffect;
        
        public override bool CanActivate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!base.CanActivate(user)) return false;
            
            // Can only use Freeze Shot when player has the ball
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
            
            // Start the freeze shot sequence
            user.StartCoroutine(FreezeShotSequence(user));
            
            return true;
        }
        
        private IEnumerator FreezeShotSequence(PlushLeague.Gameplay.Player.PlayerController user)
        {
            // Step 1: Freeze all opposing players
            var opposingPlayers = FreezeOpposingPlayers(user);
            
            // Step 2: Wait for freeze duration
            yield return new WaitForSeconds(freezeDuration);
            
            // Step 3: Execute the shot
            ExecuteShot(user);
            
            // Step 4: Unfreeze players
            UnfreezePlayers(opposingPlayers);
        }
        
        private PlushLeague.Gameplay.Player.PlayerController[] FreezeOpposingPlayers(PlushLeague.Gameplay.Player.PlayerController user)
        {
            // Find all players except the user
            var allPlayers = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            var opposingPlayers = new System.Collections.Generic.List<PlushLeague.Gameplay.Player.PlayerController>();
            
            foreach (var player in allPlayers)
            {
                if (player != user && player.gameObject.activeInHierarchy)
                {
                    // Freeze the player
                    FreezePlayer(player);
                    opposingPlayers.Add(player);
                }
            }
            
            return opposingPlayers.ToArray();
        }
        
        private void FreezePlayer(PlushLeague.Gameplay.Player.PlayerController player)
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
            var freezeComponent = player.gameObject.GetComponent<FreezeEffect>();
            if (freezeComponent == null)
            {
                freezeComponent = player.gameObject.AddComponent<FreezeEffect>();
            }
            
            freezeComponent.ApplyFreezeEffect(freezeEffectColor, freezeEffect);
        }
        
        private void UnfreezePlayers(PlushLeague.Gameplay.Player.PlayerController[] players)
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    UnfreezePlayer(player);
                }
            }
        }
        
        private void UnfreezePlayer(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Re-enable player input
            player.SetInputEnabled(true);
            
            // Remove freeze effect
            var freezeComponent = player.GetComponent<FreezeEffect>();
            if (freezeComponent != null)
            {
                freezeComponent.RemoveFreezeEffect();
                Destroy(freezeComponent);
            }
        }
        
        private void ExecuteShot(PlushLeague.Gameplay.Player.PlayerController user)
        {
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager == null) return;
            
            var ballController = ballManager.GetBallController();
            if (ballController == null) return;
            
            // Calculate shot direction based on user's facing direction
            Vector3 shotDirection = user.transform.forward;
            
            // Apply shot force to the ball
            var ballRb = ballController.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.AddForce(shotDirection * shotForce, ForceMode.Impulse);
            }
            
            // Release the ball from player
            ballManager.ReleaseBall();
            
            // Play shot effect
            if (shotEffect != null)
            {
                GameObject effect = Instantiate(shotEffect, ballController.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
    }
    
    /// <summary>
    /// Component that handles the visual freeze effect on players
    /// </summary>
    public class FreezeEffect : MonoBehaviour
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
                    Color tintedColor = Color.Lerp(originalColor, freezeColor, 0.6f);
                    renderer.material.color = tintedColor;
                }
            }
            
            // Instantiate freeze effect
            if (freezeEffectPrefab != null)
            {
                freezeEffectObject = Instantiate(freezeEffectPrefab, transform.position, transform.rotation, transform);
            }
        }
        
        public void RemoveFreezeEffect()
        {
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
