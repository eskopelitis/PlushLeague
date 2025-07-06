using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Comprehensive data analytics system for playtesting.
    /// Collects, analyzes, and reports on gameplay metrics and player behavior.
    /// </summary>
    public class PlaytestingAnalytics : MonoBehaviour
    {
        [Header("Analytics Configuration")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool autoGenerateReports = true;
        [SerializeField] private float reportInterval = 300f; // 5 minutes
        [SerializeField] private bool enableRealTimeMonitoring = true;
        [SerializeField] private string dataOutputPath = "PlaytestingData";
        
        [Header("Data Collection")]
        [SerializeField] private bool collectPlayerMetrics = true;
        [SerializeField] private bool collectPerformanceMetrics = true;
        [SerializeField] private bool collectGameplayMetrics = true;
        [SerializeField] private bool collectBalanceMetrics = true;
        [SerializeField] private bool collectUserExperienceMetrics = true;
        
        [Header("Analytics Thresholds")]
        [SerializeField] private float significantWinRateDeviation = 0.15f;
        [SerializeField] private float averageMatchDurationTarget = 180f; // 3 minutes
        [SerializeField] private float playerEngagementThreshold = 0.7f;
        [SerializeField] private int minSampleSize = 10;
        
        // Data storage
        private Dictionary<string, MetricData> metrics = new Dictionary<string, MetricData>();
        private List<PlayerSession> playerSessions = new List<PlayerSession>();
        private List<MatchAnalysis> matchAnalyses = new List<MatchAnalysis>();
        private List<PerformanceSnapshot> performanceSnapshots = new List<PerformanceSnapshot>();
        
        // Analytics state
        private bool analyticsActive = false;
        private DateTime sessionStartTime;
        private Coroutine analyticsCoroutine;
        private Coroutine reportCoroutine;
        
        // Dependencies
        private PlaytestingManager playtestingManager;
        private AutomatedTestFramework testFramework;
        
        // Events
        public static event System.Action<AnalyticsReport> OnReportGenerated;
        public static event System.Action<string, object> OnMetricUpdated;
        public static event System.Action<AnalyticsInsight> OnInsightGenerated;
        
        [System.Serializable]
        public class MetricData
        {
            public string name;
            public string category;
            public List<float> values = new List<float>();
            public DateTime lastUpdated;
            public float currentValue;
            public float averageValue;
            public float minValue = float.MaxValue;
            public float maxValue = float.MinValue;
            public int sampleCount;
            
            public void AddValue(float value)
            {
                values.Add(value);
                currentValue = value;
                lastUpdated = DateTime.Now;
                sampleCount++;
                
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
                
                // Calculate average
                float sum = 0f;
                foreach (float v in values)
                {
                    sum += v;
                }
                averageValue = sum / values.Count;
                
                // Keep reasonable history size
                if (values.Count > 1000)
                {
                    values.RemoveAt(0);
                }
            }
            
            public float GetStandardDeviation()
            {
                if (values.Count < 2) return 0f;
                
                float avg = averageValue;
                float sum = 0f;
                
                foreach (float value in values)
                {
                    sum += (value - avg) * (value - avg);
                }
                
                return Mathf.Sqrt(sum / values.Count);
            }
            
            public float GetTrend()
            {
                if (values.Count < 2) return 0f;
                
                int recentCount = Mathf.Min(10, values.Count);
                float recentAvg = 0f;
                float previousAvg = 0f;
                
                for (int i = values.Count - recentCount; i < values.Count; i++)
                {
                    recentAvg += values[i];
                }
                recentAvg /= recentCount;
                
                int previousStart = Mathf.Max(0, values.Count - recentCount * 2);
                int previousEnd = values.Count - recentCount;
                int previousCount = previousEnd - previousStart;
                
                if (previousCount > 0)
                {
                    for (int i = previousStart; i < previousEnd; i++)
                    {
                        previousAvg += values[i];
                    }
                    previousAvg /= previousCount;
                    
                    return (recentAvg - previousAvg) / previousAvg;
                }
                
                return 0f;
            }
        }
        
        [System.Serializable]
        public class PlayerSession
        {
            public string playerId;
            public DateTime startTime;
            public DateTime endTime;
            public float duration;
            public int matchesPlayed;
            public int wins;
            public int losses;
            public float averageMatchDuration;
            public float engagementScore;
            public List<string> actions = new List<string>();
            public Dictionary<string, float> customMetrics = new Dictionary<string, float>();
        }
        
        [System.Serializable]
        public class MatchAnalysis
        {
            public string matchId;
            public DateTime timestamp;
            public float duration;
            public string winner;
            public string loser;
            public int totalEvents;
            public float averageEventRate;
            public float competitiveness; // How close the match was
            public List<string> significantEvents = new List<string>();
            public Dictionary<string, float> playerMetrics = new Dictionary<string, float>();
        }
        
        [System.Serializable]
        public class PerformanceSnapshot
        {
            public DateTime timestamp;
            public float fps;
            public float memoryUsageMB;
            public float cpuUsagePercent;
            public int activeObjects;
            public float renderTime;
            public float updateTime;
        }
        
        [System.Serializable]
        public class AnalyticsReport
        {
            public string reportId;
            public DateTime generatedAt;
            public TimeSpan reportPeriod;
            public Dictionary<string, MetricData> metrics;
            public List<PlayerSession> sessions;
            public List<MatchAnalysis> matches;
            public List<AnalyticsInsight> insights;
            public Dictionary<string, object> summary;
            
            public AnalyticsReport()
            {
                reportId = System.Guid.NewGuid().ToString();
                generatedAt = DateTime.Now;
                metrics = new Dictionary<string, MetricData>();
                sessions = new List<PlayerSession>();
                matches = new List<MatchAnalysis>();
                insights = new List<AnalyticsInsight>();
                summary = new Dictionary<string, object>();
            }
        }
        
        [System.Serializable]
        public class AnalyticsInsight
        {
            public string title;
            public string description;
            public string category;
            public float confidence;
            public string recommendation;
            public Dictionary<string, object> supportingData;
            
            public AnalyticsInsight(string t, string d, string c, float conf, string rec)
            {
                title = t;
                description = d;
                category = c;
                confidence = conf;
                recommendation = rec;
                supportingData = new Dictionary<string, object>();
            }
        }
        
        private void Awake()
        {
            InitializeAnalytics();
        }
        
        private void Start()
        {
            if (enableAnalytics)
            {
                StartAnalytics();
            }
        }
        
        private void Update()
        {
            if (analyticsActive && enableRealTimeMonitoring)
            {
                CollectRealTimeMetrics();
            }
        }
        
        /// <summary>
        /// Initialize the analytics system
        /// </summary>
        private void InitializeAnalytics()
        {
            // Find dependencies
            playtestingManager = FindFirstObjectByType<PlaytestingManager>();
            testFramework = FindFirstObjectByType<AutomatedTestFramework>();
            
            // Create output directory
            string fullPath = Path.Combine(Application.persistentDataPath, dataOutputPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Playtesting Analytics initialized");
        }
        
        /// <summary>
        /// Subscribe to relevant events for data collection
        /// </summary>
        private void SubscribeToEvents()
        {
            if (playtestingManager != null)
            {
                PlaytestingManager.OnMatchCompleted += OnMatchCompleted;
                PlaytestingManager.OnSessionStarted += OnSessionStarted;
                PlaytestingManager.OnSessionEnded += OnSessionEnded;
            }
            
            if (testFramework != null)
            {
                AutomatedTestFramework.OnTestCompleted += OnTestCompleted;
                AutomatedTestFramework.OnTestReportGenerated += OnTestReportGenerated;
            }
        }
        
        /// <summary>
        /// Start analytics collection
        /// </summary>
        public void StartAnalytics()
        {
            if (analyticsActive)
            {
                Debug.LogWarning("Analytics already active");
                return;
            }
            
            analyticsActive = true;
            sessionStartTime = DateTime.Now;
            
            // Start analytics coroutine
            analyticsCoroutine = StartCoroutine(AnalyticsMainLoop());
            
            // Start report generation
            if (autoGenerateReports)
            {
                reportCoroutine = StartCoroutine(GeneratePeriodicReports());
            }
            
            Debug.Log("Analytics started");
        }
        
        /// <summary>
        /// Stop analytics collection
        /// </summary>
        public void StopAnalytics()
        {
            analyticsActive = false;
            
            if (analyticsCoroutine != null)
            {
                StopCoroutine(analyticsCoroutine);
                analyticsCoroutine = null;
            }
            
            if (reportCoroutine != null)
            {
                StopCoroutine(reportCoroutine);
                reportCoroutine = null;
            }
            
            Debug.Log("Analytics stopped");
        }
        
        /// <summary>
        /// Main analytics loop
        /// </summary>
        private IEnumerator AnalyticsMainLoop()
        {
            while (analyticsActive)
            {
                // Collect periodic metrics
                CollectPeriodicMetrics();
                
                // Analyze collected data
                AnalyzeData();
                
                // Generate insights
                GenerateInsights();
                
                // Wait before next collection
                yield return new WaitForSeconds(10f);
            }
        }
        
        /// <summary>
        /// Collect real-time metrics
        /// </summary>
        private void CollectRealTimeMetrics()
        {
            if (collectPerformanceMetrics)
            {
                UpdateMetric("FPS", "Performance", 1f / Time.deltaTime);
                UpdateMetric("DeltaTime", "Performance", Time.deltaTime * 1000f); // milliseconds
                
                if (Time.frameCount % 60 == 0) // Every 60 frames
                {
                    float memoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
                    UpdateMetric("MemoryUsage", "Performance", memoryMB);
                    
                    CreatePerformanceSnapshot();
                }
            }
        }
        
        /// <summary>
        /// Collect periodic metrics
        /// </summary>
        private void CollectPeriodicMetrics()
        {
            if (collectGameplayMetrics)
            {
                // Collect gameplay metrics from playtesting manager
                if (playtestingManager != null && playtestingManager.IsSessionActive())
                {
                    var session = playtestingManager.GetCurrentSession();
                    
                    UpdateMetric("TotalMatches", "Gameplay", session.matches.Count);
                    
                    if (session.matches.Count > 0)
                    {
                        float averageMatchDuration = session.matches.Average(m => m.duration);
                        UpdateMetric("AverageMatchDuration", "Gameplay", averageMatchDuration);
                        
                        // Calculate win rate balance
                        var winnerCounts = session.matches.GroupBy(m => m.winner).ToDictionary(g => g.Key, g => g.Count());
                        if (winnerCounts.Count > 0)
                        {
                            float maxWinRate = winnerCounts.Values.Max() / (float)session.matches.Count;
                            UpdateMetric("MaxWinRate", "Balance", maxWinRate);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Create a performance snapshot
        /// </summary>
        private void CreatePerformanceSnapshot()
        {
            var snapshot = new PerformanceSnapshot
            {
                timestamp = DateTime.Now,
                fps = 1f / Time.deltaTime,
                memoryUsageMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f,
                cpuUsagePercent = 0f, // Would need platform-specific implementation
                activeObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length,
                renderTime = Time.deltaTime * 1000f,
                updateTime = Time.fixedDeltaTime * 1000f
            };
            
            performanceSnapshots.Add(snapshot);
            
            // Keep reasonable history
            if (performanceSnapshots.Count > 500)
            {
                performanceSnapshots.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Update a metric with a new value
        /// </summary>
        public void UpdateMetric(string name, string category, float value)
        {
            if (!metrics.ContainsKey(name))
            {
                metrics[name] = new MetricData { name = name, category = category };
            }
            
            metrics[name].AddValue(value);
            OnMetricUpdated?.Invoke(name, value);
        }
        
        /// <summary>
        /// Analyze collected data for patterns and issues
        /// </summary>
        private void AnalyzeData()
        {
            // Analyze win rate balance
            if (metrics.ContainsKey("MaxWinRate"))
            {
                var winRateMetric = metrics["MaxWinRate"];
                if (winRateMetric.sampleCount >= minSampleSize)
                {
                    float deviation = Mathf.Abs(winRateMetric.averageValue - 0.5f);
                    if (deviation > significantWinRateDeviation)
                    {
                        var insight = new AnalyticsInsight(
                            "Win Rate Imbalance",
                            $"Win rates are significantly imbalanced (deviation: {deviation:P2})",
                            "Balance",
                            0.8f,
                            "Review game balance parameters and consider adjustments"
                        );
                        insight.supportingData["WinRateDeviation"] = deviation;
                        insight.supportingData["SampleSize"] = winRateMetric.sampleCount;
                        
                        OnInsightGenerated?.Invoke(insight);
                    }
                }
            }
            
            // Analyze match duration
            if (metrics.ContainsKey("AverageMatchDuration"))
            {
                var durationMetric = metrics["AverageMatchDuration"];
                if (durationMetric.sampleCount >= minSampleSize)
                {
                    float deviation = Mathf.Abs(durationMetric.averageValue - averageMatchDurationTarget);
                    if (deviation > averageMatchDurationTarget * 0.3f) // 30% deviation
                    {
                        var insight = new AnalyticsInsight(
                            "Match Duration Variance",
                            $"Match duration deviates significantly from target (current: {durationMetric.averageValue:F1}s, target: {averageMatchDurationTarget:F1}s)",
                            "Gameplay",
                            0.7f,
                            "Consider adjusting game pacing or win conditions"
                        );
                        insight.supportingData["CurrentDuration"] = durationMetric.averageValue;
                        insight.supportingData["TargetDuration"] = averageMatchDurationTarget;
                        insight.supportingData["Deviation"] = deviation;
                        
                        OnInsightGenerated?.Invoke(insight);
                    }
                }
            }
            
            // Analyze performance trends
            if (metrics.ContainsKey("FPS"))
            {
                var fpsMetric = metrics["FPS"];
                if (fpsMetric.sampleCount >= minSampleSize)
                {
                    float trend = fpsMetric.GetTrend();
                    if (trend < -0.1f) // 10% decline
                    {
                        var insight = new AnalyticsInsight(
                            "Performance Decline",
                            $"FPS showing declining trend ({trend:P2})",
                            "Performance",
                            0.8f,
                            "Investigate potential memory leaks or performance bottlenecks"
                        );
                        insight.supportingData["FPSTrend"] = trend;
                        insight.supportingData["CurrentFPS"] = fpsMetric.currentValue;
                        insight.supportingData["AverageFPS"] = fpsMetric.averageValue;
                        
                        OnInsightGenerated?.Invoke(insight);
                    }
                }
            }
        }
        
        /// <summary>
        /// Generate insights from collected data
        /// </summary>
        private void GenerateInsights()
        {
            // Generate summary insights
            var insights = new List<AnalyticsInsight>();
            
            // Overall session health
            if (metrics.Count > 0)
            {
                float performanceScore = CalculatePerformanceScore();
                float balanceScore = CalculateBalanceScore();
                float engagementScore = CalculateEngagementScore();
                
                string overallStatus = "Good";
                if (performanceScore < 0.7f || balanceScore < 0.7f || engagementScore < 0.7f)
                {
                    overallStatus = "Needs Attention";
                }
                
                var overallInsight = new AnalyticsInsight(
                    "Overall Session Health",
                    $"Session status: {overallStatus}",
                    "Summary",
                    0.9f,
                    "Continue monitoring key metrics"
                );
                overallInsight.supportingData["PerformanceScore"] = performanceScore;
                overallInsight.supportingData["BalanceScore"] = balanceScore;
                overallInsight.supportingData["EngagementScore"] = engagementScore;
                
                insights.Add(overallInsight);
            }
        }
        
        /// <summary>
        /// Calculate overall performance score
        /// </summary>
        private float CalculatePerformanceScore()
        {
            float score = 1.0f;
            
            if (metrics.ContainsKey("FPS"))
            {
                var fpsMetric = metrics["FPS"];
                score *= Mathf.Clamp01(fpsMetric.averageValue / 60f);
            }
            
            if (metrics.ContainsKey("MemoryUsage"))
            {
                var memoryMetric = metrics["MemoryUsage"];
                score *= Mathf.Clamp01(1f - (memoryMetric.averageValue / 1000f)); // Penalty for high memory usage
            }
            
            return score;
        }
        
        /// <summary>
        /// Calculate balance score
        /// </summary>
        private float CalculateBalanceScore()
        {
            float score = 1.0f;
            
            if (metrics.ContainsKey("MaxWinRate"))
            {
                var winRateMetric = metrics["MaxWinRate"];
                float deviation = Mathf.Abs(winRateMetric.averageValue - 0.5f);
                score *= Mathf.Clamp01(1f - deviation * 2f);
            }
            
            return score;
        }
        
        /// <summary>
        /// Calculate engagement score
        /// </summary>
        private float CalculateEngagementScore()
        {
            float score = 1.0f;
            
            if (metrics.ContainsKey("AverageMatchDuration"))
            {
                var durationMetric = metrics["AverageMatchDuration"];
                float deviation = Mathf.Abs(durationMetric.averageValue - averageMatchDurationTarget);
                score *= Mathf.Clamp01(1f - (deviation / averageMatchDurationTarget));
            }
            
            return score;
        }
        
        /// <summary>
        /// Generate periodic reports
        /// </summary>
        private IEnumerator GeneratePeriodicReports()
        {
            while (analyticsActive)
            {
                yield return new WaitForSeconds(reportInterval);
                
                var report = GenerateReport();
                OnReportGenerated?.Invoke(report);
                
                // Save report to file
                SaveReportToFile(report);
                
                Debug.Log($"Analytics report generated: {report.reportId}");
            }
        }
        
        /// <summary>
        /// Generate a comprehensive analytics report
        /// </summary>
        public AnalyticsReport GenerateReport()
        {
            var report = new AnalyticsReport();
            report.reportPeriod = DateTime.Now - sessionStartTime;
            
            // Copy metrics
            foreach (var metric in metrics)
            {
                report.metrics[metric.Key] = metric.Value;
            }
            
            // Copy sessions and matches
            report.sessions = new List<PlayerSession>(playerSessions);
            report.matches = new List<MatchAnalysis>(matchAnalyses);
            
            // Generate summary
            report.summary["TotalMetrics"] = metrics.Count;
            report.summary["TotalSessions"] = playerSessions.Count;
            report.summary["TotalMatches"] = matchAnalyses.Count;
            report.summary["ReportPeriod"] = report.reportPeriod.TotalMinutes;
            report.summary["PerformanceScore"] = CalculatePerformanceScore();
            report.summary["BalanceScore"] = CalculateBalanceScore();
            report.summary["EngagementScore"] = CalculateEngagementScore();
            
            return report;
        }
        
        /// <summary>
        /// Save report to file
        /// </summary>
        private void SaveReportToFile(AnalyticsReport report)
        {
            try
            {
                string fileName = $"analytics_report_{report.generatedAt:yyyyMMdd_HHmmss}.json";
                string fullPath = Path.Combine(Application.persistentDataPath, dataOutputPath, fileName);
                
                string json = JsonUtility.ToJson(report, true);
                File.WriteAllText(fullPath, json);
                
                Debug.Log($"Analytics report saved to: {fullPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save analytics report: {e.Message}");
            }
        }
        
        // Event handlers
        private void OnMatchCompleted(PlaytestingManager.MatchData matchData)
        {
            var analysis = new MatchAnalysis
            {
                matchId = matchData.matchId,
                timestamp = DateTime.Now,
                duration = matchData.duration,
                winner = matchData.winner,
                loser = matchData.loser,
                totalEvents = matchData.events.Count,
                averageEventRate = matchData.events.Count / matchData.duration,
                competitiveness = CalculateCompetitiveness(matchData)
            };
            
            matchAnalyses.Add(analysis);
            
            // Keep reasonable history
            if (matchAnalyses.Count > 200)
            {
                matchAnalyses.RemoveAt(0);
            }
            
            UpdateMetric("MatchDuration", "Gameplay", matchData.duration);
            UpdateMetric("EventRate", "Gameplay", analysis.averageEventRate);
        }
        
        private void OnSessionStarted(PlaytestingManager.PlaytestSession session)
        {
            Debug.Log($"Analytics tracking session: {session.sessionName}");
        }
        
        private void OnSessionEnded(PlaytestingManager.PlaytestSession session)
        {
            Debug.Log($"Analytics session ended: {session.sessionName}");
        }
        
        private void OnTestCompleted(AutomatedTestFramework.TestResult testResult)
        {
            UpdateMetric("TestSuccessRate", "Testing", testResult.passed ? 1f : 0f);
            UpdateMetric("TestDuration", "Testing", testResult.duration);
        }
        
        private void OnTestReportGenerated(AutomatedTestFramework.TestReport testReport)
        {
            UpdateMetric("OverallTestSuccessRate", "Testing", testReport.successRate / 100f);
        }
        
        /// <summary>
        /// Calculate how competitive a match was
        /// </summary>
        private float CalculateCompetitiveness(PlaytestingManager.MatchData matchData)
        {
            // Simple competitiveness calculation based on match duration and events
            float expectedDuration = averageMatchDurationTarget;
            float durationScore = 1f - Mathf.Abs(matchData.duration - expectedDuration) / expectedDuration;
            
            float eventRate = matchData.events.Count / matchData.duration;
            float eventScore = Mathf.Clamp01(eventRate / 10f); // Normalize to expected event rate
            
            return (durationScore + eventScore) / 2f;
        }
        
        /// <summary>
        /// Get metric by name
        /// </summary>
        public MetricData GetMetric(string name)
        {
            return metrics.ContainsKey(name) ? metrics[name] : null;
        }
        
        /// <summary>
        /// Get all metrics in a category
        /// </summary>
        public Dictionary<string, MetricData> GetMetricsByCategory(string category)
        {
            return metrics.Where(kvp => kvp.Value.category == category).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        /// <summary>
        /// Export data to CSV
        /// </summary>
        public void ExportToCSV(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, dataOutputPath, fileName);
                
                using (var writer = new StreamWriter(fullPath))
                {
                    // Write header
                    writer.WriteLine("Metric,Category,Value,Timestamp");
                    
                    // Write data
                    foreach (var metric in metrics.Values)
                    {
                        for (int i = 0; i < metric.values.Count; i++)
                        {
                            writer.WriteLine($"{metric.name},{metric.category},{metric.values[i]},{metric.lastUpdated:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
                }
                
                Debug.Log($"Data exported to: {fullPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to export data: {e.Message}");
            }
        }
        
        private void OnDestroy()
        {
            StopAnalytics();
            
            // Unsubscribe from events
            if (playtestingManager != null)
            {
                PlaytestingManager.OnMatchCompleted -= OnMatchCompleted;
                PlaytestingManager.OnSessionStarted -= OnSessionStarted;
                PlaytestingManager.OnSessionEnded -= OnSessionEnded;
            }
            
            if (testFramework != null)
            {
                AutomatedTestFramework.OnTestCompleted -= OnTestCompleted;
                AutomatedTestFramework.OnTestReportGenerated -= OnTestReportGenerated;
            }
        }
    }
}
