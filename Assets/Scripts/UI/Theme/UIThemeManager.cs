using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace PlushLeague.UI.Theme
{
    /// <summary>
    /// UI Theme Manager for applying plush-themed styling to UI elements.
    /// Handles sprite updates, animations, and themed visual feedback.
    /// </summary>
    public class UIThemeManager : MonoBehaviour
    {
        [Header("Button Sprites")]
        [SerializeField] private Sprite defaultButtonSprite;
        [SerializeField] private Sprite hoveredButtonSprite;
        [SerializeField] private Sprite pressedButtonSprite;
        [SerializeField] private Sprite disabledButtonSprite;
        
        [Header("UI Element Sprites")]
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite progressBarFillSprite;
        [SerializeField] private Sprite progressBarBackgroundSprite;
        [SerializeField] private Sprite sliderHandleSprite;
        [SerializeField] private Sprite sliderBackgroundSprite;
        
        [Header("Power Selection Icons")]
        [SerializeField] private Sprite[] superpowerIcons;
        [SerializeField] private Sprite[] roleIcons;
        [SerializeField] private Sprite[] playerIcons;
        
        [Header("HUD Icons")]
        [SerializeField] private Sprite healthIcon;
        [SerializeField] private Sprite staminaIcon;
        [SerializeField] private Sprite cooldownIcon;
        [SerializeField] private Sprite scoreIcon;
        [SerializeField] private Sprite timerIcon;
        
        [Header("Animation Settings")]
        [SerializeField] private float buttonHoverScale = 1.1f;
        [SerializeField] private float buttonPressScale = 0.95f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private AnimationCurve buttonAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Color Themes")]
        [SerializeField] private Color primaryColor = Color.white;
        [SerializeField] private Color secondaryColor = Color.gray;
        [SerializeField] private Color accentColor = Color.yellow;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color errorColor = Color.red;
        
        // Singleton instance
        private static UIThemeManager _instance;
        public static UIThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIThemeManager>();
                    if (_instance == null)
                    {
                        var themeManagerObject = new GameObject("UIThemeManager");
                        _instance = themeManagerObject.AddComponent<UIThemeManager>();
                    }
                }
                return _instance;
            }
        }
        
        // UI element tracking
        private Dictionary<string, Sprite> spriteLibrary = new Dictionary<string, Sprite>();
        private Dictionary<Button, Coroutine> buttonAnimations = new Dictionary<Button, Coroutine>();
        private List<ThemedButton> themedButtons = new List<ThemedButton>();
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSpriteLibrary();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ApplyThemeToExistingUI();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the sprite library for easy access
        /// </summary>
        private void InitializeSpriteLibrary()
        {
            // Button sprites
            if (defaultButtonSprite != null) spriteLibrary["button_default"] = defaultButtonSprite;
            if (hoveredButtonSprite != null) spriteLibrary["button_hovered"] = hoveredButtonSprite;
            if (pressedButtonSprite != null) spriteLibrary["button_pressed"] = pressedButtonSprite;
            if (disabledButtonSprite != null) spriteLibrary["button_disabled"] = disabledButtonSprite;
            
            // UI element sprites
            if (panelSprite != null) spriteLibrary["panel"] = panelSprite;
            if (progressBarFillSprite != null) spriteLibrary["progress_fill"] = progressBarFillSprite;
            if (progressBarBackgroundSprite != null) spriteLibrary["progress_background"] = progressBarBackgroundSprite;
            if (sliderHandleSprite != null) spriteLibrary["slider_handle"] = sliderHandleSprite;
            if (sliderBackgroundSprite != null) spriteLibrary["slider_background"] = sliderBackgroundSprite;
            
            // HUD icons
            if (healthIcon != null) spriteLibrary["health_icon"] = healthIcon;
            if (staminaIcon != null) spriteLibrary["stamina_icon"] = staminaIcon;
            if (cooldownIcon != null) spriteLibrary["cooldown_icon"] = cooldownIcon;
            if (scoreIcon != null) spriteLibrary["score_icon"] = scoreIcon;
            if (timerIcon != null) spriteLibrary["timer_icon"] = timerIcon;
            
            // Power selection icons
            for (int i = 0; i < superpowerIcons.Length; i++)
            {
                if (superpowerIcons[i] != null)
                {
                    spriteLibrary[$"superpower_{i}"] = superpowerIcons[i];
                }
            }
            
            for (int i = 0; i < roleIcons.Length; i++)
            {
                if (roleIcons[i] != null)
                {
                    spriteLibrary[$"role_{i}"] = roleIcons[i];
                }
            }
            
            for (int i = 0; i < playerIcons.Length; i++)
            {
                if (playerIcons[i] != null)
                {
                    spriteLibrary[$"player_{i}"] = playerIcons[i];
                }
            }
        }
        
        /// <summary>
        /// Apply theme to all existing UI elements in the scene
        /// </summary>
        private void ApplyThemeToExistingUI()
        {
            // Apply theme to all buttons
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                ApplyButtonTheme(button);
            }
            
            // Apply theme to all panels
            Image[] panels = FindObjectsByType<Image>(FindObjectsSortMode.None);
            foreach (Image panel in panels)
            {
                if (panel.name.ToLower().Contains("panel"))
                {
                    ApplyPanelTheme(panel);
                }
            }
            
            // Apply theme to all sliders
            Slider[] sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            foreach (Slider slider in sliders)
            {
                ApplySliderTheme(slider);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Update UI sprite by element name
        /// </summary>
        public void UpdateUISprite(string uiElement, Sprite newSprite)
        {
            if (newSprite != null)
            {
                spriteLibrary[uiElement] = newSprite;
                
                // Update any UI elements currently using this sprite
                UpdateUIElementsWithSprite(uiElement, newSprite);
            }
        }
        
        /// <summary>
        /// Get sprite by name
        /// </summary>
        public Sprite GetSprite(string spriteName)
        {
            if (spriteLibrary.ContainsKey(spriteName))
            {
                return spriteLibrary[spriteName];
            }
            return null;
        }
        
        /// <summary>
        /// Apply theme to a specific button
        /// </summary>
        public void ApplyButtonTheme(Button button)
        {
            if (button == null) return;
            
            // Apply sprites
            if (defaultButtonSprite != null)
            {
                button.image.sprite = defaultButtonSprite;
            }
            
            // Setup color block
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = primaryColor;
            colorBlock.highlightedColor = accentColor;
            colorBlock.pressedColor = secondaryColor;
            colorBlock.disabledColor = Color.gray;
            button.colors = colorBlock;
            
            // Add themed button component
            var themedButton = button.GetComponent<ThemedButton>();
            if (themedButton == null)
            {
                themedButton = button.gameObject.AddComponent<ThemedButton>();
            }
            themedButton.Initialize(this);
            
            if (!themedButtons.Contains(themedButton))
            {
                themedButtons.Add(themedButton);
            }
        }
        
        /// <summary>
        /// Apply theme to a panel
        /// </summary>
        public void ApplyPanelTheme(Image panel)
        {
            if (panel == null) return;
            
            if (panelSprite != null)
            {
                panel.sprite = panelSprite;
            }
            
            panel.color = primaryColor;
        }
        
        /// <summary>
        /// Apply theme to a slider
        /// </summary>
        public void ApplySliderTheme(Slider slider)
        {
            if (slider == null) return;
            
            // Background
            if (sliderBackgroundSprite != null && slider.GetComponent<Image>() != null)
            {
                slider.GetComponent<Image>().sprite = sliderBackgroundSprite;
            }
            
            // Handle
            if (sliderHandleSprite != null && slider.handleRect != null)
            {
                var handleImage = slider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.sprite = sliderHandleSprite;
                }
            }
            
            // Fill area
            if (progressBarFillSprite != null && slider.fillRect != null)
            {
                var fillImage = slider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.sprite = progressBarFillSprite;
                }
            }
        }
        
        /// <summary>
        /// Apply theme to a progress bar
        /// </summary>
        public void ApplyProgressBarTheme(Image progressBar, Image background = null)
        {
            if (progressBar != null && progressBarFillSprite != null)
            {
                progressBar.sprite = progressBarFillSprite;
                progressBar.color = accentColor;
            }
            
            if (background != null && progressBarBackgroundSprite != null)
            {
                background.sprite = progressBarBackgroundSprite;
                background.color = secondaryColor;
            }
        }
        
        /// <summary>
        /// Apply superpower icon to an image
        /// </summary>
        public void ApplySuperpowerIcon(Image icon, int superpowerIndex)
        {
            if (icon != null && superpowerIndex >= 0 && superpowerIndex < superpowerIcons.Length)
            {
                if (superpowerIcons[superpowerIndex] != null)
                {
                    icon.sprite = superpowerIcons[superpowerIndex];
                }
            }
        }
        
        /// <summary>
        /// Apply role icon to an image
        /// </summary>
        public void ApplyRoleIcon(Image icon, int roleIndex)
        {
            if (icon != null && roleIndex >= 0 && roleIndex < roleIcons.Length)
            {
                if (roleIcons[roleIndex] != null)
                {
                    icon.sprite = roleIcons[roleIndex];
                }
            }
        }
        
        /// <summary>
        /// Apply HUD icon to an image
        /// </summary>
        public void ApplyHUDIcon(Image icon, string iconType)
        {
            if (icon == null) return;
            
            Sprite hudSprite = GetSprite($"{iconType}_icon");
            if (hudSprite != null)
            {
                icon.sprite = hudSprite;
            }
        }
        
        /// <summary>
        /// Animate button press
        /// </summary>
        public void AnimateButtonPress(Button button)
        {
            if (button == null) return;
            
            // Stop any existing animation
            if (buttonAnimations.ContainsKey(button))
            {
                StopCoroutine(buttonAnimations[button]);
            }
            
            // Start new animation
            buttonAnimations[button] = StartCoroutine(ButtonPressAnimation(button));
        }
        
        /// <summary>
        /// Animate button hover
        /// </summary>
        public void AnimateButtonHover(Button button, bool isHovered)
        {
            if (button == null) return;
            
            // Stop any existing animation
            if (buttonAnimations.ContainsKey(button))
            {
                StopCoroutine(buttonAnimations[button]);
            }
            
            // Start new animation
            buttonAnimations[button] = StartCoroutine(ButtonHoverAnimation(button, isHovered));
        }
        
        /// <summary>
        /// Set color theme
        /// </summary>
        public void SetColorTheme(Color primary, Color secondary, Color accent)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            accentColor = accent;
            
            // Update all themed buttons
            foreach (var themedButton in themedButtons)
            {
                if (themedButton != null)
                {
                    themedButton.UpdateColors();
                }
            }
        }
        
        #endregion
        
        #region Animation Coroutines
        
        /// <summary>
        /// Button press animation
        /// </summary>
        private IEnumerator ButtonPressAnimation(Button button)
        {
            Transform buttonTransform = button.transform;
            Vector3 originalScale = buttonTransform.localScale;
            Vector3 targetScale = originalScale * buttonPressScale;
            
            // Press down
            float timer = 0f;
            while (timer < animationDuration / 2f)
            {
                timer += Time.deltaTime;
                float progress = timer / (animationDuration / 2f);
                progress = buttonAnimationCurve.Evaluate(progress);
                
                buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }
            
            // Release
            timer = 0f;
            while (timer < animationDuration / 2f)
            {
                timer += Time.deltaTime;
                float progress = timer / (animationDuration / 2f);
                progress = buttonAnimationCurve.Evaluate(progress);
                
                buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                yield return null;
            }
            
            buttonTransform.localScale = originalScale;
            
            // Remove from active animations
            if (buttonAnimations.ContainsKey(button))
            {
                buttonAnimations.Remove(button);
            }
        }
        
        /// <summary>
        /// Button hover animation
        /// </summary>
        private IEnumerator ButtonHoverAnimation(Button button, bool isHovered)
        {
            Transform buttonTransform = button.transform;
            Vector3 originalScale = buttonTransform.localScale;
            Vector3 targetScale = isHovered ? originalScale * buttonHoverScale : originalScale;
            Vector3 startScale = buttonTransform.localScale;
            
            float timer = 0f;
            while (timer < animationDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / animationDuration;
                progress = buttonAnimationCurve.Evaluate(progress);
                
                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }
            
            buttonTransform.localScale = targetScale;
            
            // Remove from active animations
            if (buttonAnimations.ContainsKey(button))
            {
                buttonAnimations.Remove(button);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Update UI elements currently using a specific sprite
        /// </summary>
        private void UpdateUIElementsWithSprite(string spriteName, Sprite newSprite)
        {
            // This would involve tracking which UI elements use which sprites
            // For now, we'll just update the library
            UnityEngine.Debug.Log($"Updated sprite '{spriteName}' in UI theme library");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Component for themed buttons that handles animation and styling
    /// </summary>
    public class ThemedButton : MonoBehaviour
    {
        private Button button;
        private UIThemeManager themeManager;
        
        public void Initialize(UIThemeManager manager)
        {
            themeManager = manager;
            button = GetComponent<Button>();
            
            if (button != null)
            {
                SetupButtonEvents();
            }
        }
        
        private void SetupButtonEvents()
        {
            // Add event listeners for hover and press
            var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Hover enter
            var hoverEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => { OnHoverEnter(); });
            eventTrigger.triggers.Add(hoverEnter);
            
            // Hover exit
            var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            hoverExit.callback.AddListener((data) => { OnHoverExit(); });
            eventTrigger.triggers.Add(hoverExit);
            
            // Click
            var click = new UnityEngine.EventSystems.EventTrigger.Entry();
            click.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            click.callback.AddListener((data) => { OnClick(); });
            eventTrigger.triggers.Add(click);
        }
        
        private void OnHoverEnter()
        {
            if (themeManager != null)
            {
                themeManager.AnimateButtonHover(button, true);
            }
        }
        
        private void OnHoverExit()
        {
            if (themeManager != null)
            {
                themeManager.AnimateButtonHover(button, false);
            }
        }
        
        private void OnClick()
        {
            if (themeManager != null)
            {
                themeManager.AnimateButtonPress(button);
            }
        }
        
        public void UpdateColors()
        {
            if (button != null && themeManager != null)
            {
                themeManager.ApplyButtonTheme(button);
            }
        }
    }
}
