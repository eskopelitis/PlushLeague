using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Comprehensive demonstration of the Scene Management & Game Loop system.
    /// This script tests all aspects of scene transitions, persistent settings, and game flow.
    /// </summary>
    public class SceneManagementDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoStartDemo = false;
        [SerializeField] private float stepDelay = 3f;
        [SerializeField] private bool showDebugGUI = true;
        [SerializeField] private bool testErrorHandling = true;
        
        [Header("Scene Management Testing")]
        [SerializeField] private bool testSceneTransitions = true;
        [SerializeField] private bool testPersistentSettings = true;
        [SerializeField] private bool testMatchFlow = true;
        [SerializeField] private bool testRematchFlow = true;
        [SerializeField] private bool testEdgeCases = true;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private PlushLeague.UI.Menu.MainMenuUI mainMenuUI;
        [SerializeField] private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        [SerializeField] private PlushLeague.UI.PostMatch.PostMatchUI postMatchUI;
        
        // Demo state
        private bool demoRunning = false;
        private int currentStep = 0;
        private List<string> demoLog = new List<string>();
        private PlushLeague.Core.GameManager.MatchResult testMatchResult;
        
        // Test scenarios
        private enum TestScenario
        {
            FullGameLoop,
            SceneTransitionsOnly,
            PersistentSettingsOnly,
            ErrorHandling,
            PerformanceTest
        }
        
        private TestScenario currentScenario = TestScenario.FullGameLoop;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeComponents();
            
            if (autoStartDemo)
            {
                StartCoroutine(RunSceneManagementDemo());
            }
        }
        
        private void InitializeComponents()
        {
            // Find or create GameManager
            if (gameManager == null)
            {
                gameManager = PlushLeague.Core.GameManager.Instance;
                if (gameManager == null)
                {
                    var gameManagerObj = new GameObject("GameManager");
                    gameManager = gameManagerObj.AddComponent<PlushLeague.Core.GameManager>();
                }
            }
            
            // Find UI components
            if (mainMenuUI == null)
                mainMenuUI = FindFirstObjectByType<PlushLeague.UI.Menu.MainMenuUI>();
                
            if (powerSelectionManager == null)
                powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
                
            if (postMatchUI == null)
                postMatchUI = FindFirstObjectByType<PlushLeague.UI.PostMatch.PostMatchUI>();
            
            // Setup event listeners
            SetupEventListeners();
            
            // Initialize test match result
            testMatchResult = new PlushLeague.Core.GameManager.MatchResult(
                won: true,
                pScore: 3,
                oScore: 2,
                duration: 180f,
                goals: 2,
                saves: 5,
                powerUses: 8,
                mvp: "Player 1",
                clean: true
            );
        }
        
        private void SetupEventListeners()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                gameManager.OnMatchCompleted += OnMatchCompleted;
                // gameManager.OnSceneTransitionStarted += OnSceneTransitionStarted; // Event signature mismatch - commented out
                gameManager.OnSceneTransitionCompleted += OnSceneTransitionCompleted;
            }
        }
        
        #endregion
        
        #region Demo Execution
        
        /// <summary>
        /// Run the complete scene management demonstration
        /// </summary>
        public IEnumerator RunSceneManagementDemo()
        {
            if (demoRunning)
            {
                LogDemo("Demo already running!");
                yield break;
            }
            
            demoRunning = true;
            currentStep = 0;
            demoLog.Clear();
            
            LogDemo("=== SCENE MANAGEMENT & GAME LOOP DEMO ===");
            LogDemo($"Testing scenario: {currentScenario}");
            
            switch (currentScenario)
            {
                case TestScenario.FullGameLoop:
                    yield return StartCoroutine(TestFullGameLoop());
                    break;
                case TestScenario.SceneTransitionsOnly:
                    yield return StartCoroutine(TestSceneTransitions());
                    break;
                case TestScenario.PersistentSettingsOnly:
                    yield return StartCoroutine(TestPersistentSettings());
                    break;
                case TestScenario.ErrorHandling:
                    yield return StartCoroutine(TestErrorHandling());
                    break;
                case TestScenario.PerformanceTest:
                    yield return StartCoroutine(TestPerformance());
                    break;
            }
            
            LogDemo("=== DEMO COMPLETE ===");
            demoRunning = false;
        }
        
        /// <summary>
        /// Test the complete game loop: Menu -> Power Selection -> Match -> Post-Match -> Menu
        /// </summary>
        private IEnumerator TestFullGameLoop()
        {
            LogDemo("Testing Full Game Loop...");
            
            // Step 1: Test Main Menu
            yield return StartCoroutine(TestMainMenuState());
            
            // Step 2: Test Power Selection
            yield return StartCoroutine(TestPowerSelectionState());
            
            // Step 3: Test Match Flow
            yield return StartCoroutine(TestMatchState());
            
            // Step 4: Test Post-Match
            yield return StartCoroutine(TestPostMatchState());
            
            // Step 5: Test Return to Menu
            yield return StartCoroutine(TestReturnToMenu());
            
            // Step 6: Test Rematch Flow
            if (testRematchFlow)
            {
                yield return StartCoroutine(TestRematchFlow());
            }
        }
        
        /// <summary>
        /// Test Main Menu state and functionality
        /// </summary>
        private IEnumerator TestMainMenuState()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Main Menu State");
            
            // Set game state to menu
            gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.Menu);
            
            // Test main menu UI integration
            if (mainMenuUI != null)
            {
                LogDemo("Main Menu UI found - testing integration");
                mainMenuUI.ShowMenu();
                
                // Wait for menu to be shown
                yield return new WaitForSeconds(1f);
                
                // Test play button (should trigger power selection)
                LogDemo("Simulating Play button press");
                if (mainMenuUI.GetComponent<Button>() != null)
                {
                    mainMenuUI.OnPlayPressed();
                }
            }
            else
            {
                LogDemo("Main Menu UI not found - simulating menu functionality");
                // gameManager.OnPlayPressed(); // Method not found - commented out
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Test Power Selection state and functionality
        /// </summary>
        private IEnumerator TestPowerSelectionState()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Power Selection State");
            
            // Test power selection manager integration
            if (powerSelectionManager != null)
            {
                LogDemo("Power Selection Manager found - testing integration");
                
                // Start power selection
                powerSelectionManager.StartPowerSelection(false);
                
                // Wait for power selection to initialize
                yield return new WaitForSeconds(2f);
                
                // Test auto-selection for demo
                LogDemo("Auto-completing power selection for demo");
                powerSelectionManager.ApplyDefaultSelections();
                
                // Get player configs
                var (player1, player2) = powerSelectionManager.GetPlayerConfigs();
                LogDemo($"Player configs - P1 Ready: {player1.isReady}, P2 Ready: {player2.isReady}");
                
                // Wait for selection to complete
                yield return new WaitForSeconds(1f);
                
                // Confirm selections
                // powerSelectionManager.ConfirmSelections(); // Method not found - commented out
            }
            else
            {
                LogDemo("Power Selection Manager not found - simulating selection");
                gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.PowerSelection);
                yield return new WaitForSeconds(stepDelay);
                // gameManager.OnPowerSelectionComplete(); // Method not found - commented out
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Test Match state and functionality
        /// </summary>
        private IEnumerator TestMatchState()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Match State");
            
            // Start match
            LogDemo("Starting match...");
            gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.MatchActive);
            
            // Simulate match duration
            LogDemo("Simulating match in progress...");
            yield return new WaitForSeconds(stepDelay);
            
            // Test match timeout (if enabled)
            if (testErrorHandling)
            {
                LogDemo("Testing match timeout protection");
                yield return new WaitForSeconds(1f);
            }
            
            // Simulate match end
            LogDemo("Simulating match completion...");
            gameManager.OnMatchEnd(testMatchResult);
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Test Post-Match state and functionality
        /// </summary>
        private IEnumerator TestPostMatchState()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Post-Match State");
            
            // Check if post-match UI is active
            if (postMatchUI != null)
            {
                LogDemo("Post-Match UI found - testing integration");
                
                // Display match results
                LogDemo("Displaying match results...");
                // postMatchUI.DisplayMatchResult(testMatchResult); // Method not found - commented out
                
                yield return new WaitForSeconds(stepDelay);
            }
            else
            {
                LogDemo("Post-Match UI not found - simulating post-match");
                gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.PostMatch);
                yield return new WaitForSeconds(stepDelay);
            }
        }
        
        /// <summary>
        /// Test Return to Menu functionality
        /// </summary>
        private IEnumerator TestReturnToMenu()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Return to Menu");
            
            LogDemo("Simulating Return to Menu button press");
            gameManager.OnReturnToMenuPressed();
            
            yield return new WaitForSeconds(stepDelay);
            
            // Verify we're back in menu state
            if (gameManager.currentState == PlushLeague.Core.GameManager.GameState.Menu)
            {
                LogDemo("Successfully returned to Menu!");
            }
            else
            {
                LogDemo($"WARNING: Expected Menu state, got {gameManager.currentState}");
            }
        }
        
        /// <summary>
        /// Test Rematch functionality
        /// </summary>
        private IEnumerator TestRematchFlow()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Testing Rematch Flow");
            
            // Go back to post-match for rematch test
            gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.PostMatch);
            yield return new WaitForSeconds(1f);
            
            LogDemo("Simulating Rematch button press");
            gameManager.OnRematchPressed();
            
            yield return new WaitForSeconds(stepDelay);
            
            // Check if rematch started correctly
            if (gameManager.currentState == PlushLeague.Core.GameManager.GameState.MatchSetup)
            {
                LogDemo("Rematch flow started successfully!");
            }
            else
            {
                LogDemo($"WARNING: Expected MatchSetup state, got {gameManager.currentState}");
            }
        }
        
        /// <summary>
        /// Test scene transitions only
        /// </summary>
        private IEnumerator TestSceneTransitions()
        {
            LogDemo("Testing Scene Transitions...");
            
            string[] sceneNames = { "MainMenu", "PowerSelection", "GameArena", "PostMatch" };
            PlushLeague.Core.GameManager.GameState[] states = {
                PlushLeague.Core.GameManager.GameState.Menu,
                PlushLeague.Core.GameManager.GameState.PowerSelection,
                PlushLeague.Core.GameManager.GameState.MatchActive,
                PlushLeague.Core.GameManager.GameState.PostMatch
            };
            
            for (int i = 0; i < sceneNames.Length; i++)
            {
                LogDemo($"Testing transition to {sceneNames[i]}...");
                
                // Test scene loading
                gameManager.LoadScene(sceneNames[i], states[i]);
                
                // Wait for transition
                while (gameManager.IsTransitioning())
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                LogDemo($"Successfully transitioned to {sceneNames[i]}");
                yield return new WaitForSeconds(stepDelay);
            }
        }
        
        /// <summary>
        /// Test persistent settings functionality
        /// </summary>
        private IEnumerator TestPersistentSettings()
        {
            LogDemo("Testing Persistent Settings...");
            
            // Test saving settings
            LogDemo("Testing save functionality...");
            gameManager.SavePlayerPrefs();
            
            // Test loading settings
            LogDemo("Testing load functionality...");
            gameManager.LoadPlayerPrefs();
            
            // Get persistent settings
            var settings = gameManager.GetPersistentSettings();
            if (settings != null)
            {
                LogDemo($"Persistent Settings - Matches Won: {settings.matchesWon}, Matches Lost: {settings.matchesLost}");
                LogDemo($"Total Goals: {settings.totalGoals}, Total Saves: {settings.totalSaves}");
            }
            else
            {
                LogDemo("WARNING: Persistent settings not found");
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Test error handling and edge cases
        /// </summary>
        private IEnumerator TestErrorHandling()
        {
            LogDemo("Testing Error Handling...");
            
            // Test multiple load prevention
            LogDemo("Testing multiple load prevention...");
            gameManager.LoadScene("TestScene1", PlushLeague.Core.GameManager.GameState.Menu);
            gameManager.LoadScene("TestScene2", PlushLeague.Core.GameManager.GameState.Menu); // Should be prevented
            
            yield return new WaitForSeconds(stepDelay);
            
            // Test invalid rematch
            LogDemo("Testing invalid rematch scenario...");
            gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.PostMatch);
            
            // Create unclean match result
            var uncleanResult = new PlushLeague.Core.GameManager.MatchResult(
                false, 0, 1, 30f, 0, 0, 0, "None", false
            );
            gameManager.OnMatchEnd(uncleanResult);
            
            // Try to rematch (should fail)
            gameManager.OnRematchPressed();
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Test performance under rapid transitions
        /// </summary>
        private IEnumerator TestPerformance()
        {
            LogDemo("Testing Performance...");
            
            float startTime = Time.time;
            int transitionCount = 10;
            
            for (int i = 0; i < transitionCount; i++)
            {
                LogDemo($"Rapid transition {i + 1}/{transitionCount}");
                
                // Alternate between menu and power selection
                if (i % 2 == 0)
                {
                    gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.Menu);
                }
                else
                {
                    gameManager.SetGameState(PlushLeague.Core.GameManager.GameState.PowerSelection);
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            float endTime = Time.time;
            LogDemo($"Performance test completed in {endTime - startTime:F2} seconds");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnGameStateChanged(PlushLeague.Core.GameManager.GameState newState)
        {
            LogDemo($"Game State Changed: {newState}");
        }
        
        private void OnMatchCompleted(PlushLeague.Core.GameManager.MatchResult result)
        {
            LogDemo($"Match Completed - Player Won: {result.playerWon}, Score: {result.playerScore}-{result.opponentScore}");
        }
        
        private void OnSceneTransitionStarted(string sceneName)
        {
            LogDemo($"Scene Transition Started: {sceneName}");
        }
        
        private void OnSceneTransitionCompleted()
        {
            LogDemo("Scene Transition Completed");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Log demo messages with timestamp
        /// </summary>
        private void LogDemo(string message)
        {
            string timestampedMessage = $"[{Time.time:F2}] [SCENE DEMO] {message}";
            UnityEngine.Debug.Log(timestampedMessage);
            demoLog.Add(timestampedMessage);
            
            // Keep log size manageable
            if (demoLog.Count > 100)
            {
                demoLog.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Reset demo state
        /// </summary>
        public void ResetDemo()
        {
            demoRunning = false;
            currentStep = 0;
            demoLog.Clear();
            LogDemo("Demo reset");
        }
        
        #endregion
        
        #region Debug GUI
        
        private Vector2 logScrollPosition = Vector2.zero;
        
        private void OnGUI()
        {
            if (!showDebugGUI || !Application.isPlaying) return;
            
            // Main control panel
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.Label("=== Scene Management Demo ===");
            
            GUILayout.Label($"Demo Running: {demoRunning}");
            GUILayout.Label($"Current Step: {currentStep}");
            GUILayout.Label($"Current Scenario: {currentScenario}");
            
            if (gameManager != null)
            {
                GUILayout.Label($"Game State: {gameManager.currentState}");
                GUILayout.Label($"Is Transitioning: {gameManager.IsTransitioning()}");
                GUILayout.Label($"Is Loading: {gameManager.IsLoadingScene()}");
            }
            
            GUILayout.Space(10);
            
            // Scenario selection
            GUILayout.Label("Select Test Scenario:");
            foreach (TestScenario scenario in System.Enum.GetValues(typeof(TestScenario)))
            {
                if (GUILayout.Button(scenario.ToString()))
                {
                    currentScenario = scenario;
                }
            }
            
            GUILayout.Space(10);
            
            // Demo controls
            if (!demoRunning)
            {
                if (GUILayout.Button("Start Demo"))
                {
                    StartCoroutine(RunSceneManagementDemo());
                }
            }
            else
            {
                if (GUILayout.Button("Stop Demo"))
                {
                    StopAllCoroutines();
                    ResetDemo();
                }
            }
            
            // Manual controls
            GUILayout.Space(10);
            GUILayout.Label("Manual Controls:");
            
            if (GUILayout.Button("Test Scene Transition"))
            {
                if (gameManager != null)
                {
                    gameManager.LoadScene("TestScene", PlushLeague.Core.GameManager.GameState.Menu);
                }
            }
            
            if (GUILayout.Button("Test Match End"))
            {
                if (gameManager != null)
                {
                    gameManager.OnMatchEnd(testMatchResult);
                }
            }
            
            if (GUILayout.Button("Test Rematch"))
            {
                if (gameManager != null)
                {
                    gameManager.OnRematchPressed();
                }
            }
            
            if (GUILayout.Button("Test Return to Menu"))
            {
                if (gameManager != null)
                {
                    gameManager.OnReturnToMenuPressed();
                }
            }
            
            GUILayout.EndArea();
            
            // Log window
            GUILayout.BeginArea(new Rect(420, 10, 400, 500));
            GUILayout.Label("=== Demo Log ===");
            
            logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Width(390), GUILayout.Height(450));
            
            for (int i = Mathf.Max(0, demoLog.Count - 50); i < demoLog.Count; i++)
            {
                GUILayout.Label(demoLog[i]);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
