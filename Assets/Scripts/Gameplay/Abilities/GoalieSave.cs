using UnityEngine;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Goalie Save ability - handles A button press during save opportunities
    /// Only active when player is designated as goalie and shot is detected
    /// </summary>
    [CreateAssetMenu(fileName = "GoalieSave", menuName = "Plush League/Abilities/Goalie Save")]
    public class GoalieSave : BaseAbility
    {
        [Header("Goalie Save Settings")]
        [SerializeField] private float maxGoalieDistance = 8.0f;
        [SerializeField] private bool onlyActiveOnShots = true;
        [SerializeField] private LayerMask goalLayerMask = 1;
        
        [Header("Save Effects")]
        [SerializeField] private float perfectSaveStaminaBonus = 50f;
        [SerializeField] private float saveAnimationDuration = 1.0f;
        
        private PlushLeague.Gameplay.Goal.GoalieSaveManager saveManager;
        private bool isGoalieContext = false;
        
        private void Awake()
        {
            // Find save manager
            saveManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Goal.GoalieSaveManager>();
        }
        
        public override bool CanUse(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Only usable during save opportunities
            if (onlyActiveOnShots)
            {
                if (saveManager == null || !saveManager.IsSaveInProgress)
                {
                    return false;
                }
                
                // Only the current goalie can use this
                if (saveManager.CurrentGoalie != player)
                {
                    return false;
                }
            }
            
            // Check if player is in goalie position
            if (!IsPlayerInGoaliePosition(player))
            {
                return false;
            }
            
            return base.CanUse(player);
        }
        
        protected override bool ExecuteAbility(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            // Trigger save input in save manager
            if (saveManager != null)
            {
                saveManager.TriggerSaveInput();
                
                // Log save animation duration for debugging
                UnityEngine.Debug.Log($"Goalie save triggered for {player.name} (animation duration: {saveAnimationDuration}s)");
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if player is in position to be a goalie
        /// </summary>
        private bool IsPlayerInGoaliePosition(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Find nearest goal (simple implementation)
            var goals = Object.FindObjectsByType<PlushLeague.Gameplay.Goal.ShotDetector>(FindObjectsSortMode.None);
            
            foreach (var goal in goals)
            {
                if (goal == null) continue;
                
                float distance = Vector3.Distance(player.transform.position, goal.transform.position);
                if (distance <= maxGoalieDistance)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Set whether this ability should only be active during shots
        /// </summary>
        public void SetShotOnlyMode(bool shotOnly)
        {
            onlyActiveOnShots = shotOnly;
        }
        
        /// <summary>
        /// Check if player is currently the active goalie
        /// </summary>
        public bool IsActiveGoalie(PlushLeague.Gameplay.Player.PlayerController player)
        {
            return saveManager != null && saveManager.CurrentGoalie == player;
        }
        
        protected override void OnAbilityUsed(PlushLeague.Gameplay.Player.PlayerController player, Vector2 input)
        {
            base.OnAbilityUsed(player, input);
            
            UnityEngine.Debug.Log($"Goalie {player.name} attempted save");
        }
        
        /// <summary>
        /// Force enable goalie context (for testing)
        /// </summary>
        public void SetGoalieContext(bool enabled)
        {
            isGoalieContext = enabled;
        }
        
        /// <summary>
        /// Get status text for UI display
        /// </summary>
        public string GetStatusText(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (!IsPlayerInGoaliePosition(player))
            {
                return "Not in goal area";
            }
            
            if (onlyActiveOnShots && (saveManager == null || !saveManager.IsSaveInProgress))
            {
                return "Waiting for shot";
            }
            
            if (saveManager != null && saveManager.CurrentGoalie != player)
            {
                return "Not active goalie";
            }
            
            return "Ready to save";
        }
        
        /// <summary>
        /// Apply stamina bonus for perfect saves
        /// </summary>
        public void ApplyPerfectSaveBonus(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // This could be called by the save manager when a perfect save is detected
            if (player != null)
            {
                // Add stamina bonus - this would require a stamina system in PlayerController
                UnityEngine.Debug.Log($"Perfect save! Applying {perfectSaveStaminaBonus} stamina bonus to {player.name}");
                
                // Future implementation: player.AddStamina(perfectSaveStaminaBonus);
            }
        }
    }
}
