using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Scene controller for the Step 13 Debug Scene.
    /// Sets up and manages all debug tools for comprehensive playtesting.
    /// </summary>
    public class Step13DebugScene : MonoBehaviour
    {
        [Header("Debug Tools Setup")]
        [SerializeField] private GameObject debugToolsPrefab;
        [SerializeField] private GameObject analyticsSystemPrefab;
        [SerializeField] private GameObject testFrameworkPrefab;
        [SerializeField] private GameObject dashboardPrefab;
        [SerializeField] private GameObject bugReportingPrefab;
        
        [Header("Game Components")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private GameObject arenaPrefab;
        
        [Header("Configuration")]
        [SerializeField] private bool autoStartPlaytesting = true;
        [SerializeField] private bool showWelcomeMessage = true;
        [SerializeField] private bool enableAllDebugTools = true;
        [SerializeField] private float welcomeMessageDuration = 5f;
        
        [Header("Demo Settings")]
        [SerializeField] private bool runDemoMatch = true;
        [SerializeField] private bool simulatePlayerActions = true;
        [SerializeField] private float demoMatchDuration = 120f;
        
        // Scene components
        private Step13Implementation step13Implementation;
        private AutomatedTestFramework testFramework;
        private PlaytestingAnalytics analytics;
        private PlaytestingManager playtestingManager;
        private DebugDashboard debugDashboard;
        private BugReportUI bugReportUI;
        
        // Demo state
        private bool demoActive = false;
        private Coroutine demoCoroutine;
        
        private void Start()
        {
            StartCoroutine(InitializeDebugScene());
        }
        
        /// <summary>
        /// Initialize the debug scene
        /// </summary>
        private IEnumerator InitializeDebugScene()
        {
            Debug.Log("=== INITIALIZING STEP 13 DEBUG SCENE ===");
            
            // Show welcome message
            if (showWelcomeMessage)
            {
                ShowWelcomeMessage();
                yield return new WaitForSeconds(welcomeMessageDuration);
            }
            
            // Setup game environment
            yield return StartCoroutine(SetupGameEnvironment());
            
            // Setup debug tools
            yield return StartCoroutine(SetupDebugTools());
            
            // Configure tools
            yield return StartCoroutine(ConfigureDebugTools());
            
            // Start playtesting
            if (autoStartPlaytesting)
            {
                StartPlaytesting();
            }
            
            // Start demo if enabled
            if (runDemoMatch)
            {
                yield return new WaitForSeconds(2f);
                StartDemo();
            }
            
            Debug.Log("=== STEP 13 DEBUG SCENE INITIALIZATION COMPLETE ===");
        }
        
        /// <summary>
        /// Setup the game environment
        /// </summary>
        private IEnumerator SetupGameEnvironment()
        {
            Debug.Log("Setting up game environment...");
            
            // Instantiate core game components
            if (gameManagerPrefab != null)
            {
                GameObject gameManager = Instantiate(gameManagerPrefab);
                gameManager.name = "GameManager";
            }
            
            if (arenaPrefab != null)
            {
                GameObject arena = Instantiate(arenaPrefab);
                arena.name = "Arena";
            }
            
            if (playerPrefab != null)
            {
                GameObject player1 = Instantiate(playerPrefab, new Vector3(-3f, 0f, 0f), Quaternion.identity);
                player1.name = "Player1";
                
                GameObject player2 = Instantiate(playerPrefab, new Vector3(3f, 0f, 0f), Quaternion.identity);
                player2.name = "Player2";
            }
            
            if (ballPrefab != null)
            {
                GameObject ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
                ball.name = "Ball";
            }
            
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Game environment setup complete");
        }
        
        /// <summary>
        /// Setup all debug tools
        /// </summary>
        private IEnumerator SetupDebugTools()
        {
            Debug.Log("Setting up debug tools...");
            
            // Core debug tools
            if (debugToolsPrefab != null)
            {
                GameObject debugTools = Instantiate(debugToolsPrefab);
                debugTools.name = "DebugTools";
                step13Implementation = debugTools.GetComponent<Step13Implementation>();
                playtestingManager = debugTools.GetComponent<PlaytestingManager>();
            }
            
            // Analytics system
            if (analyticsSystemPrefab != null)
            {
                GameObject analyticsSystem = Instantiate(analyticsSystemPrefab);
                analyticsSystem.name = "AnalyticsSystem";
                analytics = analyticsSystem.GetComponent<PlaytestingAnalytics>();
            }
            
            // Test framework
            if (testFrameworkPrefab != null)
            {
                GameObject testFramework = Instantiate(testFrameworkPrefab);
                testFramework.name = "TestFramework";
                this.testFramework = testFramework.GetComponent<AutomatedTestFramework>();
            }
            
            // Dashboard
            if (dashboardPrefab != null)
            {
                GameObject dashboard = Instantiate(dashboardPrefab);
                dashboard.name = "DebugDashboard";
                debugDashboard = dashboard.GetComponent<DebugDashboard>();
            }
            
            // Bug reporting
            if (bugReportingPrefab != null)
            {
                GameObject bugReporting = Instantiate(bugReportingPrefab);
                bugReporting.name = "BugReporting";
                bugReportUI = bugReporting.GetComponent<BugReportUI>();
            }
            
            yield return new WaitForSeconds(1f);
            Debug.Log("Debug tools setup complete");
        }
        
        /// <summary>
        /// Configure debug tools for optimal playtesting
        /// </summary>
        private IEnumerator ConfigureDebugTools()
        {
            Debug.Log("Configuring debug tools...");
            
            // Configure Step 13 implementation
            if (step13Implementation != null)
            {
                // Enable all testing features
                step13Implementation.enabled = enableAllDebugTools;
                
                // Start Step 13 implementation
                if (enableAllDebugTools)
                {
                    step13Implementation.StartStep13Implementation();
                }
            }
            
            // Configure analytics
            if (analytics != null)
            {
                analytics.StartAnalytics();
            }
            
            // Configure test framework
            if (testFramework != null && enableAllDebugTools)
            {
                // Don't auto-start testing immediately, let the scene settle
                yield return new WaitForSeconds(5f);
                testFramework.StartTesting();
            }
            
            yield return new WaitForSeconds(1f);
            Debug.Log("Debug tools configuration complete");
        }
        
        /// <summary>
        /// Start playtesting session
        /// </summary>
        private void StartPlaytesting()
        {
            Debug.Log("Starting playtesting session...");
            
            if (playtestingManager != null)
            {
                playtestingManager.StartPlaytestSession();
            }
            
            Debug.Log("Playtesting session started");
        }
        
        /// <summary>
        /// Start demo match
        /// </summary>
        private void StartDemo()
        {
            if (demoActive)
            {
                Debug.LogWarning("Demo already active");
                return;
            }
            
            Debug.Log("Starting demo match...");
            demoActive = true;
            demoCoroutine = StartCoroutine(RunDemoMatch());
        }
        
        /// <summary>
        /// Run a demo match with simulated actions
        /// </summary>
        private IEnumerator RunDemoMatch()
        {
            float demoStartTime = Time.time;
            
            Debug.Log("=== DEMO MATCH STARTED ===");
            
            while (Time.time - demoStartTime < demoMatchDuration)
            {
                if (simulatePlayerActions)
                {
                    // Simulate player actions periodically
                    yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));
                    SimulatePlayerAction();
                }
                
                yield return null;
            }
            
            Debug.Log("=== DEMO MATCH ENDED ===");
            demoActive = false;
            
            // Generate match report
            if (playtestingManager != null)
            {
                var matchData = new PlaytestingManager.MatchData
                {
                    matchId = System.Guid.NewGuid().ToString(),
                    duration = demoMatchDuration,
                    winner = UnityEngine.Random.Range(0, 2) == 0 ? "Player1" : "Player2",
                    loser = UnityEngine.Random.Range(0, 2) == 0 ? "Player1" : "Player2",
                    events = new System.Collections.Generic.List<PlaytestingManager.GameEvent>()
                };
                
                // Simulate some events
                for (int i = 0; i < UnityEngine.Random.Range(10, 25); i++)
                {
                    matchData.events.Add(new PlaytestingManager.GameEvent
                    {
                        eventType = GetRandomEventType(),
                        timestamp = UnityEngine.Random.Range(0f, demoMatchDuration),
                        data = "Demo event"
                    });
                }
                
                PlaytestingManager.OnMatchCompleted?.Invoke(matchData);
            }
        }
        
        /// <summary>
        /// Simulate a player action
        /// </summary>
        private void SimulatePlayerAction()
        {
            string[] actions = { "Move", "Jump", "Dash", "Kick", "Block", "Special" };
            string action = actions[UnityEngine.Random.Range(0, actions.Length)];
            
            Debug.Log($"Simulated player action: {action}");
            
            // Record action in analytics
            if (analytics != null)
            {
                analytics.UpdateMetric("PlayerActions", "Gameplay", 1f);
            }
        }
        
        /// <summary>
        /// Get a random event type for demo purposes
        /// </summary>
        private string GetRandomEventType()
        {
            string[] eventTypes = { "Goal", "Save", "Collision", "PowerUp", "Foul", "Timeout" };
            return eventTypes[UnityEngine.Random.Range(0, eventTypes.Length)];
        }
        
        /// <summary>
        /// Show welcome message
        /// </summary>
        private void ShowWelcomeMessage()
        {
            Debug.Log("=== WELCOME TO STEP 13 DEBUG SCENE ===");
            Debug.Log("This scene demonstrates comprehensive playtesting tools:");
            Debug.Log("• Press F1 to open Debug Dashboard");
            Debug.Log("• Press F2 to run test suite");
            Debug.Log("• Press F3 to report bugs");
            Debug.Log("• Press F4 to open parameter tuning");
            Debug.Log("• Press F5 for emergency reset");
            Debug.Log("========================================");
            
            // Show on-screen message if GUI is available
            StartCoroutine(ShowOnScreenWelcome());
        }
        
        /// <summary>
        /// Show on-screen welcome message
        /// </summary>
        private IEnumerator ShowOnScreenWelcome()
        {
            float startTime = Time.time;
            
            while (Time.time - startTime < welcomeMessageDuration)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// Handle scene-specific input
        /// </summary>
        private void Update()
        {
            // Handle additional debug keys
            if (Input.GetKeyDown(KeyCode.F6))
            {
                ToggleDemo();
            }
            
            if (Input.GetKeyDown(KeyCode.F7))
            {
                GenerateTestBug();
            }
            
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ShowSceneInfo();
            }
            
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadMainMenu();
            }
        }
        
        /// <summary>
        /// Toggle demo match
        /// </summary>
        private void ToggleDemo()
        {
            if (demoActive)
            {
                StopDemo();
            }
            else
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// Stop demo match
        /// </summary>
        private void StopDemo()
        {
            if (demoCoroutine != null)
            {
                StopCoroutine(demoCoroutine);
                demoCoroutine = null;
            }
            
            demoActive = false;
            Debug.Log("Demo match stopped");
        }
        
        /// <summary>
        /// Generate a test bug report
        /// </summary>
        private void GenerateTestBug()
        {
            if (playtestingManager != null)
            {
                string[] bugTypes = { "Gameplay Bug", "UI Bug", "Performance Issue", "Audio Bug" };
                string[] descriptions = { 
                    "Ball gets stuck in wall", 
                    "Player animation glitches", 
                    "Frame rate drops during special effects",
                    "Audio cuts out during intense moments"
                };
                
                int index = UnityEngine.Random.Range(0, bugTypes.Length);
                playtestingManager.RecordBugReport(descriptions[index], bugTypes[index], "Medium");
                
                Debug.Log($"Test bug reported: {bugTypes[index]} - {descriptions[index]}");
            }
        }
        
        /// <summary>
        /// Show scene information
        /// </summary>
        private void ShowSceneInfo()
        {
            Debug.Log("=== STEP 13 DEBUG SCENE INFO ===");
            Debug.Log($"Scene: {SceneManager.GetActiveScene().name}");
            Debug.Log($"Demo Active: {demoActive}");
            Debug.Log($"Debug Tools Active: {enableAllDebugTools}");
            Debug.Log($"Components Active:");
            Debug.Log($"  - Step 13 Implementation: {step13Implementation != null}");
            Debug.Log($"  - Test Framework: {testFramework != null}");
            Debug.Log($"  - Analytics: {analytics != null}");
            Debug.Log($"  - Playtesting Manager: {playtestingManager != null}");
            Debug.Log($"  - Debug Dashboard: {debugDashboard != null}");
            Debug.Log($"  - Bug Report UI: {bugReportUI != null}");
            Debug.Log("================================");
        }
        
        /// <summary>
        /// Load main menu
        /// </summary>
        private void LoadMainMenu()
        {
            Debug.Log("Loading main menu...");
            
            // Stop all debug activities
            if (step13Implementation != null)
            {
                step13Implementation.StopStep13Implementation();
            }
            
            if (analytics != null)
            {
                analytics.StopAnalytics();
            }
            
            if (testFramework != null)
            {
                testFramework.StopTesting();
            }
            
            StopDemo();
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// GUI for debug scene
        /// </summary>
        private void OnGUI()
        {
            // Show welcome message
            if (showWelcomeMessage && Time.time < welcomeMessageDuration)
            {
                GUI.Box(new Rect(10, 10, 400, 150), "");
                GUI.Label(new Rect(20, 20, 380, 30), "Welcome to Step 13 Debug Scene!");
                GUI.Label(new Rect(20, 50, 380, 20), "Press F1 - Debug Dashboard");
                GUI.Label(new Rect(20, 70, 380, 20), "Press F2 - Run Tests");
                GUI.Label(new Rect(20, 90, 380, 20), "Press F3 - Bug Report");
                GUI.Label(new Rect(20, 110, 380, 20), "Press F6 - Toggle Demo");
                GUI.Label(new Rect(20, 130, 380, 20), "Press F9 - Main Menu");
            }
            
            // Show scene status
            GUI.Box(new Rect(Screen.width - 210, 10, 200, 120), "");
            GUI.Label(new Rect(Screen.width - 200, 20, 180, 20), "Step 13 Debug Scene");
            GUI.Label(new Rect(Screen.width - 200, 40, 180, 20), $"Demo: {(demoActive ? "Active" : "Inactive")}");
            GUI.Label(new Rect(Screen.width - 200, 60, 180, 20), $"Tools: {(enableAllDebugTools ? "Enabled" : "Disabled")}");
            GUI.Label(new Rect(Screen.width - 200, 80, 180, 20), $"FPS: {(1f / Time.deltaTime):F1}");
            
            if (GUI.Button(new Rect(Screen.width - 200, 100, 80, 25), "Main Menu"))
            {
                LoadMainMenu();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            StopDemo();
        }
    }
}
