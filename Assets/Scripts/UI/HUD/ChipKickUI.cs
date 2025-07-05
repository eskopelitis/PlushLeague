using UnityEngine;
using UnityEngine.UI;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// UI component for displaying chip kick cooldown and availability
    /// Shows visual feedback for the chip kick ability
    /// </summary>
    public class ChipKickUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button chipKickButton;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text cooldownText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color unavailableColor = Color.red;
        [SerializeField] private float pulseSpeed = 2f;
        
        [Header("Animation")]
        [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private float usageFlashDuration = 0.2f;
        
        private PlushLeague.Gameplay.Abilities.AbilityManager abilityManager;
        private PlushLeague.Gameplay.Abilities.ChipKick chipKickAbility;
        private bool isOnCooldown = false;
        private float pulseTimer = 0f;
        private Coroutine flashCoroutine;
        
        private void Start()
        {
            InitializeUI();
            FindAbilityManager();
        }
        
        private void Update()
        {
            UpdateCooldownDisplay();
            UpdateAvailabilityIndicator();
        }
        
        #region Initialization
        
        private void InitializeUI()
        {
            // Set up button click handler
            if (chipKickButton != null)
            {
                chipKickButton.onClick.AddListener(OnChipKickButtonClicked);
            }
            
            // Initialize UI elements
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
            }
            
            if (cooldownText != null)
            {
                cooldownText.text = "";
            }
        }
        
        private void FindAbilityManager()
        {
            // Find ability manager in scene (could be passed as reference instead)
            abilityManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Abilities.AbilityManager>();
            
            if (abilityManager != null)
            {
                // Subscribe to ability events
                abilityManager.OnAbilityUsed += OnAbilityUsed;
                abilityManager.OnAbilityCooldownChanged += OnAbilityCooldownChanged;
                
                // Get chip kick ability reference
                var abilities = abilityManager.GetAvailableAbilities();
                foreach (var ability in abilities)
                {
                    if (ability is PlushLeague.Gameplay.Abilities.ChipKick chipKick)
                    {
                        chipKickAbility = chipKick;
                        break;
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("ChipKickUI: No AbilityManager found in scene");
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateCooldownDisplay()
        {
            if (chipKickAbility == null) return;
            
            bool newCooldownState = chipKickAbility.IsOnCooldown;
            
            // Update cooldown fill
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 1f - chipKickAbility.CooldownProgress;
            }
            
            // Update cooldown text
            if (cooldownText != null)
            {
                if (newCooldownState)
                {
                    cooldownText.text = chipKickAbility.CooldownRemaining.ToString("F1");
                }
                else
                {
                    cooldownText.text = "";
                }
            }
            
            isOnCooldown = newCooldownState;
        }
        
        private void UpdateAvailabilityIndicator()
        {
            if (chipKickAbility == null || abilityManager == null) return;
            
            bool canUse = abilityManager.CanUseAbility<PlushLeague.Gameplay.Abilities.ChipKick>();
            Color targetColor;
            
            if (canUse)
            {
                targetColor = availableColor;
                
                // Pulse effect when available
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulseValue = pulseCurve.Evaluate(Mathf.PingPong(pulseTimer, 1f));
                targetColor = Color.Lerp(availableColor, Color.white, pulseValue * 0.3f);
            }
            else if (isOnCooldown)
            {
                targetColor = cooldownColor;
            }
            else
            {
                targetColor = unavailableColor;
            }
            
            // Apply color to icon
            if (iconImage != null)
            {
                iconImage.color = targetColor;
            }
            
            // Update button interactability
            if (chipKickButton != null)
            {
                chipKickButton.interactable = canUse;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnChipKickButtonClicked()
        {
            if (abilityManager != null)
            {
                abilityManager.SetChipKickInput(true);
            }
        }
        
        private void OnAbilityUsed(PlushLeague.Gameplay.Abilities.BaseAbility ability)
        {
            if (ability is PlushLeague.Gameplay.Abilities.ChipKick)
            {
                TriggerUsageFlash();
            }
        }
        
        private void OnAbilityCooldownChanged(PlushLeague.Gameplay.Abilities.BaseAbility ability, float progress)
        {
            // This is handled in UpdateCooldownDisplay
        }
        
        #endregion
        
        #region Visual Effects
        
        private void TriggerUsageFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            
            flashCoroutine = StartCoroutine(UsageFlashCoroutine());
        }
        
        private System.Collections.IEnumerator UsageFlashCoroutine()
        {
            float elapsed = 0f;
            Color originalColor = iconImage != null ? iconImage.color : Color.white;
            
            while (elapsed < usageFlashDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / usageFlashDuration;
                
                // Flash white then back to original
                Color flashColor = Color.Lerp(Color.white, originalColor, progress);
                
                if (iconImage != null)
                {
                    iconImage.color = flashColor;
                }
                
                yield return null;
            }
            
            flashCoroutine = null;
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Manually assign ability manager reference
        /// </summary>
        /// <param name="manager">Ability manager to use</param>
        public void SetAbilityManager(PlushLeague.Gameplay.Abilities.AbilityManager manager)
        {
            if (abilityManager != null)
            {
                // Unsubscribe from old manager
                abilityManager.OnAbilityUsed -= OnAbilityUsed;
                abilityManager.OnAbilityCooldownChanged -= OnAbilityCooldownChanged;
            }
            
            abilityManager = manager;
            
            if (abilityManager != null)
            {
                // Subscribe to new manager
                abilityManager.OnAbilityUsed += OnAbilityUsed;
                abilityManager.OnAbilityCooldownChanged += OnAbilityCooldownChanged;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            if (abilityManager != null)
            {
                abilityManager.OnAbilityUsed -= OnAbilityUsed;
                abilityManager.OnAbilityCooldownChanged -= OnAbilityCooldownChanged;
            }
        }
        
        #endregion
    }
}
