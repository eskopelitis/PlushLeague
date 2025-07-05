using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Central integration manager for all debug and playtesting tools.
    /// Coordinates DebugTools, PlaytestingManager, DebugConsole, and EdgeCaseTester.
    /// </summary>
    public class DebugIntegrationManager : MonoBehaviour
    {
        [Header("Debug Tool References")]
        [SerializeField] private DebugTools debugTools;
        [SerializeField] private PlaytestingManager playtestingManager;
        [SerializeField] private DebugConsole debugConsole;
        [SerializeField] private EdgeCaseTester edgeCaseTester;
        [SerializeField] private BalanceTester balanceTester;
        [SerializeField] private BugReportUI bugReportUI;
        
        [Header("Integration Settings")]
        [SerializeField] private bool autoInitializeTools = true;
        [SerializeField] private bool enableCrossToolCommunication = true;
        [SerializeField] private bool enableUnifiedLogging = true;
        [SerializeField] private bool enableDataSharing = true;
        
        [Header("Hotkey Configuration")]
        [SerializeField] private KeyCode masterDebugKey = KeyCode.F12;
        [SerializeField] private KeyCode quickTestKey = KeyCode.F9;
        [SerializeField] private KeyCode emergencyResetKey = KeyCode.F8;
        [SerializeField] private KeyCode dataExportKey = KeyCode.F7;
        
        // Integration state
        private bool toolsInitialized = false;
        private bool masterDebugMode = false;
        private Dictionary<string, System.Action> quickActions = new Dictionary<string, System.Action>();
        
        // Events for cross-tool communication
        public static event System.Action<string, object> OnDebugEvent;
        public static event System.Action<string, float> OnParameterChanged;
        public static event System.Action<string> OnTestCompleted;
        
        private static DebugIntegrationManager instance;
        public static DebugIntegrationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DebugIntegrationManager>();
                    if (instance == null)
                    {
                        GameObject integrationObject = new GameObject("DebugIntegrationManager");
                        instance = integrationObject.AddComponent<DebugIntegrationManager>();
                        DontDestroyOnLoad(integrationObject);
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
            
            if (autoInitializeTools)
            {
                InitializeDebugTools();
            }
        }
        
        private void Start()
        {
            SetupCrossToolCommunication();
            RegisterQuickActions();
            
            UnityEngine.Debug.Log("Debug Integration Manager initialized");
        }
        
        private void Update()
        {
            HandleMasterDebugInput();
        }
        
        /// <summary>
        /// Initialize all debug tools and establish references
        /// </summary>
        public void InitializeDebugTools()
        {
            if (toolsInitialized) return;
            
            // Find or create debug tools
            if (debugTools == null)
            {
                debugTools = DebugTools.Instance;
            }
            
            if (playtestingManager == null)
            {
                playtestingManager = PlaytestingManager.Instance;
            }
            
            if (debugConsole == null)
            {
                debugConsole = DebugConsole.Instance;
            }
            
            if (edgeCaseTester == null)
            {
                edgeCaseTester = EdgeCaseTester.Instance;
            }
            
            if (balanceTester == null)
            {
                balanceTester = FindFirstObjectByType<BalanceTester>();
            }
            
            if (bugReportUI == null)
            {
                bugReportUI = FindFirstObjectByType<BugReportUI>();
            }
            
            toolsInitialized = true;
            UnityEngine.Debug.Log("All debug tools initialized");
        }
        
        /// <summary>
        /// Setup cross-tool communication and event handlers
        /// </summary>
        private void SetupCrossToolCommunication()
        {
            if (!enableCrossToolCommunication) return;
            
            // Subscribe to events
            OnDebugEvent += HandleDebugEvent;
            OnParameterChanged += HandleParameterChanged;
            OnTestCompleted += HandleTestCompleted;
            
            // Register console commands for other tools
            if (debugConsole != null)
            {
                debugConsole.RegisterCommand("playtest", StartPlaytestSession);
                debugConsole.RegisterCommand("edgetest", StartEdgeCaseTesting);
                debugConsole.RegisterCommand("balance", StartBalanceTesting);
                debugConsole.RegisterCommand("export", ExportAllData);
                debugConsole.RegisterCommand("reset", EmergencyReset);
                debugConsole.RegisterCommand("bug", ShowBugReport);
                debugConsole.RegisterCommand("status", ShowToolStatus);
            }
            
            UnityEngine.Debug.Log("Cross-tool communication setup complete");
        }
        
        /// <summary>
        /// Register quick actions for rapid testing
        /// </summary>
        private void RegisterQuickActions()
        {
            quickActions["ResetMatch"] = () => {
                debugTools?.ResetMatch();
                BroadcastDebugEvent("QuickAction", "ResetMatch");
            };
            
            quickActions["FillStamina"] = () => {
                debugTools?.FillAllStamina();
                BroadcastDebugEvent("QuickAction", "FillStamina");
            };
            
            quickActions["ResetBall"] = () => {
                debugTools?.ResetBallIfStuck();
                BroadcastDebugEvent("QuickAction", "ResetBall");
            };
            
            quickActions["ToggleSlowMo"] = () => {
                debugTools?.ToggleSlowMotion();
                BroadcastDebugEvent("QuickAction", "ToggleSlowMotion");
            };
            
            quickActions["RunEdgeTest"] = () => {
                edgeCaseTester?.StartEdgeCaseTesting();
                BroadcastDebugEvent("QuickAction", "RunEdgeTest");
            };
            
            quickActions["ExportData"] = () => {
                ExportAllData(new string[0]);
                BroadcastDebugEvent("QuickAction", "ExportData");
            };
        }
        
        /// <summary>
        /// Handle master debug input
        /// </summary>
        private void HandleMasterDebugInput()
        {
            if (Input.GetKeyDown(masterDebugKey))
            {
                ToggleMasterDebugMode();
            }
            
            if (Input.GetKeyDown(quickTestKey))
            {
                RunQuickTest();
            }
            
            if (Input.GetKeyDown(emergencyResetKey))
            {
                EmergencyReset(new string[0]);
            }
            
            if (Input.GetKeyDown(dataExportKey))
            {
                ExportAllData(new string[0]);
            }
        }
        
        /// <summary>
        /// Toggle master debug mode (enables/disables all debug tools)
        /// </summary>
        public void ToggleMasterDebugMode()
        {
            masterDebugMode = !masterDebugMode;
            
            if (debugTools != null)
            {
                debugTools.enabled = masterDebugMode;
            }
            
            if (debugConsole != null)
            {
                debugConsole.enabled = masterDebugMode;
            }
            
            if (edgeCaseTester != null)
            {
                edgeCaseTester.enabled = masterDebugMode;
            }
            
            BroadcastDebugEvent("MasterDebugMode", masterDebugMode);
            UnityEngine.Debug.Log($"Master debug mode: {(masterDebugMode ? "ON" : "OFF")}");
        }
        
        /// <summary>
        /// Run a quick test sequence
        /// </summary>
        public void RunQuickTest()
        {
            StartCoroutine(QuickTestSequence());
        }
        
        private IEnumerator QuickTestSequence()
        {
            UnityEngine.Debug.Log("Starting quick test sequence...");
            
            // Test 1: Reset match
            quickActions["ResetMatch"]();
            yield return new WaitForSeconds(1f);
            
            // Test 2: Fill stamina
            quickActions["FillStamina"]();
            yield return new WaitForSeconds(0.5f);
            
            // Test 3: Reset ball
            quickActions["ResetBall"]();
            yield return new WaitForSeconds(0.5f);
            
            // Test 4: Parameter test
            if (debugTools != null)
            {
                float originalSpeed = debugTools.GetParameter("playerspeed");
                debugTools.SetParameter("playerspeed", originalSpeed * 2f);
                yield return new WaitForSeconds(2f);
                debugTools.SetParameter("playerspeed", originalSpeed);
            }
            
            UnityEngine.Debug.Log("Quick test sequence completed");
            BroadcastDebugEvent("QuickTestCompleted", Time.time);
        }
        
        /// <summary>
        /// Handle debug events from other tools
        /// </summary>
        private void HandleDebugEvent(string eventType, object data)
        {
            if (enableUnifiedLogging)
            {
                UnityEngine.Debug.Log($"Debug Event: {eventType} - {data}");
            }
            
            // Log to playtesting manager
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordGameEvent(eventType, data.ToString(), Vector3.zero);
            }
            
            // Log to debug console
            if (debugConsole != null)
            {
                debugConsole.Log($"Event: {eventType} - {data}");
            }
        }
        
        /// <summary>
        /// Handle parameter changes from other tools
        /// </summary>
        private void HandleParameterChanged(string paramName, float value)
        {
            if (enableDataSharing)
            {
                // Sync parameter across all tools
                if (debugTools != null)
                {
                    debugTools.SetParameter(paramName, value);
                }
                
                if (debugConsole != null)
                {
                    debugConsole.SetParameterValue(paramName, value);
                }
            }
            
            BroadcastDebugEvent("ParameterChanged", $"{paramName}={value}");
        }
        
        /// <summary>
        /// Handle test completion from other tools
        /// </summary>
        private void HandleTestCompleted(string testName)
        {
            UnityEngine.Debug.Log($"Test completed: {testName}");
            
            if (playtestingManager != null && playtestingManager.IsSessionActive())
            {
                playtestingManager.RecordBalanceNote("TestCompletion", $"Test {testName} completed", 1f);
            }
        }
        
        // Console command implementations
        private void StartPlaytestSession(string[] args)
        {
            if (playtestingManager != null)
            {
                if (!playtestingManager.IsSessionActive())
                {
                    playtestingManager.StartPlaytestSession();
                    UnityEngine.Debug.Log("Playtest session started");
                }
                else
                {
                    UnityEngine.Debug.Log("Playtest session already active");
                }
            }
        }
        
        private void StartEdgeCaseTesting(string[] args)
        {
            if (edgeCaseTester != null)
            {
                edgeCaseTester.StartEdgeCaseTesting();
                UnityEngine.Debug.Log("Edge case testing started");
            }
        }
        
        private void StartBalanceTesting(string[] args)
        {
            if (balanceTester != null)
            {
                StartCoroutine(balanceTester.RunAutomatedBalanceTests());
                UnityEngine.Debug.Log("Balance testing started");
            }
        }
        
        private void ExportAllData(string[] args)
        {
            UnityEngine.Debug.Log("Exporting all debug data...");
            
            if (playtestingManager != null)
            {
                playtestingManager.ExportSessionData();
            }
            
            // Note: DebugConsole.SaveParameters is private, skipping console data export
            
            UnityEngine.Debug.Log("Data export completed");
        }
        
        private void EmergencyReset(string[] args)
        {
            UnityEngine.Debug.Log("Emergency reset triggered!");
            
            // Reset all systems
            if (debugTools != null)
            {
                debugTools.ResetMatch();
                debugTools.FillAllStamina();
                debugTools.ResetBallIfStuck();
            }
            
            // Reset time scale
            Time.timeScale = 1f;
            
            // Force garbage collection
            System.GC.Collect();
            
            BroadcastDebugEvent("EmergencyReset", Time.time);
        }
        
        private void ShowBugReport(string[] args)
        {
            if (bugReportUI != null)
            {
                bugReportUI.ShowBugReportPanel();
            }
        }
        
        private void ShowToolStatus(string[] args)
        {
            UnityEngine.Debug.Log("=== DEBUG TOOL STATUS ===");
            UnityEngine.Debug.Log($"Debug Tools: {(debugTools != null ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Playtesting Manager: {(playtestingManager != null && playtestingManager.IsSessionActive() ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Debug Console: {(debugConsole != null ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Edge Case Tester: {(edgeCaseTester != null ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Balance Tester: {(balanceTester != null ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Bug Report UI: {(bugReportUI != null ? "Active" : "Inactive")}");
            UnityEngine.Debug.Log($"Master Debug Mode: {masterDebugMode}");
        }
        
        // Public API for external tools
        public static void BroadcastDebugEvent(string eventType, object data)
        {
            OnDebugEvent?.Invoke(eventType, data);
        }
        
        public static void BroadcastParameterChange(string paramName, float value)
        {
            OnParameterChanged?.Invoke(paramName, value);
        }
        
        public static void BroadcastTestCompletion(string testName)
        {
            OnTestCompleted?.Invoke(testName);
        }
        
        public void ExecuteQuickAction(string actionName)
        {
            if (quickActions.ContainsKey(actionName))
            {
                quickActions[actionName]();
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Unknown quick action: {actionName}");
            }
        }
        
        public bool IsToolInitialized(string toolName)
        {
            switch (toolName.ToLower())
            {
                case "debugtools":
                    return debugTools != null;
                case "playtestingmanager":
                    return playtestingManager != null;
                case "debugconsole":
                    return debugConsole != null;
                case "edgecasetester":
                    return edgeCaseTester != null;
                case "balancetester":
                    return balanceTester != null;
                case "bugreportui":
                    return bugReportUI != null;
                default:
                    return false;
            }
        }
        
        public T GetDebugTool<T>() where T : MonoBehaviour
        {
            if (typeof(T) == typeof(DebugTools))
                return debugTools as T;
            else if (typeof(T) == typeof(PlaytestingManager))
                return playtestingManager as T;
            else if (typeof(T) == typeof(DebugConsole))
                return debugConsole as T;
            else if (typeof(T) == typeof(EdgeCaseTester))
                return edgeCaseTester as T;
            else if (typeof(T) == typeof(BalanceTester))
                return balanceTester as T;
            else if (typeof(T) == typeof(BugReportUI))
                return bugReportUI as T;
            else
                return null;
        }
        
        // Performance monitoring
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                BroadcastDebugEvent("ApplicationPaused", Time.time);
            }
            else
            {
                BroadcastDebugEvent("ApplicationResumed", Time.time);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            BroadcastDebugEvent("ApplicationFocus", hasFocus);
        }
        
        private void OnDestroy()
        {
            // Cleanup events
            OnDebugEvent -= HandleDebugEvent;
            OnParameterChanged -= HandleParameterChanged;
            OnTestCompleted -= HandleTestCompleted;
        }
    }
}
