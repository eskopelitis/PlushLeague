using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Gameplay.Match
{
    /// <summary>
    /// Orchestrates the entire lifecycle of a match: kickoff, goal events, timer, score tracking, golden goal overtime, and match conclusion.
    /// Handles all resets (after goals), match-end conditions, and transitions to post-match summary.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        [Header("Match Settings")]
        [SerializeField] private float matchTime = 90.0f; // seconds
        [SerializeField] private int maxGoals = 3;
        [SerializeField] private float goalResetDelay = 2.0f; // pause after a goal for celebration
        [SerializeField] private float countdownTime = 3.0f; // 3-2-1 countdown
        
        [Header("Audio/Visual")]
        [SerializeField] private AudioClip countdownSFX;
        [SerializeField] private AudioClip whistleSFX;
        [SerializeField] private AudioClip goalScoredSFX;
        [SerializeField] private AudioClip cheerSFX;
        [SerializeField] private AudioClip warningBeepSFX;
        [SerializeField] private AudioClip goldenGoalSFX;
        [SerializeField] private AudioClip victoryFanfareSFX;
        [SerializeField] private AudioClip defeatSFX;
        
        [Header("Effects")]
        [SerializeField] private GameObject goalVFXPrefab;
        [SerializeField] private GameObject goldenGoalVFXPrefab;
        [SerializeField] private GameObject victoryVFXPrefab;
        [SerializeField] private float screenShakeIntensity = 1.0f;
        [SerializeField] private float screenShakeDuration = 0.5f;
        
        [Header("References")]
        [SerializeField] private PlushLeague.UI.HUD.GameHUD gameHUD;
        [SerializeField] private PlushLeague.Gameplay.Ball.BallManager ballManager;
        [SerializeField] private Transform[] teamASpawnPoints;
        [SerializeField] private Transform[] teamBSpawnPoints;
        [SerializeField] private Transform ballSpawnPoint;
        
        // Runtime state
        private float currentTime;
        private int teamAScore = 0;
        private int teamBScore = 0;
        private bool isGoldenGoalActive = false;
        private bool isMatchActive = false;
        private bool isMatchPaused = false;
        private bool isMatchEnded = false;
        
        // Player tracking
        private List<PlushLeague.Gameplay.Player.PlayerController> teamAPlayers = new List<PlushLeague.Gameplay.Player.PlayerController>();
        private List<PlushLeague.Gameplay.Player.PlayerController> teamBPlayers = new List<PlushLeague.Gameplay.Player.PlayerController>();
        private List<PlushLeague.Gameplay.Player.PlayerController> allPlayers = new List<PlushLeague.Gameplay.Player.PlayerController>();
        
        // Audio
        private AudioSource audioSource;
        
        // Events
        public System.Action<int> OnGoalScored; // teamId
        public System.Action<int, int> OnScoreUpdated; // teamAScore, teamBScore
        public System.Action OnGoldenGoalStarted;
        public System.Action<int> OnMatchEnded; // winningTeamId (0 = draw, 1 = teamA, 2 = teamB)
        public System.Action<float> OnTimerUpdated; // currentTime
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Find HUD if not assigned
            if (gameHUD == null)
            {
                gameHUD = FindFirstObjectByType<PlushLeague.UI.HUD.GameHUD>();
            }
            
            // Find ball manager if not assigned
            if (ballManager == null)
            {
                ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            }
        }
        
        private void Start()
        {
            // Initialize match
            InitializeMatch();
        }
        
        private void Update()
        {
            if (!isMatchActive || isMatchPaused || isMatchEnded) return;
            
            HandleTimer();
        }
        
        #endregion
        
        #region Match Initialization
        
        /// <summary>
        /// Initialize the match and find all players
        /// </summary>
        private void InitializeMatch()
        {
            currentTime = matchTime;
            teamAScore = 0;
            teamBScore = 0;
            isGoldenGoalActive = false;
            isMatchActive = false;
            isMatchPaused = false;
            isMatchEnded = false;
            
            // Find all players in scene
            FindAllPlayers();
            
            // Subscribe to goal events
            if (ballManager != null)
            {
                // Subscribe to goal events from ball manager or goal triggers
                SubscribeToGoalEvents();
            }
            
            // Update HUD
            UpdateScoreDisplay();
            UpdateTimerDisplay();
            
            // Start match after a brief delay
            StartCoroutine(StartMatchSequence());
        }
        
        /// <summary>
        /// Find and categorize all players in the scene
        /// </summary>
        private void FindAllPlayers()
        {
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            
            teamAPlayers.Clear();
            teamBPlayers.Clear();
            allPlayers.Clear();
            
            foreach (var player in players)
            {
                allPlayers.Add(player);
                
                // Determine team based on position or tag
                // Simple logic: players closer to teamA spawn points are teamA
                if (IsPlayerOnTeamA(player))
                {
                    teamAPlayers.Add(player);
                }
                else
                {
                    teamBPlayers.Add(player);
                }
            }
            
            UnityEngine.Debug.Log($"Found {teamAPlayers.Count} Team A players, {teamBPlayers.Count} Team B players");
        }
        
        /// <summary>
        /// Determine if a player belongs to Team A based on spawn point proximity
        /// </summary>
        private bool IsPlayerOnTeamA(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (teamASpawnPoints == null || teamASpawnPoints.Length == 0) return true;
            if (teamBSpawnPoints == null || teamBSpawnPoints.Length == 0) return false;
            
            float distanceToTeamA = Vector3.Distance(player.transform.position, teamASpawnPoints[0].position);
            float distanceToTeamB = Vector3.Distance(player.transform.position, teamBSpawnPoints[0].position);
            
            return distanceToTeamA < distanceToTeamB;
        }
        
        /// <summary>
        /// Subscribe to goal scoring events
        /// </summary>
        private void SubscribeToGoalEvents()
        {
            // Find goal triggers and subscribe
            var goalTriggers = FindObjectsByType<PlushLeague.Gameplay.Goal.GoalTrigger>(FindObjectsSortMode.None);
            foreach (var goalTrigger in goalTriggers)
            {
                goalTrigger.OnGoalScored += HandleGoalScored;
            }
        }
        
        #endregion
        
        #region Match Flow
        
        /// <summary>
        /// Start the match with countdown sequence
        /// </summary>
        public void StartMatch()
        {
            StartCoroutine(StartMatchSequence());
        }
        
        /// <summary>
        /// Coroutine for match start sequence with countdown
        /// </summary>
        private System.Collections.IEnumerator StartMatchSequence()
        {
            // Reset positions
            ResetForKickoff();
            
            // Freeze all players and ball
            FreezeAllGameplay(true);
            
            // Show countdown
            yield return StartCoroutine(CountdownSequence());
            
            // Unfreeze and start match
            FreezeAllGameplay(false);
            isMatchActive = true;
            
            // Update HUD
            if (gameHUD != null)
            {
                gameHUD.ShowMatchStarted();
            }
            
            UnityEngine.Debug.Log("Match started!");
        }
        
        /// <summary>
        /// Handle the 3-2-1 countdown sequence
        /// </summary>
        private System.Collections.IEnumerator CountdownSequence()
        {
            float timePerStep = countdownTime / 4f; // Divide by 4 to account for 3-2-1-GO sequence
            
            for (int i = 3; i > 0; i--)
            {
                // Show countdown number
                if (gameHUD != null)
                {
                    gameHUD.ShowCountdown(i);
                }
                
                // Play countdown sound
                if (countdownSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(countdownSFX);
                }
                
                yield return new WaitForSeconds(timePerStep);
            }
            
            // Final whistle
            if (gameHUD != null)
            {
                gameHUD.ShowCountdown(0); // "GO!"
            }
            
            if (whistleSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(whistleSFX);
            }
            
            yield return new WaitForSeconds(timePerStep);
            
            // Hide countdown
            if (gameHUD != null)
            {
                gameHUD.HideCountdown();
            }
        }
        
        /// <summary>
        /// Reset all players and ball to starting positions
        /// </summary>
        private void ResetForKickoff()
        {
            // Reset ball position
            if (ballManager != null && ballSpawnPoint != null)
            {
                ballManager.ResetBallPosition(ballSpawnPoint.position);
            }
            
            // Reset player positions
            ResetPlayerPositions();
            
            // Partial stamina/cooldown refill (optional)
            RefillPlayerResources();
        }
        
        /// <summary>
        /// Reset all players to their spawn positions
        /// </summary>
        private void ResetPlayerPositions()
        {
            // Reset Team A players
            for (int i = 0; i < teamAPlayers.Count && i < teamASpawnPoints.Length; i++)
            {
                teamAPlayers[i].transform.position = teamASpawnPoints[i].position;
                teamAPlayers[i].transform.rotation = teamASpawnPoints[i].rotation;
            }
            
            // Reset Team B players
            for (int i = 0; i < teamBPlayers.Count && i < teamBSpawnPoints.Length; i++)
            {
                teamBPlayers[i].transform.position = teamBSpawnPoints[i].position;
                teamBPlayers[i].transform.rotation = teamBSpawnPoints[i].rotation;
            }
        }
        
        /// <summary>
        /// Partially refill player stamina and ability cooldowns
        /// </summary>
        private void RefillPlayerResources()
        {
            foreach (var player in allPlayers)
            {
                if (player == null) continue;
                
                // Partial stamina refill (could add this method to PlayerController)
                // player.RefillStamina(0.5f); // 50% refill
                
                // Reset ability cooldowns (could add this to PowerupController)
                var powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
                if (powerupController != null)
                {
                    // powerupController.ReduceCooldown(10f); // Reduce by 10 seconds
                }
            }
        }
        
        #endregion
        
        #region Timer Handling
        
        /// <summary>
        /// Handle match timer countdown
        /// </summary>
        private void HandleTimer()
        {
            if (isGoldenGoalActive) return; // No timer in golden goal
            
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(0, currentTime);
            
            UpdateTimerDisplay();
            OnTimerUpdated?.Invoke(currentTime);
            
            // Warning beep in final 10 seconds
            if (currentTime <= 10f && currentTime > 9f)
            {
                PlayWarningBeep();
            }
            
            // Check if time expired
            if (currentTime <= 0)
            {
                HandleTimeExpired();
            }
        }
        
        /// <summary>
        /// Handle when match time expires
        /// </summary>
        private void HandleTimeExpired()
        {
            isMatchActive = false;
            
            // Check scores
            if (teamAScore > teamBScore)
            {
                EndMatch(1); // Team A wins
            }
            else if (teamBScore > teamAScore)
            {
                EndMatch(2); // Team B wins
            }
            else
            {
                StartGoldenGoal(); // Tied, go to golden goal
            }
        }
        
        /// <summary>
        /// Play warning beep sound for final seconds
        /// </summary>
        private void PlayWarningBeep()
        {
            if (warningBeepSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(warningBeepSFX);
            }
            
            // Flash timer in HUD
            if (gameHUD != null)
            {
                gameHUD.FlashTimer();
            }
        }
        
        /// <summary>
        /// Update timer display in HUD
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (gameHUD != null)
            {
                if (isGoldenGoalActive)
                {
                    gameHUD.SetTimerText("GOLDEN GOAL");
                }
                else
                {
                    int minutes = Mathf.FloorToInt(currentTime / 60);
                    int seconds = Mathf.FloorToInt(currentTime % 60);
                    gameHUD.SetTimerText($"{minutes:00}:{seconds:00}");
                }
            }
        }
        
        #endregion
        
        #region Goal Scoring
        
        /// <summary>
        /// Handle when a goal is scored
        /// </summary>
        public void GoalScored(int teamId)
        {
            if (isMatchEnded) return;
            
            HandleGoalScored(teamId);
        }
        
        /// <summary>
        /// Internal goal scoring handler
        /// </summary>
        private void HandleGoalScored(int teamId)
        {
            UnityEngine.Debug.Log($"Goal scored by Team {teamId}!");
            
            // Update score
            if (teamId == 1)
            {
                teamAScore++;
            }
            else if (teamId == 2)
            {
                teamBScore++;
            }
            
            // Freeze gameplay
            FreezeAllGameplay(true);
            isMatchPaused = true;
            
            // Play goal effects
            PlayGoalEffects();
            
            // Update displays
            UpdateScoreDisplay();
            OnGoalScored?.Invoke(teamId);
            OnScoreUpdated?.Invoke(teamAScore, teamBScore);
            
            // Check win condition
            if (CheckWinCondition())
            {
                StartCoroutine(GoalScoredToMatchEnd(teamId));
            }
            else
            {
                StartCoroutine(GoalScoredSequence());
            }
        }
        
        /// <summary>
        /// Check if match should end due to score
        /// </summary>
        private bool CheckWinCondition()
        {
            // Golden goal - instant win
            if (isGoldenGoalActive)
            {
                return true;
            }
            
            // Regular match - check max goals
            return teamAScore >= maxGoals || teamBScore >= maxGoals;
        }
        
        /// <summary>
        /// Coroutine for goal scored sequence (when match continues)
        /// </summary>
        private System.Collections.IEnumerator GoalScoredSequence()
        {
            // Wait for celebration
            yield return new WaitForSeconds(goalResetDelay);
            
            // Reset for kickoff
            ResetForKickoff();
            
            // Mini countdown
            yield return StartCoroutine(MiniCountdownSequence());
            
            // Resume match
            FreezeAllGameplay(false);
            isMatchPaused = false;
            
            // Resume timer if not in golden goal
            if (!isGoldenGoalActive)
            {
                isMatchActive = true;
            }
        }
        
        /// <summary>
        /// Coroutine for goal scored to match end
        /// </summary>
        private System.Collections.IEnumerator GoalScoredToMatchEnd(int scoringTeamId)
        {
            // Wait for celebration
            yield return new WaitForSeconds(goalResetDelay);
            
            // End match
            int winningTeam = 0;
            if (teamAScore > teamBScore) winningTeam = 1;
            else if (teamBScore > teamAScore) winningTeam = 2;
            
            EndMatch(winningTeam);
        }
        
        /// <summary>
        /// Quick mini-countdown for resuming play
        /// </summary>
        private System.Collections.IEnumerator MiniCountdownSequence()
        {
            if (gameHUD != null)
            {
                gameHUD.ShowMiniCountdown("Ready?");
            }
            
            yield return new WaitForSeconds(1.0f);
            
            if (gameHUD != null)
            {
                gameHUD.ShowMiniCountdown("Go!");
            }
            
            if (whistleSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(whistleSFX);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (gameHUD != null)
            {
                gameHUD.HideMiniCountdown();
            }
        }
        
        /// <summary>
        /// Play visual and audio effects for goal
        /// </summary>
        private void PlayGoalEffects()
        {
            // Play goal SFX
            if (goalScoredSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(goalScoredSFX);
            }
            
            if (cheerSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(cheerSFX);
            }
            
            // Create goal VFX
            if (goalVFXPrefab != null && ballManager != null)
            {
                GameObject goalVFX = Instantiate(goalVFXPrefab, ballManager.GetBallPosition(), Quaternion.identity);
                Destroy(goalVFX, 3f);
            }
            
            // Screen shake
            StartCoroutine(ScreenShakeCoroutine());
            
            // Show goal celebration in HUD
            if (gameHUD != null)
            {
                gameHUD.ShowGoalCelebration();
            }
        }
        
        /// <summary>
        /// Screen shake coroutine
        /// </summary>
        private System.Collections.IEnumerator ScreenShakeCoroutine()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) yield break;
            
            Vector3 originalPosition = mainCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < screenShakeDuration)
            {
                float strength = screenShakeIntensity * (1f - elapsed / screenShakeDuration);
                
                Vector3 randomOffset = Random.insideUnitSphere * strength;
                randomOffset.z = 0; // Keep camera on same Z plane
                
                mainCamera.transform.localPosition = originalPosition + randomOffset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            mainCamera.transform.localPosition = originalPosition;
        }
        
        #endregion
        
        #region Golden Goal
        
        /// <summary>
        /// Start golden goal (sudden death) mode
        /// </summary>
        private void StartGoldenGoal()
        {
            isGoldenGoalActive = true;
            isMatchActive = true; // Resume gameplay in golden goal mode
            
            UnityEngine.Debug.Log("Golden Goal started!");
            
            // Play golden goal effects
            if (goldenGoalSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(goldenGoalSFX);
            }
            
            if (goldenGoalVFXPrefab != null)
            {
                GameObject goldenVFX = Instantiate(goldenGoalVFXPrefab, Vector3.zero, Quaternion.identity);
                Destroy(goldenVFX, 5f);
            }
            
            // Update HUD
            if (gameHUD != null)
            {
                gameHUD.ShowGoldenGoal();
            }
            
            UpdateTimerDisplay();
            OnGoldenGoalStarted?.Invoke();
        }
        
        #endregion
        
        #region Match End
        
        /// <summary>
        /// End the match
        /// </summary>
        /// <param name="winningTeamId">0 = draw, 1 = team A, 2 = team B</param>
        public void EndMatch(int winningTeamId)
        {
            if (isMatchEnded) return;
            
            isMatchEnded = true;
            isMatchActive = false;
            isMatchPaused = false;
            
            UnityEngine.Debug.Log($"Match ended! Winner: Team {winningTeamId} (0=draw)");
            
            // Freeze all gameplay
            FreezeAllGameplay(true);
            
            // Play match end effects
            PlayMatchEndEffects(winningTeamId);
            
            // Show final results
            if (gameHUD != null)
            {
                gameHUD.ShowMatchResults(teamAScore, teamBScore, winningTeamId);
            }
            
            OnMatchEnded?.Invoke(winningTeamId);
            
            // Start post-match sequence
            StartCoroutine(PostMatchSequence(winningTeamId));
        }
        
        /// <summary>
        /// Play visual and audio effects for match end
        /// </summary>
        private void PlayMatchEndEffects(int winningTeamId)
        {
            if (winningTeamId > 0)
            {
                // Victory
                if (victoryFanfareSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(victoryFanfareSFX);
                }
                
                if (victoryVFXPrefab != null)
                {
                    GameObject victoryVFX = Instantiate(victoryVFXPrefab, Vector3.zero, Quaternion.identity);
                    Destroy(victoryVFX, 10f);
                }
            }
            else
            {
                // Draw
                if (defeatSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(defeatSFX);
                }
            }
        }
        
        /// <summary>
        /// Post-match sequence coroutine
        /// </summary>
        private System.Collections.IEnumerator PostMatchSequence(int winningTeamId)
        {
            // Wait for celebration/results viewing
            yield return new WaitForSeconds(5f);
            
            // Show options (Play Again, Return to Menu, etc.)
            if (gameHUD != null)
            {
                gameHUD.ShowPostMatchOptions();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Freeze or unfreeze all gameplay elements
        /// </summary>
        private void FreezeAllGameplay(bool freeze)
        {
            // Freeze/unfreeze all players
            foreach (var player in allPlayers)
            {
                if (player != null)
                {
                    player.SetInputEnabled(!freeze);
                }
            }
            
            // Freeze/unfreeze ball
            if (ballManager != null)
            {
                ballManager.SetBallFrozen(freeze);
            }
        }
        
        /// <summary>
        /// Update score display in HUD
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (gameHUD != null)
            {
                gameHUD.UpdateScore(teamAScore, teamBScore);
            }
        }
        
        #endregion
        
        #region Public Getters
        
        public float GetCurrentTime() => currentTime;
        public int GetTeamAScore() => teamAScore;
        public int GetTeamBScore() => teamBScore;
        public bool IsGoldenGoalActive() => isGoldenGoalActive;
        public bool IsMatchActive() => isMatchActive;
        public bool IsMatchEnded() => isMatchEnded;
        
        #endregion
        
        #region Edge Cases
        
        /// <summary>
        /// Handle player disconnect
        /// </summary>
        public void HandlePlayerDisconnected(PlushLeague.Gameplay.Player.PlayerController player)
        {
            if (isMatchEnded) return;
            
            // Pause match briefly
            isMatchPaused = true;
            
            // Remove from player lists
            teamAPlayers.Remove(player);
            teamBPlayers.Remove(player);
            allPlayers.Remove(player);
            
            UnityEngine.Debug.Log($"Player disconnected. Continuing match.");
            
            // Resume after brief pause
            StartCoroutine(ResumeAfterDisconnect());
        }
        
        /// <summary>
        /// Resume match after player disconnect
        /// </summary>
        private System.Collections.IEnumerator ResumeAfterDisconnect()
        {
            yield return new WaitForSeconds(2f);
            isMatchPaused = false;
        }
        
        #endregion
    }
}
