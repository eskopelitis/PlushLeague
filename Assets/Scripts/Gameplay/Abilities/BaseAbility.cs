using UnityEngine;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Base class for all player abilities (chip kick, tackle, etc.)
    /// Provides common functionality like cooldowns and input handling
    /// </summary>
    [System.Serializable]
    public abstract class BaseAbility : ScriptableObject
    {
        [Header("Base Ability Settings")]
        [SerializeField] protected string abilityName = "Base Ability";
        [SerializeField] protected string description = "Base ability description";
        [SerializeField] protected float cooldownTime = 1.0f;
        [SerializeField] protected bool requiresBallPossession = false;
        [SerializeField] protected bool consumesStamina = false;
        [SerializeField] protected float staminaCost = 0f;
        
        [Header("Audio")]
        [SerializeField] protected AudioClip abilitySound;
        [SerializeField] protected float soundVolume = 1f;
        
        // Runtime state
        protected float lastUsedTime = -999f;
        protected bool isOnCooldown = false;
        
        // Properties
        public string AbilityName => abilityName;
        public string Description => description;
        public float CooldownTime => cooldownTime;
        public bool RequiresBallPossession => requiresBallPossession;
        public bool IsOnCooldown => Time.time < lastUsedTime + cooldownTime;
        public float CooldownRemaining => Mathf.Max(0f, lastUsedTime + cooldownTime - Time.time);
        public float CooldownProgress => IsOnCooldown ? (Time.time - lastUsedTime) / cooldownTime : 1f;
        
        /// <summary>
        /// Check if ability can be used by the given player
        /// </summary>
        /// <param name="player">Player attempting to use ability</param>
        /// <returns>True if ability can be used</returns>
        public virtual bool CanUse(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Check cooldown
            if (IsOnCooldown)
            {
                return false;
            }
            
            // Check ball possession requirement
            if (requiresBallPossession && !player.HasBall())
            {
                return false;
            }
            
            // Check stamina requirement
            if (consumesStamina && player.GetStaminaPercentage() * 100f < staminaCost)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Use the ability
        /// </summary>
        /// <param name="player">Player using the ability</param>
        /// <param name="input">Input direction/parameters</param>
        /// <returns>True if ability was successfully used</returns>
        public bool Use(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input = default)
        {
            if (!CanUse(player))
            {
                return false;
            }
            
            // Consume stamina if required
            if (consumesStamina)
            {
                // Note: This would require adding a ConsumeStamina method to PlayerController
                // player.ConsumeStamina(staminaCost);
            }
            
            // Execute ability
            bool success = ExecuteAbility(player, input);
            
            if (success)
            {
                // Start cooldown
                lastUsedTime = Time.time;
                
                // Play sound
                PlayAbilitySound(player);
                
                // Trigger any additional effects
                OnAbilityUsed(player, input);
            }
            
            return success;
        }
        
        /// <summary>
        /// Abstract method for ability implementation
        /// </summary>
        /// <param name="player">Player using ability</param>
        /// <param name="input">Input parameters</param>
        /// <returns>True if ability executed successfully</returns>
        protected abstract bool ExecuteAbility(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input);
        
        /// <summary>
        /// Called when ability is successfully used (for additional effects)
        /// </summary>
        /// <param name="player">Player who used ability</param>
        /// <param name="input">Input parameters</param>
        protected virtual void OnAbilityUsed(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            // Override in derived classes for additional effects
        }
        
        /// <summary>
        /// Play ability sound effect
        /// </summary>
        /// <param name="player">Player using ability</param>
        protected virtual void PlayAbilitySound(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (abilitySound == null) return;
            
            // Try to get AudioSource from player
            var audioSource = player.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(abilitySound, soundVolume);
            }
            else
            {
                // Use AudioSource.PlayClipAtPoint as fallback
                AudioSource.PlayClipAtPoint(abilitySound, player.transform.position, soundVolume);
            }
        }
        
        /// <summary>
        /// Reset cooldown (for debugging or special cases)
        /// </summary>
        public virtual void ResetCooldown()
        {
            lastUsedTime = -999f;
        }
    }
}
