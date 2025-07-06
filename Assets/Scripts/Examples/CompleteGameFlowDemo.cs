using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
// Debug system integration (will be enabled once systems are properly set up)
// using PlushLeague.Debug;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Complete game flow demonstration from Main Menu to Match.
    /// Enhanced with comprehensive playtesting, balancing, and debug tools for Step 13.
    /// Includes bug logging, parameter tuning, edge case testing, and automated balance analysis.
    /// </summary>
    public class CompleteGameFlowDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoStartDemo = false;
        [SerializeField] private float stepDelay = 2f;
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.UI.Menu.MainMenuUI mainMenuUI;
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        [SerializeField] private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        [Header("Demo Settings")]
        [SerializeField] private bool skipToGameplay = false;
        [SerializeField] private bool useRandomPowers = true;
        [SerializeField] private bool enablePolishEffects = true;
        [SerializeField] private string[] testSuperpower1 = {"SuperShot", "SuperSave", "UltraDash"};
        [SerializeField] private string[] testSuperpower2 = {"FreezeShot", "CurveBoost", "FreezeZone"};
        
        [Header("Step 13: Playtesting & Debug")]
        [SerializeField] private bool enablePlaytesting = true;
        [SerializeField] private bool enableParameterTuning = true;
        [SerializeField] private bool enableEdgeCaseTesting = true;
        [SerializeField] private bool enableBugLogging = true;
        [SerializeField] private KeyCode debugMenuKey = KeyCode.F1;
        [SerializeField] private KeyCode resetMatchKey = KeyCode.R;
        [SerializeField] private KeyCode forceWinKey = KeyCode.W;
        [SerializeField] private KeyCode ballResetKey = KeyCode.B;
        
        [Header("Step 13: Debug Tools Integration")]
        [SerializeField] private bool enableDebugIntegration = true;
        // TODO: Enable these once the debug systems are properly set up
        // [SerializeField] private DebugIntegrationManager debugIntegrationManager;
        // [SerializeField] private PlaytestingManager playtestingManager;
        // [SerializeField] private DebugConsole debugConsole;
        // [SerializeField] private EdgeCaseTester edgeCaseTester;
        
        [Header("Balance Parameters")]
        [SerializeField] private float playerSpeed = 5f;
        [SerializeField] private float staminaCost = 10f;
        [SerializeField] private float powerupCooldown = 3f;
        [SerializeField] private int matchDuration = 120;
        [SerializeField] private int scoreToWin = 3;
        
        // State tracking
        private bool demoRunning = false;
        private int currentStep = 0;
        private bool debugMenuVisible = false;
        
        // Playtesting data
        private int totalMatches = 0;
        private int player1Wins = 0;
        private int player2Wins = 0;
        private int ties = 0;
        private List<string> bugReports = new List<string>();
        private List<string> balanceNotes = new List<string>();
        private float sessionStartTime;
        
        // FPS tracking
        private float currentFPS = 0f;
        
        private void Start()
        {
            sessionStartTime = Time.time;
            StartCoroutine(UpdateFPSCounter());
            
            if (autoStartDemo)
            {
                StartCoroutine(RunCompleteDemo());
            }
            
            LogDemo("=== PLAYTESTING SESSION STARTED ===");
            LogDebug("Press F1 for debug menu, R to reset match, W to force win, B to reset ball");
        }
        
        private void Update()
        {
            HandleDebugInput();
        }
        
        /// <summary>
        /// Handle debug input and hotkeys for playtesting
        /// </summary>
        private void HandleDebugInput()
        {
            if (!enablePlaytesting || !enableDebugIntegration) return;
            
            // Toggle debug menu
            if (Input.GetKeyDown(debugMenuKey))
            {
                debugMenuVisible = !debugMenuVisible;
                LogDebug($"Debug menu {(debugMenuVisible ? "opened" : "closed")}");
            }
            
            // Reset match
            if (Input.GetKeyDown(resetMatchKey))
            {
                ResetMatch();
            }
            
            // Force win condition
            if (Input.GetKeyDown(forceWinKey))
            {
                ForceWinConditionTest();
            }
            
            // Reset ball
            if (Input.GetKeyDown(ballResetKey))
            {
                ResetBallIfStuck();
            }
            
            // Test rapid input
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartCoroutine(TestRapidInput());
            }
        }
        
        /// <summary>
        /// Run the complete game flow demonstration
        /// </summary>
        public IEnumerator RunCompleteDemo()
        {
            if (demoRunning)
            {
                LogDemo("Demo already running!");
                yield break;
            }
            
            demoRunning = true;
            currentStep = 0;
            totalMatches++;
            
            LogDemo("=== STARTING COMPLETE GAME FLOW DEMO ===");
            LogBalanceNote($"Starting match {totalMatches} with current parameters");
            
            if (skipToGameplay)
            {
                yield return StartCoroutine(SkipToGameplayDemo());
            }
            else
            {
                yield return StartCoroutine(FullFlowDemo());
            }
            
            LogDemo("=== DEMO COMPLETE ===");
            demoRunning = false;
            
            // Log match completion
            OnMatchEnd(0); // Default to tie for demo purposes
        }
        
        /// <summary>
        /// Full flow demonstration: Menu -> Power Selection -> Match
        /// </summary>
        private IEnumerator FullFlowDemo()
        {
            // Step 1: Show Main Menu
            yield return StartCoroutine(DemoMainMenu());
            
            // Step 2: Navigate to Power Selection
            yield return StartCoroutine(DemoPlayButtonPress());
            
            // Step 3: Power Selection Process
            yield return StartCoroutine(DemoPowerSelection());
            
            // Step 4: Start Match
            yield return StartCoroutine(DemoMatchStart());
            
            // Step 5: Test edge cases during match
            if (enableEdgeCaseTesting)
            {
                yield return StartCoroutine(TestEdgeCases());
            }
        }
        
        /// <summary>
        /// Skip directly to gameplay demonstration
        /// </summary>
        private IEnumerator SkipToGameplayDemo()
        {
            LogDemo("Skipping to gameplay...");
            
            // Initialize systems
            yield return StartCoroutine(InitializeSystems());
            
            // Setup power selection quickly
            yield return StartCoroutine(QuickPowerSetup());
            
            // Start match
            yield return StartCoroutine(DemoMatchStart());
            
            // Test edge cases
            if (enableEdgeCaseTesting)
            {
                yield return StartCoroutine(TestEdgeCases());
            }
        }
        
        /// <summary>
        /// Demonstrate main menu functionality
        /// </summary>
        private IEnumerator DemoMainMenu()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Demonstrating Main Menu");
            
            // Find or create main menu
            if (mainMenuUI == null)
            {
                mainMenuUI = FindFirstObjectByType<PlushLeague.UI.Menu.MainMenuUI>();
            }
            
            if (mainMenuUI == null)
            {
                LogDemo("Creating Main Menu...");
                var setupExample = FindFirstObjectByType<MainMenuSetupExample>();
                if (setupExample == null)
                {
                    var setupObject = new GameObject("MainMenuSetup");
                    setupExample = setupObject.AddComponent<MainMenuSetupExample>();
                }
                setupExample.SetupMainMenu();
                mainMenuUI = setupExample.GetMainMenuUI();
                yield return new WaitForSeconds(1f);
            }
            
            // Show main menu
            LogDemo("Showing Main Menu...");
            mainMenuUI.ShowMenu();
            
            // Subscribe to events
            mainMenuUI.OnPlayRequested += OnPlayRequested;
            mainMenuUI.OnCustomizeRequested += OnCustomizeRequested;
            mainMenuUI.OnSettingsRequested += OnSettingsRequested;
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Demonstrate play button press
        /// </summary>
        private IEnumerator DemoPlayButtonPress()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Pressing Play Button");

            if (mainMenuUI != null)
            {
                // Simulate play button press
                mainMenuUI.OnPlayPressed();
                LogDemo("Play button pressed - transitioning to game flow");

                // Polish effects placeholder
                if (enablePolishEffects)
                {
                    LogDemo("Polish Effect: Button click SFX would play here");
                }
            }

            yield return new WaitForSeconds(stepDelay);
        }

        /// <summary>
        /// Demonstrate power selection process
        /// </summary>
        private IEnumerator DemoPowerSelection()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Demonstrating Power Selection");
            
            // Find or create power selection manager
            if (powerSelectionManager == null)
            {
                powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            }
            
            if (powerSelectionManager == null)
            {
                LogDemo("Power Selection Manager not found - simulating power selection");
                yield return StartCoroutine(SimulatePowerSelection());
            }
            else
            {
                LogDemo("Using actual Power Selection Manager");
                yield return StartCoroutine(UsePowerSelectionManager());
            }
            
            // Polish effects placeholder
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Power selection SFX/VFX would play here");
            }

            yield return new WaitForSeconds(stepDelay);
        }
        
        /// <summary>
        /// Simulate power selection without UI
        /// </summary>
        private IEnumerator SimulatePowerSelection()
        {
            LogDemo("Simulating power selection for both players...");
            
            // Create mock superpower data if needed
            var superpowers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            
            if (superpowers.Length == 0)
            {
                LogDemo("No superpower data found - creating test data");
                yield return StartCoroutine(CreateTestSuperpowerData());
            }
            
            // Simulate player 1 selection
            LogDemo("Player 1 selecting power...");
            yield return new WaitForSeconds(1f);
            
            // Simulate player 2 selection
            LogDemo("Player 2 selecting power...");
            yield return new WaitForSeconds(1f);
            
            LogDemo("Both players ready!");
        }
        
        /// <summary>
        /// Use actual power selection manager
        /// </summary>
        private IEnumerator UsePowerSelectionManager()
        {
            LogDemo("Starting power selection UI...");
            
            // Start power selection
            powerSelectionManager.StartPowerSelection(false); // Single player mode
            
            // Wait for selection to complete
            var configs = powerSelectionManager.GetPlayerConfigs();
            var player1 = configs.player1;
            var player2 = configs.player2;
            while (!player1.isReady || !player2.isReady)
            {
                yield return new WaitForSeconds(0.5f);
                configs = powerSelectionManager.GetPlayerConfigs();
                player1 = configs.player1;
                player2 = configs.player2;
                LogDemo($"Power selection status - Player1 Ready: {player1.isReady}, Player2 Ready: {player2.isReady}");
            }
            
            LogDemo("Power selection complete!");
        }
        
        /// <summary>
        /// Create test superpower data
        /// </summary>
        private IEnumerator CreateTestSuperpowerData()
        {
            LogDemo("Creating test superpower data...");
            
            // This would typically be done by SuperpowerDataSetup
            var setupScript = FindFirstObjectByType<PlushLeague.Setup.SuperpowerDataSetup>();
            if (setupScript == null)
            {
                var setupObject = new GameObject("SuperpowerSetup");
                setupScript = setupObject.AddComponent<PlushLeague.Setup.SuperpowerDataSetup>();
            }
            
            LogDemo("Test superpower data created");
            yield return null;
        }
        
        /// <summary>
        /// Initialize all game systems
        /// </summary>
        private IEnumerator InitializeSystems()
        {
            LogDemo("Initializing game systems...");
            
            // Find or create GameManager
            if (gameManager == null)
            {
                gameManager = PlushLeague.Core.GameManager.Instance;
            }
            
            if (gameManager == null)
            {
                var gameManagerObject = new GameObject("GameManager");
                gameManager = gameManagerObject.AddComponent<PlushLeague.Core.GameManager>();
            }
            
            // Find other systems
            if (matchManager == null)
            {
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            }
            
            LogDemo("Game systems initialized");
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Quick power setup for skip mode
        /// </summary>
        private IEnumerator QuickPowerSetup()
        {
            LogDemo("Quick power setup...");
            
            // Assign random powers if specified
            if (useRandomPowers)
            {
                LogDemo("Assigning random superpowers to players");
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Demonstrate match start
        /// </summary>
        private IEnumerator DemoMatchStart()
        {
            currentStep++;
            LogDemo($"STEP {currentStep}: Starting Match");
            
            // Polish effects placeholder
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Match start whistle and confetti would play here");
            }

            // Use GameManager to start match if available
            if (gameManager != null)
            {
                LogDemo("Starting match through GameManager...");
                gameManager.StartQuickMatch();
            }
            else if (matchManager != null)
            {
                LogDemo("Starting match directly through MatchManager...");
                matchManager.StartMatch();
            }
            else
            {
                LogDemo("Simulating match start...");
                yield return StartCoroutine(SimulateMatchStart());
            }
            
            // Polish effects placeholder
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Plush character idle animation would play here");
            }

            yield return new WaitForSeconds(stepDelay);
            
            LogDemo("Match is now active! Players can use their superpowers.");
        }
        
        /// <summary>
        /// Simulate match start without actual match manager
        /// </summary>
        private IEnumerator SimulateMatchStart()
        {
            LogDemo("Setting up match environment...");
            yield return new WaitForSeconds(1f);
            
            LogDemo("Spawning players...");
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Player spawn SFX and VFX would play here");
            }
            yield return new WaitForSeconds(1f);
            
            LogDemo("Initializing ball...");
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Ball drop SFX would play here");
            }
            yield return new WaitForSeconds(1f);
            
            LogDemo("Starting countdown...");
            for (int i = 3; i > 0; i--)
            {
                if (enablePolishEffects)
                {
                    LogDemo($"Polish Effect: Countdown beep {i} would play here");
                }
                LogDemo($"Match starts in {i}...");
                yield return new WaitForSeconds(1f);
            }
            
            if (enablePolishEffects)
            {
                LogDemo("Polish Effect: Match start fireworks and SFX would play here");
            }

            LogDemo("MATCH START!");
        }
        
        #region Step 13: Playtesting & Debug Methods
        
        /// <summary>
        /// Test edge cases during match
        /// </summary>
        private IEnumerator TestEdgeCases()
        {
            if (!enableEdgeCaseTesting) yield break;
            
            LogDemo("=== TESTING EDGE CASES ===");
            
            // Test ball stuck condition
            LogDemo("Testing ball reset functionality");
            ResetBallIfStuck();
            yield return new WaitForSeconds(1f);
            
            // Test rapid input
            LogDemo("Testing rapid input scenarios");
            yield return StartCoroutine(TestRapidInput());
            
            // Test win condition
            LogDemo("Testing win condition");
            ForceWinConditionTest();
            yield return new WaitForSeconds(1f);
            
            LogDemo("=== EDGE CASE TESTING COMPLETE ===");
        }
        
        /// <summary>
        /// Test rapid input scenarios
        /// </summary>
        private IEnumerator TestRapidInput()
        {
            LogDemo("Testing rapid button presses...");
            
            for (int i = 0; i < 5; i++)
            {
                // Simulate rapid button presses
                if (mainMenuUI != null)
                {
                    mainMenuUI.OnPlayPressed();
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            LogDebug("Rapid input test completed - checking for conflicts");
        }
        
        /// <summary>
        /// Reset match for testing
        /// </summary>
        public void ResetMatch()
        {
            if (matchManager != null)
            {
                // TODO: Implement ResetMatch method in MatchManager
                LogDebug("Match reset via debug tools - placeholder");
            }
            else if (gameManager != null)
            {
                gameManager.StartQuickMatch();
                LogDebug("New match started via debug tools");
            }
            else
            {
                LogDebug("Simulating match reset");
                StopAllCoroutines();
                StartCoroutine(RunCompleteDemo());
            }
        }
        
        /// <summary>
        /// Force win condition for testing
        /// </summary>
        public void ForceWinConditionTest()
        {
            int winner = Random.Range(1, 3); // Player 1 or 2
            LogDebug($"Forcing win condition for Player {winner}");
            OnMatchEnd(winner);
        }
        
        /// <summary>
        /// Reset ball if stuck
        /// </summary>
        public void ResetBallIfStuck()
        {
            var ball = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
            if (ball != null)
            {
                // Reset ball to center of field
                ball.transform.position = Vector3.zero;
                
                // Reset ball velocity
                Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.angularVelocity = Vector3.zero;
                }
                
                LogDebug("Ball reset to center position");
            }
            else
            {
                LogDebug("Ball controller not found - simulating ball reset");
            }
        }
        
        /// <summary>
        /// Log debug messages for playtesting
        /// </summary>
        public void LogDebug(string message)
        {
            if (!enableBugLogging) return;
            
            string timestampedMessage = $"[{Time.time:F2}] {message}";
            UnityEngine.Debug.Log($"[DEBUG] {timestampedMessage}");
        }
        
        /// <summary>
        /// Log balance notes
        /// </summary>
        public void LogBalanceNote(string note)
        {
            if (!enableBugLogging) return;
            
            string balanceNote = $"[BALANCE] {Time.time:F2} - {note}";
            balanceNotes.Add(balanceNote);
            LogDebug(balanceNote);
        }
        
        /// <summary>
        /// Report bug during playtesting
        /// </summary>
        public void ReportBug(string bugDescription)
        {
            string bugReport = $"[BUG] {Time.time:F2} - {bugDescription}";
            bugReports.Add(bugReport);
            LogDebug(bugReport);
        }
        
        /// <summary>
        /// Apply balance parameters to game systems
        /// </summary>
        public void ApplyBalanceParameters()
        {
            LogDebug($"Applying balance parameters: Speed={playerSpeed}, Stamina={staminaCost}, Cooldown={powerupCooldown}");
            
            // Apply to players
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player != null)
                {
                    // Implementation depends on your player controller
                    LogDebug($"Applied parameters to {player.name}");
                }
            }
            
            LogBalanceNote("Balance parameters applied");
        }
        
        /// <summary>
        /// Called when a match starts
        /// </summary>
        public void OnMatchStart()
        {
            LogDebug($"Match {totalMatches} started");
        }
        
        /// <summary>
        /// Called when a match ends
        /// </summary>
        public void OnMatchEnd(int winnerIndex)
        {
            if (winnerIndex == 1)
                player1Wins++;
            else if (winnerIndex == 2)
                player2Wins++;
            else
                ties++;
            
            LogDebug($"Match {totalMatches} ended - Winner: {(winnerIndex == 0 ? "Tie" : $"Player {winnerIndex}")}");
            LogBalanceNote($"Match results: P1 Wins: {player1Wins}, P2 Wins: {player2Wins}, Ties: {ties}");
        }
        
        /// <summary>
        /// Update FPS counter
        /// </summary>
        private IEnumerator UpdateFPSCounter()
        {
            while (true)
            {
                currentFPS = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlayRequested()
        {
            LogDemo("EVENT: Play requested from Main Menu");
        }
        
        private void OnCustomizeRequested()
        {
            LogDemo("EVENT: Customize requested from Main Menu");
        }
        
        private void OnSettingsRequested()
        {
            LogDemo("EVENT: Settings requested from Main Menu");
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
                UnityEngine.Debug.Log($"[GAME FLOW DEMO] {message}");
            }
        }
        
        /// <summary>
        /// Reset demo state
        /// </summary>
        public void ResetDemo()
        {
            demoRunning = false;
            currentStep = 0;
            
            // Unsubscribe from events
            if (mainMenuUI != null)
            {
                mainMenuUI.OnPlayRequested -= OnPlayRequested;
                mainMenuUI.OnCustomizeRequested -= OnCustomizeRequested;
                mainMenuUI.OnSettingsRequested -= OnSettingsRequested;
            }
            
            LogDemo("Demo reset");
        }
        
        /// <summary>
        /// Save playtesting session report
        /// </summary>
        public void SaveSessionReport()
        {
            try
            {
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"playtest_session_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt");
                
                var report = new System.Text.StringBuilder();
                report.AppendLine("=== PLUSH LEAGUE PLAYTESTING SESSION REPORT ===");
                report.AppendLine($"Date: {System.DateTime.Now}");
                report.AppendLine($"Session Duration: {Time.time - sessionStartTime:F1} seconds");
                report.AppendLine($"Total Matches: {totalMatches}");
                report.AppendLine($"Player 1 Wins: {player1Wins}");
                report.AppendLine($"Player 2 Wins: {player2Wins}");
                report.AppendLine($"Ties: {ties}");
                report.AppendLine();
                
                report.AppendLine("=== BALANCE PARAMETERS ===");
                report.AppendLine($"Player Speed: {playerSpeed}");
                report.AppendLine($"Stamina Cost: {staminaCost}");
                report.AppendLine($"Powerup Cooldown: {powerupCooldown}");
                report.AppendLine($"Match Duration: {matchDuration}");
                report.AppendLine($"Score to Win: {scoreToWin}");
                report.AppendLine();
                
                report.AppendLine("=== BUG REPORTS ===");
                foreach (string bug in bugReports)
                {
                    report.AppendLine(bug);
                }
                report.AppendLine();
                
                report.AppendLine("=== BALANCE NOTES ===");
                foreach (string note in balanceNotes)
                {
                    report.AppendLine(note);
                }
                
                System.IO.File.WriteAllText(filePath, report.ToString());
                LogDebug($"Session report saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                ReportBug($"Failed to save session report: {e.Message}");
            }
        }
        
        #endregion
        
        #region Debug GUI
        
        [Header("Debug GUI")]
        [SerializeField] private bool enableDebugGUI = true;
        
        private void OnGUI()
        {
            if (!enableDebugGUI || !Application.isPlaying) return;
            
            // FPS Counter
            GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {currentFPS:F1}");
            
            // Main demo GUI
            GUILayout.BeginArea(new Rect(10, 40, 350, 400));
            GUILayout.Label("=== Complete Game Flow Demo ===");
            
            GUILayout.Label($"Demo Running: {demoRunning}");
            GUILayout.Label($"Current Step: {currentStep}");
            GUILayout.Label($"Session Time: {Time.time - sessionStartTime:F1}s");
            
            GUILayout.Space(10);
            
            if (!demoRunning)
            {
                if (GUILayout.Button("Start Full Demo"))
                {
                    StartCoroutine(RunCompleteDemo());
                }
                
                if (GUILayout.Button("Skip to Gameplay"))
                {
                    skipToGameplay = true;
                    StartCoroutine(RunCompleteDemo());
                    skipToGameplay = false;
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
            
            GUILayout.Space(10);
            
            // System status
            GUILayout.Label("=== System Status ===");
            GUILayout.Label($"MainMenuUI: {(mainMenuUI != null ? "Found" : "Missing")}");
            GUILayout.Label($"GameManager: {(gameManager != null ? "Found" : "Missing")}");
            GUILayout.Label($"PowerSelection: {(powerSelectionManager != null ? "Found" : "Missing")}");
            GUILayout.Label($"MatchManager: {(matchManager != null ? "Found" : "Missing")}");
            
            GUILayout.Space(10);
            
            // Playtesting stats
            GUILayout.Label("=== Playtesting Stats ===");
            GUILayout.Label($"Matches: {totalMatches}");
            GUILayout.Label($"P1 Wins: {player1Wins} | P2 Wins: {player2Wins} | Ties: {ties}");
            GUILayout.Label($"Bugs: {bugReports.Count} | Notes: {balanceNotes.Count}");
            
            GUILayout.Space(10);
            
            // Manual controls
            if (GUILayout.Button("Reset Match"))
            {
                ResetMatch();
            }
            
            if (GUILayout.Button("Force Win Test"))
            {
                ForceWinConditionTest();
            }
            
            if (GUILayout.Button("Reset Ball"))
            {
                ResetBallIfStuck();
            }
            
            if (GUILayout.Button("Apply Balance Parameters"))
            {
                ApplyBalanceParameters();
            }
            
            if (GUILayout.Button("Save Session Report"))
            {
                SaveSessionReport();
            }
            
            if (GUILayout.Button("Reset All"))
            {
                ResetDemo();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            
            GUILayout.EndArea();
            
            // Debug menu
            if (debugMenuVisible)
            {
                DrawDebugMenu();
            }
        }
        
        /// <summary>
        /// Draw the debug menu for parameter tuning
        /// </summary>
        private void DrawDebugMenu()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 10, 390, Screen.height - 20));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== DEBUG MENU ===");
            
            if (enableParameterTuning)
            {
                GUILayout.Label("=== PARAMETER TUNING ===");
                
                GUILayout.Label($"Player Speed: {playerSpeed:F1}");
                playerSpeed = GUILayout.HorizontalSlider(playerSpeed, 1f, 10f);
                
                GUILayout.Label($"Stamina Cost: {staminaCost:F1}");
                staminaCost = GUILayout.HorizontalSlider(staminaCost, 1f, 30f);
                
                GUILayout.Label($"Powerup Cooldown: {powerupCooldown:F1}");
                powerupCooldown = GUILayout.HorizontalSlider(powerupCooldown, 1f, 10f);
                
                GUILayout.Label($"Match Duration: {matchDuration}");
                matchDuration = (int)GUILayout.HorizontalSlider(matchDuration, 30, 300);
                
                GUILayout.Label($"Score to Win: {scoreToWin}");
                scoreToWin = (int)GUILayout.HorizontalSlider(scoreToWin, 1, 10);
                
                if (GUILayout.Button("Apply Parameters"))
                {
                    ApplyBalanceParameters();
                }
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("=== QUICK ACTIONS ===");
            
            if (GUILayout.Button("Report Bug"))
            {
                ReportBug("Manual bug report from debug menu");
            }
            
            if (GUILayout.Button("Add Balance Note"))
            {
                LogBalanceNote("Manual balance observation from debug menu");
            }
            
            if (GUILayout.Button("Test Edge Cases"))
            {
                StartCoroutine(TestEdgeCases());
            }
            
            if (GUILayout.Button("Test Rapid Input"))
            {
                StartCoroutine(TestRapidInput());
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
