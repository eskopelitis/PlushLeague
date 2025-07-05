using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace PlushLeague.UI.Menu
{
    /// <summary>
    /// Main menu UI controller that provides the entry point for the game.
    /// Handles navigation to power selection, settings, and other game features.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button customizeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image logoImage;
        
        [Header("Scene Configuration")]
        [SerializeField] private string gameSceneName = "GameArena";
        [SerializeField] private string settingsSceneName = "Settings";
        [SerializeField] private string customizeSceneName = "Customize";
        [SerializeField] private bool useIntegratedGameFlow = true; // Use GameManager for flow
        
        [Header("Visual Effects")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private ParticleSystem backgroundEffect;
        [SerializeField] private GameObject plushAvatar;
        [SerializeField] private Animator plushAvatarAnimator;
        
        [Header("Audio")]
        [SerializeField] private AudioClip menuMusicClip;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float menuMusicVolume = 0.5f;
        
        [Header("Transition Settings")]
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private bool disableButtonsDuringTransition = true;
        
        [Header("Feature Flags")]
        [SerializeField] private bool enableCustomizeButton = false; // MVP: Can be disabled
        [SerializeField] private bool enableSettingsButton = true;
        [SerializeField] private bool enableQuitButton = true;
        
        // State
        private bool isTransitioning = false;
        private bool isInitialized = false;
        private PlushLeague.Core.GameManager gameManager;
        
        // Events
        public System.Action OnPlayRequested;
        public System.Action OnCustomizeRequested;
        public System.Action OnSettingsRequested;
        public System.Action OnQuitRequested;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            StartCoroutine(InitializeMenu());
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize menu components and references
        /// </summary>
        private void InitializeComponents()
        {
            // Find GameManager if using integrated flow
            if (useIntegratedGameFlow)
            {
                gameManager = PlushLeague.Core.GameManager.Instance;
            }
            
            // Setup audio source
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            // Setup canvas group for fading
            if (menuCanvasGroup == null)
                menuCanvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// Initialize menu with fade-in and setup
        /// </summary>
        private IEnumerator InitializeMenu()
        {
            yield return StartCoroutine(SetupMenu());
            yield return StartCoroutine(FadeInMenu());
            
            isInitialized = true;
            UnityEngine.Debug.Log("Main Menu initialized");
        }
        
        /// <summary>
        /// Setup menu UI and event listeners
        /// </summary>
        private IEnumerator SetupMenu()
        {
            // Setup button listeners
            SetupButtonListeners();
            
            // Configure button states
            ConfigureButtonStates();
            
            // Setup game title and logo
            SetupTitleAndLogo();
            
            // Start background effects
            StartBackgroundEffects();
            
            // Start menu music
            StartMenuMusic();
            
            // Wait a frame for everything to initialize
            yield return null;
        }
        
        /// <summary>
        /// Setup button event listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayPressed);
                AddButtonHoverEffects(playButton);
            }
            
            if (customizeButton != null)
            {
                customizeButton.onClick.AddListener(OnCustomizePressed);
                AddButtonHoverEffects(customizeButton);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsPressed);
                AddButtonHoverEffects(settingsButton);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitPressed);
                AddButtonHoverEffects(quitButton);
            }
        }
        
        /// <summary>
        /// Add hover effects to buttons
        /// </summary>
        private void AddButtonHoverEffects(Button button)
        {
            var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            // Hover enter
            var hoverEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => OnButtonHover());
            trigger.triggers.Add(hoverEnter);
        }
        
        /// <summary>
        /// Configure button states based on feature flags
        /// </summary>
        private void ConfigureButtonStates()
        {
            if (customizeButton != null)
                customizeButton.gameObject.SetActive(enableCustomizeButton);
                
            if (settingsButton != null)
                settingsButton.gameObject.SetActive(enableSettingsButton);
                
            if (quitButton != null)
                quitButton.gameObject.SetActive(enableQuitButton);
        }
        
        /// <summary>
        /// Setup game title and logo display
        /// </summary>
        private void SetupTitleAndLogo()
        {
            if (titleText != null)
            {
                titleText.text = "PLUSH LEAGUE";
                
                // Add title animation if available
                if (menuAnimator != null)
                    menuAnimator.SetTrigger("ShowTitle");
            }
        }
        
        /// <summary>
        /// Start background visual effects
        /// </summary>
        private void StartBackgroundEffects()
        {
            if (backgroundEffect != null)
                backgroundEffect.Play();
                
            if (plushAvatar != null && plushAvatarAnimator != null)
            {
                plushAvatar.SetActive(true);
                plushAvatarAnimator.SetTrigger("IdleBounce");
            }
        }
        
        /// <summary>
        /// Start menu background music
        /// </summary>
        private void StartMenuMusic()
        {
            if (audioSource != null && menuMusicClip != null)
            {
                audioSource.clip = menuMusicClip;
                audioSource.volume = menuMusicVolume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
        #endregion
        
        #region Button Event Handlers
        
        /// <summary>
        /// Handle Play button press - main game entry point
        /// </summary>
        public void OnPlayPressed()
        {
            if (isTransitioning || !isInitialized) return;
            
            PlayButtonClickSound();
            OnPlayRequested?.Invoke();
            
            UnityEngine.Debug.Log("Play button pressed");
            
            if (useIntegratedGameFlow && gameManager != null)
            {
                // Use GameManager for integrated flow (recommended)
                StartCoroutine(StartGameFlow());
            }
            else
            {
                // Direct scene loading (fallback)
                StartCoroutine(FadeOutAndLoadScene(gameSceneName));
            }
        }
        
        /// <summary>
        /// Handle Customize button press
        /// </summary>
        public void OnCustomizePressed()
        {
            if (isTransitioning || !isInitialized) return;
            
            PlayButtonClickSound();
            OnCustomizeRequested?.Invoke();
            
            UnityEngine.Debug.Log("Customize button pressed");
            
            if (enableCustomizeButton)
            {
                StartCoroutine(FadeOutAndLoadScene(customizeSceneName));
            }
            else
            {
                ShowFeatureNotAvailable("Customization");
            }
        }
        
        /// <summary>
        /// Handle Settings button press
        /// </summary>
        public void OnSettingsPressed()
        {
            if (isTransitioning || !isInitialized) return;
            
            PlayButtonClickSound();
            OnSettingsRequested?.Invoke();
            
            UnityEngine.Debug.Log("Settings button pressed");
            
            if (enableSettingsButton)
            {
                StartCoroutine(FadeOutAndLoadScene(settingsSceneName));
            }
            else
            {
                ShowFeatureNotAvailable("Settings");
            }
        }
        
        /// <summary>
        /// Handle Quit button press
        /// </summary>
        public void OnQuitPressed()
        {
            if (isTransitioning || !isInitialized) return;
            
            PlayButtonClickSound();
            OnQuitRequested?.Invoke();
            
            UnityEngine.Debug.Log("Quit button pressed");
            
            StartCoroutine(QuitGame());
        }
        
        /// <summary>
        /// Handle button hover effects
        /// </summary>
        private void OnButtonHover()
        {
            PlayButtonHoverSound();
        }
        
        #endregion
        
        #region Game Flow
        
        /// <summary>
        /// Start the integrated game flow using GameManager
        /// </summary>
        private IEnumerator StartGameFlow()
        {
            isTransitioning = true;
            SetButtonsInteractable(false);
            
            // Fade out menu music
            yield return StartCoroutine(FadeOutMusic());
            
            // Start fade out animation
            if (menuAnimator != null)
                menuAnimator.SetTrigger("FadeOut");
            
            yield return StartCoroutine(FadeOutMenu());
            
            // Start game through GameManager
            if (gameManager != null)
            {
                gameManager.StartNewGame(false); // Start single-player by default
            }
            else
            {
                UnityEngine.Debug.LogError("GameManager not found! Falling back to direct scene load.");
                SceneManager.LoadScene(gameSceneName);
            }
        }
        
        /// <summary>
        /// Show feature not available message
        /// </summary>
        private void ShowFeatureNotAvailable(string featureName)
        {
            UnityEngine.Debug.Log($"{featureName} feature is not available in MVP");
            
            // Could show a UI popup here
            if (titleText != null)
            {
                StartCoroutine(ShowTemporaryMessage($"{featureName} Coming Soon!"));
            }
        }
        
        /// <summary>
        /// Show temporary message in title area
        /// </summary>
        private IEnumerator ShowTemporaryMessage(string message)
        {
            if (titleText == null) yield break;
            
            string originalText = titleText.text;
            Color originalColor = titleText.color;
            
            titleText.text = message;
            titleText.color = Color.yellow;
            
            yield return new WaitForSeconds(2f);
            
            titleText.text = originalText;
            titleText.color = originalColor;
        }
        
        /// <summary>
        /// Quit the game
        /// </summary>
        private IEnumerator QuitGame()
        {
            isTransitioning = true;
            SetButtonsInteractable(false);
            
            // Fade out
            yield return StartCoroutine(FadeOutMenu());
            
            // Quit application
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Scene Transitions
        
        /// <summary>
        /// Fade out and load specified scene
        /// </summary>
        public IEnumerator FadeOutAndLoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.Debug.LogWarning("Scene name is empty, cannot load scene");
                yield break;
            }
            
            isTransitioning = true;
            SetButtonsInteractable(false);
            
            // Fade out music
            yield return StartCoroutine(FadeOutMusic());
            
            // Fade out menu
            yield return StartCoroutine(FadeOutMenu());
            
            // Load scene
            SceneManager.LoadScene(sceneName);
        }
        
        /// <summary>
        /// Fade in the menu
        /// </summary>
        private IEnumerator FadeInMenu()
        {
            if (menuCanvasGroup == null) yield break;
            
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
        }
        
        /// <summary>
        /// Fade out the menu
        /// </summary>
        private IEnumerator FadeOutMenu()
        {
            if (menuCanvasGroup == null) yield break;
            
            menuCanvasGroup.interactable = false;
            
            float elapsedTime = 0f;
            float startAlpha = menuCanvasGroup.alpha;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            menuCanvasGroup.alpha = 0f;
        }
        
        #endregion
        
        #region Audio Management
        
        /// <summary>
        /// Fade out menu music
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            if (audioSource == null || !audioSource.isPlaying) yield break;
            
            float startVolume = audioSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            audioSource.volume = 0f;
            audioSource.Stop();
        }
        
        /// <summary>
        /// Play button hover sound
        /// </summary>
        private void PlayButtonHoverSound()
        {
            if (buttonHoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(buttonHoverSound, 0.3f);
            }
        }
        
        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonClickSound()
        {
            if (buttonClickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(buttonClickSound, 0.5f);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Set all buttons interactable state
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (!disableButtonsDuringTransition) return;
            
            if (playButton != null) playButton.interactable = interactable;
            if (customizeButton != null) customizeButton.interactable = interactable;
            if (settingsButton != null) settingsButton.interactable = interactable;
            if (quitButton != null) quitButton.interactable = interactable;
        }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (playButton != null) playButton.onClick.RemoveAllListeners();
            if (customizeButton != null) customizeButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Show the main menu (called when returning from other scenes)
        /// </summary>
        public void ShowMenu()
        {
            if (!isInitialized)
            {
                StartCoroutine(InitializeMenu());
            }
            else
            {
                StartCoroutine(FadeInMenu());
                StartMenuMusic();
                isTransitioning = false;
                SetButtonsInteractable(true);
            }
        }
        
        /// <summary>
        /// Hide the main menu
        /// </summary>
        public void HideMenu()
        {
            StartCoroutine(FadeOutMenu());
        }
        
        /// <summary>
        /// Check if menu is currently transitioning
        /// </summary>
        public bool IsTransitioning => isTransitioning;
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 250, 150));
            GUILayout.Label("=== Main Menu Debug ===");
            GUILayout.Label($"Initialized: {isInitialized}");
            GUILayout.Label($"Transitioning: {isTransitioning}");
            GUILayout.Label($"Music Playing: {audioSource?.isPlaying}");
            GUILayout.Label($"Canvas Alpha: {menuCanvasGroup?.alpha:F2}");
            
            if (GUILayout.Button("Test Play Flow"))
            {
                OnPlayPressed();
            }
            
            if (GUILayout.Button("Reset Menu"))
            {
                ShowMenu();
            }
            
            GUILayout.EndArea();
        }
        #endif
        
        #endregion
    }
}
