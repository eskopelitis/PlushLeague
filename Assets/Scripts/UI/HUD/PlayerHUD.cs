using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// Individual player's HUD showing stamina, cooldowns, role icon, and action buttons
    /// Displays all player-specific information and ability states
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController player;
        [SerializeField] private bool isLocalPlayer = true;
        
        [Header("Stamina Display")]
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Color fullStaminaColor = Color.green;
        [SerializeField] private Color halfStaminaColor = Color.yellow;
        [SerializeField] private Color lowStaminaColor = Color.red;
        [SerializeField] private float lowStaminaThreshold = 0.3f;
        [SerializeField] private float halfStaminaThreshold = 0.6f;
        
        [Header("Action Buttons")]
        [SerializeField] private ActionButtonUI actionButton; // A button
        [SerializeField] private ActionButtonUI kickButton; // B button
        [SerializeField] private ActionButtonUI abilityButton; // X button
        [SerializeField] private ActionButtonUI superpowerButton; // Y button
        
        [Header("Player Status")]
        [SerializeField] private Image roleIcon;
        [SerializeField] private Sprite strikerIcon;
        [SerializeField] private Sprite goalieIcon;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private GameObject sprintIndicator;
        
        [Header("Visual Effects")]
        [SerializeField] private float staminaBarAnimSpeed = 2f;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private bool enablePulseEffects = true;
        
        // Runtime state
        private float targetStaminaValue = 1f;
        private float currentStaminaDisplay = 1f;
        private PlushLeague.Gameplay.Superpowers.PowerupController powerupController;
        private bool isInitialized = false;
        
        // Player role enum
        public enum PlayerRole
        {
            Striker,
            Goalkeeper
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            InitializeHUD();
        }
        
        private void Update()
        {
            if (isInitialized && player != null)
            {
                UpdateStaminaDisplay();
                UpdateActionButtons();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize HUD components and find references
        /// </summary>
        private void InitializeComponents()
        {
            // Auto-find player if not assigned
            if (player == null)
            {
                player = GetComponentInParent<PlushLeague.Gameplay.Player.PlayerController>();
            }
            
            // Get powerup controller
            if (player != null)
            {
                powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
            }
            
            // Configure HUD visibility based on whether this is the local player
            ConfigureForLocalPlayer();
        }
        
        /// <summary>
        /// Configure HUD elements based on local/remote player status
        /// </summary>
        private void ConfigureForLocalPlayer()
        {
            // Only show detailed HUD elements for the local player
            if (!isLocalPlayer)
            {
                // Hide action buttons for remote players
                if (abilityButton != null)
                    abilityButton.gameObject.SetActive(false);
                if (superpowerButton != null)
                    superpowerButton.gameObject.SetActive(false);
                    
                // Could also reduce update frequency for remote players
                // or show simplified information
            }
            
            // Local players get full HUD with action buttons and detailed feedback
            UnityEngine.Debug.Log($"PlayerHUD configured for {(isLocalPlayer ? "local" : "remote")} player");
        }
        
        /// <summary>
        /// Initialize HUD and subscribe to events
        /// </summary>
        private void InitializeHUD()
        {
            if (player == null)
            {
                UnityEngine.Debug.LogWarning("PlayerHUD: No player assigned!");
                return;
            }
            
            // Subscribe to player events
            player.OnStaminaChanged += OnStaminaChanged;
            player.OnSprintStateChanged += OnSprintStateChanged;
            
            // Initialize stamina display
            OnStaminaChanged(player.GetCurrentStamina(), player.GetMaxStamina());
            
            // Set player name
            if (playerNameText != null)
            {
                playerNameText.text = player.name;
            }
            
            // Set role icon (simple logic - can be enhanced)
            SetPlayerRole(DeterminePlayerRole());
            
            // Initialize action buttons
            InitializeActionButtons();
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Initialize action button references and states
        /// </summary>
        private void InitializeActionButtons()
        {
            // Initialize each button with appropriate settings
            if (actionButton != null)
                actionButton.Initialize("Action", KeyCode.Space);
                
            if (kickButton != null)
                kickButton.Initialize("Kick", KeyCode.B);
                
            if (abilityButton != null)
                abilityButton.Initialize("Ability", KeyCode.X);
                
            if (superpowerButton != null)
                superpowerButton.Initialize("Superpower", KeyCode.Y);
        }
        
        /// <summary>
        /// Determine player role based on position or other criteria
        /// </summary>
        private PlayerRole DeterminePlayerRole()
        {
            // Simple logic - can be enhanced with actual role assignment
            if (player.name.ToLower().Contains("goalie") || player.name.ToLower().Contains("keeper"))
                return PlayerRole.Goalkeeper;
                
            return PlayerRole.Striker;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set the player this HUD represents
        /// </summary>
        public void SetPlayer(PlushLeague.Gameplay.Player.PlayerController newPlayer)
        {
            // Unsubscribe from old player
            if (player != null)
            {
                player.OnStaminaChanged -= OnStaminaChanged;
                player.OnSprintStateChanged -= OnSprintStateChanged;
            }
            
            // Set new player
            player = newPlayer;
            
            if (player != null)
            {
                powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
                InitializeHUD();
            }
        }
        
        /// <summary>
        /// Get the player this HUD represents
        /// </summary>
        public PlushLeague.Gameplay.Player.PlayerController GetPlayer()
        {
            return player;
        }
        
        /// <summary>
        /// Set player role and update icon
        /// </summary>
        public void SetPlayerRole(PlayerRole role)
        {
            if (roleIcon == null) return;
            
            switch (role)
            {
                case PlayerRole.Striker:
                    roleIcon.sprite = strikerIcon;
                    roleIcon.color = Color.yellow;
                    break;
                case PlayerRole.Goalkeeper:
                    roleIcon.sprite = goalieIcon;
                    roleIcon.color = Color.blue;
                    break;
            }
        }
        
        /// <summary>
        /// Update stamina bar with specified values
        /// </summary>
        public void UpdateStamina(float current, float max)
        {
            targetStaminaValue = max > 0 ? current / max : 0f;
        }
        
        /// <summary>
        /// Set cooldown for specific action button
        /// </summary>
        public void SetCooldown(string actionName, float cooldownTime, float maxCooldown)
        {
            ActionButtonUI button = GetActionButton(actionName);
            if (button != null)
            {
                button.SetCooldown(cooldownTime, maxCooldown);
            }
        }
        
        /// <summary>
        /// Flash action button when used
        /// </summary>
        public void FlashActionButton(string actionName)
        {
            ActionButtonUI button = GetActionButton(actionName);
            if (button != null)
            {
                button.FlashUsage();
            }
        }
        
        /// <summary>
        /// Set action button availability
        /// </summary>
        public void SetActionAvailable(string actionName, bool available)
        {
            ActionButtonUI button = GetActionButton(actionName);
            if (button != null)
            {
                button.SetAvailable(available);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle stamina change events from player
        /// </summary>
        private void OnStaminaChanged(float current, float max)
        {
            UpdateStamina(current, max);
        }
        
        /// <summary>
        /// Handle sprint state changes
        /// </summary>
        private void OnSprintStateChanged(bool isSprinting)
        {
            if (sprintIndicator != null)
            {
                sprintIndicator.SetActive(isSprinting);
            }
        }
        
        #endregion
        
        #region Update Methods
        
        /// <summary>
        /// Update stamina bar display with smooth animation
        /// </summary>
        private void UpdateStaminaDisplay()
        {
            if (staminaBar == null) return;
            
            // Smooth stamina bar animation
            currentStaminaDisplay = Mathf.Lerp(currentStaminaDisplay, targetStaminaValue, Time.deltaTime * staminaBarAnimSpeed);
            staminaBar.value = currentStaminaDisplay;
            
            // Update stamina color based on current level
            UpdateStaminaColor();
            
            // Pulse effect for low stamina
            if (enablePulseEffects && targetStaminaValue < lowStaminaThreshold)
            {
                ApplyLowStaminaPulse();
            }
        }
        
        /// <summary>
        /// Update stamina bar color based on current stamina level
        /// </summary>
        private void UpdateStaminaColor()
        {
            if (staminaFill == null) return;
            
            Color targetColor;
            if (targetStaminaValue < lowStaminaThreshold)
                targetColor = lowStaminaColor;
            else if (targetStaminaValue < halfStaminaThreshold)
                targetColor = halfStaminaColor;
            else
                targetColor = fullStaminaColor;
                
            staminaFill.color = Color.Lerp(staminaFill.color, targetColor, Time.deltaTime * 2f);
        }
        
        /// <summary>
        /// Apply pulsing effect for low stamina warning
        /// </summary>
        private void ApplyLowStaminaPulse()
        {
            if (staminaFill == null) return;
            
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
            Color currentColor = staminaFill.color;
            currentColor.a = pulse;
            staminaFill.color = currentColor;
        }
        
        /// <summary>
        /// Update all action buttons with current states
        /// </summary>
        private void UpdateActionButtons()
        {
            if (powerupController != null && superpowerButton != null)
            {
                // Update superpower button
                float cooldownRemaining = powerupController.GetCooldownRemaining();
                float maxCooldown = powerupController.GetMaxCooldown();
                
                if (cooldownRemaining > 0)
                {
                    superpowerButton.SetCooldown(cooldownRemaining, maxCooldown);
                }
                else
                {
                    superpowerButton.SetAvailable(powerupController.CanUsePowerup());
                }
            }
            
            // Update other action buttons based on player state
            UpdateBasicActionButtons();
        }
        
        /// <summary>
        /// Update basic action buttons (kick, ability, etc.)
        /// </summary>
        private void UpdateBasicActionButtons()
        {
            // Update kick button availability
            if (kickButton != null)
            {
                bool canKick = player.HasBall();
                kickButton.SetAvailable(canKick);
            }
            
            // Update action button (context-sensitive)
            if (actionButton != null)
            {
                bool hasAction = player.HasBall() || player.GetNearbyBall() != null;
                actionButton.SetAvailable(hasAction);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get action button by name
        /// </summary>
        private ActionButtonUI GetActionButton(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "action":
                case "a":
                    return actionButton;
                case "kick":
                case "b":
                    return kickButton;
                case "ability":
                case "x":
                    return abilityButton;
                case "superpower":
                case "y":
                    return superpowerButton;
                default:
                    return null;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (player != null)
            {
                player.OnStaminaChanged -= OnStaminaChanged;
                player.OnSprintStateChanged -= OnSprintStateChanged;
            }
        }
        
        #endregion
    }
}
