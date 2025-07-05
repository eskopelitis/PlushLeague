using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlushLeague.Demo
{
    /// <summary>
    /// Demo script that shows how to integrate the GameManager with PowerSelection system.
    /// Provides simple UI buttons to test the complete flow from menu to power selection to match.
    /// </summary>
    public class GameFlowDemo : MonoBehaviour
    {
        [Header("Demo UI")]
        [SerializeField] private GameObject demoPanel;
        [SerializeField] private Button startSingleplayerButton;
        [SerializeField] private Button startMultiplayerButton;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private TextMeshProUGUI currentStateText;
        [SerializeField] private TextMeshProUGUI playerConfigText;
        
        [Header("Settings")]
        [SerializeField] private bool showDemoUI = true;
        [SerializeField] private bool autoHideInMatch = true;
        
        private PlushLeague.Core.GameManager gameManager;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeDemo();
        }
        
        private void Update()
        {
            UpdateUI();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize demo components and setup
        /// </summary>
        private void InitializeDemo()
        {
            // Find or create GameManager
            gameManager = PlushLeague.Core.GameManager.Instance;
            if (gameManager == null)
            {
                UnityEngine.Debug.LogWarning("GameManager not found! Creating one for demo.");
                var gameManagerGO = new GameObject("GameManager");
                gameManager = gameManagerGO.AddComponent<PlushLeague.Core.GameManager>();
            }
            
            // Setup UI
            SetupDemoUI();
            
            // Subscribe to events
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                gameManager.OnPlayersConfigured += OnPlayersConfigured;
            }
            
            // Show/hide demo panel
            if (demoPanel != null)
            {
                demoPanel.SetActive(showDemoUI);
            }
        }
        
        /// <summary>
        /// Setup demo UI buttons and events
        /// </summary>
        private void SetupDemoUI()
        {
            if (startSingleplayerButton != null)
                startSingleplayerButton.onClick.AddListener(() => StartGame(false));
                
            if (startMultiplayerButton != null)
                startMultiplayerButton.onClick.AddListener(() => StartGame(true));
                
            if (quickMatchButton != null)
                quickMatchButton.onClick.AddListener(StartQuickMatch);
                
            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(ReturnToMenu);
        }
        
        #endregion
        
        #region Demo Controls
        
        /// <summary>
        /// Start a new game with power selection
        /// </summary>
        private void StartGame(bool multiplayer)
        {
            if (gameManager != null)
            {
                gameManager.StartNewGame(multiplayer);
            }
        }
        
        /// <summary>
        /// Start a quick match without power selection
        /// </summary>
        private void StartQuickMatch()
        {
            if (gameManager != null)
            {
                gameManager.StartQuickMatch();
            }
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        private void ReturnToMenu()
        {
            if (gameManager != null)
            {
                gameManager.ReturnToMenu();
            }
        }
        
        /// <summary>
        /// Toggle demo UI visibility
        /// </summary>
        public void ToggleDemoUI()
        {
            if (demoPanel != null)
            {
                demoPanel.SetActive(!demoPanel.activeSelf);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Called when game state changes
        /// </summary>
        private void OnGameStateChanged(PlushLeague.Core.GameManager.GameState newState)
        {
            UnityEngine.Debug.Log($"[GameFlowDemo] Game state changed to: {newState}");
            
            // Auto-hide demo UI during match if enabled
            if (autoHideInMatch && demoPanel != null)
            {
                bool shouldShow = newState != PlushLeague.Core.GameManager.GameState.MatchActive;
                demoPanel.SetActive(shouldShow && showDemoUI);
            }
        }
        
        /// <summary>
        /// Called when players are configured
        /// </summary>
        private void OnPlayersConfigured(PlushLeague.Core.GameManager.PlayerGameConfig player1, 
                                       PlushLeague.Core.GameManager.PlayerGameConfig player2)
        {
            UnityEngine.Debug.Log($"[GameFlowDemo] Players configured - P1: {player1.playerName} ({player1.selectedRole}), P2: {player2.playerName} ({player2.selectedRole})");
        }
        
        #endregion
        
        #region UI Updates
        
        /// <summary>
        /// Update demo UI with current information
        /// </summary>
        private void UpdateUI()
        {
            if (!showDemoUI) return;
            
            UpdateStateDisplay();
            UpdatePlayerConfigDisplay();
        }
        
        /// <summary>
        /// Update current state display
        /// </summary>
        private void UpdateStateDisplay()
        {
            if (currentStateText != null && gameManager != null)
            {
                currentStateText.text = $"State: {gameManager.currentState}";
            }
        }
        
        /// <summary>
        /// Update player configuration display
        /// </summary>
        private void UpdatePlayerConfigDisplay()
        {
            if (playerConfigText != null && gameManager != null)
            {
                var (player1, player2) = gameManager.GetPlayerConfigs();
                
                string configText = $"Player 1: {player1.playerName}\\n" +
                                  $"Power: {(player1.selectedPower != null ? player1.selectedPower.displayName : "None")}\\n" +
                                  $"Role: {player1.selectedRole}\\n\\n" +
                                  $"Player 2: {player2.playerName}\\n" +
                                  $"Power: {(player2.selectedPower != null ? player2.selectedPower.displayName : "None")}\\n" +
                                  $"Role: {player2.selectedRole}";
                
                playerConfigText.text = configText;
            }
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Debug method to trigger power selection manually
        /// </summary>
        [ContextMenu("Debug: Show Power Selection")]
        public void DebugShowPowerSelection()
        {
            var powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            if (powerSelectionManager != null)
            {
                powerSelectionManager.StartPowerSelection(false);
            }
            else
            {
                UnityEngine.Debug.LogWarning("PowerSelectionManager not found in scene!");
            }
        }
        
        /// <summary>
        /// Debug method to print current game state
        /// </summary>
        [ContextMenu("Debug: Print Game State")]
        public void DebugPrintGameState()
        {
            if (gameManager != null)
            {
                gameManager.PrintCurrentState();
            }
            else
            {
                UnityEngine.Debug.LogWarning("GameManager not found!");
            }
        }
        
        #endregion
    }
}
