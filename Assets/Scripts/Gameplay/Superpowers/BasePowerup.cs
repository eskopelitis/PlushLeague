using UnityEngine;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Base class for all superpower effects
    /// Each specific superpower inherits from this and implements its unique behavior
    /// </summary>
    public abstract class BasePowerup : ScriptableObject
    {
        [Header("Powerup Data")]
        public SuperpowerData data;
        
        /// <summary>
        /// Activate the superpower effect
        /// </summary>
        /// <param name="user">The player using the superpower</param>
        /// <returns>True if activation was successful</returns>
        public abstract bool Activate(PlushLeague.Gameplay.Player.PlayerController user);
        
        /// <summary>
        /// Called when the powerup effect ends (for cleanup)
        /// </summary>
        /// <param name="user">The player who used the superpower</param>
        public virtual void OnEffectEnd(PlushLeague.Gameplay.Player.PlayerController user)
        {
            // Override in derived classes if cleanup is needed
        }
        
        /// <summary>
        /// Check if the powerup can be activated
        /// </summary>
        /// <param name="user">The player trying to use the superpower</param>
        /// <returns>True if the powerup can be used</returns>
        public virtual bool CanActivate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (data == null) return false;
            
            // Basic checks - derived classes can add more specific conditions
            return user != null && user.gameObject.activeInHierarchy;
        }
        
        /// <summary>
        /// Get the display name for UI
        /// </summary>
        public virtual string GetDisplayName()
        {
            return data != null ? data.powerName : "Unknown Power";
        }
        
        /// <summary>
        /// Get the description for UI
        /// </summary>
        public virtual string GetDescription()
        {
            return data != null ? data.description : "No description available";
        }
    }
}
