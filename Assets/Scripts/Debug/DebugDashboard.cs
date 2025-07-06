using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Comprehensive debug dashboard for Step 13 playtesting tools.
    /// Provides a unified interface for all debug and analytics features.
    /// </summary>
    public class DebugDashboard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dashboardPanel;
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Tab Templates")]
        [SerializeField] private GameObject tabButtonPrefab;
        [SerializeField] private GameObject contentPanelPrefab;
        
        [Header("Overview Tab")]
        [SerializeField] private TextMeshProUGUI overviewText;
        [SerializeField] private Slider performanceSlider;
        [SerializeField] private Slider balanceSlider;
        [SerializeField] private Slider engagementSlider;
        [SerializeField] private TextMeshProUGUI metricsText;
        
        [Header("Testing Tab")]
        [SerializeField] private Button runTestSuiteButton;
        [SerializeField] private Button runSingleTestButton;
        [SerializeField] private TMP_Dropdown testSelectionDropdown;
        [SerializeField] private TextMeshProUGUI testResultsText;
        [SerializeField] private ScrollRect testResultsScroll;
        
        [Header("Analytics Tab")]
        [SerializeField] private Button generateReportButton;
        [SerializeField] private Button exportDataButton;
        [SerializeField] private TextMeshProUGUI analyticsText;
        [SerializeField] private Transform metricsContainer;
        [SerializeField] private GameObject metricDisplayPrefab;
        
        [Header("Bug Reporting Tab")]
        [SerializeField] private TMP_InputField bugDescriptionField;
        [SerializeField] private TMP_Dropdown bugTypeDropdown;
        [SerializeField] private TMP_Dropdown bugSeverityDropdown;
        [SerializeField] private Button submitBugButton;
        [SerializeField] private TextMeshProUGUI bugHistoryText;
        
        [Header("Configuration")]
        [SerializeField] private KeyCode toggleDashboardKey = KeyCode.F1;
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private int maxLogEntries = 100;
        
        // Dashboard state
        private bool dashboardActive = false;
        private int currentTabIndex = 0;
        private List<TabInfo> tabs = new List<TabInfo>();
        private Dictionary<string, GameObject> tabPanels = new Dictionary<string, GameObject>();
        private List<string> logEntries = new List<string>();
        
        // Dependencies
        private Step13Implementation step13Implementation;
        private AutomatedTestFramework testFramework;
        private PlaytestingAnalytics analytics;
        private PlaytestingManager playtestingManager;
        private BugReportUI bugReportUI;
        private DebugConsole debugConsole;
        
        // Update tracking
        private float lastUpdateTime;
        
        [System.Serializable]
        public class TabInfo
        {
            public string name;
            public GameObject button;
            public GameObject panel;
            public System.Action updateAction;
            
            public TabInfo(string n, GameObject b, GameObject p, System.Action a)
            {
                name = n;
                button = b;
                panel = p;
                updateAction = a;
            }
        }
        
        private void Awake()
        {
            InitializeDashboard();
        }
        
        private void Start()
        {
            if (showOnStart)
            {
                ShowDashboard();
            }
            else
            {
                HideDashboard();
            }
        }
        
        private void Update()
        {
            HandleInput();
            
            if (dashboardActive && Time.time - lastUpdateTime > updateInterval)
            {
                UpdateDashboard();
                lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize the dashboard system
        /// </summary>
        private void InitializeDashboard()
        {
            // Find dependencies
            step13Implementation = FindFirstObjectByType<Step13Implementation>();
            testFramework = FindFirstObjectByType<AutomatedTestFramework>();
            analytics = FindFirstObjectByType<PlaytestingAnalytics>();
            playtestingManager = FindFirstObjectByType<PlaytestingManager>();
            bugReportUI = FindFirstObjectByType<BugReportUI>();
            debugConsole = DebugConsole.Instance;
            
            // Setup UI
            SetupTabs();
            SetupEventHandlers();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Debug Dashboard initialized");
        }
        
        /// <summary>
        /// Setup dashboard tabs
        /// </summary>
        private void SetupTabs()
        {
            // Clear existing tabs
            tabs.Clear();
            tabPanels.Clear();
            
            // Create tabs
            CreateTab("Overview", UpdateOverviewTab);
            CreateTab("Testing", UpdateTestingTab);
            CreateTab("Analytics", UpdateAnalyticsTab);
            CreateTab("Bug Reports", UpdateBugReportsTab);
            CreateTab("Performance", UpdatePerformanceTab);
            CreateTab("Console", UpdateConsoleTab);
            
            // Select first tab
            SelectTab(0);
        }
        
        /// <summary>
        /// Create a new tab
        /// </summary>
        private void CreateTab(string name, System.Action updateAction)
        {
            // Create tab button
            GameObject tabButton = Instantiate(tabButtonPrefab, tabContainer);
            Button buttonComponent = tabButton.GetComponent<Button>();
            TextMeshProUGUI buttonText = tabButton.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = name;
            }
            
            // Create content panel
            GameObject contentPanel = Instantiate(contentPanelPrefab, contentContainer);
            contentPanel.name = $"{name}Panel";
            
            // Setup tab button click
            int tabIndex = tabs.Count;
            buttonComponent.onClick.AddListener(() => SelectTab(tabIndex));
            
            // Create tab info
            var tabInfo = new TabInfo(name, tabButton, contentPanel, updateAction);
            tabs.Add(tabInfo);
            tabPanels[name] = contentPanel;
        }
        
        /// <summary>
        /// Select a tab by index
        /// </summary>
        private void SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;
            
            currentTabIndex = index;
            
            // Update tab buttons
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                var button = tab.button.GetComponent<Button>();
                var colors = button.colors;
                
                if (i == currentTabIndex)
                {
                    colors.normalColor = Color.white;
                    tab.panel.SetActive(true);
                }
                else
                {
                    colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
                    tab.panel.SetActive(false);
                }
                
                button.colors = colors;
            }
            
            // Update immediately
            UpdateCurrentTab();
        }
        
        /// <summary>
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideDashboard);
            }
            
            if (runTestSuiteButton != null)
            {
                runTestSuiteButton.onClick.AddListener(RunTestSuite);
            }
            
            if (runSingleTestButton != null)
            {
                runSingleTestButton.onClick.AddListener(RunSingleTest);
            }
            
            if (generateReportButton != null)
            {
                generateReportButton.onClick.AddListener(GenerateAnalyticsReport);
            }
            
            if (exportDataButton != null)
            {
                exportDataButton.onClick.AddListener(ExportAnalyticsData);
            }
            
            if (submitBugButton != null)
            {
                submitBugButton.onClick.AddListener(SubmitBugReport);
            }
        }
        
        /// <summary>
        /// Subscribe to debug events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (testFramework != null)
            {
                AutomatedTestFramework.OnTestCompleted += OnTestCompleted;
                AutomatedTestFramework.OnTestReportGenerated += OnTestReportGenerated;
            }
            
            if (analytics != null)
            {
                PlaytestingAnalytics.OnReportGenerated += OnAnalyticsReportGenerated;
                PlaytestingAnalytics.OnInsightGenerated += OnInsightGenerated;
            }
            
            if (playtestingManager != null)
            {
                PlaytestingManager.OnMatchCompleted += OnMatchCompleted;
                PlaytestingManager.OnBugReported += OnBugReported;
            }
        }
        
        /// <summary>
        /// Handle input
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(toggleDashboardKey))
            {
                ToggleDashboard();
            }
        }
        
        /// <summary>
        /// Toggle dashboard visibility
        /// </summary>
        public void ToggleDashboard()
        {
            if (dashboardActive)
            {
                HideDashboard();
            }
            else
            {
                ShowDashboard();
            }
        }
        
        /// <summary>
        /// Show the dashboard
        /// </summary>
        public void ShowDashboard()
        {
            dashboardActive = true;
            
            if (dashboardPanel != null)
            {
                dashboardPanel.SetActive(true);
            }
            
            UpdateDashboard();
        }
        
        /// <summary>
        /// Hide the dashboard
        /// </summary>
        public void HideDashboard()
        {
            dashboardActive = false;
            
            if (dashboardPanel != null)
            {
                dashboardPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update the dashboard
        /// </summary>
        private void UpdateDashboard()
        {
            if (!dashboardActive) return;
            
            // Update status
            UpdateStatus();
            
            // Update current tab
            UpdateCurrentTab();
        }
        
        /// <summary>
        /// Update dashboard status
        /// </summary>
        private void UpdateStatus()
        {
            if (statusText != null)
            {
                string status = "Dashboard Active";
                
                if (step13Implementation != null)
                {
                    status += $" | Step 13: {(step13Implementation.enabled ? "Active" : "Inactive")}";
                }
                
                if (testFramework != null)
                {
                    status += $" | Testing: {(testFramework.IsTestingInProgress() ? "Running" : "Idle")}";
                }
                
                statusText.text = status;
            }
        }
        
        /// <summary>
        /// Update current tab
        /// </summary>
        private void UpdateCurrentTab()
        {
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
            {
                var tab = tabs[currentTabIndex];
                tab.updateAction?.Invoke();
            }
        }
        
        /// <summary>
        /// Update overview tab
        /// </summary>
        private void UpdateOverviewTab()
        {
            if (overviewText != null)
            {
                var overview = GenerateOverview();
                overviewText.text = overview;
            }
            
            // Update performance indicators
            if (analytics != null)
            {
                if (performanceSlider != null)
                {
                    var fpsMetric = analytics.GetMetric("FPS");
                    if (fpsMetric != null)
                    {
                        performanceSlider.value = Mathf.Clamp01(fpsMetric.currentValue / 60f);
                    }
                }
                
                if (balanceSlider != null)
                {
                    var winRateMetric = analytics.GetMetric("MaxWinRate");
                    if (winRateMetric != null)
                    {
                        float balance = 1f - Mathf.Abs(winRateMetric.averageValue - 0.5f) * 2f;
                        balanceSlider.value = Mathf.Clamp01(balance);
                    }
                }
                
                if (engagementSlider != null)
                {
                    var durationMetric = analytics.GetMetric("AverageMatchDuration");
                    if (durationMetric != null)
                    {
                        float target = 180f; // 3 minutes
                        float engagement = 1f - Mathf.Abs(durationMetric.averageValue - target) / target;
                        engagementSlider.value = Mathf.Clamp01(engagement);
                    }
                }
            }
        }
        
        /// <summary>
        /// Update testing tab
        /// </summary>
        private void UpdateTestingTab()
        {
            if (testResultsText != null && testFramework != null)
            {
                var stats = testFramework.GetTestStatistics();
                string results = $"Tests Run: {stats.total}\n";
                results += $"Passed: {stats.passed}\n";
                results += $"Failed: {stats.failed}\n";
                results += $"Success Rate: {stats.successRate:F1}%\n\n";
                
                // Add recent test results
                var testResults = testFramework.GetTestResults();
                foreach (var result in testResults.Values.TakeLast(5))
                {
                    results += $"{result.testName}: {(result.passed ? "PASS" : "FAIL")}\n";
                }
                
                testResultsText.text = results;
            }
        }
        
        /// <summary>
        /// Update analytics tab
        /// </summary>
        private void UpdateAnalyticsTab()
        {
            if (analyticsText != null && analytics != null)
            {
                string analyticsInfo = "=== Analytics Summary ===\n\n";
                
                // Performance metrics
                var performanceMetrics = analytics.GetMetricsByCategory("Performance");
                if (performanceMetrics.Count > 0)
                {
                    analyticsInfo += "Performance:\n";
                    foreach (var metric in performanceMetrics)
                    {
                        analyticsInfo += $"  {metric.Key}: {metric.Value.currentValue:F2}\n";
                    }
                    analyticsInfo += "\n";
                }
                
                // Gameplay metrics
                var gameplayMetrics = analytics.GetMetricsByCategory("Gameplay");
                if (gameplayMetrics.Count > 0)
                {
                    analyticsInfo += "Gameplay:\n";
                    foreach (var metric in gameplayMetrics)
                    {
                        analyticsInfo += $"  {metric.Key}: {metric.Value.currentValue:F2}\n";
                    }
                    analyticsInfo += "\n";
                }
                
                // Balance metrics
                var balanceMetrics = analytics.GetMetricsByCategory("Balance");
                if (balanceMetrics.Count > 0)
                {
                    analyticsInfo += "Balance:\n";
                    foreach (var metric in balanceMetrics)
                    {
                        analyticsInfo += $"  {metric.Key}: {metric.Value.currentValue:F2}\n";
                    }
                }
                
                analyticsText.text = analyticsInfo;
            }
        }
        
        /// <summary>
        /// Update bug reports tab
        /// </summary>
        private void UpdateBugReportsTab()
        {
            if (bugHistoryText != null && playtestingManager != null)
            {
                string bugInfo = "=== Recent Bug Reports ===\n\n";
                
                if (playtestingManager.IsSessionActive())
                {
                    var session = playtestingManager.GetCurrentSession();
                    var recentBugs = session.bugReports.TakeLast(5);
                    
                    foreach (var bug in recentBugs)
                    {
                        bugInfo += $"[{bug.severity}] {bug.type}: {bug.description}\n";
                        bugInfo += $"  Reported: {bug.timestamp:HH:mm:ss}\n\n";
                    }
                }
                
                if (bugInfo.Length <= 30) // Only header
                {
                    bugInfo += "No recent bug reports.";
                }
                
                bugHistoryText.text = bugInfo;
            }
        }
        
        /// <summary>
        /// Update performance tab
        /// </summary>
        private void UpdatePerformanceTab()
        {
            // Performance tab content would be handled by dedicated performance monitoring UI
        }
        
        /// <summary>
        /// Update console tab
        /// </summary>
        private void UpdateConsoleTab()
        {
            // Console tab content would be handled by the debug console
        }
        
        /// <summary>
        /// Generate overview text
        /// </summary>
        private string GenerateOverview()
        {
            string overview = "=== Step 13 Debug Dashboard ===\n\n";
            
            // System status
            overview += "System Status:\n";
            overview += $"  Step 13 Tools: {(step13Implementation != null && step13Implementation.enabled ? "Active" : "Inactive")}\n";
            overview += $"  Testing Framework: {(testFramework != null ? "Available" : "Unavailable")}\n";
            overview += $"  Analytics: {(analytics != null ? "Active" : "Inactive")}\n";
            overview += $"  Playtesting Manager: {(playtestingManager != null ? "Active" : "Inactive")}\n\n";
            
            // Current metrics
            if (analytics != null)
            {
                overview += "Current Metrics:\n";
                
                var fpsMetric = analytics.GetMetric("FPS");
                if (fpsMetric != null)
                {
                    overview += $"  FPS: {fpsMetric.currentValue:F1}\n";
                }
                
                var memoryMetric = analytics.GetMetric("MemoryUsage");
                if (memoryMetric != null)
                {
                    overview += $"  Memory: {memoryMetric.currentValue:F1} MB\n";
                }
                
                var matchesMetric = analytics.GetMetric("TotalMatches");
                if (matchesMetric != null)
                {
                    overview += $"  Matches: {matchesMetric.currentValue:F0}\n";
                }
                
                overview += "\n";
            }
            
            // Recent activity
            overview += "Recent Activity:\n";
            foreach (var entry in logEntries.TakeLast(3))
            {
                overview += $"  {entry}\n";
            }
            
            return overview;
        }
        
        // Button handlers
        private void RunTestSuite()
        {
            if (testFramework != null)
            {
                testFramework.StartTesting();
                AddLogEntry("Test suite started");
            }
        }
        
        private void RunSingleTest()
        {
            AddLogEntry("Single test run requested");
        }
        
        private void GenerateAnalyticsReport()
        {
            if (analytics != null)
            {
                var report = analytics.GenerateReport();
                AddLogEntry($"Analytics report generated: {report.reportId}");
            }
        }
        
        private void ExportAnalyticsData()
        {
            if (analytics != null)
            {
                string fileName = $"analytics_export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
                analytics.ExportToCSV(fileName);
                AddLogEntry($"Analytics data exported to {fileName}");
            }
        }
        
        private void SubmitBugReport()
        {
            if (bugDescriptionField != null && bugDescriptionField.text.Length > 0)
            {
                string description = bugDescriptionField.text;
                string type = bugTypeDropdown != null ? bugTypeDropdown.options[bugTypeDropdown.value].text : "General";
                string severity = bugSeverityDropdown != null ? bugSeverityDropdown.options[bugSeverityDropdown.value].text : "Medium";
                
                // Submit bug report
                if (playtestingManager != null)
                {
                    playtestingManager.RecordBugReport(description, type, severity);
                    AddLogEntry($"Bug report submitted: {type} - {description}");
                    
                    // Clear form
                    bugDescriptionField.text = "";
                }
            }
        }
        
        // Event handlers
        private void OnTestCompleted(AutomatedTestFramework.TestResult result)
        {
            AddLogEntry($"Test completed: {result.testName} - {(result.passed ? "PASS" : "FAIL")}");
        }
        
        private void OnTestReportGenerated(AutomatedTestFramework.TestReport report)
        {
            AddLogEntry($"Test report generated - Success rate: {report.successRate:F1}%");
        }
        
        private void OnAnalyticsReportGenerated(PlaytestingAnalytics.AnalyticsReport report)
        {
            AddLogEntry($"Analytics report generated: {report.reportId}");
        }
        
        private void OnInsightGenerated(PlaytestingAnalytics.AnalyticsInsight insight)
        {
            AddLogEntry($"Insight: {insight.title} - {insight.confidence:P0} confidence");
        }
        
        private void OnMatchCompleted(PlaytestingManager.MatchData match)
        {
            AddLogEntry($"Match completed: {match.winner} won in {match.duration:F1}s");
        }
        
        private void OnBugReported(PlaytestingManager.BugReport bug)
        {
            AddLogEntry($"Bug reported: {bug.type} - {bug.severity}");
        }
        
        /// <summary>
        /// Add a log entry
        /// </summary>
        private void AddLogEntry(string entry)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            logEntries.Add($"{timestamp}: {entry}");
            
            // Keep reasonable history
            if (logEntries.Count > maxLogEntries)
            {
                logEntries.RemoveAt(0);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (testFramework != null)
            {
                AutomatedTestFramework.OnTestCompleted -= OnTestCompleted;
                AutomatedTestFramework.OnTestReportGenerated -= OnTestReportGenerated;
            }
            
            if (analytics != null)
            {
                PlaytestingAnalytics.OnReportGenerated -= OnAnalyticsReportGenerated;
                PlaytestingAnalytics.OnInsightGenerated -= OnInsightGenerated;
            }
            
            if (playtestingManager != null)
            {
                PlaytestingManager.OnMatchCompleted -= OnMatchCompleted;
                PlaytestingManager.OnBugReported -= OnBugReported;
            }
        }
    }
}
