using UnityEngine;
using System.Collections;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Complete example demonstrating the full Power Selection system flow from start to finish.
    /// This script shows how to properly integrate all components and handle the complete game flow.
    /// </summary>
    public class CompletePowerSelectionExample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [SerializeField] private bool autoStartExample = false;
        [SerializeField] private bool useMultiplayer = false;
        [SerializeField] private float stepDelay = 2f;
        
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Button startGameButton;
        [SerializeField] private UnityEngine.UI.Button quickMatchButton;
        [SerializeField] private TMPro.TextMeshProUGUI statusText;
        [SerializeField] private GameObject examplePanel;
        
        // Component references
        private PlushLeague.Core.GameManager gameManager;
        private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        private PlushLeague.UI.PowerSelection.PowerSelectionUI powerSelectionUI;
        private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        // Example state
        private bool exampleRunning = false;
        private int currentStep = 0;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeExample();
            
            if (autoStartExample)
            {
                StartCoroutine(RunCompleteExample());
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the complete example
        /// </summary>
        private void InitializeExample()
        {
            UnityEngine.Debug.Log("=== Initializing Complete Power Selection Example ===");
            
            // Find or create components
            SetupComponents();
            
            // Setup UI
            SetupExampleUI();
            
            // Subscribe to events
            SubscribeToEvents();
            
            UpdateStatus("Example initialized. Ready to start!");
        }
        
        /// <summary>
        /// Setup all necessary components
        /// </summary>
        private void SetupComponents()
        {
            // Get GameManager instance
            gameManager = PlushLeague.Core.GameManager.Instance;
            if (gameManager == null)
            {
                UnityEngine.Debug.LogWarning("GameManager not found, creating one...");
                var gameManagerGO = new GameObject("GameManager");
                gameManager = gameManagerGO.AddComponent<PlushLeague.Core.GameManager>();
            }
            
            // Find other components
            powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            powerSelectionUI = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionUI>();
            matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            
            UnityEngine.Debug.Log($"Components found - Manager: {powerSelectionManager != null}, UI: {powerSelectionUI != null}, Match: {matchManager != null}");
        }
        
        /// <summary>
        /// Setup example UI
        /// </summary>
        private void SetupExampleUI()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(() => StartCoroutine(RunCompleteExample()));
                
            if (quickMatchButton != null)
                quickMatchButton.onClick.AddListener(() => StartCoroutine(RunQuickMatchExample()));
                
            if (examplePanel != null)
                examplePanel.SetActive(true);
        }
        
        /// <summary>
        /// Subscribe to all relevant events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                gameManager.OnPlayersConfigured += OnPlayersConfigured;
                gameManager.OnMatchStartRequested += OnMatchStartRequested;
            }
            
            if (powerSelectionManager != null)
            {
                powerSelectionManager.OnSelectionsConfirmed += OnSelectionsConfirmed;
            }
        }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= OnGameStateChanged;
                gameManager.OnPlayersConfigured -= OnPlayersConfigured;
                gameManager.OnMatchStartRequested -= OnMatchStartRequested;
            }
            
            if (powerSelectionManager != null)
            {
                powerSelectionManager.OnSelectionsConfirmed -= OnSelectionsConfirmed;
            }
        }
        
        #endregion
        
        #region Example Flows
        
        /// <summary>
        /// Run the complete power selection example
        /// </summary>
        public IEnumerator RunCompleteExample()
        {
            if (exampleRunning)
            {
                UnityEngine.Debug.LogWarning("Example already running!");
                yield break;
            }
            
            exampleRunning = true;
            currentStep = 1;
            
            UnityEngine.Debug.Log("=== Starting Complete Power Selection Example ===");
            UpdateStatus("Starting complete example...");
            
            // Step 1: Initialize game state
            yield return StartCoroutine(ExampleStep1_Initialize());
            
            // Step 2: Start power selection
            yield return StartCoroutine(ExampleStep2_StartPowerSelection());
            
            // Step 3: Simulate player selections
            yield return StartCoroutine(ExampleStep3_SimulateSelections());
            
            // Step 4: Handle role conflicts (if any)
            yield return StartCoroutine(ExampleStep4_HandleRoleConflicts());
            
            // Step 5: Confirm selections and start match
            yield return StartCoroutine(ExampleStep5_StartMatch());
            
            UnityEngine.Debug.Log("=== Complete Power Selection Example Finished ===");
            UpdateStatus("Example completed successfully!");
            
            exampleRunning = false;
        }
        
        /// <summary>
        /// Run a quick match example (skip power selection)
        /// </summary>
        public IEnumerator RunQuickMatchExample()
        {
            if (exampleRunning)
            {
                UnityEngine.Debug.LogWarning("Example already running!");
                yield break;
            }
            
            exampleRunning = true;
            
            UnityEngine.Debug.Log("=== Starting Quick Match Example ===");
            UpdateStatus("Starting quick match...");
            
            if (gameManager != null)
            {
                gameManager.StartQuickMatch();
            }
            
            yield return new WaitForSeconds(stepDelay);
            
            UnityEngine.Debug.Log("=== Quick Match Example Finished ===");
            UpdateStatus("Quick match started!");
            
            exampleRunning = false;
        }
        
        #endregion
        
        #region Example Steps
        
        /// <summary>
        /// Step 1: Initialize the game state
        /// </summary>
        private IEnumerator ExampleStep1_Initialize()
        {
            currentStep = 1;
            UpdateStatus($"Step {currentStep}: Initializing game state...");
            
            // Reset game to menu state
            if (gameManager != null)
            {
                gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.Menu);
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Step 2: Start power selection process
        /// </summary>
        private IEnumerator ExampleStep2_StartPowerSelection()
        {
            currentStep = 2;
            UpdateStatus($"Step {currentStep}: Starting power selection...");
            
            if (gameManager != null)
            {
                gameManager.StartNewGame(useMultiplayer);
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Step 3: Simulate player power and role selections
        /// </summary>
        private IEnumerator ExampleStep3_SimulateSelections()
        {
            currentStep = 3;
            UpdateStatus($"Step {currentStep}: Simulating player selections...");
            
            // Load available powers
            var powers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            
            if (powers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("No powers found! Creating sample powers...");
                CreateSamplePowersForExample();
                powers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            }
            
            if (powers.Length >= 2 && powerSelectionUI != null)
            {
                // Simulate Player 1 selection
                powerSelectionUI.OnPowerSelected(powers[0]); // First power
                powerSelectionUI.OnRoleSelected(PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Striker);
                
                yield return new WaitForSeconds(stepDelay * 0.5f);
                
                // Simulate Player 2 selection
                powerSelectionUI.OnPowerSelected(powers[1]); // Second power
                powerSelectionUI.OnRoleSelected(PlushLeague.UI.PowerSelection.PowerSelectionUI.RoleType.Goalkeeper);
                
                yield return new WaitForSeconds(stepDelay * 0.5f);
                
                // Mark both players as ready
                powerSelectionUI.OnReadyPressed();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Not enough powers or UI not available for simulation");
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Step 4: Handle any role conflicts
        /// </summary>
        private IEnumerator ExampleStep4_HandleRoleConflicts()
        {
            currentStep = 4;
            UpdateStatus($"Step {currentStep}: Checking for role conflicts...");
            
            // In this example, we set different roles so no conflict should occur
            // In a real scenario, you might need to handle rock-paper-scissors here
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Step 5: Confirm selections and transition to match
        /// </summary>
        private IEnumerator ExampleStep5_StartMatch()
        {
            currentStep = 5;
            UpdateStatus($"Step {currentStep}: Starting match with selected powers...");
            
            // The GameManager should automatically handle the transition
            // from PowerSelection to MatchSetup to MatchActive
            
            yield return new WaitForSeconds(stepDelay * 2);
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(PlushLeague.Core.GameManager.GameState newState)
        {
            UnityEngine.Debug.Log($"[Example] Game state changed to: {newState}");
            
            string stateMessage = newState switch
            {
                PlushLeague.Core.GameManager.GameState.Menu => "In main menu",
                PlushLeague.Core.GameManager.GameState.PowerSelection => "Power selection active",
                PlushLeague.Core.GameManager.GameState.MatchSetup => "Setting up match...",
                PlushLeague.Core.GameManager.GameState.MatchActive => "Match is active!",
                PlushLeague.Core.GameManager.GameState.MatchEnded => "Match ended",
                PlushLeague.Core.GameManager.GameState.PostMatch => "Showing post-match results",
                _ => $"Unknown state: {newState}"
            };
            
            if (!exampleRunning)
            {
                UpdateStatus(stateMessage);
            }
        }
        
        /// <summary>
        /// Handle player configuration completion
        /// </summary>
        private void OnPlayersConfigured(PlushLeague.Core.GameManager.PlayerGameConfig player1, 
                                        PlushLeague.Core.GameManager.PlayerGameConfig player2)
        {
            UnityEngine.Debug.Log($"[Example] Players configured:");
            UnityEngine.Debug.Log($"  Player 1: {player1.playerName} - {player1.selectedPower?.displayName} ({player1.selectedRole})");
            UnityEngine.Debug.Log($"  Player 2: {player2.playerName} - {player2.selectedPower?.displayName} ({player2.selectedRole})");
        }
        
        /// <summary>
        /// Handle match start request
        /// </summary>
        private void OnMatchStartRequested()
        {
            UnityEngine.Debug.Log("[Example] Match start requested");
        }
        
        /// <summary>
        /// Handle power selection confirmation
        /// </summary>
        private void OnSelectionsConfirmed(PlushLeague.UI.PowerSelection.PowerSelectionManager.PlayerSelectionConfig p1,
                                         PlushLeague.UI.PowerSelection.PowerSelectionManager.PlayerSelectionConfig p2)
        {
            UnityEngine.Debug.Log("[Example] Power selections confirmed");
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Update status display
        /// </summary>
        private void UpdateStatus(string message)
        {
            UnityEngine.Debug.Log($"[Example Status] {message}");
            
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
        
        /// <summary>
        /// Create sample powers for the example if none exist
        /// </summary>
        private void CreateSamplePowersForExample()
        {
            UnityEngine.Debug.Log("Creating sample powers for example...");
            
            // This would typically be done in the editor
            // For runtime, you might load from a different source or create programmatically
            #if UNITY_EDITOR
            var setup = gameObject.AddComponent<PlushLeague.Setup.SuperpowerDataSetup>();
            setup.CreateSampleSuperpowers();
            DestroyImmediate(setup);
            #endif
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Start the complete example manually
        /// </summary>
        [ContextMenu("Run Complete Example")]
        public void StartCompleteExample()
        {
            StartCoroutine(RunCompleteExample());
        }
        
        /// <summary>
        /// Start the quick match example manually
        /// </summary>
        [ContextMenu("Run Quick Match Example")]
        public void StartQuickMatchExample()
        {
            StartCoroutine(RunQuickMatchExample());
        }
        
        /// <summary>
        /// Stop any running example
        /// </summary>
        [ContextMenu("Stop Example")]
        public void StopExample()
        {
            StopAllCoroutines();
            exampleRunning = false;
            UpdateStatus("Example stopped.");
        }
        
        #endregion
    }
}
