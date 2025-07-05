using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Enhanced debugging console with real-time parameter tuning and monitoring.
    /// Provides a comprehensive interface for playtesting and debugging.
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        [Header("Console Configuration")]
        [SerializeField] private bool enableConsole = true;
        [SerializeField] private KeyCode consoleToggleKey = KeyCode.F3;
        [SerializeField] private bool showOnStartup = false;
        [SerializeField] private bool persistentConsole = false;
        
        [Header("UI Settings")]
        [SerializeField] private int maxLogEntries = 100;
        [SerializeField] private float consoleHeight = 300f;
        [SerializeField] private float consoleWidth = 600f;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color successColor = Color.green;
        
        // Console state
        private bool consoleVisible = false;
        private List<LogEntry> logEntries = new List<LogEntry>();
        private Vector2 scrollPosition = Vector2.zero;
        private string commandInput = "";
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        
        // Parameter monitoring
        private Dictionary<string, ParameterMonitor> parameterMonitors = new Dictionary<string, ParameterMonitor>();
        private bool showParameterPanel = true;
        private bool showLogPanel = true;
        private bool showCommandPanel = true;
        
        // Console commands
        private Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();
        
        [System.Serializable]
        public class LogEntry
        {
            public string message;
            public LogType type;
            public float timestamp;
            public string stackTrace;
            
            public LogEntry(string msg, LogType logType, string stack = "")
            {
                message = msg;
                type = logType;
                timestamp = Time.time;
                stackTrace = stack;
            }
        }
        
        [System.Serializable]
        public class ParameterMonitor
        {
            public string name;
            public float value;
            public float minValue;
            public float maxValue;
            public float defaultValue;
            public string description;
            public System.Action<float> onValueChanged;
            
            public ParameterMonitor(string paramName, float defaultVal, float min, float max, string desc = "")
            {
                name = paramName;
                value = defaultVal;
                defaultValue = defaultVal;
                minValue = min;
                maxValue = max;
                description = desc;
            }
        }
        
        private static DebugConsole instance;
        public static DebugConsole Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DebugConsole>();
                    if (instance == null)
                    {
                        GameObject consoleObject = new GameObject("DebugConsole");
                        instance = consoleObject.AddComponent<DebugConsole>();
                        if (instance.persistentConsole)
                        {
                            DontDestroyOnLoad(consoleObject);
                        }
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
                if (persistentConsole)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeConsole();
        }
        
        private void Start()
        {
            consoleVisible = showOnStartup;
            
            // Subscribe to Unity's log messages
            Application.logMessageReceived += OnLogMessageReceived;
            
            Log("Debug Console initialized", LogType.Log);
            Log("Type 'help' for available commands", LogType.Log);
        }
        
        private void Update()
        {
            HandleInput();
            UpdateParameterMonitors();
        }
        
        private void InitializeConsole()
        {
            // Register default commands
            RegisterCommand("help", ShowHelp);
            RegisterCommand("clear", ClearLog);
            RegisterCommand("set", SetParameter);
            RegisterCommand("get", GetParameter);
            RegisterCommand("reset", ResetParameter);
            RegisterCommand("list", ListParameters);
            RegisterCommand("save", SaveParameters);
            RegisterCommand("load", LoadParameters);
            RegisterCommand("quit", QuitGame);
            RegisterCommand("scene", LoadScene);
            RegisterCommand("time", SetTimeScale);
            RegisterCommand("fps", ShowFPS);
            RegisterCommand("memory", ShowMemory);
            RegisterCommand("gc", ForceGarbageCollection);
            
            // Initialize default parameters
            AddParameter("player_speed", 5f, 1f, 15f, "Player movement speed");
            AddParameter("stamina_cost", 10f, 1f, 50f, "Stamina cost per action");
            AddParameter("powerup_cooldown", 3f, 0.5f, 10f, "Powerup cooldown time");
            AddParameter("ball_speed", 8f, 2f, 20f, "Ball movement speed");
            AddParameter("match_duration", 120f, 30f, 600f, "Match duration in seconds");
            AddParameter("score_to_win", 3f, 1f, 10f, "Score needed to win");
            
            UnityEngine.Debug.Log("Debug Console commands and parameters initialized");
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(consoleToggleKey))
            {
                ToggleConsole();
            }
            
            if (consoleVisible)
            {
                // Handle command history navigation
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    NavigateCommandHistory(-1);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    NavigateCommandHistory(1);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ExecuteCommand();
                }
            }
        }
        
        private void UpdateParameterMonitors()
        {
            foreach (var monitor in parameterMonitors.Values)
            {
                monitor.onValueChanged?.Invoke(monitor.value);
            }
        }
        
        private void OnGUI()
        {
            if (!enableConsole || !consoleVisible) return;
            
            // Calculate console rect
            float x = (Screen.width - consoleWidth) * 0.5f;
            float y = 50f;
            Rect consoleRect = new Rect(x, y, consoleWidth, consoleHeight);
            
            // Draw console background
            GUI.backgroundColor = backgroundColor;
            GUI.Box(consoleRect, "");
            
            GUILayout.BeginArea(consoleRect);
            
            // Console tabs
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Log", GUILayout.Width(60)))
            {
                showLogPanel = !showLogPanel;
            }
            if (GUILayout.Button("Params", GUILayout.Width(60)))
            {
                showParameterPanel = !showParameterPanel;
            }
            if (GUILayout.Button("Cmd", GUILayout.Width(60)))
            {
                showCommandPanel = !showCommandPanel;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                ToggleConsole();
            }
            GUILayout.EndHorizontal();
            
            // Panel content
            if (showLogPanel)
            {
                DrawLogPanel();
            }
            
            if (showParameterPanel)
            {
                DrawParameterPanel();
            }
            
            if (showCommandPanel)
            {
                DrawCommandPanel();
            }
            
            GUILayout.EndArea();
        }
        
        private void DrawLogPanel()
        {
            GUILayout.Label("=== LOG ===", GUILayout.ExpandWidth(false));
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            
            foreach (var entry in logEntries.TakeLast(maxLogEntries))
            {
                Color oldColor = GUI.contentColor;
                
                switch (entry.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        GUI.contentColor = errorColor;
                        break;
                    case LogType.Warning:
                        GUI.contentColor = warningColor;
                        break;
                    case LogType.Log:
                        GUI.contentColor = textColor;
                        break;
                }
                
                GUILayout.Label($"[{entry.timestamp:F2}] {entry.message}");
                GUI.contentColor = oldColor;
            }
            
            GUILayout.EndScrollView();
        }
        
        private void DrawParameterPanel()
        {
            GUILayout.Label("=== PARAMETERS ===", GUILayout.ExpandWidth(false));
            
            foreach (var monitor in parameterMonitors.Values)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(monitor.name, GUILayout.Width(120));
                
                float newValue = GUILayout.HorizontalSlider(monitor.value, monitor.minValue, monitor.maxValue, GUILayout.Width(150));
                if (newValue != monitor.value)
                {
                    monitor.value = newValue;
                    Log($"Parameter {monitor.name} set to {newValue:F2}", LogType.Log);
                }
                
                GUILayout.Label(monitor.value.ToString("F2"), GUILayout.Width(40));
                
                if (GUILayout.Button("R", GUILayout.Width(20)))
                {
                    monitor.value = monitor.defaultValue;
                    Log($"Parameter {monitor.name} reset to default", LogType.Log);
                }
                
                GUILayout.EndHorizontal();
            }
        }
        
        private void DrawCommandPanel()
        {
            GUILayout.Label("=== COMMANDS ===", GUILayout.ExpandWidth(false));
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(">", GUILayout.Width(15));
            
            GUI.SetNextControlName("CommandInput");
            commandInput = GUILayout.TextField(commandInput);
            
            if (GUILayout.Button("Execute", GUILayout.Width(60)))
            {
                ExecuteCommand();
            }
            
            GUILayout.EndHorizontal();
            
            // Auto-focus command input
            if (consoleVisible && showCommandPanel)
            {
                GUI.FocusControl("CommandInput");
            }
        }
        
        private void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(commandInput.Trim()))
                return;
            
            Log($"> {commandInput}", LogType.Log);
            
            // Add to history
            commandHistory.Add(commandInput);
            historyIndex = commandHistory.Count;
            
            // Parse and execute command
            string[] parts = commandInput.Split(' ');
            string command = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();
            
            if (commands.ContainsKey(command))
            {
                try
                {
                    commands[command](args);
                }
                catch (System.Exception e)
                {
                    LogError($"Command execution failed: {e.Message}");
                }
            }
            else
            {
                LogError($"Unknown command: {command}");
            }
            
            commandInput = "";
        }
        
        private void NavigateCommandHistory(int direction)
        {
            if (commandHistory.Count == 0) return;
            
            historyIndex += direction;
            historyIndex = Mathf.Clamp(historyIndex, 0, commandHistory.Count);
            
            if (historyIndex < commandHistory.Count)
            {
                commandInput = commandHistory[historyIndex];
            }
            else
            {
                commandInput = "";
            }
        }
        
        // Command implementations
        private void ShowHelp(string[] args)
        {
            Log("Available commands:", LogType.Log);
            foreach (var cmd in commands.Keys)
            {
                Log($"  {cmd}", LogType.Log);
            }
        }
        
        private void ClearLog(string[] args)
        {
            logEntries.Clear();
            Log("Log cleared", LogType.Log);
        }
        
        private void SetParameter(string[] args)
        {
            if (args.Length < 2)
            {
                LogError("Usage: set <parameter> <value>");
                return;
            }
            
            string paramName = args[0];
            if (float.TryParse(args[1], out float value))
            {
                SetParameterValue(paramName, value);
            }
            else
            {
                LogError("Invalid value format");
            }
        }
        
        private void GetParameter(string[] args)
        {
            if (args.Length < 1)
            {
                LogError("Usage: get <parameter>");
                return;
            }
            
            string paramName = args[0];
            if (parameterMonitors.ContainsKey(paramName))
            {
                var monitor = parameterMonitors[paramName];
                Log($"{paramName} = {monitor.value:F2} (range: {monitor.minValue:F2} - {monitor.maxValue:F2})", LogType.Log);
            }
            else
            {
                LogError($"Parameter '{paramName}' not found");
            }
        }
        
        private void ResetParameter(string[] args)
        {
            if (args.Length < 1)
            {
                LogError("Usage: reset <parameter>");
                return;
            }
            
            string paramName = args[0];
            if (parameterMonitors.ContainsKey(paramName))
            {
                parameterMonitors[paramName].value = parameterMonitors[paramName].defaultValue;
                Log($"Parameter {paramName} reset to default", LogType.Log);
            }
            else
            {
                LogError($"Parameter '{paramName}' not found");
            }
        }
        
        private void ListParameters(string[] args)
        {
            Log("Available parameters:", LogType.Log);
            foreach (var monitor in parameterMonitors.Values)
            {
                Log($"  {monitor.name}: {monitor.value:F2} ({monitor.description})", LogType.Log);
            }
        }
        
        private void SaveParameters(string[] args)
        {
            // Save current parameter values to PlayerPrefs
            foreach (var monitor in parameterMonitors.Values)
            {
                PlayerPrefs.SetFloat($"DebugParam_{monitor.name}", monitor.value);
            }
            PlayerPrefs.Save();
            Log("Parameters saved", LogType.Log);
        }
        
        private void LoadParameters(string[] args)
        {
            // Load parameter values from PlayerPrefs
            foreach (var monitor in parameterMonitors.Values)
            {
                if (PlayerPrefs.HasKey($"DebugParam_{monitor.name}"))
                {
                    monitor.value = PlayerPrefs.GetFloat($"DebugParam_{monitor.name}");
                }
            }
            Log("Parameters loaded", LogType.Log);
        }
        
        private void QuitGame(string[] args)
        {
            Log("Quitting game...", LogType.Log);
            Application.Quit();
        }
        
        private void LoadScene(string[] args)
        {
            if (args.Length < 1)
            {
                LogError("Usage: scene <scene_name>");
                return;
            }
            
            string sceneName = args[0];
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            Log($"Loading scene: {sceneName}", LogType.Log);
        }
        
        private void SetTimeScale(string[] args)
        {
            if (args.Length < 1)
            {
                LogError("Usage: time <scale>");
                return;
            }
            
            if (float.TryParse(args[0], out float scale))
            {
                Time.timeScale = scale;
                Log($"Time scale set to {scale:F2}", LogType.Log);
            }
            else
            {
                LogError("Invalid time scale format");
            }
        }
        
        private void ShowFPS(string[] args)
        {
            float fps = 1f / Time.unscaledDeltaTime;
            Log($"Current FPS: {fps:F1}", LogType.Log);
        }
        
        private void ShowMemory(string[] args)
        {
            float memory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            Log($"Memory usage: {memory:F2} MB", LogType.Log);
        }
        
        private void ForceGarbageCollection(string[] args)
        {
            System.GC.Collect();
            Log("Garbage collection forced", LogType.Log);
        }
        
        // Public API
        public void ToggleConsole()
        {
            consoleVisible = !consoleVisible;
            Log($"Console {(consoleVisible ? "opened" : "closed")}", LogType.Log);
        }
        
        public void Log(string message, LogType type = LogType.Log)
        {
            logEntries.Add(new LogEntry(message, type));
            
            if (logEntries.Count > maxLogEntries)
            {
                logEntries.RemoveAt(0);
            }
            
            // Keep scroll at bottom
            scrollPosition.y = float.MaxValue;
        }
        
        public void LogError(string message)
        {
            Log(message, LogType.Error);
        }
        
        public void LogWarning(string message)
        {
            Log(message, LogType.Warning);
        }
        
        public void AddParameter(string name, float defaultValue, float minValue, float maxValue, string description = "")
        {
            var monitor = new ParameterMonitor(name, defaultValue, minValue, maxValue, description);
            parameterMonitors[name] = monitor;
        }
        
        public void SetParameterValue(string name, float value)
        {
            if (parameterMonitors.ContainsKey(name))
            {
                parameterMonitors[name].value = Mathf.Clamp(value, parameterMonitors[name].minValue, parameterMonitors[name].maxValue);
                Log($"Parameter {name} set to {parameterMonitors[name].value:F2}", LogType.Log);
            }
            else
            {
                LogError($"Parameter '{name}' not found");
            }
        }
        
        public float GetParameterValue(string name)
        {
            return parameterMonitors.ContainsKey(name) ? parameterMonitors[name].value : 0f;
        }
        
        public void RegisterCommand(string command, System.Action<string[]> action)
        {
            commands[command.ToLower()] = action;
        }
        
        public void UnregisterCommand(string command)
        {
            commands.Remove(command.ToLower());
        }
        
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            logEntries.Add(new LogEntry(logString, type, stackTrace));
            
            if (logEntries.Count > maxLogEntries)
            {
                logEntries.RemoveAt(0);
            }
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
    }
}
