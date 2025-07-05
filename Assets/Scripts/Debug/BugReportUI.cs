using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PlushLeague.Debug
{
    /// <summary>
    /// In-game bug reporting interface for playtesting.
    /// Allows players to quickly report bugs with context information.
    /// </summary>
    public class BugReportUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject bugReportPanel;
        [SerializeField] private TMP_InputField bugDescriptionInput;
        [SerializeField] private TMP_Dropdown bugTypeDropdown;
        [SerializeField] private TMP_Dropdown severityDropdown;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button screenshotButton;
        [SerializeField] private Toggle includeSystemInfoToggle;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Configuration")]
        [SerializeField] private KeyCode reportBugKey = KeyCode.F2;
        [SerializeField] private bool enableQuickReport = true;
        [SerializeField] private float statusMessageDuration = 3f;
        
        // Bug report data
        private List<string> bugTypes = new List<string>
        {
            "Gameplay Bug",
            "UI Bug",
            "Audio Bug",
            "Visual Bug",
            "Performance Issue",
            "Crash",
            "Input Issue",
            "Balance Issue",
            "Other"
        };
        
        private List<string> severityLevels = new List<string>
        {
            "Critical - Game Breaking",
            "High - Major Impact",
            "Medium - Moderate Impact",
            "Low - Minor Issue",
            "Suggestion"
        };
        
        private bool isReportingBug = false;
        private string screenshotPath = "";
        
        private void Start()
        {
            InitializeUI();
            HideBugReportPanel();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(reportBugKey))
            {
                if (isReportingBug)
                    HideBugReportPanel();
                else
                    ShowBugReportPanel();
            }
        }
        
        /// <summary>
        /// Initialize the bug report UI
        /// </summary>
        private void InitializeUI()
        {
            // Setup dropdown options
            if (bugTypeDropdown != null)
            {
                bugTypeDropdown.ClearOptions();
                bugTypeDropdown.AddOptions(bugTypes);
            }
            
            if (severityDropdown != null)
            {
                severityDropdown.ClearOptions();
                severityDropdown.AddOptions(severityLevels);
            }
            
            // Setup button events
            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitBugReport);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(HideBugReportPanel);
            
            if (screenshotButton != null)
                screenshotButton.onClick.AddListener(TakeScreenshot);
            
            // Set default values
            if (includeSystemInfoToggle != null)
                includeSystemInfoToggle.isOn = true;
        }
        
        /// <summary>
        /// Show the bug report panel
        /// </summary>
        public void ShowBugReportPanel()
        {
            if (bugReportPanel != null)
            {
                bugReportPanel.SetActive(true);
                isReportingBug = true;
                
                // Pause game if in single player
                if (Time.timeScale == 1f)
                {
                    Time.timeScale = 0f;
                }
                
                // Focus on description input
                if (bugDescriptionInput != null)
                {
                    bugDescriptionInput.Select();
                    bugDescriptionInput.ActivateInputField();
                }
                
                // Clear previous data
                ClearReportData();
                
                ShowStatusMessage("Bug report panel opened. Press F2 to close.");
            }
        }
        
        /// <summary>
        /// Hide the bug report panel
        /// </summary>
        public void HideBugReportPanel()
        {
            if (bugReportPanel != null)
            {
                bugReportPanel.SetActive(false);
                isReportingBug = false;
                
                // Resume game
                Time.timeScale = 1f;
                
                ShowStatusMessage("Bug report panel closed.");
            }
        }
        
        /// <summary>
        /// Submit the bug report
        /// </summary>
        public void SubmitBugReport()
        {
            if (string.IsNullOrEmpty(bugDescriptionInput.text))
            {
                ShowStatusMessage("Please enter a bug description.");
                return;
            }
            
            // Collect report data
            var bugReport = new BugReport
            {
                description = bugDescriptionInput.text,
                type = bugTypes[bugTypeDropdown.value],
                severity = severityLevels[severityDropdown.value],
                timestamp = System.DateTime.Now,
                screenshotPath = screenshotPath,
                includeSystemInfo = includeSystemInfoToggle.isOn,
                gameState = CollectGameStateInfo(),
                systemInfo = includeSystemInfoToggle.isOn ? CollectSystemInfo() : ""
            };
            
            // Save bug report
            SaveBugReport(bugReport);
            
            // Log to debug tools
            var debugTools = DebugTools.Instance;
            if (debugTools != null)
            {
                debugTools.LogBug($"[{bugReport.severity}] {bugReport.type}: {bugReport.description}");
            }
            
            ShowStatusMessage($"Bug report submitted successfully! ({bugReport.type})");
            
            // Clear and hide panel
            ClearReportData();
            HideBugReportPanel();
        }
        
        /// <summary>
        /// Take a screenshot for the bug report
        /// </summary>
        public void TakeScreenshot()
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            screenshotPath = $"bug_screenshot_{timestamp}.png";
            
            // Take screenshot
            ScreenCapture.CaptureScreenshot(screenshotPath);
            
            ShowStatusMessage($"Screenshot saved: {screenshotPath}");
            
            // Update screenshot button text
            if (screenshotButton != null)
            {
                var buttonText = screenshotButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Screenshot Taken ✓";
                }
            }
        }
        
        /// <summary>
        /// Clear report data
        /// </summary>
        private void ClearReportData()
        {
            if (bugDescriptionInput != null)
                bugDescriptionInput.text = "";
            
            if (bugTypeDropdown != null)
                bugTypeDropdown.value = 0;
            
            if (severityDropdown != null)
                severityDropdown.value = 2; // Default to Medium
            
            screenshotPath = "";
            
            // Reset screenshot button text
            if (screenshotButton != null)
            {
                var buttonText = screenshotButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Take Screenshot";
                }
            }
        }
        
        /// <summary>
        /// Collect current game state information
        /// </summary>
        private string CollectGameStateInfo()
        {
            var gameState = new System.Text.StringBuilder();
            
            // Current scene
            gameState.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
            // Game manager state
            var gameManager = PlushLeague.Core.GameManager.Instance;
            if (gameManager != null)
            {
                gameState.AppendLine($"Game State: {gameManager.GetType().Name} active");
                // Add more specific state info based on your GameManager
            }
            
            // Match state
            var matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            if (matchManager != null)
            {
                gameState.AppendLine($"Match State: Active");
                // Add match-specific info like score, time remaining, etc.
            }
            
            // Player state
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            gameState.AppendLine($"Active Players: {players.Length}");
            
            // Ball state
            var ball = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
            if (ball != null)
            {
                gameState.AppendLine($"Ball Position: {ball.transform.position}");
                var ballRb = ball.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    gameState.AppendLine($"Ball Velocity: {ballRb.linearVelocity}");
                }
            }
            
            // Time info
            gameState.AppendLine($"Time Scale: {Time.timeScale}");
            gameState.AppendLine($"Game Time: {Time.time:F2}");
            
            return gameState.ToString();
        }
        
        /// <summary>
        /// Collect system information
        /// </summary>
        private string CollectSystemInfo()
        {
            var systemInfo = new System.Text.StringBuilder();
            
            systemInfo.AppendLine($"Unity Version: {Application.unityVersion}");
            systemInfo.AppendLine($"Platform: {Application.platform}");
            systemInfo.AppendLine($"Device Model: {SystemInfo.deviceModel}");
            systemInfo.AppendLine($"Device Name: {SystemInfo.deviceName}");
            systemInfo.AppendLine($"Operating System: {SystemInfo.operatingSystem}");
            systemInfo.AppendLine($"Processor: {SystemInfo.processorType}");
            systemInfo.AppendLine($"Memory: {SystemInfo.systemMemorySize} MB");
            systemInfo.AppendLine($"Graphics Device: {SystemInfo.graphicsDeviceName}");
            systemInfo.AppendLine($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
            systemInfo.AppendLine($"Screen Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
            systemInfo.AppendLine($"Target Frame Rate: {Application.targetFrameRate}");
            
            return systemInfo.ToString();
        }
        
        /// <summary>
        /// Save bug report to file
        /// </summary>
        private void SaveBugReport(BugReport report)
        {
            try
            {
                string filename = $"bug_report_{report.timestamp:yyyyMMdd_HHmmss}.txt";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                
                var reportText = new System.Text.StringBuilder();
                reportText.AppendLine("=== PLUSH LEAGUE BUG REPORT ===");
                reportText.AppendLine($"Date: {report.timestamp}");
                reportText.AppendLine($"Type: {report.type}");
                reportText.AppendLine($"Severity: {report.severity}");
                reportText.AppendLine();
                reportText.AppendLine("DESCRIPTION:");
                reportText.AppendLine(report.description);
                reportText.AppendLine();
                
                if (!string.IsNullOrEmpty(report.screenshotPath))
                {
                    reportText.AppendLine($"Screenshot: {report.screenshotPath}");
                    reportText.AppendLine();
                }
                
                if (report.includeSystemInfo)
                {
                    reportText.AppendLine("SYSTEM INFO:");
                    reportText.AppendLine(report.systemInfo);
                    reportText.AppendLine();
                }
                
                reportText.AppendLine("GAME STATE:");
                reportText.AppendLine(report.gameState);
                
                System.IO.File.WriteAllText(filePath, reportText.ToString());
                
                UnityEngine.Debug.Log($"Bug report saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save bug report: {e.Message}");
                ShowStatusMessage("Failed to save bug report!");
            }
        }
        
        /// <summary>
        /// Show status message
        /// </summary>
        private void ShowStatusMessage(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                StartCoroutine(ClearStatusMessage());
            }
        }
        
        /// <summary>
        /// Clear status message after delay
        /// </summary>
        private System.Collections.IEnumerator ClearStatusMessage()
        {
            yield return new WaitForSecondsRealtime(statusMessageDuration);
            if (statusText != null)
            {
                statusText.text = "";
            }
        }
        
        /// <summary>
        /// Quick bug report (minimal UI)
        /// </summary>
        public void QuickBugReport(string description, string type = "Gameplay Bug")
        {
            if (!enableQuickReport) return;
            
            var bugReport = new BugReport
            {
                description = description,
                type = type,
                severity = "Medium - Moderate Impact",
                timestamp = System.DateTime.Now,
                includeSystemInfo = true,
                gameState = CollectGameStateInfo(),
                systemInfo = CollectSystemInfo()
            };
            
            SaveBugReport(bugReport);
            
            var debugTools = DebugTools.Instance;
            if (debugTools != null)
            {
                debugTools.LogBug($"[QUICK REPORT] {type}: {description}");
            }
            
            ShowStatusMessage($"Quick bug report submitted: {type}");
        }
        
        [System.Serializable]
        private class BugReport
        {
            public string description;
            public string type;
            public string severity;
            public System.DateTime timestamp;
            public string screenshotPath;
            public bool includeSystemInfo;
            public string gameState;
            public string systemInfo;
        }
        
        #region Public API
        
        /// <summary>
        /// Report a bug from code
        /// </summary>
        public static void ReportBug(string description, string type = "Gameplay Bug", string severity = "Medium - Moderate Impact")
        {
            var bugReportUI = FindFirstObjectByType<BugReportUI>();
            if (bugReportUI != null)
            {
                bugReportUI.QuickBugReport(description, type);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"BugReportUI not found - Bug: {description}");
            }
        }
        
        #endregion
    }
}
