using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// UI component for displaying slide tackle ability cooldown and status
    /// Shows button state, cooldown timer, and availability
    /// </summary>
    public class SlideTackleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button slideTackleButton;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Image buttonIcon;
        
        [Header("Visual States")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color disabledColor = Color.red;
        
        [Header("Settings")]
        [SerializeField] private bool showCooldownText = true;
        [SerializeField] private bool vibrateFeedback = true;
        
        private PlushLeague.Gameplay.Abilities.AbilityManager abilityManager;
        private PlushLeague.Gameplay.Abilities.SlideTackle slideTackleAbility;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                UpdateUI();
            }
        }
        
        private void InitializeUI()
        {
            // Find ability manager in scene
            abilityManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Abilities.AbilityManager>();
            if (abilityManager == null)
            {
                UnityEngine.Debug.LogWarning("SlideTackleUI: No AbilityManager found in scene");
                return;
            }
            
            // Find slide tackle ability
            var abilities = abilityManager.GetAvailableAbilities();
            foreach (var ability in abilities)
            {
                if (ability != null && ability.GetType().Name == "SlideTackle")
                {
                    slideTackleAbility = ability as PlushLeague.Gameplay.Abilities.SlideTackle;
                    break;
                }
            }
            
            if (slideTackleAbility == null)
            {
                UnityEngine.Debug.LogWarning("SlideTackleUI: No SlideTackle ability found");
                return;
            }
            
            // Setup button click handler
            if (slideTackleButton != null)
            {
                slideTackleButton.onClick.AddListener(OnSlideTackleButtonPressed);
            }
            
            // Initialize UI state
            UpdateUI();
            isInitialized = true;
            
            UnityEngine.Debug.Log("SlideTackleUI: Initialized successfully");
        }
        
        private void UpdateUI()
        {
            if (slideTackleAbility == null) return;
            
            bool canUse = abilityManager.CanUseAbility<PlushLeague.Gameplay.Abilities.SlideTackle>();
            bool isOnCooldown = slideTackleAbility.IsOnCooldown;
            float cooldownProgress = slideTackleAbility.CooldownProgress;
            
            // Update button interactability
            if (slideTackleButton != null)
            {
                slideTackleButton.interactable = canUse;
            }
            
            // Update cooldown fill
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = isOnCooldown ? (1f - cooldownProgress) : 0f;
            }
            
            // Update cooldown text
            if (cooldownText != null && showCooldownText)
            {
                if (isOnCooldown)
                {
                    float remainingTime = slideTackleAbility.CooldownRemaining;
                    cooldownText.text = remainingTime.ToString("F1") + "s";
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }
            
            // Update button icon color
            if (buttonIcon != null)
            {
                if (!canUse && !isOnCooldown)
                {
                    buttonIcon.color = disabledColor;
                }
                else if (isOnCooldown)
                {
                    buttonIcon.color = cooldownColor;
                }
                else
                {
                    buttonIcon.color = availableColor;
                }
            }
        }
        
        private void OnSlideTackleButtonPressed()
        {
            if (abilityManager == null) return;
            
            // Try to use slide tackle ability
            bool success = abilityManager.UseAbility<PlushLeague.Gameplay.Abilities.SlideTackle>();
            
            if (success)
            {
                // Provide haptic feedback on mobile
                if (vibrateFeedback)
                {
                    #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                    #endif
                }
                
                UnityEngine.Debug.Log("SlideTackleUI: Slide tackle activated");
            }
            else
            {
                UnityEngine.Debug.Log("SlideTackleUI: Cannot use slide tackle");
            }
        }
        
        /// <summary>
        /// Manually set the ability manager reference
        /// </summary>
        /// <param name="manager">Ability manager to use</param>
        public void SetAbilityManager(PlushLeague.Gameplay.Abilities.AbilityManager manager)
        {
            abilityManager = manager;
            
            if (isInitialized)
            {
                InitializeUI();
            }
        }
        
        /// <summary>
        /// Show/hide the slide tackle UI
        /// </summary>
        /// <param name="show">Whether to show the UI</param>
        public void SetVisible(bool show)
        {
            gameObject.SetActive(show);
        }
        
        /// <summary>
        /// Flash the button to indicate it's available (e.g., when near an opponent with ball)
        /// </summary>
        public void FlashAvailable()
        {
            if (buttonIcon != null && !slideTackleAbility.IsOnCooldown)
            {
                StartCoroutine(FlashCoroutine());
            }
        }
        
        private System.Collections.IEnumerator FlashCoroutine()
        {
            Color originalColor = buttonIcon.color;
            
            for (int i = 0; i < 3; i++)
            {
                buttonIcon.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                buttonIcon.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying || !isInitialized) return;
            
            GUILayout.BeginArea(new Rect(260, 200, 200, 100));
            GUILayout.Label("=== Slide Tackle UI Debug ===");
            GUILayout.Label($"Ability Found: {slideTackleAbility != null}");
            GUILayout.Label($"Can Use: {abilityManager?.CanUseAbility<PlushLeague.Gameplay.Abilities.SlideTackle>()}");
            GUILayout.Label($"On Cooldown: {slideTackleAbility?.IsOnCooldown}");
            if (slideTackleAbility != null && slideTackleAbility.IsOnCooldown)
            {
                GUILayout.Label($"Cooldown: {slideTackleAbility.CooldownRemaining:F1}s");
            }
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
