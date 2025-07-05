using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// UI component for individual action buttons with cooldown, availability, and visual feedback
    /// Handles A, B, X, Y buttons with radial fill cooldowns and press animations
    /// </summary>
    public class ActionButtonUI : MonoBehaviour
    {
        [Header("Button Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image buttonIcon;
        [SerializeField] private Image cooldownFill; // Radial fill for cooldown
        [SerializeField] private TextMeshProUGUI buttonLabel;
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        [Header("Visual States")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color unavailableColor = Color.red;
        [SerializeField] private Color pressedColor = Color.yellow;
        [SerializeField] private float disabledAlpha = 0.5f;
        
        [Header("Animation Settings")]
        [SerializeField] private float pressScaleAmount = 0.9f;
        [SerializeField] private float pressAnimDuration = 0.1f;
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private AnimationCurve pressAnimCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Feedback")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private bool enableSoundFeedback = true;
        [SerializeField] private AudioClip pressSound;
        [SerializeField] private AudioClip cooldownCompleteSound;
        
        // Runtime state
        private string actionName;
        private KeyCode keyCode;
        private bool isAvailable = true;
        private bool isOnCooldown = false;
        private float currentCooldown = 0f;
        private float maxCooldown = 0f;
        private Vector3 originalScale;
        private Color originalIconColor;
        private AudioSource audioSource;
        
        // Coroutines
        private Coroutine pressAnimCoroutine;
        private Coroutine flashCoroutine;
        private Coroutine cooldownCoroutine;
        
        // Events
        public System.Action<string> OnButtonPressed;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            InitializeButton();
        }
        
        private void Update()
        {
            HandleKeyboardInput();
            UpdateVisualState();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize components and references
        /// </summary>
        private void InitializeComponents()
        {
            // Get components
            if (button == null)
                button = GetComponent<Button>();
                
            if (buttonIcon == null)
                buttonIcon = GetComponentInChildren<Image>();
                
            // Setup audio
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            // Store original values
            originalScale = transform.localScale;
            if (buttonIcon != null)
                originalIconColor = buttonIcon.color;
        }
        
        /// <summary>
        /// Initialize button state and setup
        /// </summary>
        private void InitializeButton()
        {
            // Setup button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            
            // Initialize cooldown fill
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 0f;
                cooldownFill.type = Image.Type.Filled;
                cooldownFill.fillMethod = Image.FillMethod.Radial360;
            }
            
            // Set initial state
            SetAvailable(true);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize button with name and key binding
        /// </summary>
        public void Initialize(string name, KeyCode key)
        {
            actionName = name;
            keyCode = key;
            
            if (buttonLabel != null)
                buttonLabel.text = name;
        }
        
        /// <summary>
        /// Set button availability
        /// </summary>
        public void SetAvailable(bool available)
        {
            isAvailable = available && !isOnCooldown;
            
            if (button != null)
                button.interactable = isAvailable;
                
            UpdateButtonColor();
        }
        
        /// <summary>
        /// Set cooldown state
        /// </summary>
        public void SetCooldown(float cooldownRemaining, float totalCooldown)
        {
            currentCooldown = cooldownRemaining;
            maxCooldown = totalCooldown;
            isOnCooldown = cooldownRemaining > 0f;
            
            if (cooldownCoroutine != null)
                StopCoroutine(cooldownCoroutine);
                
            if (isOnCooldown)
            {
                cooldownCoroutine = StartCoroutine(CooldownCoroutine());
            }
            else
            {
                CompleteCooldown();
            }
            
            SetAvailable(!isOnCooldown);
        }
        
        /// <summary>
        /// Flash button when used
        /// </summary>
        public void FlashUsage()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
                
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
        
        /// <summary>
        /// Simulate button press (for external triggering)
        /// </summary>
        public void PressButton()
        {
            OnButtonClicked();
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle button click events
        /// </summary>
        private void OnButtonClicked()
        {
            if (!isAvailable) return;
            
            // Play press animation
            PlayPressAnimation();
            
            // Play sound
            PlayPressSound();
            
            // Haptic feedback (mobile)
            PlayHapticFeedback();
            
            // Notify listeners
            OnButtonPressed?.Invoke(actionName);
        }
        
        #endregion
        
        #region Input Handling
        
        /// <summary>
        /// Handle keyboard input for this button
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(keyCode) && isAvailable)
            {
                OnButtonClicked();
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        /// <summary>
        /// Update button visual state
        /// </summary>
        private void UpdateVisualState()
        {
            UpdateCooldownDisplay();
        }
        
        /// <summary>
        /// Update cooldown fill and text
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (cooldownFill != null && isOnCooldown)
            {
                float fillAmount = maxCooldown > 0 ? (maxCooldown - currentCooldown) / maxCooldown : 0f;
                cooldownFill.fillAmount = fillAmount;
            }
            
            if (cooldownText != null)
            {
                if (isOnCooldown && currentCooldown > 0.1f)
                {
                    cooldownText.text = currentCooldown.ToString("F1");
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Update button color based on state
        /// </summary>
        private void UpdateButtonColor()
        {
            if (buttonIcon == null) return;
            
            Color targetColor;
            float targetAlpha = 1f;
            
            if (isOnCooldown)
            {
                targetColor = cooldownColor;
            }
            else if (isAvailable)
            {
                targetColor = availableColor;
            }
            else
            {
                targetColor = unavailableColor;
                targetAlpha = disabledAlpha;
            }
            
            targetColor.a = targetAlpha;
            buttonIcon.color = Color.Lerp(buttonIcon.color, targetColor, Time.deltaTime * 5f);
        }
        
        #endregion
        
        #region Animations
        
        /// <summary>
        /// Play button press animation
        /// </summary>
        private void PlayPressAnimation()
        {
            if (pressAnimCoroutine != null)
                StopCoroutine(pressAnimCoroutine);
                
            pressAnimCoroutine = StartCoroutine(PressAnimationCoroutine());
        }
        
        /// <summary>
        /// Button press animation coroutine
        /// </summary>
        private IEnumerator PressAnimationCoroutine()
        {
            float elapsed = 0f;
            
            // Scale down
            while (elapsed < pressAnimDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pressAnimDuration * 0.5f);
                float scale = Mathf.Lerp(1f, pressScaleAmount, pressAnimCurve.Evaluate(t));
                transform.localScale = originalScale * scale;
                yield return null;
            }
            
            // Scale back up
            elapsed = 0f;
            while (elapsed < pressAnimDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pressAnimDuration * 0.5f);
                float scale = Mathf.Lerp(pressScaleAmount, 1f, pressAnimCurve.Evaluate(t));
                transform.localScale = originalScale * scale;
                yield return null;
            }
            
            // Ensure final scale
            transform.localScale = originalScale;
        }
        
        /// <summary>
        /// Flash effect coroutine
        /// </summary>
        private IEnumerator FlashCoroutine()
        {
            if (buttonIcon == null) yield break;
            
            Color originalColor = buttonIcon.color;
            
            // Flash to pressed color
            float elapsed = 0f;
            while (elapsed < flashDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (flashDuration * 0.5f);
                buttonIcon.color = Color.Lerp(originalColor, pressedColor, t);
                yield return null;
            }
            
            // Flash back to original
            elapsed = 0f;
            while (elapsed < flashDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (flashDuration * 0.5f);
                buttonIcon.color = Color.Lerp(pressedColor, originalColor, t);
                yield return null;
            }
            
            buttonIcon.color = originalColor;
        }
        
        /// <summary>
        /// Cooldown countdown coroutine
        /// </summary>
        private IEnumerator CooldownCoroutine()
        {
            while (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
                yield return null;
            }
            
            CompleteCooldown();
        }
        
        /// <summary>
        /// Complete cooldown and reset state
        /// </summary>
        private void CompleteCooldown()
        {
            isOnCooldown = false;
            currentCooldown = 0f;
            
            if (cooldownFill != null)
                cooldownFill.fillAmount = 0f;
                
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
                
            // Play completion sound
            if (cooldownCompleteSound != null && audioSource != null)
                audioSource.PlayOneShot(cooldownCompleteSound);
                
            SetAvailable(true);
        }
        
        #endregion
        
        #region Audio & Haptic Feedback
        
        /// <summary>
        /// Play button press sound
        /// </summary>
        private void PlayPressSound()
        {
            if (!enableSoundFeedback || pressSound == null || audioSource == null) return;
            
            audioSource.PlayOneShot(pressSound);
        }
        
        /// <summary>
        /// Play haptic feedback (mobile)
        /// </summary>
        private void PlayHapticFeedback()
        {
            if (!enableHapticFeedback) return;
            
            // Mobile haptic feedback
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Stop all coroutines
            if (pressAnimCoroutine != null) StopCoroutine(pressAnimCoroutine);
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        }
        
        #endregion
    }
}
