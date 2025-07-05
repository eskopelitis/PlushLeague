using UnityEngine;
using System.Collections.Generic;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Manages player abilities, cooldowns, and input handling
    /// Attached to player objects to handle ability usage
    /// </summary>
    public class AbilityManager : MonoBehaviour
    {
        [Header("Abilities")]
        [SerializeField] private List<BaseAbility> availableAbilities = new List<BaseAbility>();
        [SerializeField] private ChipKick chipKickAbility;
        [SerializeField] private BaseAbility slideTackleAbility;
        [SerializeField] private BaseAbility goalieSaveAbility;
        
        [Header("Input Mapping")]
        [SerializeField] private KeyCode chipKickKey = KeyCode.X;
        [SerializeField] private KeyCode slideTackleKey = KeyCode.B;
        [SerializeField] private KeyCode goalieSaveKey = KeyCode.A;
        [SerializeField] private bool useInputSystem = true;
        
        [Header("UI")]
        [SerializeField] private bool showCooldownUI = true;
        [SerializeField] private GameObject abilityUIPrefab;
        
        private PlushLeague.Gameplay.Player.PlayerController playerController;
        private Dictionary<System.Type, BaseAbility> abilityLookup;
        private Canvas uiCanvas;
        
        // Input state
        private bool chipKickPressed = false;
        private bool slideTacklePressed = false;
        private bool goalieSavePressed = false;
        
        // Events
        public System.Action<BaseAbility> OnAbilityUsed;
        public System.Action<BaseAbility, float> OnAbilityCooldownChanged;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            playerController = GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
            InitializeAbilities();
        }
        
        private void Start()
        {
            SetupUI();
        }
        
        private void Update()
        {
            HandleInput();
            UpdateCooldowns();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeAbilities()
        {
            abilityLookup = new Dictionary<System.Type, BaseAbility>();
            
            // Add chip kick if assigned
            if (chipKickAbility != null)
            {
                if (!availableAbilities.Contains(chipKickAbility))
                {
                    availableAbilities.Add(chipKickAbility);
                }
            }
            
            // Add slide tackle if assigned
            if (slideTackleAbility != null)
            {
                if (!availableAbilities.Contains(slideTackleAbility))
                {
                    availableAbilities.Add(slideTackleAbility);
                }
            }
            
            // Add goalie save if assigned
            if (goalieSaveAbility != null)
            {
                if (!availableAbilities.Contains(goalieSaveAbility))
                {
                    availableAbilities.Add(goalieSaveAbility);
                }
            }
            foreach (var ability in availableAbilities)
            {
                if (ability != null)
                {
                    abilityLookup[ability.GetType()] = ability;
                }
            }
            
            UnityEngine.Debug.Log($"AbilityManager: Initialized {availableAbilities.Count} abilities for {gameObject.name}");
        }
        
        private void SetupUI()
        {
            if (!showCooldownUI || abilityUIPrefab == null) return;
            
            // Find or create UI canvas
            uiCanvas = Object.FindFirstObjectByType<Canvas>();
            if (uiCanvas == null)
            {
                UnityEngine.Debug.LogWarning("AbilityManager: No Canvas found for UI display");
                return;
            }
            
            // Create ability UI elements (implementation would depend on UI design)
            // This is a placeholder for future UI integration
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleInput()
        {
            if (playerController == null) return;
            
            // Handle chip kick input
            HandleChipKickInput();
            
            // Handle slide tackle input
            HandleSlideTackleInput();
            
            // Handle goalie save input
            HandleGoalieSaveInput();
        }
        
        private void HandleChipKickInput()
        {
            bool chipKickInput = false;
            
            if (useInputSystem)
            {
                // This would integrate with the input system
                // For now, fall back to direct key input
                chipKickInput = Input.GetKeyDown(chipKickKey);
            }
            else
            {
                chipKickInput = Input.GetKeyDown(chipKickKey);
            }
            
            if (chipKickInput && !chipKickPressed)
            {
                chipKickPressed = true;
                TryUseChipKick();
            }
            else if (!chipKickInput)
            {
                chipKickPressed = false;
            }
        }
        
        /// <summary>
        /// Set chip kick input from external input system
        /// </summary>
        /// <param name="pressed">Whether chip kick button was pressed</param>
        public void SetChipKickInput(bool pressed)
        {
            if (pressed && !chipKickPressed)
            {
                chipKickPressed = true;
                TryUseChipKick();
            }
            else if (!pressed)
            {
                chipKickPressed = false;
            }
        }
        
        private void HandleSlideTackleInput()
        {
            bool slideTackleInput = false;
            
            if (useInputSystem)
            {
                // This would integrate with the input system
                // For now, fall back to direct key input
                slideTackleInput = Input.GetKeyDown(slideTackleKey);
            }
            else
            {
                slideTackleInput = Input.GetKeyDown(slideTackleKey);
            }
            
            if (slideTackleInput && !slideTacklePressed)
            {
                slideTacklePressed = true;
                TryUseSlideTackle();
            }
            else if (!slideTackleInput)
            {
                slideTacklePressed = false;
            }
        }
        
        /// <summary>
        /// Set slide tackle input from external input system
        /// </summary>
        /// <param name="pressed">Whether slide tackle button was pressed</param>
        public void SetSlideTackleInput(bool pressed)
        {
            if (pressed && !slideTacklePressed)
            {
                slideTacklePressed = true;
                TryUseSlideTackle();
            }
            else if (!pressed)
            {
                slideTacklePressed = false;
            }
        }
        
        private void HandleGoalieSaveInput()
        {
            bool goalieSaveInput = false;
            
            if (useInputSystem)
            {
                // This would integrate with the input system
                // For now, fall back to direct key input
                goalieSaveInput = Input.GetKeyDown(goalieSaveKey);
            }
            else
            {
                goalieSaveInput = Input.GetKeyDown(goalieSaveKey);
            }
            
            if (goalieSaveInput && !goalieSavePressed)
            {
                goalieSavePressed = true;
                TryUseGoalieSave();
            }
            else if (!goalieSaveInput)
            {
                goalieSavePressed = false;
            }
        }
        
        /// <summary>
        /// Set goalie save input from external input system
        /// </summary>
        /// <param name="pressed">Whether goalie save button was pressed</param>
        public void SetGoalieSaveInput(bool pressed)
        {
            if (pressed && !goalieSavePressed)
            {
                goalieSavePressed = true;
                TryUseGoalieSave();
            }
            else if (!pressed)
            {
                goalieSavePressed = false;
            }
        }
        
        #endregion
        
        #region Ability Usage
        
        /// <summary>
        /// Attempt to use chip kick ability
        /// </summary>
        private void TryUseChipKick()
        {
            if (chipKickAbility == null)
            {
                UnityEngine.Debug.LogWarning("AbilityManager: Chip kick ability not assigned");
                return;
            }
            
            // Get movement input for kick direction
            Vector2 kickDirection = GetKickDirection();
            
            // Try to use the ability
            bool success = chipKickAbility.Use(playerController, kickDirection);
            
            if (success)
            {
                OnAbilityUsed?.Invoke(chipKickAbility);
                UnityEngine.Debug.Log($"Player {playerController.name} used chip kick");
            }
            else
            {
                // Provide feedback for why ability couldn't be used
                string reason = GetAbilityFailureReason(chipKickAbility);
                UnityEngine.Debug.Log($"Chip kick failed: {reason}");
                
                // Could trigger UI feedback here (red flash, sound, etc.)
            }
        }
        
        /// <summary>
        /// Attempt to use slide tackle ability
        /// </summary>
        private void TryUseSlideTackle()
        {
            if (slideTackleAbility == null)
            {
                UnityEngine.Debug.LogWarning("AbilityManager: Slide tackle ability not assigned");
                return;
            }
            
            // Get movement input for tackle direction
            Vector2 tackleDirection = GetTackleDirection();
            
            // Try to use the ability
            bool success = slideTackleAbility.Use(playerController, tackleDirection);
            
            if (success)
            {
                OnAbilityUsed?.Invoke(slideTackleAbility);
                UnityEngine.Debug.Log($"Player {playerController.name} used slide tackle");
            }
            else
            {
                // Provide feedback for why ability couldn't be used
                string reason = GetAbilityFailureReason(slideTackleAbility);
                UnityEngine.Debug.Log($"Slide tackle failed: {reason}");
                
                // Could trigger UI feedback here (red flash, sound, etc.)
            }
        }
        
        /// <summary>
        /// Attempt to use goalie save ability
        /// </summary>
        private void TryUseGoalieSave()
        {
            if (goalieSaveAbility == null)
            {
                UnityEngine.Debug.LogWarning("AbilityManager: Goalie save ability not assigned");
                return;
            }
            
            // Get save input direction (usually not needed for goalie saves)
            Vector2 saveDirection = Vector2.zero;
            
            // Try to use the ability
            bool success = goalieSaveAbility.Use(playerController, saveDirection);
            
            if (success)
            {
                OnAbilityUsed?.Invoke(goalieSaveAbility);
                UnityEngine.Debug.Log($"Player {playerController.name} attempted goalie save");
            }
            else
            {
                // Provide feedback for why ability couldn't be used
                string reason = GetAbilityFailureReason(goalieSaveAbility);
                UnityEngine.Debug.Log($"Goalie save failed: {reason}");
                
                // Could trigger UI feedback here (red flash, sound, etc.)
            }
        }
        
        /// <summary>
        /// Use specific ability by type
        /// </summary>
        /// <typeparam name="T">Ability type</typeparam>
        /// <param name="input">Input parameters</param>
        /// <returns>True if ability was used successfully</returns>
        public bool UseAbility<T>(Vector2 input = default) where T : BaseAbility
        {
            if (abilityLookup.TryGetValue(typeof(T), out BaseAbility ability))
            {
                bool success = ability.Use(playerController, input);
                if (success)
                {
                    OnAbilityUsed?.Invoke(ability);
                }
                return success;
            }
            return false;
        }
        
        /// <summary>
        /// Check if specific ability can be used
        /// </summary>
        /// <typeparam name="T">Ability type</typeparam>
        /// <returns>True if ability can be used</returns>
        public bool CanUseAbility<T>() where T : BaseAbility
        {
            if (abilityLookup.TryGetValue(typeof(T), out BaseAbility ability))
            {
                return ability.CanUse(playerController);
            }
            return false;
        }
        
        /// <summary>
        /// Get ability cooldown progress
        /// </summary>
        /// <typeparam name="T">Ability type</typeparam>
        /// <returns>Cooldown progress (0-1)</returns>
        public float GetAbilityCooldownProgress<T>() where T : BaseAbility
        {
            if (abilityLookup.TryGetValue(typeof(T), out BaseAbility ability))
            {
                return ability.CooldownProgress;
            }
            return 1f;
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Get current kick direction based on player state
        /// </summary>
        /// <returns>Normalized direction vector</returns>
        private Vector2 GetKickDirection()
        {
            // Try to get movement input from player controller
            // This would require exposing current movement input
            // For now, use player's facing direction
            return playerController.transform.up;
        }
        
        /// <summary>
        /// Get current tackle direction based on player state
        /// </summary>
        /// <returns>Normalized direction vector</returns>
        private Vector2 GetTackleDirection()
        {
            // Try to get movement input from player controller
            // This would require exposing current movement input
            // For now, use player's facing direction
            return playerController.transform.up;
        }
        
        /// <summary>
        /// Get reason why ability couldn't be used (for UI feedback)
        /// </summary>
        /// <param name="ability">Ability that failed</param>
        /// <returns>Human-readable failure reason</returns>
        private string GetAbilityFailureReason(BaseAbility ability)
        {
            if (ability.IsOnCooldown)
            {
                return $"On cooldown ({ability.CooldownRemaining:F1}s remaining)";
            }
            
            if (ability.RequiresBallPossession && !playerController.HasBall())
            {
                return "Need ball possession";
            }
            
            if (ability.GetType() == typeof(ChipKick))
            {
                // Specific chip kick checks
                if (!playerController.HasBall())
                {
                    return "No ball to chip";
                }
            }
            else if (ability.GetType().Name == "SlideTackle")
            {
                // Specific slide tackle checks
                if (playerController.IsStunned())
                {
                    return "Cannot tackle while stunned";
                }
            }
            
            return "Cannot use ability";
        }
        
        private void UpdateCooldowns()
        {
            // Update UI and notify listeners of cooldown changes
            foreach (var ability in availableAbilities)
            {
                if (ability != null && ability.IsOnCooldown)
                {
                    OnAbilityCooldownChanged?.Invoke(ability, ability.CooldownProgress);
                }
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Add a new ability to the player
        /// </summary>
        /// <param name="ability">Ability to add</param>
        public void AddAbility(BaseAbility ability)
        {
            if (ability != null && !availableAbilities.Contains(ability))
            {
                availableAbilities.Add(ability);
                abilityLookup[ability.GetType()] = ability;
            }
        }
        
        /// <summary>
        /// Remove an ability from the player
        /// </summary>
        /// <param name="ability">Ability to remove</param>
        public void RemoveAbility(BaseAbility ability)
        {
            if (ability != null)
            {
                availableAbilities.Remove(ability);
                abilityLookup.Remove(ability.GetType());
            }
        }
        
        /// <summary>
        /// Get all available abilities
        /// </summary>
        /// <returns>List of available abilities</returns>
        public List<BaseAbility> GetAvailableAbilities()
        {
            return new List<BaseAbility>(availableAbilities);
        }
        
        /// <summary>
        /// Reset all ability cooldowns (for debugging)
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var ability in availableAbilities)
            {
                ability?.ResetCooldown();
            }
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying || availableAbilities.Count == 0) return;
            
            // Debug UI for abilities
            GUILayout.BeginArea(new Rect(10, 200, 250, 250));
            GUILayout.Label("=== Abilities Debug ===");
            
            foreach (var ability in availableAbilities)
            {
                if (ability == null) continue;
                
                string status = ability.CanUse(playerController) ? "READY" : "NOT READY";
                string cooldownText = ability.IsOnCooldown ? $" ({ability.CooldownRemaining:F1}s)" : "";
                
                GUILayout.Label($"{ability.AbilityName}: {status}{cooldownText}");
                
                if (ability.GetType() == typeof(ChipKick))
                {
                    GUILayout.Label($"  Has Ball: {playerController.HasBall()}");
                    GUILayout.Label($"  Press {chipKickKey} to use");
                }
                else if (ability.GetType().Name == "SlideTackle")
                {
                    GUILayout.Label($"  Is Stunned: {playerController.IsStunned()}");
                    GUILayout.Label($"  Press {slideTackleKey} to use");
                }
                else if (ability.GetType().Name == "GoalieSave")
                {
                    GUILayout.Label($"  In Goal Area: {ability.CanUse(playerController)}");
                    GUILayout.Label($"  Press {goalieSaveKey} to use");
                }
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
