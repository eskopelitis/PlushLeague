using UnityEngine;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Metadata for superpowers - holds configuration data
    /// </summary>
    [CreateAssetMenu(fileName = "SuperpowerData", menuName = "Plush League/Superpowers/Superpower Data")]
    public class SuperpowerData : ScriptableObject
    {
        [Header("Basic Info")]
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        
        [Header("Configuration")]
        public float cooldownDuration = 10f;
        public float duration = 0f; // Effect duration (0 = instant)
        public SuperpowerRole role = SuperpowerRole.Universal;
        
        [Header("Audio/Visual")]
        public AudioClip activationSound;
        public GameObject activationEffect;
        
        [Header("Power Reference")]
        public BasePowerup powerupScript;
        
        // Legacy property names for compatibility
        public string powerName => displayName;
        public float cooldownTime => cooldownDuration;
        
        /// <summary>
        /// Check if this superpower can be activated
        /// </summary>
        public bool CanActivate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (powerupScript == null)
                return false;
                
            return powerupScript.CanActivate(user);
        }
        
        /// <summary>
        /// Activate the superpower
        /// </summary>
        public void Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (powerupScript != null)
            {
                powerupScript.Activate(user);
            }
        }
    }
    
    public enum SuperpowerRole
    {
        Universal,
        Striker,
        Goalie
    }
}
