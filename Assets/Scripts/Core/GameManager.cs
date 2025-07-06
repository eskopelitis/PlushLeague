using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace PlushLeague.Core
{
    /// <summary>
    /// Central game manager that coordinates the overall game flow from menu to power selection to match.
    /// Manages transitions between game states and coordinates between different systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private string menuSceneName = "MainMenu";
        [SerializeField] private string powerSelectionSceneName = "PowerSelection";
        [SerializeField] private string matchSceneName = "GameArena";
        [SerializeField] private string postMatchSceneName = "PostMatch";
        [SerializeField] private bool useIntegratedPowerSelection = true; // If true, power selection is part of game scene
        
        [Header("Scene Transitions")]
        [SerializeField] private float sceneTransitionDuration = 1f;
        [SerializeField] private bool enableSceneFadeTransitions = true;
        [SerializeField] private bool showLoadingScreen = true;
        [SerializeField] private float loadingScreenMinDuration = 2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip sceneTransitionSound;
        [SerializeField] private AudioClip matchStartSound;
        [SerializeField] private AudioClip matchEndSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("UI References")]
        [SerializeField] private PlushLeague.UI.Menu.MainMenuUI mainMenuUI;
        [SerializeField] private PlushLeague.UI.HUD.GameHUD gameHUD;
        [SerializeField] private Canvas sceneTransitionCanvas;
        [SerializeField] private UnityEngine.UI.Image fadeOverlay;
        [SerializeField] private UnityEngine.UI.Image loadingSpinner;
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;
        
        [Header("Persistent Settings")]
        [SerializeField] private PersistentPlayerSettings persistentSettings;
        
        [Header("Game Loop Control")]
        [SerializeField] private bool preventMultipleLoads = true;
        [SerializeField] private bool autoReturnToMenuOnError = true;
        [SerializeField] private float matchTimeoutDuration = 600f; // 10 minutes
        
        [Header("Game Flow")]
        [SerializeField] private GameState _currentState = GameState.Menu;
        [SerializeField] private float transitionDelay = 1f;
        
        public GameState currentState => _currentState;
        
        [Header("References")]
        [SerializeField] private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        [SerializeField] private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        [Header("Player Configuration")]
        [SerializeField] private PlayerGameConfig player1Config;
        [SerializeField] private PlayerGameConfig player2Config;
        
        [Header("Game Settings")]
        [SerializeField] private bool isMultiplayer = false;
        [SerializeField] private bool skipPowerSelection = false; // For testing
        
        // Static instance for singleton access
        public static GameManager Instance { get; private set; }
        
        // Scene loading state
        private bool isLoadingScene = false;
        private bool isTransitioning = false;
        private string currentSceneName;
        private Coroutine sceneLoadCoroutine;
        private Coroutine matchTimeoutCoroutine;
        
        // Post-match state
        private bool matchCompletedCleanly = false;
        private MatchResult lastMatchResult;
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<PlayerGameConfig, PlayerGameConfig> OnPlayersConfigured;
        public System.Action OnMatchStartRequested;
        public System.Action<MatchResult> OnMatchCompleted;
        public System.Action OnSceneTransitionStarted;
        public System.Action OnSceneTransitionCompleted;
        
        [System.Serializable]
        public class PersistentPlayerSettings
        {
            public string preferredPlayerName = "Player";
            public string preferredPlushType = "Default";
            public int preferredSuperpowerIndex = 0;
            public PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType preferredRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker;
            public float masterVolume = 1f;
            public float musicVolume = 0.7f;
            public float sfxVolume = 0.8f;
            public bool enableVibration = true;
            public int matchesWon = 0;
            public int matchesLost = 0;
            public int totalGoals = 0;
            public int totalSaves = 0;
            
            /// <summary>
            /// Save settings to PlayerPrefs
            /// </summary>
            public void SaveToPlayerPrefs()
            {
                PlayerPrefs.SetString("PlayerName", preferredPlayerName);
                PlayerPrefs.SetString("PlushType", preferredPlushType);
                PlayerPrefs.SetInt("SuperpowerIndex", preferredSuperpowerIndex);
                PlayerPrefs.SetInt("PreferredRole", (int)preferredRole);
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
                PlayerPrefs.SetInt("EnableVibration", enableVibration ? 1 : 0);
                PlayerPrefs.SetInt("MatchesWon", matchesWon);
                PlayerPrefs.SetInt("MatchesLost", matchesLost);
                PlayerPrefs.SetInt("TotalGoals", totalGoals);
                PlayerPrefs.SetInt("TotalSaves", totalSaves);
                PlayerPrefs.Save();
            }
            
            /// <summary>
            /// Load settings from PlayerPrefs
            /// </summary>
            public void LoadFromPlayerPrefs()
            {
                preferredPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
                preferredPlushType = PlayerPrefs.GetString("PlushType", "Default");
                preferredSuperpowerIndex = PlayerPrefs.GetInt("SuperpowerIndex", 0);
                preferredRole = (PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType)PlayerPrefs.GetInt("PreferredRole", 0);
                masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
                enableVibration = PlayerPrefs.GetInt("EnableVibration", 1) == 1;
                matchesWon = PlayerPrefs.GetInt("MatchesWon", 0);
                matchesLost = PlayerPrefs.GetInt("MatchesLost", 0);
                totalGoals = PlayerPrefs.GetInt("TotalGoals", 0);
                totalSaves = PlayerPrefs.GetInt("TotalSaves", 0);
            }
        }
        
        [System.Serializable]
        public struct MatchResult
        {
            public bool playerWon;
            public int playerScore;
            public int opponentScore;
            public float matchDuration;
            public int playerGoals;
            public int playerSaves;
            public int superpowerUsageCount;
            public string mvpPlayerName;
            public bool wasCleanMatch; // No disconnects or errors
            
            public MatchResult(bool won, int pScore, int oScore, float duration, int goals, int saves, int powerUses, string mvp, bool clean)
            {
                playerWon = won;
                playerScore = pScore;
                opponentScore = oScore;
                matchDuration = duration;
                playerGoals = goals;
                playerSaves = saves;
                superpowerUsageCount = powerUses;
                mvpPlayerName = mvp;
                wasCleanMatch = clean;
            }
        }
        
        public enum GameState
        {
            Menu,
            PowerSelection,
            MatchSetup,
            MatchActive,
            MatchEnded,
            PostMatch
        }
        
        [System.Serializable]
        public class PlayerGameConfig
        {
            public string playerName = "Player";
            public PlushLeague.Gameplay.Superpowers.SuperpowerData selectedPower;
            public PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType selectedRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker;
            public bool isLocalPlayer = true;
            public bool isReady = false;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeComponents();
                LoadPlayerPrefs(); // Load persistent settings
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Initialize current scene name
            currentSceneName = SceneManager.GetActiveScene().name;
            
            // Apply loaded preferences
            ApplyPreferencesToPlayerConfigs();
            
            // Set initial state
            SetGameState(GameState.Menu);
            
            // Setup audio source
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize references and components
        /// </summary>
        private void InitializeComponents()
        {
            // Initialize player configs
            if (player1Config == null)
                player1Config = new PlayerGameConfig { playerName = "Player 1", isLocalPlayer = true };
                
            if (player2Config == null)
                player2Config = new PlayerGameConfig { playerName = "Player 2", isLocalPlayer = !isMultiplayer };
                
            // Find managers in scene if not assigned
            FindManagers();
            
            // Subscribe to events
            SetupEventListeners();
        }
        
        /// <summary>
        /// Find managers in the current scene
        /// </summary>
        private void FindManagers()
        {
            if (powerSelectionManager == null)
                powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
                
            if (matchManager == null)
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
        }
        
        /// <summary>
        /// Setup event listeners for managers
        /// </summary>
        private void SetupEventListeners()
        {
            if (powerSelectionManager != null)
            {
                powerSelectionManager.OnSelectionsConfirmed += OnPowerSelectionsConfirmed;
                powerSelectionManager.OnMatchStartRequested += OnPowerSelectionMatchStartRequested;
            }
        }
        
        #endregion
        
        #region Game State Management
        
        /// <summary>
        /// Change the current game state
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (_currentState == newState) return;
            
            var previousState = _currentState;
            _currentState = newState;
            
            UnityEngine.Debug.Log($"Game state changed from {previousState} to {newState}");
            
            HandleGameStateChange(previousState, newState);
            OnGameStateChanged?.Invoke(newState);
        }
        
        /// <summary>
        /// Handle state transitions
        /// </summary>
        private void HandleGameStateChange(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Menu:
                    HandleMenuState();
                    break;
                    
                case GameState.PowerSelection:
                    HandlePowerSelectionState();
                    break;
                    
                case GameState.MatchSetup:
                    HandleMatchSetupState();
                    break;
                    
                case GameState.MatchActive:
                    HandleMatchActiveState();
                    break;
                    
                case GameState.MatchEnded:
                    HandleMatchEndedState();
                    break;
                    
                case GameState.PostMatch:
                    HandlePostMatchState();
                    break;
            }
        }
        
        #endregion
        
        #region State Handlers
        
        private void HandleMenuState()
        {
            // Reset configurations
            ResetPlayerConfigs();
        }
        
        private void HandlePowerSelectionState()
        {
            if (skipPowerSelection)
            {
                // Skip power selection and use defaults
                UseDefaultPowerSelections();
                SetGameState(GameState.MatchSetup);
                return;
            }
            
            if (powerSelectionManager != null)
            {
                // Start power selection process
                powerSelectionManager.StartPowerSelection(isMultiplayer);
            }
            else
            {
                UnityEngine.Debug.LogWarning("PowerSelectionManager not found! Skipping to match setup with defaults.");
                UseDefaultPowerSelections();
                SetGameState(GameState.MatchSetup);
            }
        }
        
        private void HandleMatchSetupState()
        {
            // Load match scene if not integrated and not already in match scene
            if (!useIntegratedPowerSelection && !string.IsNullOrEmpty(matchSceneName) && currentSceneName != matchSceneName)
            {
                LoadScene(matchSceneName, GameState.MatchSetup);
            }
            else
            {
                StartCoroutine(SetupMatchCoroutine());
            }
        }
        
        private void HandleMatchActiveState()
        {
            // Match is now active, all setup complete
            UnityEngine.Debug.Log("Match is now active with configured players");
            
            // Start match timeout
            StartMatchTimeout();
            
            // Play match start sound
            if (matchStartSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(matchStartSound);
            }
        }
        
        private void HandleMatchEndedState()
        {
            // Handle match end logic
            UnityEngine.Debug.Log("Match has ended");
            
            // This will be called from OnMatchEnd() with proper result data
        }
        
        private void HandlePostMatchState()
        {
            // Handle post-match summary, statistics, etc.
            UnityEngine.Debug.Log("Showing post-match summary");
            
            // If we have a post-match scene, load it
            if (!string.IsNullOrEmpty(postMatchSceneName) && currentSceneName != postMatchSceneName)
            {
                LoadScene(postMatchSceneName, GameState.PostMatch);
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Start a new game (called from menu)
        /// </summary>
        public void StartNewGame(bool multiplayer = false)
        {
            isMultiplayer = multiplayer;
            player2Config.isLocalPlayer = !multiplayer;
            
            // Reset match state
            matchCompletedCleanly = false;
            
            if (useIntegratedPowerSelection)
            {
                // Power selection is in the same scene as the game
                SetGameState(GameState.PowerSelection);
            }
            else
            {
                // Load power selection scene
                LoadScene(powerSelectionSceneName, GameState.PowerSelection);
            }
        }
        
        /// <summary>
        /// Skip power selection and start match with defaults (for testing)
        /// </summary>
        public void StartQuickMatch()
        {
            skipPowerSelection = true;
            StartNewGame(false);
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            LoadScene(menuSceneName, GameState.Menu);
        }
        
        /// <summary>
        /// Get current player configurations
        /// </summary>
        public (PlayerGameConfig player1, PlayerGameConfig player2) GetPlayerConfigs()
        {
            return (player1Config, player2Config);
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Called when power selections are confirmed
        /// </summary>
        private void OnPowerSelectionsConfirmed(PlushLeague.UI.PowerSelection.PowerSelectionManager.PlayerSelectionConfig p1, 
                                               PlushLeague.UI.PowerSelection.PowerSelectionManager.PlayerSelectionConfig p2)
        {
            // Convert power selection configs to game configs
            player1Config.selectedPower = p1.selectedPower;
            player1Config.selectedRole = p1.selectedRole;
            player1Config.isReady = p1.isReady;
            
            player2Config.selectedPower = p2.selectedPower;
            player2Config.selectedRole = p2.selectedRole;
            player2Config.isReady = p2.isReady;
            
            UnityEngine.Debug.Log($"Power selections confirmed - P1: {p1.selectedPower?.displayName} ({p1.selectedRole}), P2: {p2.selectedPower?.displayName} ({p2.selectedRole})");
            
            // Notify other systems
            OnPlayersConfigured?.Invoke(player1Config, player2Config);
            
            // Proceed to match setup
            SetGameState(GameState.MatchSetup);
        }
        
        /// <summary>
        /// Called when match start is requested from power selection
        /// </summary>
        private void OnPowerSelectionMatchStartRequested()
        {
            OnMatchStartRequested?.Invoke();
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Reset player configurations to defaults
        /// </summary>
        private void ResetPlayerConfigs()
        {
            player1Config.selectedPower = null;
            player1Config.selectedRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker;
            player1Config.isReady = false;
            
            player2Config.selectedPower = null;
            player2Config.selectedRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker;
            player2Config.isReady = false;
        }
        
        /// <summary>
        /// Use default power selections when skipping power selection phase
        /// </summary>
        private void UseDefaultPowerSelections()
        {
            // Load default powers from resources if available
            var defaultPowers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            
            if (defaultPowers.Length > 0)
            {
                player1Config.selectedPower = defaultPowers[0];
                player2Config.selectedPower = defaultPowers.Length > 1 ? defaultPowers[1] : defaultPowers[0];
            }
            
            // Set default roles
            player1Config.selectedRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker;
            player2Config.selectedRole = PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Goalkeeper;
            
            player1Config.isReady = true;
            player2Config.isReady = true;
            
            UnityEngine.Debug.Log("Using default power selections");
        }
        
        /// <summary>
        /// Setup match with configured players
        /// </summary>
        private System.Collections.IEnumerator SetupMatchCoroutine()
        {
            UnityEngine.Debug.Log("Setting up match with player configurations...");
            
            // Wait a frame to ensure all systems are ready
            yield return null;
            
            // Find match manager if not already found
            if (matchManager == null)
            {
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            }
            
            if (matchManager != null)
            {
                // Apply player configurations to match manager
                ApplyPlayerConfigurationsToMatch();
                
                // Wait for transition delay
                yield return new WaitForSeconds(transitionDelay);
                
                // Set match active state
                SetGameState(GameState.MatchActive);
            }
            else
            {
                UnityEngine.Debug.LogError("MatchManager not found! Cannot start match.");
            }
        }
        
        /// <summary>
        /// Apply player configurations to the match
        /// </summary>
        private void ApplyPlayerConfigurationsToMatch()
        {
            // Find all players in the scene and assign configurations
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            
            int playerIndex = 0;
            foreach (var player in players)
            {
                PlayerGameConfig config = playerIndex == 0 ? player1Config : player2Config;
                
                // Apply superpower
                if (config.selectedPower != null)
                {
                    var powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
                    if (powerupController != null)
                    {
                        powerupController.SetSuperpower(config.selectedPower);
                    }
                }
                
                // Apply role-specific logic here if needed
                // For example, positioning goalkeepers vs strikers
                
                playerIndex++;
                if (playerIndex >= 2) break; // Only handle first 2 players for now
            }
            
            UnityEngine.Debug.Log("Player configurations applied to match");
        }
        
        #endregion
        
        #region Scene Management
        
        /// <summary>
        /// Load a scene with proper transitions and state management
        /// </summary>
        public void LoadScene(string sceneName, GameState targetState = GameState.Menu)
        {
            if (preventMultipleLoads && isLoadingScene)
            {
                UnityEngine.Debug.LogWarning("Scene loading already in progress, ignoring request");
                return;
            }
            
            StartCoroutine(LoadSceneCoroutine(sceneName, targetState));
        }
        
        /// <summary>
        /// Load scene with fade transition
        /// </summary>
        private IEnumerator LoadSceneCoroutine(string sceneName, GameState targetState)
        {
            isLoadingScene = true;
            isTransitioning = true;
            OnSceneTransitionStarted?.Invoke();
            
            // Play transition sound
            PlayTransitionSound();
            
            // Start fade out
            if (enableSceneFadeTransitions)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // Show loading screen
            if (showLoadingScreen)
            {
                ShowLoadingScreen(true);
            }
            
            // Record start time for minimum loading duration
            float loadStartTime = Time.time;
            
            // Load the scene
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                if (loadingText != null)
                {
                    loadingText.text = $"Loading... {Mathf.RoundToInt(asyncLoad.progress * 100)}%";
                }
                
                if (loadingSpinner != null)
                {
                    loadingSpinner.transform.Rotate(0, 0, -90 * Time.deltaTime);
                }
                
                // Allow scene activation when ready
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time
                    if (Time.time - loadStartTime >= loadingScreenMinDuration)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                
                yield return null;
            }
            
            // Update current scene name
            currentSceneName = sceneName;
            
            // Set the new game state
            SetGameState(targetState);
            
            // Find managers in the new scene
            yield return new WaitForSeconds(0.1f); // Small delay to ensure scene is fully loaded
            FindManagers();
            SetupEventListeners();
            
            // Hide loading screen
            if (showLoadingScreen)
            {
                ShowLoadingScreen(false);
            }
            
            // Fade in
            if (enableSceneFadeTransitions)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            isLoadingScene = false;
            isTransitioning = false;
            OnSceneTransitionCompleted?.Invoke();
            
            UnityEngine.Debug.Log($"Scene loaded: {sceneName}, State: {targetState}");
        }
        
        /// <summary>
        /// Handle match completion
        /// </summary>
        public void OnMatchEnd(MatchResult result)
        {
            lastMatchResult = result;
            matchCompletedCleanly = result.wasCleanMatch;
            
            // Stop match timeout
            if (matchTimeoutCoroutine != null)
            {
                StopCoroutine(matchTimeoutCoroutine);
                matchTimeoutCoroutine = null;
            }
            
            // Update persistent stats
            UpdatePersistentStats(result);
            
            // Play match end sound
            if (matchEndSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(matchEndSound);
            }
            
            // Transition to post-match
            SetGameState(GameState.PostMatch);
            
            // Fire event
            OnMatchCompleted?.Invoke(result);
            
            UnityEngine.Debug.Log($"Match ended - Player {(result.playerWon ? "Won" : "Lost")} ({result.playerScore}-{result.opponentScore})");
        }
        
        /// <summary>
        /// Handle rematch request
        /// </summary>
        public void OnRematchPressed()
        {
            if (!matchCompletedCleanly)
            {
                UnityEngine.Debug.LogWarning("Cannot rematch - previous match did not complete cleanly");
                return;
            }
            
            UnityEngine.Debug.Log("Rematch requested - restarting match with current settings");
            
            // Keep current player configurations
            // Reset match state
            matchCompletedCleanly = false;
            
            // Go directly to match setup
            SetGameState(GameState.MatchSetup);
        }
        
        /// <summary>
        /// Handle return to menu request
        /// </summary>
        public void OnReturnToMenuPressed()
        {
            UnityEngine.Debug.Log("Returning to main menu");
            
            // Clean up match state
            CleanupMatchState();
            
            // Load menu scene
            LoadScene(menuSceneName, GameState.Menu);
        }
        
        /// <summary>
        /// Save player preferences
        /// </summary>
        public void SavePlayerPrefs()
        {
            if (persistentSettings != null)
            {
                persistentSettings.SaveToPlayerPrefs();
                UnityEngine.Debug.Log("Player preferences saved");
            }
        }
        
        /// <summary>
        /// Load player preferences
        /// </summary>
        public void LoadPlayerPrefs()
        {
            if (persistentSettings == null)
            {
                persistentSettings = new PersistentPlayerSettings();
            }
            
            persistentSettings.LoadFromPlayerPrefs();
            UnityEngine.Debug.Log("Player preferences loaded");
        }
        
        /// <summary>
        /// Apply loaded preferences to current player configs
        /// </summary>
        public void ApplyPreferencesToPlayerConfigs()
        {
            if (persistentSettings == null) return;
            
            // Apply to player 1 (local player)
            player1Config.playerName = persistentSettings.preferredPlayerName;
            player1Config.selectedRole = persistentSettings.preferredRole;
            
            // Apply superpower if available
            var availablePowers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            if (availablePowers.Length > persistentSettings.preferredSuperpowerIndex)
            {
                player1Config.selectedPower = availablePowers[persistentSettings.preferredSuperpowerIndex];
            }
            
            UnityEngine.Debug.Log($"Applied preferences to player config: {player1Config.playerName}, Role: {player1Config.selectedRole}");
        }
        
        #endregion
        
        #region Scene Transition Effects
        
        /// <summary>
        /// Fade out transition
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (fadeOverlay == null) yield break;
            
            fadeOverlay.gameObject.SetActive(true);
            float alpha = 0f;
            
            while (alpha < 1f)
            {
                alpha += Time.deltaTime / sceneTransitionDuration;
                fadeOverlay.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            
            fadeOverlay.color = new Color(0, 0, 0, 1f);
        }
        
        /// <summary>
        /// Fade in transition
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (fadeOverlay == null) yield break;
            
            float alpha = 1f;
            
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime / sceneTransitionDuration;
                fadeOverlay.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            
            fadeOverlay.color = new Color(0, 0, 0, 0f);
            fadeOverlay.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Show or hide loading screen
        /// </summary>
        private void ShowLoadingScreen(bool show)
        {
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(show);
                if (show) loadingText.text = "Loading...";
            }
            
            if (loadingSpinner != null)
            {
                loadingSpinner.gameObject.SetActive(show);
            }
        }
        
        /// <summary>
        /// Play scene transition sound
        /// </summary>
        private void PlayTransitionSound()
        {
            if (sceneTransitionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(sceneTransitionSound);
            }
        }
        
        #endregion
        
        #region Error Handling and Safety
        
        /// <summary>
        /// Handle abnormal match termination
        /// </summary>
        public void HandleMatchError(string errorMessage)
        {
            UnityEngine.Debug.LogError($"Match error: {errorMessage}");
            
            // Mark match as unclean
            matchCompletedCleanly = false;
            
            // Stop match timeout
            if (matchTimeoutCoroutine != null)
            {
                StopCoroutine(matchTimeoutCoroutine);
                matchTimeoutCoroutine = null;
            }
            
            // Return to menu if auto-return is enabled
            if (autoReturnToMenuOnError)
            {
                UnityEngine.Debug.Log("Auto-returning to menu due to match error");
                StartCoroutine(SafeReturnToMenu());
            }
        }
        
        /// <summary>
        /// Safely return to menu with error handling
        /// </summary>
        private IEnumerator SafeReturnToMenu()
        {
            // Wait a moment for any ongoing operations to complete
            yield return new WaitForSeconds(1f);
            
            // Clean up
            CleanupMatchState();
            
            // Load menu scene
            try
            {
                LoadScene(menuSceneName, GameState.Menu);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to return to menu: {e.Message}");
                // Force scene reload as last resort
                SceneManager.LoadScene(menuSceneName);
            }
        }
        
        /// <summary>
        /// Start match timeout coroutine
        /// </summary>
        private void StartMatchTimeout()
        {
            if (matchTimeoutCoroutine != null)
            {
                StopCoroutine(matchTimeoutCoroutine);
            }
            
            matchTimeoutCoroutine = StartCoroutine(MatchTimeoutCoroutine());
        }
        
        /// <summary>
        /// Handle match timeout
        /// </summary>
        private IEnumerator MatchTimeoutCoroutine()
        {
            yield return new WaitForSeconds(matchTimeoutDuration);
            
            UnityEngine.Debug.LogWarning("Match timeout reached - ending match");
            
            // Create timeout result
            var timeoutResult = new MatchResult(
                false, 0, 0, matchTimeoutDuration, 0, 0, 0, "None", false
            );
            
            OnMatchEnd(timeoutResult);
        }
        
        /// <summary>
        /// Clean up match state
        /// </summary>
        private void CleanupMatchState()
        {
            // Stop any running coroutines
            if (matchTimeoutCoroutine != null)
            {
                StopCoroutine(matchTimeoutCoroutine);
                matchTimeoutCoroutine = null;
            }
            
            // Reset match flags
            matchCompletedCleanly = false;
            
            // Reset player ready states
            if (player1Config != null) player1Config.isReady = false;
            if (player2Config != null) player2Config.isReady = false;
            
            UnityEngine.Debug.Log("Match state cleaned up");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Update persistent player statistics
        /// </summary>
        private void UpdatePersistentStats(MatchResult result)
        {
            if (persistentSettings == null) return;
            
            if (result.playerWon)
            {
                persistentSettings.matchesWon++;
            }
            else
            {
                persistentSettings.matchesLost++;
            }
            
            persistentSettings.totalGoals += result.playerGoals;
            persistentSettings.totalSaves += result.playerSaves;
            
            // Auto-save updated stats
            SavePlayerPrefs();
        }
        
        /// <summary>
        /// Get current scene name
        /// </summary>
        public string GetCurrentSceneName()
        {
            return currentSceneName ?? SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// Check if currently transitioning between scenes
        /// </summary>
        public bool IsTransitioning()
        {
            return isTransitioning;
        }
        
        /// <summary>
        /// Check if a scene is currently loading
        /// </summary>
        public bool IsLoadingScene()
        {
            return isLoadingScene;
        }
        
        /// <summary>
        /// Get match result from last completed match
        /// </summary>
        public MatchResult GetLastMatchResult()
        {
            return lastMatchResult;
        }
        
        /// <summary>
        /// Check if last match completed cleanly
        /// </summary>
        public bool WasLastMatchClean()
        {
            return matchCompletedCleanly;
        }
        
        /// <summary>
        /// Get persistent player settings
        /// </summary>
        public PersistentPlayerSettings GetPersistentSettings()
        {
            return persistentSettings;
        }
        
        /// <summary>
        /// Print current game state to debug console
        /// </summary>
        public void PrintCurrentState()
        {
            UnityEngine.Debug.Log($"[GameManager] Current State: {_currentState}");
            UnityEngine.Debug.Log($"[GameManager] Scene: {GetCurrentSceneName()}");
            UnityEngine.Debug.Log($"[GameManager] Is Transitioning: {IsTransitioning()}");
            UnityEngine.Debug.Log($"[GameManager] Is Loading: {IsLoadingScene()}");
            if (persistentSettings != null)
            {
                UnityEngine.Debug.Log($"[GameManager] Matches Won: {persistentSettings.matchesWon}, Lost: {persistentSettings.matchesLost}");
            }
        }
        
        #endregion
    }
}
