using UnityEngine;
using System.Collections;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Complete game loop demonstration that shows the full cycle:
    /// Main Menu -> Power Selection -> Match -> Post-Match Summary -> Rematch/Return to Menu
    /// Tests all scene transitions, state management, and persistence features.
    /// </summary>
    public class CompleteGameLoopDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoStartDemo = false;
        [SerializeField] private float stepDelay = 3f;
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool enableContinuousLoop = false;
        
        [Header("Demo Scenarios")]
        [SerializeField] private bool testVictoryScenario = true;
        [SerializeField] private bool testDefeatScenario = true;
        [SerializeField] private bool testRematchFlow = true;
        [SerializeField] private bool testReturnToMenuFlow = true;
        [SerializeField] private bool testErrorHandling = false;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private PlushLeague.UI.Menu.MainMenuUI mainMenuUI;
        [SerializeField] private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        [SerializeField] private PlushLeague.UI.PostMatch.PostMatchUI postMatchUI;
        [SerializeField] private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        [Header("Demo State")]
        [SerializeField] private bool demoRunning = false;
        [SerializeField] private int currentScenario = 0;
        [SerializeField] private int totalLoops = 0;
        [SerializeField] private string currentDemoPhase = "Idle";
        
        // Demo scenarios
        private System.Collections.Generic.List<DemoScenario> scenarios;
        private int currentScenarioIndex = 0;
        
        [System.Serializable]
        public class DemoScenario
        {
            public string name;
            public bool testVictory;
            public bool testRematch;
            public bool includeError;
            public string description;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeDemo();
        }
        
        private void Start()
        {
            if (autoStartDemo)
            {
                StartCoroutine(RunCompleteGameLoopDemo());
            }
        }
        
        #endregion
        
        #region Demo Initialization
        
        /// <summary>
        /// Initialize demo scenarios and find system references
        /// </summary>
        private void InitializeDemo()
        {
            // Find system references
            FindSystemReferences();
            
            // Setup demo scenarios
            SetupDemoScenarios();
            
            LogDemo("Game Loop Demo initialized");
        }
        
        /// <summary>
        /// Find all required system references
        /// </summary>
        private void FindSystemReferences()
        {
            if (gameManager == null)
                gameManager = PlushLeague.Core.GameManager.Instance;
                
            if (gameManager == null)
                gameManager = FindFirstObjectByType<PlushLeague.Core.GameManager>();
                
            if (mainMenuUI == null)
                mainMenuUI = FindFirstObjectByType<PlushLeague.UI.Menu.MainMenuUI>();
                
            if (powerSelectionManager == null)
                powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
                
            if (postMatchUI == null)
                postMatchUI = FindFirstObjectByType<PlushLeague.UI.PostMatch.PostMatchUI>();
                
            if (matchManager == null)
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
        }
        
        /// <summary>
        /// Setup demo scenarios based on configuration
        /// </summary>
        private void SetupDemoScenarios()
        {
            scenarios = new System.Collections.Generic.List<DemoScenario>();
            
            if (testVictoryScenario)
            {
                scenarios.Add(new DemoScenario
                {
                    name = "Victory -> Return to Menu",
                    testVictory = true,
                    testRematch = false,
                    includeError = false,
                    description = "Player wins and returns to main menu"
                });
            }
            
            if (testDefeatScenario)
            {
                scenarios.Add(new DemoScenario
                {
                    name = "Defeat -> Return to Menu",
                    testVictory = false,
                    testRematch = false,
                    includeError = false,
                    description = "Player loses and returns to main menu"
                });
            }
            
            if (testRematchFlow)
            {
                scenarios.Add(new DemoScenario
                {
                    name = "Victory -> Rematch",
                    testVictory = true,
                    testRematch = true,
                    includeError = false,
                    description = "Player wins and requests rematch"
                });
            }
            
            if (testErrorHandling)
            {
                scenarios.Add(new DemoScenario
                {
                    name = "Match Error Handling",
                    testVictory = false,
                    testRematch = false,
                    includeError = true,
                    description = "Test error handling and recovery"
                });
            }
            
            LogDemo($"Setup {scenarios.Count} demo scenarios");
        }
        
        #endregion
        
        #region Demo Execution
        
        /// <summary>
        /// Run the complete game loop demonstration
        /// </summary>
        public IEnumerator RunCompleteGameLoopDemo()
        {
            if (demoRunning)
            {
                LogDemo("Demo already running!");
                yield break;
            }
            
            demoRunning = true;
            LogDemo("=== STARTING COMPLETE GAME LOOP DEMO ===");
            
            do
            {
                // Run all scenarios
                for (currentScenarioIndex = 0; currentScenarioIndex < scenarios.Count; currentScenarioIndex++)
                {
                    var scenario = scenarios[currentScenarioIndex];
                    LogDemo($"\\n--- Running Scenario {currentScenarioIndex + 1}/{scenarios.Count}: {scenario.name} ---");
                    LogDemo($"Description: {scenario.description}");
                    
                    yield return StartCoroutine(RunSingleScenario(scenario));
                    
                    // Wait between scenarios
                    if (currentScenarioIndex < scenarios.Count - 1)
                    {
                        LogDemo("Waiting before next scenario...");
                        yield return new WaitForSeconds(stepDelay * 2);
                    }
                }
                
                totalLoops++;
                LogDemo($"\\n=== COMPLETED DEMO LOOP {totalLoops} ===");
                
                if (enableContinuousLoop)
                {
                    LogDemo("Continuous loop enabled - restarting demo...");
                    yield return new WaitForSeconds(stepDelay);
                }
                
            } while (enableContinuousLoop);
            
            LogDemo("=== DEMO COMPLETE ===");
            currentDemoPhase = "Complete";
            demoRunning = false;
        }
        
        /// <summary>
        /// Run a single demo scenario
        /// </summary>
        private IEnumerator RunSingleScenario(DemoScenario scenario)
        {
            currentDemoPhase = $"Scenario: {scenario.name}";
            
            // Step 1: Ensure we're at main menu
            yield return StartCoroutine(EnsureMainMenuState());
            
            // Step 2: Start new game from menu
            yield return StartCoroutine(DemoStartNewGame());
            
            // Step 3: Complete power selection
            yield return StartCoroutine(DemoPowerSelection());
            
            // Step 4: Simulate match
            yield return StartCoroutine(DemoMatch(scenario));
            
            // Step 5: Handle post-match flow
            yield return StartCoroutine(DemoPostMatch(scenario));
            
            currentDemoPhase = "Scenario Complete";
        }
        
        /// <summary>
        /// Ensure we're in the main menu state
        /// </summary>
        private IEnumerator EnsureMainMenuState()
        {
            currentDemoPhase = "Ensuring Main Menu State";
            LogDemo("STEP: Ensuring Main Menu State");
            
            if (gameManager != null)
            {
                if (gameManager.currentState != PlushLeague.Core.GameManager.GameState.Menu)
                {
                    LogDemo("Not in menu state - returning to menu");
                    gameManager.ReturnToMenu();
                    
                    // Wait for transition
                    yield return new WaitForSeconds(stepDelay);
                }
            }
            
            // Find main menu UI if needed
            if (mainMenuUI == null)
            {
                mainMenuUI = FindFirstObjectByType<PlushLeague.UI.Menu.MainMenuUI>();
            }
            
            if (mainMenuUI != null)
            {
                LogDemo("Main menu found and ready");
            }
            else
            {
                LogDemo("WARNING: Main menu not found");
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Demo starting new game from main menu
        /// </summary>
        private IEnumerator DemoStartNewGame()
        {
            currentDemoPhase = "Starting New Game";
            LogDemo("STEP: Starting New Game");
            
            if (gameManager != null)
            {
                LogDemo("Starting game through GameManager");
                gameManager.StartNewGame(false); // Single player
            }
            else if (mainMenuUI != null)
            {
                LogDemo("Starting game through MainMenuUI");
                mainMenuUI.OnPlayPressed();
            }
            else
            {
                LogDemo("ERROR: No way to start game - GameManager and MainMenuUI not found");
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Demo power selection process
        /// </summary>
        private IEnumerator DemoPowerSelection()
        {
            currentDemoPhase = "Power Selection";
            LogDemo("STEP: Power Selection");
            
            // Find power selection manager if needed
            if (powerSelectionManager == null)
            {
                powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            }
            
            if (powerSelectionManager != null)
            {
                LogDemo("Power Selection Manager found - applying default selections");
                powerSelectionManager.ApplyDefaultSelections();
                
                // Wait for selections to be processed
                yield return new WaitForSeconds(stepDelay);
                
                LogDemo("Power selection complete");
            }
            else
            {
                LogDemo("Power Selection Manager not found - using GameManager quick match");
                if (gameManager != null)
                {
                    gameManager.StartQuickMatch();
                }
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Demo match simulation
        /// </summary>
        private IEnumerator DemoMatch(DemoScenario scenario)
        {
            currentDemoPhase = "Match Simulation";
            LogDemo("STEP: Match Simulation");
            
            if (scenario.includeError)
            {
                LogDemo("Simulating match error...");
                yield return new WaitForSeconds(stepDelay);
                
                if (gameManager != null)
                {
                    gameManager.HandleMatchError("Demo error for testing");
                }
                
                yield break; // Early exit for error scenario
            }
            
            LogDemo("Simulating normal match play...");
            yield return new WaitForSeconds(stepDelay * 2);
            
            // Create mock match result
            var matchResult = new PlushLeague.Core.GameManager.MatchResult(
                won: scenario.testVictory,
                pScore: scenario.testVictory ? 3 : 1,
                oScore: scenario.testVictory ? 1 : 3,
                duration: 300f, // 5 minutes
                goals: scenario.testVictory ? 3 : 1,
                saves: 2,
                powerUses: 5,
                mvp: "Test Player",
                clean: true
            );
            
            LogDemo($"Match finished - Player {(scenario.testVictory ? "WON" : "LOST")} ({matchResult.playerScore}-{matchResult.opponentScore})");
            
            // Send result to game manager
            if (gameManager != null)
            {
                gameManager.OnMatchEnd(matchResult);
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Demo post-match flow
        /// </summary>
        private IEnumerator DemoPostMatch(DemoScenario scenario)
        {
            currentDemoPhase = "Post-Match Flow";
            LogDemo("STEP: Post-Match Flow");
            
            // Find post-match UI if needed
            if (postMatchUI == null)
            {
                postMatchUI = FindFirstObjectByType<PlushLeague.UI.PostMatch.PostMatchUI>();
            }
            
            if (postMatchUI != null)
            {
                LogDemo("Post-match UI found and should be displaying");
            }
            else
            {
                LogDemo("Post-match UI not found - results displayed via GameManager");
            }
            
            // Wait for user to "see" results
            yield return new WaitForSeconds(stepDelay * 2);
            
            // Choose action based on scenario
            if (scenario.testRematch)
            {
                LogDemo("Demo: Requesting rematch");
                if (gameManager != null)
                {
                    gameManager.OnRematchPressed();
                }
                
                // If rematch, we'll go back to match phase
                yield return StartCoroutine(DemoMatch(scenario));
                yield return StartCoroutine(DemoPostMatch(new DemoScenario 
                { 
                    name = "Rematch -> Return to Menu", 
                    testRematch = false, 
                    testVictory = scenario.testVictory 
                }));
            }
            else
            {
                LogDemo("Demo: Returning to main menu");
                if (gameManager != null)
                {
                    gameManager.OnReturnToMenuPressed();
                }
            }
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Start the demo manually
        /// </summary>
        public void StartDemo()
        {
            if (!demoRunning)
            {
                StartCoroutine(RunCompleteGameLoopDemo());
            }
        }
        
        /// <summary>
        /// Stop the demo
        /// </summary>
        public void StopDemo()
        {
            StopAllCoroutines();
            demoRunning = false;
            currentDemoPhase = "Stopped";
            LogDemo("Demo stopped manually");
        }
        
        /// <summary>
        /// Reset demo state
        /// </summary>
        public void ResetDemo()
        {
            StopDemo();
            currentScenarioIndex = 0;
            totalLoops = 0;
            currentDemoPhase = "Reset";
            LogDemo("Demo reset");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Log demo messages
        /// </summary>
        private void LogDemo(string message)
        {
            if (showDebugLogs)
            {
                UnityEngine.Debug.Log($"[GAME LOOP DEMO] {message}");
            }
        }
        
        /// <summary>
        /// Get demo status for UI
        /// </summary>
        public string GetDemoStatus()
        {
            if (!demoRunning) return "Demo not running";
            
            string status = $"Phase: {currentDemoPhase}\\n";
            status += $"Scenario: {currentScenarioIndex + 1}/{scenarios.Count}\\n";
            status += $"Loops: {totalLoops}\\n";
            
            if (scenarios.Count > currentScenarioIndex)
            {
                status += $"Current: {scenarios[currentScenarioIndex].name}";
            }
            
            return status;
        }
        
        #endregion
        
        #region Debug GUI
        
        [Header("Debug GUI")]
        [SerializeField] private bool enableDebugGUI = true;
        
        private void OnGUI()
        {
            if (!enableDebugGUI || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.Label("=== Complete Game Loop Demo ===");
            
            // Demo status
            GUILayout.Space(10);
            GUILayout.Label("DEMO STATUS:");
            GUILayout.Label($"Running: {demoRunning}");
            GUILayout.Label($"Phase: {currentDemoPhase}");
            GUILayout.Label($"Scenario: {currentScenarioIndex + 1}/{scenarios?.Count ?? 0}");
            GUILayout.Label($"Loops: {totalLoops}");
            
            // Control buttons
            GUILayout.Space(10);
            if (!demoRunning)
            {
                if (GUILayout.Button("Start Complete Demo"))
                {
                    StartDemo();
                }
                
                if (GUILayout.Button("Start Single Victory Test"))
                {
                    currentScenarioIndex = 0;
                    var testScenario = new DemoScenario 
                    { 
                        name = "Quick Victory Test", 
                        testVictory = true, 
                        testRematch = false,
                        includeError = false
                    };
                    StartCoroutine(RunSingleScenario(testScenario));
                }
                
                if (GUILayout.Button("Start Single Defeat Test"))
                {
                    currentScenarioIndex = 0;
                    var testScenario = new DemoScenario 
                    { 
                        name = "Quick Defeat Test", 
                        testVictory = false, 
                        testRematch = false,
                        includeError = false 
                    };
                    StartCoroutine(RunSingleScenario(testScenario));
                }
            }
            else
            {
                if (GUILayout.Button("Stop Demo"))
                {
                    StopDemo();
                }
            }
            
            if (GUILayout.Button("Reset Demo"))
            {
                ResetDemo();
            }
            
            // System status
            GUILayout.Space(10);
            GUILayout.Label("SYSTEM STATUS:");
            GUILayout.Label($"GameManager: {(gameManager != null ? $"Found ({gameManager.currentState})" : "Missing")}");
            GUILayout.Label($"MainMenuUI: {(mainMenuUI != null ? "Found" : "Missing")}");
            GUILayout.Label($"PowerSelection: {(powerSelectionManager != null ? "Found" : "Missing")}");
            GUILayout.Label($"PostMatchUI: {(postMatchUI != null ? "Found" : "Missing")}");
            GUILayout.Label($"MatchManager: {(matchManager != null ? "Found" : "Missing")}");
            
            // Configuration
            GUILayout.Space(10);
            GUILayout.Label("CONFIGURATION:");
            enableContinuousLoop = GUILayout.Toggle(enableContinuousLoop, "Continuous Loop");
            showDebugLogs = GUILayout.Toggle(showDebugLogs, "Show Debug Logs");
            
            GUILayout.Label($"Step Delay: {stepDelay:F1}s");
            stepDelay = GUILayout.HorizontalSlider(stepDelay, 0.5f, 5f);
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
