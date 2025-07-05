using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Comprehensive debug tools for playtesting, balancing, and bugfixing.
    /// Provides logging, cheat codes, parameter tuning, and testing utilities.
    /// </summary>
    public class DebugTools : MonoBehaviour
    {
        [Header("Debug Configuration")]
        [SerializeField] private bool enableDebugTools = true;
        [SerializeField] private bool enableCheats = true;
        [SerializeField] private bool enableParameterTuning = true;
        [SerializeField] private bool enableBugLogging = true;
        [SerializeField] private bool showFPSCounter = true;
        [SerializeField] private bool showConsoleOverlay = true;
        
        [Header("Debug Hotkeys")]
        [SerializeField] private KeyCode debugMenuKey = KeyCode.F1;
        [SerializeField] private KeyCode resetMatchKey = KeyCode.R;
        [SerializeField] private KeyCode fillStaminaKey = KeyCode.F;
        [SerializeField] private KeyCode forceWinKey = KeyCode.W;
        [SerializeField] private KeyCode forceLoseKey = KeyCode.L;
        [SerializeField] private KeyCode resetBallKey = KeyCode.B;
        [SerializeField] private KeyCode toggleSlowMoKey = KeyCode.S;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private PlushLeague.Gameplay.Match.MatchManager matchManager;
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController[] players;
        [SerializeField] private PlushLeague.Gameplay.Ball.BallController ballController;
        
        [Header("Debug Parameters")]
        [SerializeField] private float slowMotionScale = 0.5f;
        [SerializeField] private int maxLogEntries = 100;
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        
        // Debug state
        private bool debugMenuVisible = false;
        private bool slowMotionActive = false;
        private float currentFPS = 0f;
        private List<string> debugLog = new List<string>();
        private Vector2 scrollPosition = Vector2.zero;
        private StringBuilder logBuilder = new StringBuilder();
        
        // Playtesting data
        private float sessionStartTime;
        private int totalMatches = 0;
        private int player1Wins = 0;
        private int player2Wins = 0;
        private int ties = 0;
        private List<string> bugReports = new List<string>();
        private List<string> balanceNotes = new List<string>();
        
        // Parameter tuning
        [Header("Tunable Parameters")]
        [SerializeField] private float playerSpeed = 5f;
        [SerializeField] private float staminaCost = 10f;
        [SerializeField] private float powerupCooldown = 3f;
        [SerializeField] private float ballSpeed = 8f;
        [SerializeField] private int matchDuration = 120;
        [SerializeField] private int goalScore = 1;
        
        private static DebugTools instance;
        public static DebugTools Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DebugTools>();
                    if (instance == null)
                    {
                        GameObject debugObject = new GameObject("DebugTools");
                        instance = debugObject.AddComponent<DebugTools>();
                        DontDestroyOnLoad(debugObject);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            sessionStartTime = Time.time;
            StartCoroutine(UpdateFPSCounter());
        }
        
        private void Start()
        {
            InitializeDebugSystems();
            LogDebug("Debug Tools initialized - Press F1 for debug menu");
        }
        
        private void Update()
        {
            if (!enableDebugTools) return;
            
            HandleDebugInput();
            CheckForStuckConditions();
        }
        
        /// <summary>
        /// Initialize debug systems and find references
        /// </summary>
        private void InitializeDebugSystems()
        {
            // Find system references
            if (gameManager == null)
                gameManager = PlushLeague.Core.GameManager.Instance;
            
            if (matchManager == null)
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            
            if (players == null || players.Length == 0)
                players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            
            if (ballController == null)
                ballController = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
            
            LogDebug("Debug system references initialized");
        }
        
        /// <summary>
        /// Handle debug input and hotkeys
        /// </summary>
        private void HandleDebugInput()
        {
            // Toggle debug menu
            if (Input.GetKeyDown(debugMenuKey))
            {
                debugMenuVisible = !debugMenuVisible;
                LogDebug($"Debug menu {(debugMenuVisible ? "opened" : "closed")}");
            }
            
            if (!enableCheats) return;
            
            // Cheat hotkeys
            if (Input.GetKeyDown(resetMatchKey))
            {
                ResetMatch();
            }
            
            if (Input.GetKeyDown(fillStaminaKey))
            {
                FillAllStamina();
            }
            
            if (Input.GetKeyDown(forceWinKey))
            {
                ForceWinCondition(1);
            }
            
            if (Input.GetKeyDown(forceLoseKey))
            {
                ForceWinCondition(2);
            }
            
            if (Input.GetKeyDown(resetBallKey))
            {
                ResetBallIfStuck();
            }
            
            if (Input.GetKeyDown(toggleSlowMoKey))
            {
                ToggleSlowMotion();
            }
        }
        
        /// <summary>
        /// Check for stuck conditions (ball, players, etc.)
        /// </summary>
        private void CheckForStuckConditions()
        {
            // Check if ball is stuck
            if (ballController != null)
            {
                Vector3 ballPos = ballController.transform.position;
                
                // Check if ball is out of bounds
                if (Mathf.Abs(ballPos.x) > 50f || Mathf.Abs(ballPos.z) > 50f || ballPos.y < -10f || ballPos.y > 20f)
                {
                    LogBug("Ball detected out of bounds - auto-resetting");
                    ResetBallIfStuck();
                }
                
                // Check if ball velocity is zero for too long (potential stuck)
                Rigidbody ballRb = ballController.GetComponent<Rigidbody>();
                if (ballRb != null && ballRb.linearVelocity.magnitude < 0.1f)
                {
                    // This would need a timer to track how long it's been stuck
                    // Implementation depends on your ball controller structure
                }
            }
            
            // Check for player stuck conditions
            foreach (var player in players)
            {
                if (player != null)
                {
                    Vector3 playerPos = player.transform.position;
                    
                    // Check if player is out of bounds
                    if (Mathf.Abs(playerPos.x) > 40f || Mathf.Abs(playerPos.z) > 40f || playerPos.y < -5f)
                    {
                        LogBug($"Player {player.name} detected out of bounds - repositioning");
                        // Reset player position - implementation depends on your player controller
                        // player.ResetPosition();
                    }
                }
            }
        }
        
        #region Debug Logging
        
        /// <summary>
        /// Log debug messages
        /// </summary>
        public void LogDebug(string message)
        {
            if (!enableBugLogging) return;
            
            string timestampedMessage = $"[{Time.time:F2}] {message}";
            debugLog.Add(timestampedMessage);
            
            // Keep log size manageable
            if (debugLog.Count > maxLogEntries)
            {
                debugLog.RemoveAt(0);
            }
            
            UnityEngine.Debug.Log($"[DEBUG TOOLS] {timestampedMessage}");
        }
        
        /// <summary>
        /// Log bug reports
        /// </summary>
        public void LogBug(string bugDescription)
        {
            string bugReport = $"[BUG] {Time.time:F2} - {bugDescription}";
            bugReports.Add(bugReport);
            LogDebug(bugReport);
            
            // Save to file
            SaveBugReport(bugReport);
        }
        
        /// <summary>
        /// Log balance notes
        /// </summary>
        public void LogBalanceNote(string note)
        {
            string balanceNote = $"[BALANCE] {Time.time:F2} - {note}";
            balanceNotes.Add(balanceNote);
            LogDebug(balanceNote);
        }
        
        /// <summary>
        /// Save bug report to file
        /// </summary>
        private void SaveBugReport(string bugReport)
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, "bug_reports.txt");
                File.AppendAllText(filePath, bugReport + "\n");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save bug report: {e.Message}");
            }
        }
        
        #endregion
        
        #region Cheat Functions
        
        /// <summary>
        /// Reset the current match
        /// </summary>
        public void ResetMatch()
        {
            if (matchManager != null)
            {
                matchManager.EndMatch(0); // End match without a winner (draw)
                UnityEngine.Debug.Log("Match reset via debug tools");
            }
            else if (gameManager != null)
            {
                gameManager.StartQuickMatch();
                UnityEngine.Debug.Log("New match started via debug tools");
            }
        }
        
        /// <summary>
        /// Fill all players' stamina
        /// </summary>
        public void FillAllStamina()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    // Implementation depends on your player controller
                    // player.FillStamina();
                }
            }
            LogDebug("All player stamina filled");
        }
        
        /// <summary>
        /// Force win condition for testing
        /// </summary>
        public void ForceWinConditionTest()
        {
            ForceWinCondition(1);
        }
        
        /// <summary>
        /// Force win condition for specific player
        /// </summary>
        public void ForceWinCondition(int playerIndex)
        {
            if (matchManager != null)
            {
                // Implementation depends on your match manager
                // matchManager.ForceWin(playerIndex);
                LogDebug($"Forced win for player {playerIndex}");
            }
        }
        
        /// <summary>
        /// Reset ball if stuck
        /// </summary>
        public void ResetBallIfStuck()
        {
            if (ballController != null)
            {
                // Reset ball to center of field
                ballController.transform.position = Vector3.zero;
                
                // Reset ball velocity
                Rigidbody ballRb = ballController.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.angularVelocity = Vector3.zero;
                }
                
                LogDebug("Ball reset to center position");
            }
        }
        
        /// <summary>
        /// Toggle slow motion for testing
        /// </summary>
        public void ToggleSlowMotion()
        {
            slowMotionActive = !slowMotionActive;
            Time.timeScale = slowMotionActive ? slowMotionScale : 1f;
            LogDebug($"Slow motion {(slowMotionActive ? "enabled" : "disabled")}");
        }
        
        #endregion
        
        #region Parameter Tuning
        
        /// <summary>
        /// Apply tuned parameters to game systems
        /// </summary>
        public void ApplyTunedParameters()
        {
            // Apply to players
            foreach (var player in players)
            {
                if (player != null)
                {
                    // Implementation depends on your player controller
                    // player.SetSpeed(playerSpeed);
                    // player.SetStaminaCost(staminaCost);
                }
            }
            
            // Apply to ball
            if (ballController != null)
            {
                // Implementation depends on your ball controller
                // ballController.SetSpeed(ballSpeed);
            }
            
            // Apply to match settings
            if (matchManager != null)
            {
                // Implementation depends on your match manager
                // matchManager.SetMatchDuration(matchDuration);
                // matchManager.SetGoalScore(goalScore);
            }
            
            LogDebug("Tuned parameters applied to game systems");
        }
        
        /// <summary>
        /// Get parameter value by name
        /// </summary>
        public float GetParameter(string paramName)
        {
            switch (paramName.ToLower())
            {
                case "playerspeed":
                    return playerSpeed;
                case "staminacost":
                    return staminaCost;
                case "powerupcooldown":
                    return powerupCooldown;
                case "ballspeed":
                    return ballSpeed;
                case "matchduration":
                    return matchDuration;
                case "scoretowin":
                    return goalScore;
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Set parameter value by name
        /// </summary>
        public void SetParameter(string paramName, float value)
        {
            switch (paramName.ToLower())
            {
                case "playerspeed":
                    playerSpeed = value;
                    LogDebug($"Player speed set to {value}");
                    break;
                case "staminacost":
                    staminaCost = value;
                    LogDebug($"Stamina cost set to {value}");
                    break;
                case "powerupcooldown":
                    powerupCooldown = value;
                    LogDebug($"Powerup cooldown set to {value}");
                    break;
                case "ballspeed":
                    ballSpeed = value;
                    LogDebug($"Ball speed set to {value}");
                    break;
                case "matchduration":
                    matchDuration = (int)value;
                    LogDebug($"Match duration set to {value}");
                    break;
                case "scoretowin":
                    goalScore = (int)value;
                    LogDebug($"Score to win set to {value}");
                    break;
                default:
                    LogDebug($"Unknown parameter: {paramName}");
                    break;
            }
        }
        
        #endregion
        
        #region FPS Counter
        
        /// <summary>
        /// Update FPS counter
        /// </summary>
        private IEnumerator UpdateFPSCounter()
        {
            while (true)
            {
                currentFPS = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(fpsUpdateInterval);
            }
        }
        
        #endregion
        
        #region Debug GUI
        
        private void OnGUI()
        {
            if (!enableDebugTools) return;
            
            // FPS Counter
            if (showFPSCounter)
            {
                GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {currentFPS:F1}");
            }
            
            // Console overlay
            if (showConsoleOverlay && debugLog.Count > 0)
            {
                GUI.Box(new Rect(10, 40, 400, 100), "Debug Log");
                
                GUILayout.BeginArea(new Rect(15, 60, 390, 75));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                
                foreach (string logEntry in debugLog)
                {
                    GUILayout.Label(logEntry);
                }
                
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            
            // Debug menu
            if (debugMenuVisible)
            {
                DrawDebugMenu();
            }
        }
        
        /// <summary>
        /// Draw the main debug menu
        /// </summary>
        private void DrawDebugMenu()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 10, 390, Screen.height - 20));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== DEBUG TOOLS ===");
            
            // Session info
            GUILayout.Label($"Session Time: {Time.time - sessionStartTime:F1}s");
            GUILayout.Label($"Matches: {totalMatches} | P1 Wins: {player1Wins} | P2 Wins: {player2Wins} | Ties: {ties}");
            
            GUILayout.Space(10);
            
            // Cheat buttons
            if (enableCheats)
            {
                GUILayout.Label("=== CHEATS ===");
                
                if (GUILayout.Button("Reset Match"))
                    ResetMatch();
                
                if (GUILayout.Button("Fill All Stamina"))
                    FillAllStamina();
                
                if (GUILayout.Button("Force Player 1 Win"))
                    ForceWinCondition(1);
                
                if (GUILayout.Button("Force Player 2 Win"))
                    ForceWinCondition(2);
                
                if (GUILayout.Button("Reset Ball"))
                    ResetBallIfStuck();
                
                if (GUILayout.Button(slowMotionActive ? "Disable Slow Motion" : "Enable Slow Motion"))
                    ToggleSlowMotion();
                
                GUILayout.Space(10);
            }
            
            // Parameter tuning
            if (enableParameterTuning)
            {
                GUILayout.Label("=== PARAMETER TUNING ===");
                
                GUILayout.Label("Player Speed:");
                playerSpeed = GUILayout.HorizontalSlider(playerSpeed, 1f, 10f);
                GUILayout.Label($"Value: {playerSpeed:F1}");
                
                GUILayout.Label("Stamina Cost:");
                staminaCost = GUILayout.HorizontalSlider(staminaCost, 1f, 20f);
                GUILayout.Label($"Value: {staminaCost:F1}");
                
                GUILayout.Label("Powerup Cooldown:");
                powerupCooldown = GUILayout.HorizontalSlider(powerupCooldown, 1f, 10f);
                GUILayout.Label($"Value: {powerupCooldown:F1}");
                
                if (GUILayout.Button("Apply Parameters"))
                    ApplyTunedParameters();
                
                GUILayout.Space(10);
            }
            
            // Bug reporting
            GUILayout.Label("=== BUG REPORTS ===");
            GUILayout.Label($"Bugs Logged: {bugReports.Count}");
            
            if (GUILayout.Button("Save Session Report"))
                SaveSessionReport();
            
            if (GUILayout.Button("Clear All Logs"))
                ClearAllLogs();
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Save complete session report
        /// </summary>
        private void SaveSessionReport()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, $"session_report_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt");
                
                logBuilder.Clear();
                logBuilder.AppendLine("=== PLUSH LEAGUE DEBUG SESSION REPORT ===");
                logBuilder.AppendLine($"Date: {System.DateTime.Now}");
                logBuilder.AppendLine($"Session Duration: {Time.time - sessionStartTime:F1} seconds");
                logBuilder.AppendLine($"Total Matches: {totalMatches}");
                logBuilder.AppendLine($"Player 1 Wins: {player1Wins}");
                logBuilder.AppendLine($"Player 2 Wins: {player2Wins}");
                logBuilder.AppendLine($"Ties: {ties}");
                logBuilder.AppendLine();
                
                logBuilder.AppendLine("=== TUNED PARAMETERS ===");
                logBuilder.AppendLine($"Player Speed: {playerSpeed}");
                logBuilder.AppendLine($"Stamina Cost: {staminaCost}");
                logBuilder.AppendLine($"Powerup Cooldown: {powerupCooldown}");
                logBuilder.AppendLine($"Ball Speed: {ballSpeed}");
                logBuilder.AppendLine($"Match Duration: {matchDuration}");
                logBuilder.AppendLine($"Goal Score: {goalScore}");
                logBuilder.AppendLine();
                
                logBuilder.AppendLine("=== BUG REPORTS ===");
                foreach (string bug in bugReports)
                {
                    logBuilder.AppendLine(bug);
                }
                logBuilder.AppendLine();
                
                logBuilder.AppendLine("=== BALANCE NOTES ===");
                foreach (string note in balanceNotes)
                {
                    logBuilder.AppendLine(note);
                }
                logBuilder.AppendLine();
                
                logBuilder.AppendLine("=== DEBUG LOG ===");
                foreach (string logEntry in debugLog)
                {
                    logBuilder.AppendLine(logEntry);
                }
                
                File.WriteAllText(filePath, logBuilder.ToString());
                LogDebug($"Session report saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                LogBug($"Failed to save session report: {e.Message}");
            }
        }
        
        /// <summary>
        /// Clear all debug logs
        /// </summary>
        private void ClearAllLogs()
        {
            debugLog.Clear();
            bugReports.Clear();
            balanceNotes.Clear();
            LogDebug("All debug logs cleared");
        }
        
        #endregion
        
        #region Public API for Integration
        
        /// <summary>
        /// Called when a match starts
        /// </summary>
        public void OnMatchStart()
        {
            totalMatches++;
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
        }
        
        /// <summary>
        /// Called when a goal is scored
        /// </summary>
        public void OnGoalScored(int scoringPlayer, Vector3 goalPosition)
        {
            LogDebug($"Goal scored by Player {scoringPlayer} at {goalPosition}");
        }
        
        /// <summary>
        /// Called when a superpower is used
        /// </summary>
        public void OnSuperpowerUsed(int playerIndex, string superpowerName)
        {
            LogDebug($"Player {playerIndex} used superpower: {superpowerName}");
        }
        
        #endregion
    }
}
