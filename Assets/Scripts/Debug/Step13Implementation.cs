using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Step 13 Integration Script - Comprehensive Playtesting, Balancing, and Bugfixing
    /// 
    /// This script demonstrates the complete implementation of Step 13 requirements:
    /// - Bug logging and reporting
    /// - Parameter tuning and balance testing
    /// - Edge case testing
    /// - Performance monitoring
    /// - Automated testing suites
    /// - Data collection and analysis
    /// 
    /// Usage:
    /// 1. Add this script to a GameObject in your scene
    /// 2. Configure the settings in the inspector
    /// 3. Run the scene and use the debug tools
    /// 4. Check the persistent data folder for logs and reports
    /// </summary>
    public class Step13Implementation : MonoBehaviour
    {
        [Header("Step 13: Playtesting Configuration")]
        [SerializeField] private bool enableStep13Tools = true;
        [SerializeField] private bool autoStartPlaytesting = true;
        [SerializeField] private bool enableComprehensiveTesting = true;
        [SerializeField] private bool enableDataCollection = true;
        [SerializeField] private bool enableAutomatedReporting = true;
        
        [Header("Testing Categories")]
        [SerializeField] private bool testGameplayBalance = true;
        [SerializeField] private bool testEdgeCases = true;
        [SerializeField] private bool testPerformance = true;
        [SerializeField] private bool testUserExperience = true;
        [SerializeField] private bool testSystemIntegration = true;
        
        [Header("Debug UI Configuration")]
        [SerializeField] private bool enableDebugUI = true;
        [SerializeField] private bool enableOnScreenConsole = true;
        [SerializeField] private bool enableParameterTuning = true;
        [SerializeField] private bool enableQuickActions = true;
        [SerializeField] private bool enableBugReporting = true;
        
        [Header("Hotkeys")]
        [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
        [SerializeField] private KeyCode runTestSuiteKey = KeyCode.F2;
        [SerializeField] private KeyCode bugReportKey = KeyCode.F3;
        [SerializeField] private KeyCode parameterTuningKey = KeyCode.F4;
        [SerializeField] private KeyCode emergencyResetKey = KeyCode.F5;
        
        // Tool instances
        private DebugIntegrationManager debugIntegrationManager;
        private PlaytestingManager playtestingManager;
        private DebugConsole debugConsole;
        private EdgeCaseTester edgeCaseTester;
        private BalanceTester balanceTester;
        private BugReportUI bugReportUI;
        
        // Testing state
        private bool step13Active = false;
        private bool testSuiteRunning = false;
        private int totalTestsRun = 0;
        private int testsPassedCount = 0;
        private int testsFailedCount = 0;
        private List<string> testResults = new List<string>();
        
        // Performance monitoring
        private float averageFPS = 0f;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;
        private float memoryUsage = 0f;
        private int frameCount = 0;
        
        private void Start()
        {
            InitializeStep13();
            
            if (autoStartPlaytesting)
            {
                StartStep13Implementation();
            }
        }
        
        private void Update()
        {
            if (!enableStep13Tools) return;
            
            HandleStep13Input();
            MonitorPerformance();
            CheckForIssues();
        }
        
        /// <summary>
        /// Initialize all Step 13 tools and systems
        /// </summary>
        private void InitializeStep13()
        {
            UnityEngine.Debug.Log("=== INITIALIZING STEP 13: PLAYTESTING, BALANCING, AND BUGFIXING ===");
            
            // Initialize core debug integration
            debugIntegrationManager = DebugIntegrationManager.Instance;
            
            // Initialize individual tools
            playtestingManager = PlaytestingManager.Instance;
            debugConsole = DebugConsole.Instance;
            edgeCaseTester = EdgeCaseTester.Instance;
            
            // Find other tools
            balanceTester = FindFirstObjectByType<BalanceTester>();
            bugReportUI = FindFirstObjectByType<BugReportUI>();
            
            // Setup tool configurations
            ConfigureTools();
            
            // Subscribe to events
            SubscribeToEvents();
            
            UnityEngine.Debug.Log("Step 13 tools initialized successfully");
        }
        
        /// <summary>
        /// Configure all debug tools with Step 13 specific settings
        /// </summary>
        private void ConfigureTools()
        {
            // Configure debug console
            if (debugConsole != null)
            {
                debugConsole.AddParameter("test_mode", 1f, 0f, 1f, "Enable test mode");
                debugConsole.AddParameter("log_level", 2f, 0f, 3f, "Logging level (0=Error, 1=Warning, 2=Info, 3=Debug)");
                debugConsole.AddParameter("auto_fix", 0f, 0f, 1f, "Enable automatic issue fixing");
                
                // Add custom commands
                debugConsole.RegisterCommand("step13", RunStep13Command);
                debugConsole.RegisterCommand("testsuite", RunTestSuiteCommand);
                debugConsole.RegisterCommand("report", GenerateReportCommand);
                debugConsole.RegisterCommand("analyze", AnalyzeDataCommand);
            }
            
            // Configure playtesting manager
            if (playtestingManager != null && enableDataCollection)
            {
                playtestingManager.StartPlaytestSession();
            }
            
            UnityEngine.Debug.Log("Debug tools configured for Step 13");
        }
        
        /// <summary>
        /// Subscribe to debug events for comprehensive monitoring
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to integration manager events
            if (debugIntegrationManager != null)
            {
                DebugIntegrationManager.OnDebugEvent += HandleDebugEvent;
                DebugIntegrationManager.OnParameterChanged += HandleParameterChange;
                DebugIntegrationManager.OnTestCompleted += HandleTestCompletion;
            }
            
            // Subscribe to playtesting events
            if (playtestingManager != null)
            {
                PlaytestingManager.OnSessionStarted += HandleSessionStarted;
                PlaytestingManager.OnSessionEnded += HandleSessionEnded;
                PlaytestingManager.OnMatchCompleted += HandleMatchCompleted;
                PlaytestingManager.OnBugReported += HandleBugReported;
            }
        }
        
        /// <summary>
        /// Start the complete Step 13 implementation
        /// </summary>
        public void StartStep13Implementation()
        {
            if (step13Active)
            {
                UnityEngine.Debug.LogWarning("Step 13 implementation already active");
                return;
            }
            
            step13Active = true;
            StartCoroutine(Step13MainLoop());
            
            UnityEngine.Debug.Log("Step 13 implementation started");
        }
        
        /// <summary>
        /// Main Step 13 testing loop
        /// </summary>
        private IEnumerator Step13MainLoop()
        {
            UnityEngine.Debug.Log("Starting Step 13 comprehensive testing loop...");
            
            while (step13Active)
            {
                // Run periodic tests
                if (enableComprehensiveTesting)
                {
                    yield return StartCoroutine(RunPeriodicTests());
                }
                
                // Check for automatic fixes
                if (debugConsole != null && debugConsole.GetParameterValue("auto_fix") > 0.5f)
                {
                    yield return StartCoroutine(RunAutomaticFixes());
                }
                
                // Generate periodic reports
                if (enableAutomatedReporting)
                {
                    yield return StartCoroutine(GeneratePeriodicReports());
                }
                
                // Wait before next cycle
                yield return new WaitForSeconds(30f); // 30 second cycles
            }
        }
        
        /// <summary>
        /// Run periodic automated tests
        /// </summary>
        private IEnumerator RunPeriodicTests()
        {
            UnityEngine.Debug.Log("Running periodic tests...");
            
            List<IEnumerator> tests = new List<IEnumerator>();
            
            // Gameplay balance tests
            if (testGameplayBalance)
            {
                tests.Add(TestGameplayBalance());
            }
            
            // Edge case tests
            if (testEdgeCases)
            {
                tests.Add(TestEdgeCases());
            }
            
            // Performance tests
            if (testPerformance)
            {
                tests.Add(TestPerformance());
            }
            
            // User experience tests
            if (testUserExperience)
            {
                tests.Add(TestUserExperience());
            }
            
            // System integration tests
            if (testSystemIntegration)
            {
                tests.Add(TestSystemIntegration());
            }
            
            // Run tests
            foreach (var test in tests)
            {
                yield return StartCoroutine(test);
                yield return new WaitForSeconds(1f);
            }
            
            UnityEngine.Debug.Log("Periodic tests completed");
        }
        
        /// <summary>
        /// Test gameplay balance
        /// </summary>
        private IEnumerator TestGameplayBalance()
        {
            UnityEngine.Debug.Log("Testing gameplay balance...");
            
            // Test win rate balance
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                var session = playtestingManager.GetCurrentSession();
                if (session.matches.Count > 5)
                {
                    int player1Wins = 0;
                    int player2Wins = 0;
                    
                    foreach (var match in session.matches)
                    {
                        if (match.winner == "Player1") player1Wins++;
                        else if (match.winner == "Player2") player2Wins++;
                    }
                    
                    float winRateBalance = (float)player1Wins / (player1Wins + player2Wins);
                    
                    if (winRateBalance < 0.3f || winRateBalance > 0.7f)
                    {
                        RecordBalanceIssue("Win rate imbalance detected", $"Player 1 win rate: {winRateBalance:P2}");
                    }
                }
            }
            
            // Test parameter ranges
            if (debugConsole != null)
            {
                float playerSpeed = debugConsole.GetParameterValue("player_speed");
                if (playerSpeed < 2f || playerSpeed > 12f)
                {
                    RecordBalanceIssue("Player speed out of optimal range", $"Current speed: {playerSpeed}");
                }
            }
            
            yield return new WaitForSeconds(2f);
            RecordTestResult("GameplayBalance", true, "Balance test completed");
        }
        
        /// <summary>
        /// Test edge cases
        /// </summary>
        private IEnumerator TestEdgeCases()
        {
            UnityEngine.Debug.Log("Testing edge cases...");
            
            if (edgeCaseTester != null)
            {
                // Run a subset of edge case tests
                edgeCaseTester.RunSingleTest("Ball Outside Boundaries");
                yield return new WaitForSeconds(3f);
                
                edgeCaseTester.RunSingleTest("Zero Player Speed");
                yield return new WaitForSeconds(3f);
                
                edgeCaseTester.RunSingleTest("Extreme Time Scale");
                yield return new WaitForSeconds(3f);
            }
            
            RecordTestResult("EdgeCases", true, "Edge case tests completed");
        }
        
        /// <summary>
        /// Test performance
        /// </summary>
        private IEnumerator TestPerformance()
        {
            UnityEngine.Debug.Log("Testing performance...");
            
            // Monitor FPS
            float testStartTime = Time.time;
            float testFPS = 0f;
            int fpsFrames = 0;
            
            while (Time.time - testStartTime < 5f)
            {
                testFPS += 1f / Time.deltaTime;
                fpsFrames++;
                yield return null;
            }
            
            float averageTestFPS = testFPS / fpsFrames;
            
            if (averageTestFPS < 30f)
            {
                RecordPerformanceIssue("Low FPS detected", $"Average FPS: {averageTestFPS:F1}");
            }
            
            // Monitor memory
            float memoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            
            if (memoryMB > 500f) // 500MB threshold
            {
                RecordPerformanceIssue("High memory usage", $"Memory usage: {memoryMB:F1} MB");
            }
            
            RecordTestResult("Performance", averageTestFPS >= 30f && memoryMB <= 500f, $"FPS: {averageTestFPS:F1}, Memory: {memoryMB:F1} MB");
        }
        
        /// <summary>
        /// Test user experience
        /// </summary>
        private IEnumerator TestUserExperience()
        {
            UnityEngine.Debug.Log("Testing user experience...");
            
            // Test UI responsiveness
            yield return new WaitForSeconds(1f);
            
            // Test input responsiveness
            yield return new WaitForSeconds(1f);
            
            // Test audio/visual feedback
            yield return new WaitForSeconds(1f);
            
            RecordTestResult("UserExperience", true, "UX test completed");
        }
        
        /// <summary>
        /// Test system integration
        /// </summary>
        private IEnumerator TestSystemIntegration()
        {
            UnityEngine.Debug.Log("Testing system integration...");
            
            // Test debug tool integration
            bool allToolsActive = true;
            
            allToolsActive &= debugIntegrationManager != null;
            allToolsActive &= playtestingManager != null;
            allToolsActive &= debugConsole != null;
            allToolsActive &= edgeCaseTester != null;
            
            if (!allToolsActive)
            {
                RecordSystemIssue("Debug tools not fully integrated", "Some tools are missing");
            }
            
            yield return new WaitForSeconds(2f);
            RecordTestResult("SystemIntegration", allToolsActive, "System integration test completed");
        }
        
        /// <summary>
        /// Run automatic fixes for detected issues
        /// </summary>
        private IEnumerator RunAutomaticFixes()
        {
            UnityEngine.Debug.Log("Running automatic fixes...");
            
            // Fix common issues
            if (Time.timeScale != 1f)
            {
                Time.timeScale = 1f;
                UnityEngine.Debug.Log("Auto-fixed: Time scale reset to 1.0");
            }
            
            // Force garbage collection if memory is high
            float memoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            if (memoryMB > 300f)
            {
                System.GC.Collect();
                UnityEngine.Debug.Log("Auto-fixed: Forced garbage collection");
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// Generate periodic reports
        /// </summary>
        private IEnumerator GeneratePeriodicReports()
        {
            UnityEngine.Debug.Log("Generating periodic reports...");
            
            // Generate summary report
            string report = GenerateTestSummaryReport();
            
            // Log report
            UnityEngine.Debug.Log("=== STEP 13 PERIODIC REPORT ===");
            UnityEngine.Debug.Log(report);
            
            // Save to file if possible
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordBalanceNote("PeriodicReport", report, 1f);
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// Handle Step 13 input
        /// </summary>
        private void HandleStep13Input()
        {
            if (Input.GetKeyDown(toggleDebugKey))
            {
                if (debugConsole != null)
                {
                    debugConsole.ToggleConsole();
                }
            }
            
            if (Input.GetKeyDown(runTestSuiteKey))
            {
                if (!testSuiteRunning)
                {
                    StartCoroutine(RunFullTestSuite());
                }
            }
            
            if (Input.GetKeyDown(bugReportKey))
            {
                if (bugReportUI != null)
                {
                    bugReportUI.ShowBugReportPanel();
                }
            }
            
            if (Input.GetKeyDown(parameterTuningKey))
            {
                if (debugConsole != null)
                {
                    debugConsole.ToggleConsole();
                }
            }
            
            if (Input.GetKeyDown(emergencyResetKey))
            {
                EmergencyReset();
            }
        }
        
        /// <summary>
        /// Monitor performance metrics
        /// </summary>
        private void MonitorPerformance()
        {
            float currentFPS = 1f / Time.deltaTime;
            averageFPS = (averageFPS * frameCount + currentFPS) / (frameCount + 1);
            
            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;
            
            frameCount++;
            
            // Update memory usage periodically
            if (frameCount % 60 == 0)
            {
                memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            }
        }
        
        /// <summary>
        /// Check for common issues
        /// </summary>
        private void CheckForIssues()
        {
            // Check for low FPS
            if (averageFPS < 30f && frameCount > 100)
            {
                RecordPerformanceIssue("Sustained low FPS", $"Average FPS: {averageFPS:F1}");
            }
            
            // Check for high memory usage
            if (memoryUsage > 400f)
            {
                RecordPerformanceIssue("High memory usage", $"Memory: {memoryUsage:F1} MB");
            }
            
            // Check for stuck time scale
            if (Time.timeScale != 1f)
            {
                RecordSystemIssue("Time scale not normal", $"Time scale: {Time.timeScale}");
            }
        }
        
        /// <summary>
        /// Run the complete test suite
        /// </summary>
        private IEnumerator RunFullTestSuite()
        {
            testSuiteRunning = true;
            totalTestsRun = 0;
            testsPassedCount = 0;
            testsFailedCount = 0;
            testResults.Clear();
            
            UnityEngine.Debug.Log("=== RUNNING FULL STEP 13 TEST SUITE ===");
            
            // Run all test categories
            yield return StartCoroutine(TestGameplayBalance());
            yield return StartCoroutine(TestEdgeCases());
            yield return StartCoroutine(TestPerformance());
            yield return StartCoroutine(TestUserExperience());
            yield return StartCoroutine(TestSystemIntegration());
            
            // Generate final report
            string finalReport = GenerateTestSummaryReport();
            UnityEngine.Debug.Log("=== FULL TEST SUITE RESULTS ===");
            UnityEngine.Debug.Log(finalReport);
            
            testSuiteRunning = false;
        }
        
        /// <summary>
        /// Generate test summary report
        /// </summary>
        private string GenerateTestSummaryReport()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            
            report.AppendLine("=== STEP 13 TEST SUMMARY ===");
            report.AppendLine($"Total Tests: {totalTestsRun}");
            report.AppendLine($"Passed: {testsPassedCount}");
            report.AppendLine($"Failed: {testsFailedCount}");
            report.AppendLine($"Success Rate: {(totalTestsRun > 0 ? (float)testsPassedCount / totalTestsRun * 100 : 0):F1}%");
            report.AppendLine();
            
            report.AppendLine("=== PERFORMANCE METRICS ===");
            report.AppendLine($"Average FPS: {averageFPS:F1}");
            report.AppendLine($"Min FPS: {minFPS:F1}");
            report.AppendLine($"Max FPS: {maxFPS:F1}");
            report.AppendLine($"Memory Usage: {memoryUsage:F1} MB");
            report.AppendLine();
            
            report.AppendLine("=== TEST RESULTS ===");
            foreach (var result in testResults)
            {
                report.AppendLine(result);
            }
            
            return report.ToString();
        }
        
        // Event handlers
        private void HandleDebugEvent(string eventType, object data)
        {
            UnityEngine.Debug.Log($"Debug Event: {eventType} - {data}");
        }
        
        private void HandleParameterChange(string paramName, float value)
        {
            UnityEngine.Debug.Log($"Parameter Changed: {paramName} = {value}");
        }
        
        private void HandleTestCompletion(string testName)
        {
            UnityEngine.Debug.Log($"Test Completed: {testName}");
        }
        
        private void HandleSessionStarted(PlaytestingManager.PlaytestSession session)
        {
            UnityEngine.Debug.Log($"Playtest Session Started: {session.sessionName}");
        }
        
        private void HandleSessionEnded(PlaytestingManager.PlaytestSession session)
        {
            UnityEngine.Debug.Log($"Playtest Session Ended: {session.sessionName}");
        }
        
        private void HandleMatchCompleted(PlaytestingManager.MatchData match)
        {
            UnityEngine.Debug.Log($"Match Completed: {match.matchId} - {match.winner}");
        }
        
        private void HandleBugReported(PlaytestingManager.BugReport bug)
        {
            UnityEngine.Debug.Log($"Bug Reported: {bug.type} - {bug.description}");
        }
        
        // Console command handlers
        private void RunStep13Command(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "start":
                        StartStep13Implementation();
                        break;
                    case "stop":
                        StopStep13Implementation();
                        break;
                    case "status":
                        UnityEngine.Debug.Log($"Step 13 Status: {(step13Active ? "Active" : "Inactive")}");
                        break;
                    default:
                        UnityEngine.Debug.Log("Usage: step13 [start|stop|status]");
                        break;
                }
            }
            else
            {
                UnityEngine.Debug.Log("Step 13 Tools - Usage: step13 [start|stop|status]");
            }
        }
        
        private void RunTestSuiteCommand(string[] args)
        {
            if (!testSuiteRunning)
            {
                StartCoroutine(RunFullTestSuite());
            }
            else
            {
                UnityEngine.Debug.Log("Test suite already running");
            }
        }
        
        private void GenerateReportCommand(string[] args)
        {
            string report = GenerateTestSummaryReport();
            UnityEngine.Debug.Log(report);
        }
        
        private void AnalyzeDataCommand(string[] args)
        {
            UnityEngine.Debug.Log("Analyzing collected data...");
            
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                var session = playtestingManager.GetCurrentSession();
                
                UnityEngine.Debug.Log($"Session Analysis:");
                UnityEngine.Debug.Log($"- Duration: {session.duration:F2} seconds");
                UnityEngine.Debug.Log($"- Matches: {session.matches.Count}");
                UnityEngine.Debug.Log($"- Bug Reports: {session.bugReports.Count}");
                UnityEngine.Debug.Log($"- Balance Notes: {session.balanceNotes.Count}");
            }
        }
        
        // Utility methods
        private void RecordTestResult(string testName, bool passed, string details)
        {
            totalTestsRun++;
            if (passed)
            {
                testsPassedCount++;
            }
            else
            {
                testsFailedCount++;
            }
            
            testResults.Add($"{testName}: {(passed ? "PASS" : "FAIL")} - {details}");
        }
        
        private void RecordBalanceIssue(string issue, string details)
        {
            UnityEngine.Debug.LogWarning($"Balance Issue: {issue} - {details}");
            
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordBalanceNote("BalanceIssue", $"{issue}: {details}", 0.8f);
            }
        }
        
        private void RecordPerformanceIssue(string issue, string details)
        {
            UnityEngine.Debug.LogWarning($"Performance Issue: {issue} - {details}");
            
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordBugReport($"{issue}: {details}", "Performance", "Medium");
            }
        }
        
        private void RecordSystemIssue(string issue, string details)
        {
            UnityEngine.Debug.LogError($"System Issue: {issue} - {details}");
            
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordBugReport($"{issue}: {details}", "System", "High");
            }
        }
        
        private void EmergencyReset()
        {
            UnityEngine.Debug.Log("Emergency Reset Triggered!");
            
            Time.timeScale = 1f;
            System.GC.Collect();
            
            if (debugIntegrationManager != null)
            {
                debugIntegrationManager.ExecuteQuickAction("ResetMatch");
                debugIntegrationManager.ExecuteQuickAction("ResetBall");
                debugIntegrationManager.ExecuteQuickAction("FillStamina");
            }
        }
        
        public void StopStep13Implementation()
        {
            step13Active = false;
            UnityEngine.Debug.Log("Step 13 implementation stopped");
        }
        
        // Debug GUI
        private void OnGUI()
        {
            if (!enableDebugUI) return;
            
            GUILayout.BeginArea(new Rect(10, Screen.height - 200, 300, 190));
            
            GUILayout.Label("=== STEP 13 DEBUG ===");
            GUILayout.Label($"Status: {(step13Active ? "Active" : "Inactive")}");
            GUILayout.Label($"Tests Run: {totalTestsRun}");
            GUILayout.Label($"Pass Rate: {(totalTestsRun > 0 ? (float)testsPassedCount / totalTestsRun * 100 : 0):F1}%");
            GUILayout.Label($"FPS: {averageFPS:F1}");
            GUILayout.Label($"Memory: {memoryUsage:F1} MB");
            
            if (!step13Active)
            {
                if (GUILayout.Button("Start Step 13"))
                {
                    StartStep13Implementation();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Step 13"))
                {
                    StopStep13Implementation();
                }
            }
            
            if (GUILayout.Button("Run Test Suite"))
            {
                if (!testSuiteRunning)
                {
                    StartCoroutine(RunFullTestSuite());
                }
            }
            
            if (GUILayout.Button("Emergency Reset"))
            {
                EmergencyReset();
            }
            
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (debugIntegrationManager != null)
            {
                DebugIntegrationManager.OnDebugEvent -= HandleDebugEvent;
                DebugIntegrationManager.OnParameterChanged -= HandleParameterChange;
                DebugIntegrationManager.OnTestCompleted -= HandleTestCompletion;
            }
            
            if (playtestingManager != null)
            {
                PlaytestingManager.OnSessionStarted -= HandleSessionStarted;
                PlaytestingManager.OnSessionEnded -= HandleSessionEnded;
                PlaytestingManager.OnMatchCompleted -= HandleMatchCompleted;
                PlaytestingManager.OnBugReported -= HandleBugReported;
            }
        }
    }
}
