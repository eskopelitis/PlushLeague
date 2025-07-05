using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PlushLeague.Gameplay.Match
{
    /// <summary>
    /// Player statistics for match tracking and MVP determination
    /// </summary>
    [System.Serializable]
    public class PlayerMatchStats
    {
        [Header("Player Info")]
        public string playerName;
        public int playerId;
        public int teamId;
        
        [Header("Goals & Assists")]
        public int goals = 0;
        public int assists = 0;
        public int shotsOnTarget = 0;
        public int totalShots = 0;
        
        [Header("Defensive Stats")]
        public int saves = 0;
        public int tackles = 0;
        public int interceptions = 0;
        public int clearances = 0;
        
        [Header("Movement & Possession")]
        public float totalDistanceCovered = 0f;
        public float ballPossessionTime = 0f; // seconds
        public int passes = 0;
        public int passesCompleted = 0;
        
        [Header("Superpowers")]
        public int superpowersUsed = 0;
        public int superpowerGoals = 0; // Goals scored using superpowers
        public int superpowerSaves = 0; // Saves made using superpowers
        
        [Header("Special Actions")]
        public int chipKicks = 0;
        public int slideTackles = 0;
        public int ballSteals = 0;
        
        /// <summary>
        /// Calculate overall MVP score based on weighted statistics
        /// </summary>
        public float CalculateMVPScore()
        {
            float score = 0f;
            
            // Goals are highly weighted
            score += goals * 10f;
            
            // Assists are valuable
            score += assists * 6f;
            
            // Shot accuracy bonus
            if (totalShots > 0)
            {
                float accuracy = (float)shotsOnTarget / totalShots;
                score += accuracy * 5f;
            }
            
            // Defensive contributions
            score += saves * 8f; // Saves are very valuable
            score += tackles * 3f;
            score += interceptions * 4f;
            score += clearances * 2f;
            
            // Possession and passing
            score += ballPossessionTime * 0.1f; // 0.1 point per second
            if (passes > 0)
            {
                float passAccuracy = (float)passesCompleted / passes;
                score += passAccuracy * 3f;
            }
            
            // Superpower usage bonus
            score += superpowerGoals * 5f; // Extra points for superpower goals
            score += superpowerSaves * 6f; // Extra points for superpower saves
            score += superpowersUsed * 1f; // Small bonus for using abilities
            
            // Distance coverage (teamwork indicator)
            score += totalDistanceCovered * 0.01f; // 0.01 point per unit
            
            // Special action bonuses
            score += chipKicks * 2f;
            score += ballSteals * 3f;
            score += slideTackles * 2f;
            
            return score;
        }
        
        /// <summary>
        /// Get formatted stats string for display
        /// </summary>
        public string GetStatsDisplay()
        {
            return $"Goals: {goals} | Assists: {assists} | Saves: {saves} | Tackles: {tackles}";
        }
    }
    
    /// <summary>
    /// Match statistics tracking and MVP calculation system
    /// Tracks all player actions and calculates performance metrics
    /// </summary>
    public class MatchStatistics : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [SerializeField] private bool enableStatTracking = true;
        [SerializeField] private bool enableMVPCalculation = true;
        [SerializeField] private float positionUpdateInterval = 0.1f; // For distance tracking
        
        [Header("Debug")]
        [SerializeField] private bool showDebugStats = false;
        
        // Player statistics storage
        private Dictionary<int, PlayerMatchStats> playerStats = new Dictionary<int, PlayerMatchStats>();
        private Dictionary<int, Vector3> lastPlayerPositions = new Dictionary<int, Vector3>();
        private Dictionary<int, float> ballPossessionTimers = new Dictionary<int, float>();
        
        // Current ball carrier tracking
        private int currentBallCarrierId = -1;
        private float possessionStartTime = 0f;
        
        // Match state
        private bool isMatchActive = false;
        private float positionTrackingTimer = 0f;
        
        // References
        private MatchManager matchManager;
        private PlushLeague.Gameplay.Ball.BallManager ballManager;
        
        // Events
        public System.Action<PlayerMatchStats> OnPlayerStatUpdated;
        public System.Action<PlayerMatchStats> OnMVPCalculated;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            FindDependencies();
        }
        
        private void Start()
        {
            InitializeStatTracking();
            SubscribeToEvents();
        }
        
        private void Update()
        {
            if (!enableStatTracking || !isMatchActive) return;
            
            UpdatePositionTracking();
            UpdatePossessionTracking();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Find required dependencies
        /// </summary>
        private void FindDependencies()
        {
            if (matchManager == null)
                matchManager = FindFirstObjectByType<MatchManager>();
                
            if (ballManager == null)
                ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
        }
        
        /// <summary>
        /// Initialize statistics tracking for all players
        /// </summary>
        private void InitializeStatTracking()
        {
            if (!enableStatTracking) return;
            
            // Find all players and initialize their stats
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            
            foreach (var player in players)
            {
                int playerId = player.GetInstanceID();
                
                var stats = new PlayerMatchStats
                {
                    playerName = player.name,
                    playerId = playerId,
                    teamId = DeterminePlayerTeam(player) // You may need to implement this
                };
                
                playerStats[playerId] = stats;
                lastPlayerPositions[playerId] = player.transform.position;
                ballPossessionTimers[playerId] = 0f;
                
                UnityEngine.Debug.Log($"Initialized stats tracking for {player.name} (ID: {playerId})");
            }
        }
        
        /// <summary>
        /// Determine which team a player belongs to
        /// </summary>
        private int DeterminePlayerTeam(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Simple implementation - could be enhanced with team assignment system
            // For now, use player position or naming convention
            if (player.name.ToLower().Contains("team1") || player.name.ToLower().Contains("teama"))
                return 1;
            else if (player.name.ToLower().Contains("team2") || player.name.ToLower().Contains("teamb"))
                return 2;
                
            // Fallback: use position-based assignment
            return player.transform.position.x < 0 ? 1 : 2;
        }
        
        #endregion
        
        #region Event Subscription
        
        /// <summary>
        /// Subscribe to relevant game events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (matchManager != null)
            {
                matchManager.OnGoalScored += OnGoalScored;
                matchManager.OnMatchEnded += OnMatchEnded;
            }
            
            if (ballManager != null)
            {
                ballManager.OnBallSpawned += OnBallStateChanged;
            }
            
            // Subscribe to player events
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                // You might need to add these events to PlayerController
                // player.OnShotTaken += OnPlayerShotTaken;
                // player.OnTackleMade += OnPlayerTackleMade;
                // etc.
            }
        }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (matchManager != null)
            {
                matchManager.OnGoalScored -= OnGoalScored;
                matchManager.OnMatchEnded -= OnMatchEnded;
            }
            
            if (ballManager != null)
            {
                ballManager.OnBallSpawned -= OnBallStateChanged;
            }
        }
        
        #endregion
        
        #region Position & Distance Tracking
        
        /// <summary>
        /// Update player position tracking for distance calculations
        /// </summary>
        private void UpdatePositionTracking()
        {
            positionTrackingTimer += Time.deltaTime;
            
            if (positionTrackingTimer >= positionUpdateInterval)
            {
                positionTrackingTimer = 0f;
                
                foreach (var kvp in playerStats.ToList())
                {
                    int playerId = kvp.Key;
                    var stats = kvp.Value;
                    
                    // Find the player GameObject
                    var player = FindPlayerById(playerId);
                    if (player == null) continue;
                    
                    Vector3 currentPos = player.transform.position;
                    
                    if (lastPlayerPositions.ContainsKey(playerId))
                    {
                        Vector3 lastPos = lastPlayerPositions[playerId];
                        float distance = Vector3.Distance(currentPos, lastPos);
                        stats.totalDistanceCovered += distance;
                    }
                    
                    lastPlayerPositions[playerId] = currentPos;
                }
            }
        }
        
        #endregion
        
        #region Possession Tracking
        
        /// <summary>
        /// Update ball possession tracking
        /// </summary>
        private void UpdatePossessionTracking()
        {
            if (ballManager == null) return;
            
            var ballCarrier = ballManager.GetBallCarrier();
            int carrierId = ballCarrier?.GetInstanceID() ?? -1;
            
            // Check if possession changed
            if (carrierId != currentBallCarrierId)
            {
                // End previous possession
                if (currentBallCarrierId != -1 && playerStats.ContainsKey(currentBallCarrierId))
                {
                    float possessionDuration = Time.time - possessionStartTime;
                    playerStats[currentBallCarrierId].ballPossessionTime += possessionDuration;
                }
                
                // Start new possession
                currentBallCarrierId = carrierId;
                possessionStartTime = Time.time;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle goal scored event
        /// </summary>
        private void OnGoalScored(int teamId)
        {
            // Award goal to ball carrier if available
            if (currentBallCarrierId != -1 && playerStats.ContainsKey(currentBallCarrierId))
            {
                playerStats[currentBallCarrierId].goals++;
                
                // Check if it was a superpower goal (you may need additional tracking)
                // playerStats[currentBallCarrierId].superpowerGoals++;
                
                OnPlayerStatUpdated?.Invoke(playerStats[currentBallCarrierId]);
                
                UnityEngine.Debug.Log($"Goal recorded for player {playerStats[currentBallCarrierId].playerName}");
            }
        }
        
        /// <summary>
        /// Handle match end event
        /// </summary>
        private void OnMatchEnded(int winningTeam)
        {
            isMatchActive = false;
            
            // Finalize possession tracking
            if (currentBallCarrierId != -1 && playerStats.ContainsKey(currentBallCarrierId))
            {
                float finalPossessionDuration = Time.time - possessionStartTime;
                playerStats[currentBallCarrierId].ballPossessionTime += finalPossessionDuration;
            }
            
            if (enableMVPCalculation)
            {
                CalculateAndAwardMVP();
            }
            
            DisplayFinalStats();
        }
        
        /// <summary>
        /// Handle ball state changes
        /// </summary>
        private void OnBallStateChanged(PlushLeague.Gameplay.Ball.BallController ball)
        {
            // Reset possession tracking when ball is spawned/reset
            currentBallCarrierId = -1;
            possessionStartTime = Time.time;
        }
        
        #endregion
        
        #region Public Stat Recording Methods
        
        /// <summary>
        /// Record a shot taken by a player
        /// </summary>
        public void RecordShot(PlushLeague.Gameplay.Player.PlayerController player, bool onTarget)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].totalShots++;
            if (onTarget)
            {
                playerStats[playerId].shotsOnTarget++;
            }
            
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        /// <summary>
        /// Record a save made by a goalkeeper
        /// </summary>
        public void RecordSave(PlushLeague.Gameplay.Player.PlayerController player, bool wasSuperpowerSave = false)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].saves++;
            if (wasSuperpowerSave)
            {
                playerStats[playerId].superpowerSaves++;
            }
            
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        /// <summary>
        /// Record a tackle made by a player
        /// </summary>
        public void RecordTackle(PlushLeague.Gameplay.Player.PlayerController player)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].tackles++;
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        /// <summary>
        /// Record superpower usage
        /// </summary>
        public void RecordSuperpowerUsage(PlushLeague.Gameplay.Player.PlayerController player)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].superpowersUsed++;
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        /// <summary>
        /// Record a pass attempt and completion
        /// </summary>
        public void RecordPass(PlushLeague.Gameplay.Player.PlayerController player, bool completed)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].passes++;
            if (completed)
            {
                playerStats[playerId].passesCompleted++;
            }
            
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        /// <summary>
        /// Record a ball steal
        /// </summary>
        public void RecordBallSteal(PlushLeague.Gameplay.Player.PlayerController player)
        {
            int playerId = player.GetInstanceID();
            if (!playerStats.ContainsKey(playerId)) return;
            
            playerStats[playerId].ballSteals++;
            OnPlayerStatUpdated?.Invoke(playerStats[playerId]);
        }
        
        #endregion
        
        #region MVP Calculation
        
        /// <summary>
        /// Calculate and award MVP
        /// </summary>
        private void CalculateAndAwardMVP()
        {
            if (playerStats.Count == 0) return;
            
            var mvpCandidate = playerStats.Values.OrderByDescending(stats => stats.CalculateMVPScore()).First();
            
            UnityEngine.Debug.Log($"MVP: {mvpCandidate.playerName} with score {mvpCandidate.CalculateMVPScore():F1}");
            OnMVPCalculated?.Invoke(mvpCandidate);
        }
        
        /// <summary>
        /// Get MVP candidate based on current stats
        /// </summary>
        public PlayerMatchStats GetCurrentMVP()
        {
            if (playerStats.Count == 0) return null;
            
            return playerStats.Values.OrderByDescending(stats => stats.CalculateMVPScore()).FirstOrDefault();
        }
        
        /// <summary>
        /// Get top performers by category
        /// </summary>
        public PlayerMatchStats GetTopScorer()
        {
            if (playerStats.Count == 0) return null;
            return playerStats.Values.OrderByDescending(stats => stats.goals).FirstOrDefault();
        }
        
        public PlayerMatchStats GetTopSaver()
        {
            if (playerStats.Count == 0) return null;
            return playerStats.Values.OrderByDescending(stats => stats.saves).FirstOrDefault();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Find player by ID
        /// </summary>
        private PlushLeague.Gameplay.Player.PlayerController FindPlayerById(int playerId)
        {
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            return players.FirstOrDefault(p => p.GetInstanceID() == playerId);
        }
        
        /// <summary>
        /// Start match statistics tracking
        /// </summary>
        public void StartMatch()
        {
            isMatchActive = true;
            UnityEngine.Debug.Log("Match statistics tracking started");
        }
        
        /// <summary>
        /// Stop match statistics tracking
        /// </summary>
        public void StopMatch()
        {
            isMatchActive = false;
            UnityEngine.Debug.Log("Match statistics tracking stopped");
        }
        
        /// <summary>
        /// Get stats for a specific player
        /// </summary>
        public PlayerMatchStats GetPlayerStats(PlushLeague.Gameplay.Player.PlayerController player)
        {
            int playerId = player.GetInstanceID();
            return playerStats.ContainsKey(playerId) ? playerStats[playerId] : null;
        }
        
        /// <summary>
        /// Get all player statistics
        /// </summary>
        public Dictionary<int, PlayerMatchStats> GetAllPlayerStats()
        {
            return new Dictionary<int, PlayerMatchStats>(playerStats);
        }
        
        /// <summary>
        /// Display final match statistics
        /// </summary>
        private void DisplayFinalStats()
        {
            if (!showDebugStats) return;
            
            UnityEngine.Debug.Log("=== FINAL MATCH STATISTICS ===");
            foreach (var stats in playerStats.Values.OrderByDescending(s => s.CalculateMVPScore()))
            {
                UnityEngine.Debug.Log($"{stats.playerName}: {stats.GetStatsDisplay()} | MVP Score: {stats.CalculateMVPScore():F1}");
            }
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!showDebugStats || !isMatchActive) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== LIVE MATCH STATS ===");
            
            var topStats = playerStats.Values.OrderByDescending(s => s.CalculateMVPScore()).Take(3);
            foreach (var stats in topStats)
            {
                GUILayout.Label($"{stats.playerName}: G{stats.goals} A{stats.assists} S{stats.saves}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
