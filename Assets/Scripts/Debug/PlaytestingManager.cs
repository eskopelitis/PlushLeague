using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Comprehensive playtesting manager that coordinates all debug tools.
    /// Handles session management, data collection, and automated testing.
    /// </summary>
    public class PlaytestingManager : MonoBehaviour
    {
        [Header("Playtesting Configuration")]
        [SerializeField] private bool enablePlaytesting = true;
        [SerializeField] private bool autoStartSession = true;
        [SerializeField] private bool enableDataCollection = true;
        [SerializeField] private bool enableAutomatedTesting = false;
        [SerializeField] private string sessionName = "PlaytestSession";
        
        [Header("Debug Tools")]
        [SerializeField] private DebugTools debugTools;
        [SerializeField] private BalanceTester balanceTester;
        [SerializeField] private BugReportUI bugReportUI;
        
        [Header("Session Settings")]
        [SerializeField] private float sessionDuration = 1800f; // 30 minutes
        [SerializeField] private int maxMatches = 50;
        [SerializeField] private bool generateDetailedReport = true;
        [SerializeField] private bool autoSaveData = true;
        [SerializeField] private float dataSaveInterval = 300f; // 5 minutes
        
        // Session data
        private PlaytestSession currentSession;
        private bool sessionActive = false;
        private Coroutine sessionCoroutine;
        private Coroutine dataSaveCoroutine;
        
        // Events
        public static event System.Action<PlaytestSession> OnSessionStarted;
        public static event System.Action<PlaytestSession> OnSessionEnded;
        public static event System.Action<MatchData> OnMatchCompleted;
        public static event System.Action<BugReport> OnBugReported;
        
        [System.Serializable]
        public class PlaytestSession
        {
            public string sessionId;
            public string sessionName;
            public DateTime startTime;
            public DateTime endTime;
            public float duration;
            public int totalMatches;
            public List<MatchData> matches = new List<MatchData>();
            public List<BugReport> bugReports = new List<BugReport>();
            public List<BalanceNote> balanceNotes = new List<BalanceNote>();
            public SystemInformation systemInfo;
            public GameSettings gameSettings;
        }
        
        [System.Serializable]
        public class MatchData
        {
            public int matchId;
            public DateTime startTime;
            public DateTime endTime;
            public float duration;
            public int player1Score;
            public int player2Score;
            public string winner;
            public List<GameEvent> events = new List<GameEvent>();
            public PerformanceMetrics performance;
        }
        
        [System.Serializable]
        public class BugReport
        {
            public string id;
            public DateTime timestamp;
            public string description;
            public string type;
            public string severity;
            public string context;
            public Vector3 playerPosition;
            public string gameState;
            public bool includeScreenshot;
            public string screenshotPath;
        }
        
        [System.Serializable]
        public class BalanceNote
        {
            public DateTime timestamp;
            public string category;
            public string description;
            public float confidenceLevel;
            public Dictionary<string, float> suggestedChanges;
        }
        
        [System.Serializable]
        public class GameEvent
        {
            public float timestamp;
            public string type;
            public string description;
            public Vector3 position;
            public Dictionary<string, object> parameters;
        }
        
        [System.Serializable]
        public class PerformanceMetrics
        {
            public float averageFPS;
            public float minFPS;
            public float maxFPS;
            public float memoryUsage;
            public int frameDrops;
        }
        
        [System.Serializable]
        public class SystemInformation
        {
            public string deviceModel;
            public string operatingSystem;
            public string processorType;
            public int systemMemorySize;
            public string graphicsDeviceName;
            public int graphicsMemorySize;
            public string unityVersion;
        }
        
        [System.Serializable]
        public class GameSettings
        {
            public float playerSpeed;
            public float staminaCost;
            public float powerupCooldown;
            public float ballSpeed;
            public int matchDuration;
            public int scoreToWin;
        }
        
        private static PlaytestingManager instance;
        public static PlaytestingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PlaytestingManager>();
                    if (instance == null)
                    {
                        GameObject playtestObject = new GameObject("PlaytestingManager");
                        instance = playtestObject.AddComponent<PlaytestingManager>();
                        DontDestroyOnLoad(playtestObject);
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
            
            InitializePlaytesting();
        }
        
        private void Start()
        {
            if (autoStartSession)
            {
                StartPlaytestSession();
            }
        }
        
        private void InitializePlaytesting()
        {
            // Initialize debug tools if not assigned
            if (debugTools == null)
                debugTools = FindFirstObjectByType<DebugTools>();
            
            if (balanceTester == null)
                balanceTester = FindFirstObjectByType<BalanceTester>();
            
            if (bugReportUI == null)
                bugReportUI = FindFirstObjectByType<BugReportUI>();
            
            // Create directories
            Directory.CreateDirectory(Application.persistentDataPath + "/PlaytestData");
            Directory.CreateDirectory(Application.persistentDataPath + "/PlaytestData/Sessions");
            Directory.CreateDirectory(Application.persistentDataPath + "/PlaytestData/Reports");
            Directory.CreateDirectory(Application.persistentDataPath + "/PlaytestData/Screenshots");
            
            UnityEngine.Debug.Log("Playtesting Manager initialized");
        }
          public void StartPlaytestSession()
        {
            if (sessionActive)
            {
                UnityEngine.Debug.LogWarning("Playtest session already active");
                return;
            }
            
            if (!enablePlaytesting)
            {
                UnityEngine.Debug.LogWarning("Playtesting is disabled");
                return;
            }

            currentSession = new PlaytestSession
            {
                sessionId = Guid.NewGuid().ToString(),
                sessionName = sessionName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                startTime = DateTime.Now,
                systemInfo = CollectSystemInfo(),
                gameSettings = CollectGameSettings()
            };

            sessionActive = true;
            sessionCoroutine = StartCoroutine(SessionTimer());

            if (autoSaveData && enableDataCollection)
            {
                dataSaveCoroutine = StartCoroutine(AutoSaveData());
            }
            
            if (enableAutomatedTesting)
            {
                StartCoroutine(RunAutomatedTestingSuite());
            }

            OnSessionStarted?.Invoke(currentSession);
            UnityEngine.Debug.Log($"Playtest session started: {currentSession.sessionName}");
        }
        
        public void EndPlaytestSession()
        {
            if (!sessionActive)
            {
                UnityEngine.Debug.LogWarning("No active playtest session");
                return;
            }
            
            currentSession.endTime = DateTime.Now;
            currentSession.duration = (float)(currentSession.endTime - currentSession.startTime).TotalSeconds;
            sessionActive = false;
            
            if (sessionCoroutine != null)
            {
                StopCoroutine(sessionCoroutine);
                sessionCoroutine = null;
            }
            
            if (dataSaveCoroutine != null)
            {
                StopCoroutine(dataSaveCoroutine);
                dataSaveCoroutine = null;
            }
            
            SaveSessionData();
            
            if (generateDetailedReport)
            {
                GenerateDetailedReport();
            }
            
            OnSessionEnded?.Invoke(currentSession);
            UnityEngine.Debug.Log($"Playtest session ended: {currentSession.sessionName}");
        }
        
        public void RecordMatchStart(int matchId)
        {
            if (!sessionActive) return;
            
            MatchData match = new MatchData
            {
                matchId = matchId,
                startTime = DateTime.Now,
                performance = new PerformanceMetrics()
            };
            
            currentSession.matches.Add(match);
            StartCoroutine(RecordMatchPerformance(match));
        }
        
        public void RecordMatchEnd(int matchId, int player1Score, int player2Score, string winner)
        {
            if (!sessionActive) return;
            
            MatchData match = currentSession.matches.Find(m => m.matchId == matchId);
            if (match != null)
            {
                match.endTime = DateTime.Now;
                match.duration = (float)(match.endTime - match.startTime).TotalSeconds;
                match.player1Score = player1Score;
                match.player2Score = player2Score;
                match.winner = winner;
                
                currentSession.totalMatches++;
                OnMatchCompleted?.Invoke(match);
                
                UnityEngine.Debug.Log($"Match {matchId} recorded: {player1Score}-{player2Score}, Winner: {winner}");
            }
        }
        
        public void RecordGameEvent(string type, string description, Vector3 position, Dictionary<string, object> parameters = null)
        {
            if (!sessionActive) return;
            
            if (currentSession.matches.Count > 0)
            {
                MatchData currentMatch = currentSession.matches[currentSession.matches.Count - 1];
                
                GameEvent gameEvent = new GameEvent
                {
                    timestamp = (float)(DateTime.Now - currentMatch.startTime).TotalSeconds,
                    type = type,
                    description = description,
                    position = position,
                    parameters = parameters ?? new Dictionary<string, object>()
                };
                
                currentMatch.events.Add(gameEvent);
            }
        }
        
        public void RecordBugReport(string description, string type, string severity, bool includeScreenshot = false)
        {
            if (!sessionActive) return;
            
            BugReport bug = new BugReport
            {
                id = Guid.NewGuid().ToString(),
                timestamp = DateTime.Now,
                description = description,
                type = type,
                severity = severity,
                context = GetCurrentGameContext(),
                playerPosition = GetPlayerPosition(),
                gameState = GetCurrentGameState(),
                includeScreenshot = includeScreenshot
            };
            
            if (includeScreenshot)
            {
                StartCoroutine(CaptureScreenshot(bug));
            }
            
            currentSession.bugReports.Add(bug);
            OnBugReported?.Invoke(bug);
            
            UnityEngine.Debug.Log($"Bug reported: {type} - {severity} - {description}");
        }
        
        public void RecordBalanceNote(string category, string description, float confidence, Dictionary<string, float> suggestedChanges = null)
        {
            if (!sessionActive) return;
            
            BalanceNote note = new BalanceNote
            {
                timestamp = DateTime.Now,
                category = category,
                description = description,
                confidenceLevel = confidence,
                suggestedChanges = suggestedChanges ?? new Dictionary<string, float>()
            };
            
            currentSession.balanceNotes.Add(note);
            UnityEngine.Debug.Log($"Balance note recorded: {category} - {description}");
        }
        
        private IEnumerator SessionTimer()
        {
            float elapsed = 0f;
            while (elapsed < sessionDuration && sessionActive)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (sessionActive)
            {
                UnityEngine.Debug.Log("Session duration reached, ending session");
                EndPlaytestSession();
            }
        }
        
        private IEnumerator AutoSaveData()
        {
            while (sessionActive)
            {
                yield return new WaitForSeconds(dataSaveInterval);
                SaveSessionData();
                UnityEngine.Debug.Log("Session data auto-saved");
            }
        }
        
        private IEnumerator RecordMatchPerformance(MatchData match)
        {
            float elapsed = 0f;
            float totalFPS = 0f;
            int frameCount = 0;
            float minFPS = float.MaxValue;
            float maxFPS = 0f;
            int frameDrops = 0;
            float lastFrameTime = Time.unscaledTime;
            
            while (match.endTime == default(DateTime))
            {
                float currentFPS = 1f / Time.unscaledDeltaTime;
                totalFPS += currentFPS;
                frameCount++;
                
                if (currentFPS < minFPS) minFPS = currentFPS;
                if (currentFPS > maxFPS) maxFPS = currentFPS;
                
                // Count significant frame drops (> 16ms for 60fps)
                if (Time.unscaledTime - lastFrameTime > 0.016f)
                {
                    frameDrops++;
                }
                
                lastFrameTime = Time.unscaledTime;
                elapsed += Time.unscaledDeltaTime;
                
                yield return null;
            }
            
            match.performance.averageFPS = totalFPS / frameCount;
            match.performance.minFPS = minFPS;
            match.performance.maxFPS = maxFPS;
            match.performance.memoryUsage = (float)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f; // MB
            match.performance.frameDrops = frameDrops;
        }
        
        private IEnumerator CaptureScreenshot(BugReport bug)
        {
            yield return new WaitForEndOfFrame();
            
            string filename = $"bug_{bug.id}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            string path = Path.Combine(Application.persistentDataPath, "PlaytestData", "Screenshots", filename);
            
            ScreenCapture.CaptureScreenshot(path);
            bug.screenshotPath = path;
            
            UnityEngine.Debug.Log($"Screenshot captured for bug report: {path}");
        }
        
        private void SaveSessionData()
        {
            if (currentSession == null) return;
            
            string filename = $"{currentSession.sessionName}.json";
            string path = Path.Combine(Application.persistentDataPath, "PlaytestData", "Sessions", filename);
            
            try
            {
                string json = JsonUtility.ToJson(currentSession, true);
                File.WriteAllText(path, json);
                UnityEngine.Debug.Log($"Session data saved: {path}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save session data: {e.Message}");
            }
        }
        
        private void GenerateDetailedReport()
        {
            if (currentSession == null) return;
            
            StringBuilder report = new StringBuilder();
            
            // Header
            report.AppendLine("=== PLAYTEST SESSION REPORT ===");
            report.AppendLine($"Session: {currentSession.sessionName}");
            report.AppendLine($"Duration: {currentSession.duration:F2} seconds");
            report.AppendLine($"Total Matches: {currentSession.totalMatches}");
            report.AppendLine($"Bug Reports: {currentSession.bugReports.Count}");
            report.AppendLine($"Balance Notes: {currentSession.balanceNotes.Count}");
            report.AppendLine();
            
            // Match Statistics
            report.AppendLine("=== MATCH STATISTICS ===");
            if (currentSession.matches.Count > 0)
            {
                float avgDuration = 0f;
                float avgFPS = 0f;
                int player1Wins = 0;
                int player2Wins = 0;
                int ties = 0;
                
                foreach (var match in currentSession.matches)
                {
                    avgDuration += match.duration;
                    avgFPS += match.performance.averageFPS;
                    
                    if (match.winner == "Player1") player1Wins++;
                    else if (match.winner == "Player2") player2Wins++;
                    else ties++;
                }
                
                avgDuration /= currentSession.matches.Count;
                avgFPS /= currentSession.matches.Count;
                
                report.AppendLine($"Average Match Duration: {avgDuration:F2} seconds");
                report.AppendLine($"Average FPS: {avgFPS:F2}");
                report.AppendLine($"Player 1 Wins: {player1Wins}");
                report.AppendLine($"Player 2 Wins: {player2Wins}");
                report.AppendLine($"Ties: {ties}");
                report.AppendLine($"Win Rate Balance: {(float)player1Wins / currentSession.totalMatches:P2} / {(float)player2Wins / currentSession.totalMatches:P2}");
            }
            report.AppendLine();
            
            // Bug Reports
            report.AppendLine("=== BUG REPORTS ===");
            foreach (var bug in currentSession.bugReports)
            {
                report.AppendLine($"[{bug.severity}] {bug.type}: {bug.description}");
                report.AppendLine($"  Time: {bug.timestamp:HH:mm:ss}");
                report.AppendLine($"  Context: {bug.context}");
                report.AppendLine();
            }
            
            // Balance Notes
            report.AppendLine("=== BALANCE NOTES ===");
            foreach (var note in currentSession.balanceNotes)
            {
                report.AppendLine($"[{note.category}] {note.description}");
                report.AppendLine($"  Confidence: {note.confidenceLevel:P2}");
                report.AppendLine($"  Time: {note.timestamp:HH:mm:ss}");
                report.AppendLine();
            }
            
            // Save report
            string filename = $"report_{currentSession.sessionName}.txt";
            string path = Path.Combine(Application.persistentDataPath, "PlaytestData", "Reports", filename);
            
            try
            {
                File.WriteAllText(path, report.ToString());
                UnityEngine.Debug.Log($"Detailed report generated: {path}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to generate report: {e.Message}");
            }
        }
        
        private SystemInformation CollectSystemInfo()
        {
            return new SystemInformation
            {
                deviceModel = UnityEngine.SystemInfo.deviceModel,
                operatingSystem = UnityEngine.SystemInfo.operatingSystem,
                processorType = UnityEngine.SystemInfo.processorType,
                systemMemorySize = UnityEngine.SystemInfo.systemMemorySize,
                graphicsDeviceName = UnityEngine.SystemInfo.graphicsDeviceName,
                graphicsMemorySize = UnityEngine.SystemInfo.graphicsMemorySize,
                unityVersion = Application.unityVersion
            };
        }
        
        private GameSettings CollectGameSettings()
        {
            // Get current game settings from debug tools or game manager
            return new GameSettings
            {
                playerSpeed = debugTools?.GetParameter("playerSpeed") ?? 5f,
                staminaCost = debugTools?.GetParameter("staminaCost") ?? 10f,
                powerupCooldown = debugTools?.GetParameter("powerupCooldown") ?? 3f,
                ballSpeed = debugTools?.GetParameter("ballSpeed") ?? 8f,
                matchDuration = (int)(debugTools?.GetParameter("matchDuration") ?? 120f),
                scoreToWin = (int)(debugTools?.GetParameter("scoreToWin") ?? 3f)
            };
        }
        
        private string GetCurrentGameContext()
        {
            return $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, " +
                   $"Players: {FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None).Length}";
        }
        
        private Vector3 GetPlayerPosition()
        {
            var player = FindFirstObjectByType<PlushLeague.Gameplay.Player.PlayerController>();
            return player != null ? player.transform.position : Vector3.zero;
        }
        
        private string GetCurrentGameState()
        {
            var gameManager = FindFirstObjectByType<PlushLeague.Core.GameManager>();
            return gameManager != null ? "GameManager Found" : "No GameManager";
        }
        
        /// <summary>
        /// Run automated testing suite
        /// </summary>
        private IEnumerator RunAutomatedTestingSuite()
        {
            UnityEngine.Debug.Log("Running automated testing suite...");
            
            // Check if we have enough matches for meaningful testing
            while (sessionActive && currentSession.matches.Count < maxMatches)
            {
                yield return new WaitForSeconds(60f); // Check every minute
                
                // Run automated balance checks
                if (currentSession.matches.Count > 5)
                {
                    CheckWinRateBalance();
                }
            }
            
            UnityEngine.Debug.Log("Automated testing suite completed");
        }
        
        /// <summary>
        /// Check win rate balance
        /// </summary>
        private void CheckWinRateBalance()
        {
            int player1Wins = 0;
            int player2Wins = 0;
            
            foreach (var match in currentSession.matches)
            {
                if (match.winner == "Player1") player1Wins++;
                else if (match.winner == "Player2") player2Wins++;
            }
            
            int totalMatches = player1Wins + player2Wins;
            if (totalMatches > 0)
            {
                float winRate = (float)player1Wins / totalMatches;
                
                if (winRate < 0.3f || winRate > 0.7f)
                {
                    RecordBalanceNote("WinRateImbalance", 
                        $"Win rate imbalance detected: Player1 {winRate:P2}", 
                        0.8f);
                }
            }
        }
        
        // Public API for external access
        public PlaytestSession GetCurrentSession() => currentSession;
        public bool IsSessionActive() => sessionActive;
        public void ForceEndSession() => EndPlaytestSession();
        public void ExportSessionData(string format = "json") => SaveSessionData();
        
        private void OnDestroy()
        {
            if (sessionActive)
            {
                EndPlaytestSession();
            }
        }
    }
}
