using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Example script that demonstrates how to properly set up and use the MainMenuUI component.
    /// This script can be attached to a GameObject to automatically create and configure a main menu.
    /// </summary>
    public class MainMenuSetupExample : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createCanvasIfMissing = true;
        
        [Header("Menu Configuration")]
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string settingsSceneName = "Settings";
        [SerializeField] private string customizeSceneName = "Customize";
        
        [Header("Feature Flags")]
        [SerializeField] private bool enableCustomizeButton = false;
        [SerializeField] private bool enableSettingsButton = true;
        [SerializeField] private bool enableQuitButton = true;
        
        [Header("Audio Clips (Optional)")]
        [SerializeField] private AudioClip menuMusicClip;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        private PlushLeague.UI.Menu.MainMenuUI mainMenuUI;
        private Canvas menuCanvas;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupMainMenu();
            }
        }
        
        /// <summary>
        /// Sets up the main menu UI with all required components
        /// </summary>
        public void SetupMainMenu()
        {
            UnityEngine.Debug.Log("Setting up Main Menu...");
            
            // Create or find canvas
            SetupCanvas();
            
            // Create main menu UI GameObject
            var menuObject = CreateMainMenuObject();
            
            // Setup MainMenuUI component
            SetupMainMenuUI(menuObject);
            
            // Create menu buttons
            CreateMenuButtons(menuObject);
            
            // Setup title and logo
            CreateTitleAndLogo(menuObject);
            
            // Setup visual effects
            SetupVisualEffects(menuObject);
            
            UnityEngine.Debug.Log("Main Menu setup complete!");
        }
        
        /// <summary>
        /// Setup or create canvas for the menu
        /// </summary>
        private void SetupCanvas()
        {
            menuCanvas = FindFirstObjectByType<Canvas>();
            
            if (menuCanvas == null && createCanvasIfMissing)
            {
                var canvasObject = new GameObject("MainMenuCanvas");
                menuCanvas = canvasObject.AddComponent<Canvas>();
                menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                menuCanvas.sortingOrder = 0;
                
                // Add Canvas Scaler
                var scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                // Add Graphics Raycaster
                canvasObject.AddComponent<GraphicRaycaster>();
                
                UnityEngine.Debug.Log("Created new Canvas for Main Menu");
            }
        }
        
        /// <summary>
        /// Create main menu GameObject
        /// </summary>
        private GameObject CreateMainMenuObject()
        {
            var menuObject = new GameObject("MainMenu");
            menuObject.transform.SetParent(menuCanvas.transform, false);
            
            // Add RectTransform and set to full screen
            var rectTransform = menuObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Add CanvasGroup for fading
            var canvasGroup = menuObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            return menuObject;
        }
        
        /// <summary>
        /// Setup MainMenuUI component
        /// </summary>
        private void SetupMainMenuUI(GameObject menuObject)
        {
            mainMenuUI = menuObject.AddComponent<PlushLeague.UI.Menu.MainMenuUI>();
            
            // Configure via reflection to set private fields
            var mainMenuType = typeof(PlushLeague.UI.Menu.MainMenuUI);
            
            // Set scene names
            SetPrivateField(mainMenuType, "gameSceneName", gameSceneName);
            SetPrivateField(mainMenuType, "settingsSceneName", settingsSceneName);
            SetPrivateField(mainMenuType, "customizeSceneName", customizeSceneName);
            
            // Set feature flags
            SetPrivateField(mainMenuType, "enableCustomizeButton", enableCustomizeButton);
            SetPrivateField(mainMenuType, "enableSettingsButton", enableSettingsButton);
            SetPrivateField(mainMenuType, "enableQuitButton", enableQuitButton);
            
            // Set canvas group reference
            SetPrivateField(mainMenuType, "menuCanvasGroup", menuObject.GetComponent<CanvasGroup>());
            
            // Setup audio
            SetupAudio(menuObject);
        }
        
        /// <summary>
        /// Setup audio for the menu
        /// </summary>
        private void SetupAudio(GameObject menuObject)
        {
            var audioSource = menuObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            
            // Set audio clips via reflection
            var mainMenuType = typeof(PlushLeague.UI.Menu.MainMenuUI);
            SetPrivateField(mainMenuType, "audioSource", audioSource);
            SetPrivateField(mainMenuType, "menuMusicClip", menuMusicClip);
            SetPrivateField(mainMenuType, "buttonHoverSound", buttonHoverSound);
            SetPrivateField(mainMenuType, "buttonClickSound", buttonClickSound);
        }
        
        /// <summary>
        /// Create menu buttons
        /// </summary>
        private void CreateMenuButtons(GameObject menuObject)
        {
            // Create button container
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(menuObject.transform, false);
            
            var containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.3f);
            containerRect.anchorMax = new Vector2(0.5f, 0.7f);
            containerRect.sizeDelta = new Vector2(300, 400);
            
            // Add Vertical Layout Group
            var layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            // Add Content Size Fitter
            var sizeFitter = buttonContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Create buttons
            var playButton = CreateButton(buttonContainer, "PLAY", Color.green);
            var customizeButton = CreateButton(buttonContainer, "CUSTOMIZE", Color.blue);
            var settingsButton = CreateButton(buttonContainer, "SETTINGS", Color.gray);
            var quitButton = CreateButton(buttonContainer, "QUIT", Color.red);
            
            // Set button references via reflection
            var mainMenuType = typeof(PlushLeague.UI.Menu.MainMenuUI);
            SetPrivateField(mainMenuType, "playButton", playButton);
            SetPrivateField(mainMenuType, "customizeButton", customizeButton);
            SetPrivateField(mainMenuType, "settingsButton", settingsButton);
            SetPrivateField(mainMenuType, "quitButton", quitButton);
            
            // Configure button states
            customizeButton.gameObject.SetActive(enableCustomizeButton);
            settingsButton.gameObject.SetActive(enableSettingsButton);
            quitButton.gameObject.SetActive(enableQuitButton);
        }
        
        /// <summary>
        /// Create a menu button
        /// </summary>
        private Button CreateButton(GameObject parent, string text, Color color)
        {
            var buttonObject = new GameObject($"Button_{text}");
            buttonObject.transform.SetParent(parent.transform, false);
            
            var rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 60);
            
            // Add Image component
            var image = buttonObject.AddComponent<Image>();
            image.color = color;
            
            // Add Button component
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            
            // Create text child
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);
            
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 24f;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Bold;
            
            return button;
        }
        
        /// <summary>
        /// Create title and logo
        /// </summary>
        private void CreateTitleAndLogo(GameObject menuObject)
        {
            // Create title
            var titleObject = new GameObject("Title");
            titleObject.transform.SetParent(menuObject.transform, false);
            
            var titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(800, 100);
            
            var titleText = titleObject.AddComponent<TextMeshProUGUI>();
            titleText.text = "PLUSH LEAGUE";
            titleText.fontSize = 72f;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            // Add text outline
            titleText.outlineWidth = 0.2f;
            titleText.outlineColor = Color.black;
            
            // Set title reference via reflection
            var mainMenuType = typeof(PlushLeague.UI.Menu.MainMenuUI);
            SetPrivateField(mainMenuType, "titleText", titleText);
        }
        
        /// <summary>
        /// Setup visual effects
        /// </summary>
        private void SetupVisualEffects(GameObject menuObject)
        {
            // Create background
            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(menuObject.transform, false);
            backgroundObject.transform.SetAsFirstSibling(); // Put it behind everything
            
            var backgroundRect = backgroundObject.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            
            var backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark blue background
            
            // Note: Particle systems and animations would be added here
            // For now, we'll keep it simple
        }
        
        /// <summary>
        /// Helper method to set private fields via reflection
        /// </summary>
        private void SetPrivateField(System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(mainMenuUI, value);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Could not find field: {fieldName} in {type.Name}");
            }
        }
        
        /// <summary>
        /// Get reference to the created MainMenuUI
        /// </summary>
        public PlushLeague.UI.Menu.MainMenuUI GetMainMenuUI()
        {
            return mainMenuUI;
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Subscribe to menu events for testing
        /// </summary>
        public void SubscribeToMenuEvents()
        {
            if (mainMenuUI == null) return;
            
            mainMenuUI.OnPlayRequested += OnPlayRequested;
            mainMenuUI.OnCustomizeRequested += OnCustomizeRequested;
            mainMenuUI.OnSettingsRequested += OnSettingsRequested;
            mainMenuUI.OnQuitRequested += OnQuitRequested;
            
            UnityEngine.Debug.Log("Subscribed to Main Menu events");
        }
        
        private void OnPlayRequested()
        {
            UnityEngine.Debug.Log("Play requested from Main Menu");
        }
        
        private void OnCustomizeRequested()
        {
            UnityEngine.Debug.Log("Customize requested from Main Menu");
        }
        
        private void OnSettingsRequested()
        {
            UnityEngine.Debug.Log("Settings requested from Main Menu");
        }
        
        private void OnQuitRequested()
        {
            UnityEngine.Debug.Log("Quit requested from Main Menu");
        }
        
        #endregion
        
        #region Debug
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugGUI = true;
        
        private void OnGUI()
        {
            if (!enableDebugGUI || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Main Menu Setup Example ===");
            
            if (mainMenuUI == null)
            {
                if (GUILayout.Button("Setup Main Menu"))
                {
                    SetupMainMenu();
                }
            }
            else
            {
                GUILayout.Label($"Main Menu Status: Active");
                GUILayout.Label($"Transitioning: {mainMenuUI.IsTransitioning}");
                
                if (GUILayout.Button("Subscribe to Events"))
                {
                    SubscribeToMenuEvents();
                }
                
                if (GUILayout.Button("Show Menu"))
                {
                    mainMenuUI.ShowMenu();
                }
                
                if (GUILayout.Button("Hide Menu"))
                {
                    mainMenuUI.HideMenu();
                }
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
