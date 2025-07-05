using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Manages superpower activation, cooldowns, and UI feedback for a player
    /// </summary>
    public class PowerupController : MonoBehaviour
    {
        [Header("Powerup Configuration")]
        [SerializeField] private BasePowerup activePowerup;
        [SerializeField] private bool debugMode = false;
        
        [Header("UI References")]
        [SerializeField] private SuperpowerUI superpowerUI;
        
        // State tracking
        private float currentCooldown = 0f;
        private bool isOnCooldown = false;
        private bool isEffectActive = false;
        private Coroutine cooldownCoroutine;
        private Coroutine effectCoroutine;
        
        // Components
        private PlushLeague.Gameplay.Player.PlayerController playerController;
        
        // Events
        public System.Action<float> OnCooldownChanged;
        public System.Action<bool> OnSuperpowerReady;
        public System.Action<string> OnSuperpowerActivated;
        
        private void Awake()
        {
            playerController = GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            if (playerController == null)
            {
                UnityEngine.Debug.LogError($"PowerupController requires PlayerController component on {gameObject.name}");
            }
        }
        
        private void Start()
        {
            // Initialize UI
            if (superpowerUI != null && activePowerup != null)
            {
                superpowerUI.Initialize(activePowerup.data);
            }
            
            // Subscribe to events
            OnCooldownChanged += UpdateCooldownUI;
            OnSuperpowerReady += UpdateReadyStateUI;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            OnCooldownChanged -= UpdateCooldownUI;
            OnSuperpowerReady -= UpdateReadyStateUI;
            
            // Stop any running coroutines
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
            }
        }
        
        /// <summary>
        /// Set the active powerup for this player
        /// </summary>
        /// <param name="powerup">The powerup to assign</param>
        public void SetPowerup(BasePowerup powerup)
        {
            activePowerup = powerup;
            
            if (superpowerUI != null && powerup != null)
            {
                superpowerUI.Initialize(powerup.data);
            }
            
            if (debugMode)
            {
                UnityEngine.Debug.Log($"Powerup set to: {(powerup != null ? powerup.GetDisplayName() : "None")}");
            }
        }
        
        /// <summary>
        /// Set the active powerup from SuperpowerData
        /// </summary>
        /// <param name="superpowerData">The superpower data to assign</param>
        public void SetSuperpower(SuperpowerData superpowerData)
        {
            if (superpowerData?.powerupScript != null)
            {
                SetPowerup(superpowerData.powerupScript);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"SuperpowerData {superpowerData?.displayName} has no powerupScript assigned");
            }
        }
        
        /// <summary>
        /// Attempt to activate the superpower
        /// </summary>
        /// <returns>True if activation was successful</returns>
        public bool TryActivatePowerup()
        {
            if (!CanActivatePowerup())
            {
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"Cannot activate powerup: Ready={!isOnCooldown}, HasPowerup={activePowerup != null}, CanActivate={activePowerup?.CanActivate(playerController)}");
                }
                return false;
            }
            
            if (debugMode)
            {
                UnityEngine.Debug.Log($"Activating powerup: {activePowerup.GetDisplayName()}");
            }
            
            // Activate the powerup
            bool success = activePowerup.Activate(playerController);
            
            if (success)
            {
                // Mark effect as active
                isEffectActive = true;
                
                // Start cooldown
                StartCooldown();
                
                // Start effect duration timer
                if (activePowerup.data.duration > 0)
                {
                    effectCoroutine = StartCoroutine(EffectDurationCoroutine());
                }
                
                // Notify events
                OnSuperpowerActivated?.Invoke(activePowerup.GetDisplayName());
                
                // Visual/Audio feedback
                PlayActivationEffects();
            }
            
            return success;
        }
        
        /// <summary>
        /// Check if the powerup can be activated
        /// </summary>
        public bool CanActivatePowerup()
        {
            return !isOnCooldown && 
                   activePowerup != null && 
                   activePowerup.CanActivate(playerController);
        }
        
        /// <summary>
        /// Get the current cooldown progress (0-1)
        /// </summary>
        public float GetCooldownProgress()
        {
            if (!isOnCooldown || activePowerup?.data == null) return 1f;
            return 1f - (currentCooldown / activePowerup.data.cooldownTime);
        }
        
        /// <summary>
        /// Get the remaining cooldown time
        /// </summary>
        public float GetRemainingCooldown()
        {
            return isOnCooldown ? currentCooldown : 0f;
        }
        
        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        public float GetCooldownRemaining()
        {
            return isOnCooldown ? currentCooldown : 0f;
        }
        
        /// <summary>
        /// Get maximum cooldown time for this powerup
        /// </summary>
        public float GetMaxCooldown()
        {
            return activePowerup?.data?.cooldownTime ?? 0f;
        }
        
        /// <summary>
        /// Check if powerup can be used (alias for CanActivatePowerup for UI compatibility)
        /// </summary>
        public bool CanUsePowerup()
        {
            return CanActivatePowerup();
        }
        
        /// <summary>
        /// Get the current state of the superpower effect
        /// </summary>
        public bool GetEffectActiveState()
        {
            return isEffectActive;
        }
        
        /// <summary>
        /// Force end the current effect (for debugging or special circumstances)
        /// </summary>
        public void ForceEndEffect()
        {
            if (isEffectActive)
            {
                EndEffect();
            }
        }
        
        private void StartCooldown()
        {
            if (activePowerup?.data == null) return;
            
            isOnCooldown = true;
            currentCooldown = activePowerup.data.cooldownTime;
            
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
            
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
            
            // Notify UI
            OnSuperpowerReady?.Invoke(false);
        }
        
        private IEnumerator CooldownCoroutine()
        {
            while (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
                OnCooldownChanged?.Invoke(GetCooldownProgress());
                yield return null;
            }
            
            // Cooldown complete
            isOnCooldown = false;
            currentCooldown = 0f;
            
            // Notify UI
            OnCooldownChanged?.Invoke(1f);
            OnSuperpowerReady?.Invoke(true);
            
            if (debugMode)
            {
                UnityEngine.Debug.Log($"Powerup {activePowerup.GetDisplayName()} ready!");
            }
        }
        
        private IEnumerator EffectDurationCoroutine()
        {
            yield return new WaitForSeconds(activePowerup.data.duration);
            EndEffect();
        }
        
        private void EndEffect()
        {
            if (!isEffectActive) return;
            
            isEffectActive = false;
            
            // Cleanup in powerup
            activePowerup?.OnEffectEnd(playerController);
            
            if (debugMode)
            {
                UnityEngine.Debug.Log($"Powerup effect ended: {activePowerup?.GetDisplayName()}");
            }
        }
        
        private void PlayActivationEffects()
        {
            if (activePowerup?.data == null) return;
            
            // Play sound effect
            if (activePowerup.data.activationSound != null)
            {
                AudioSource.PlayClipAtPoint(activePowerup.data.activationSound, transform.position);
            }
            
            // Show visual effect
            if (activePowerup.data.activationEffect != null)
            {
                GameObject effect = Instantiate(activePowerup.data.activationEffect, transform.position, transform.rotation);
                Destroy(effect, 3f); // Clean up after 3 seconds
            }
        }
        
        private void UpdateCooldownUI(float progress)
        {
            superpowerUI?.UpdateCooldown(progress);
        }
        
        private void UpdateReadyStateUI(bool isReady)
        {
            superpowerUI?.SetReady(isReady);
        }
        
        // Debug methods
        private void OnValidate()
        {
            if (activePowerup != null && superpowerUI != null)
            {
                superpowerUI.Initialize(activePowerup.data);
            }
        }
        
        [System.Serializable]
        public class SuperpowerUI
        {
            [Header("UI Components")]
            public UnityEngine.UI.Image iconImage;
            public UnityEngine.UI.Image cooldownOverlay;
            public UnityEngine.UI.Text nameText;
            public UnityEngine.UI.Button activateButton;
            
            [Header("Visual States")]
            public Color readyColor = Color.white;
            public Color cooldownColor = Color.gray;
            
            public void Initialize(SuperpowerData data)
            {
                if (data == null) return;
                
                if (iconImage != null && data.icon != null)
                {
                    iconImage.sprite = data.icon;
                }
                
                if (nameText != null)
                {
                    nameText.text = data.powerName;
                }
                
                SetReady(true);
            }
            
            public void UpdateCooldown(float progress)
            {
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.fillAmount = 1f - progress;
                }
            }
            
            public void SetReady(bool isReady)
            {
                if (iconImage != null)
                {
                    iconImage.color = isReady ? readyColor : cooldownColor;
                }
                
                if (activateButton != null)
                {
                    activateButton.interactable = isReady;
                }
                
                if (cooldownOverlay != null && isReady)
                {
                    cooldownOverlay.fillAmount = 0f;
                }
            }
        }
    }
}
