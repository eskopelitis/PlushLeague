using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PlushLeague.UI.PowerSelection
{
    /// <summary>
    /// Individual power selection button component for the power selection grid.
    /// Displays power info and handles selection interaction.
    /// </summary>
    public class PowerButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image powerIcon;
        [SerializeField] private TextMeshProUGUI powerNameText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private GameObject lockIcon;
        
        [Header("Visual States")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color disabledColor = Color.gray;
        [SerializeField] private float animationDuration = 0.2f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem selectionEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        
        // State
        private PlushLeague.Gameplay.Superpowers.SuperpowerData powerData;
        private bool isSelected = false;
        private bool isLocked = false;
        private Action onClickCallback;
        
        // Animation
        private Coroutine animationCoroutine;
        
        public PlushLeague.Gameplay.Superpowers.SuperpowerData PowerData => powerData;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            SetupEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize components
        /// </summary>
        private void InitializeComponents()
        {
            if (button == null)
                button = GetComponent<Button>();
                
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            // Set initial state
            SetSelected(false);
            SetLocked(false);
        }
        
        /// <summary>
        /// Setup event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
                
                // Add hover effects
                var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    
                // Hover enter
                var hoverEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                hoverEnter.callback.AddListener((data) => OnHoverEnter());
                eventTrigger.triggers.Add(hoverEnter);
                
                // Hover exit
                var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                hoverExit.callback.AddListener((data) => OnHoverExit());
                eventTrigger.triggers.Add(hoverExit);
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Initialize button with power data and callback
        /// </summary>
        public void Initialize(PlushLeague.Gameplay.Superpowers.SuperpowerData power, Action onClick)
        {
            powerData = power;
            onClickCallback = onClick;
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Set selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
            
            if (selected && selectionEffect != null)
                selectionEffect.Play();
        }
        
        /// <summary>
        /// Set locked state (for powers not yet unlocked)
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            
            if (button != null)
                button.interactable = !locked;
                
            if (lockIcon != null)
                lockIcon.SetActive(locked);
                
            UpdateVisualState();
        }
        
        #endregion
        
        #region Display Updates
        
        /// <summary>
        /// Update button display with power data
        /// </summary>
        private void UpdateDisplay()
        {
            if (powerData == null) return;
            
            // Update icon
            if (powerIcon != null && powerData.icon != null)
                powerIcon.sprite = powerData.icon;
                
            // Update name
            if (powerNameText != null)
                powerNameText.text = powerData.name;
                
            // Update cooldown
            if (cooldownText != null)
                cooldownText.text = $"{powerData.cooldownTime:F0}s";
        }
        
        /// <summary>
        /// Update visual state based on current conditions
        /// </summary>
        private void UpdateVisualState()
        {
            Color targetColor = normalColor;
            
            if (isLocked)
                targetColor = disabledColor;
            else if (isSelected)
                targetColor = selectedColor;
                
            // Apply color with animation
            AnimateColorChange(targetColor);
            
            // Update selection border
            if (selectionBorder != null)
                selectionBorder.gameObject.SetActive(isSelected && !isLocked);
        }
        
        /// <summary>
        /// Animate color change
        /// </summary>
        private void AnimateColorChange(Color targetColor)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
                
            animationCoroutine = StartCoroutine(AnimateColorCoroutine(targetColor));
        }
        
        /// <summary>
        /// Color animation coroutine
        /// </summary>
        private System.Collections.IEnumerator AnimateColorCoroutine(Color targetColor)
        {
            if (backgroundImage == null) yield break;
            
            Color startColor = backgroundImage.color;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                
                backgroundImage.color = Color.Lerp(startColor, targetColor, progress);
                
                yield return null;
            }
            
            backgroundImage.color = targetColor;
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle button click
        /// </summary>
        private void OnButtonClicked()
        {
            if (isLocked || powerData == null) return;
            
            PlaySound(selectSound);
            onClickCallback?.Invoke();
            
            UnityEngine.Debug.Log($"Power button clicked: {powerData.name}");
        }
        
        /// <summary>
        /// Handle hover enter
        /// </summary>
        private void OnHoverEnter()
        {
            if (isLocked || isSelected) return;
            
            PlaySound(hoverSound);
            
            // Add hover effect
            if (backgroundImage != null && !isSelected)
            {
                AnimateColorChange(hoverColor);
            }
            
            // Scale effect
            StartCoroutine(ScaleEffect(1.05f));
        }
        
        /// <summary>
        /// Handle hover exit
        /// </summary>
        private void OnHoverExit()
        {
            if (isLocked) return;
            
            // Return to normal state if not selected
            if (!isSelected && backgroundImage != null)
            {
                AnimateColorChange(normalColor);
            }
            
            // Return to normal scale
            StartCoroutine(ScaleEffect(1f));
        }
        
        /// <summary>
        /// Scale effect coroutine
        /// </summary>
        private System.Collections.IEnumerator ScaleEffect(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;
            float duration = 0.1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                
                yield return null;
            }
            
            transform.localScale = endScale;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Play audio clip safely
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            // Auto-find components in editor
            if (button == null)
                button = GetComponent<Button>();
        }
        
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;
            
            var rect = new Rect(10, 10, 200, 100);
            GUILayout.BeginArea(rect);
            GUILayout.Label($"PowerButton Debug");
            GUILayout.Label($"Selected: {isSelected}");
            GUILayout.Label($"Locked: {isLocked}");
            GUILayout.Label($"Power: {powerData?.displayName ?? "None"}");
            GUILayout.EndArea();
        }
        #endif
        
        #endregion
    }
}
