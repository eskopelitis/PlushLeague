using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// Main game HUD that displays score, timer, match status, and countdown overlays
    /// Handles all UI feedback for match flow including goals, golden goal, and match end
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI teamAScoreText;
        [SerializeField] private TextMeshProUGUI teamBScoreText;
        [SerializeField] private TextMeshProUGUI scoreDisplay;
        [SerializeField] private GameObject scorePanel;
        
        [Header("Timer Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private Image timerBackground;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color warningTimerColor = Color.red;
        [SerializeField] private Color goldenGoalColor = Color.yellow;
        
        [Header("Countdown")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject miniCountdownPanel;
        [SerializeField] private TextMeshProUGUI miniCountdownText;
        [SerializeField] private float countdownFadeTime = 0.5f;
        
        [Header("Match Status")]
        [SerializeField] private GameObject goalCelebrationPanel;
        [SerializeField] private TextMeshProUGUI goalMessageText;
        [SerializeField] private GameObject goldenGoalPanel;
        [SerializeField] private TextMeshProUGUI goldenGoalText;
        [SerializeField] private ParticleSystem goldenGoalParticles;
        
        [Header("Match Results")]
        [SerializeField] private GameObject matchResultsPanel;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private GameObject postMatchPanel;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button returnToMenuButton;
        
        [Header("Animations")]
        [SerializeField] private Animator hudAnimator;
        [SerializeField] private float celebrationDuration = 3f;
        [SerializeField] private float resultDisplayDuration = 5f;
        
        [Header("Player HUDs")]
        [SerializeField] private GameObject[] playerHUDs; // Will be PlayerHUD when that class is ready
        [SerializeField] private Transform playerHUDContainer;
        
        [Header("Connection Status")]
        [SerializeField] private GameObject reconnectingPanel;
        [SerializeField] private TextMeshProUGUI reconnectingText;
        [SerializeField] private Image reconnectingSpinner;
        [SerializeField] private float spinnerRotationSpeed = 360f;
        
        [Header("Match Banners")]
        [SerializeField] private GameObject goalBannerPanel;
        [SerializeField] private TextMeshProUGUI goalBannerText;
        [SerializeField] private GameObject victoryBannerPanel;
        [SerializeField] private TextMeshProUGUI victoryBannerText;
        [SerializeField] private GameObject defeatBannerPanel;
        [SerializeField] private TextMeshProUGUI defeatBannerText;
        
        // State tracking
        private bool isTimerFlashing = false;
        private Coroutine timerFlashCoroutine;
        private Coroutine celebrationCoroutine;
        private bool isReconnecting = false;
        private Coroutine spinnerCoroutine;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            InitializeHUD();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize HUD components and find missing references
        /// </summary>
        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            if (countdownPanel == null)
                countdownPanel = transform.Find("CountdownPanel")?.gameObject;
            
            if (countdownText == null && countdownPanel != null)
                countdownText = countdownPanel.GetComponentInChildren<TextMeshProUGUI>();
            
            if (scorePanel == null)
                scorePanel = transform.Find("ScorePanel")?.gameObject;
                
            if (timerPanel == null)
                timerPanel = transform.Find("TimerPanel")?.gameObject;
                
            if (timerText == null && timerPanel != null)
                timerText = timerPanel.GetComponentInChildren<TextMeshProUGUI>();
                
            if (hudAnimator == null)
                hudAnimator = GetComponent<Animator>();
        }
        
        /// <summary>
        /// Initialize HUD to default state
        /// </summary>
        private void InitializeHUD()
        {
            // Hide all overlays initially
            SetPanelActive(countdownPanel, false);
            SetPanelActive(miniCountdownPanel, false);
            SetPanelActive(goalCelebrationPanel, false);
            SetPanelActive(goldenGoalPanel, false);
            SetPanelActive(matchResultsPanel, false);
            SetPanelActive(postMatchPanel, false);
            SetPanelActive(reconnectingPanel, false);
            SetPanelActive(goalBannerPanel, false);
            SetPanelActive(victoryBannerPanel, false);
            SetPanelActive(defeatBannerPanel, false);
            
            // Show main game UI
            SetPanelActive(scorePanel, true);
            SetPanelActive(timerPanel, true);
            
            // Initialize score
            UpdateScore(0, 0);
            
            // Initialize timer
            SetTimerText("00:00");
            ResetTimerColor();
        }
        
        #endregion
        
        #region Score Management
        
        /// <summary>
        /// Update the score display
        /// </summary>
        public void UpdateScore(int teamAScore, int teamBScore)
        {
            if (teamAScoreText != null)
                teamAScoreText.text = teamAScore.ToString();
                
            if (teamBScoreText != null)
                teamBScoreText.text = teamBScore.ToString();
                
            if (scoreDisplay != null)
                scoreDisplay.text = $"{teamAScore} - {teamBScore}";
        }
        
        #endregion
        
        #region Timer Management
        
        /// <summary>
        /// Set the timer display text
        /// </summary>
        public void SetTimerText(string timeText)
        {
            if (timerText != null)
                timerText.text = timeText;
        }
        
        /// <summary>
        /// Flash timer for warning (final seconds)
        /// </summary>
        public void FlashTimer()
        {
            if (isTimerFlashing) return;
            
            if (timerFlashCoroutine != null)
                StopCoroutine(timerFlashCoroutine);
                
            timerFlashCoroutine = StartCoroutine(FlashTimerCoroutine());
        }
        
        /// <summary>
        /// Coroutine for flashing timer
        /// </summary>
        private System.Collections.IEnumerator FlashTimerCoroutine()
        {
            isTimerFlashing = true;
            float flashTime = 0.5f;
            int flashCount = 6; // Flash 3 times
            
            for (int i = 0; i < flashCount; i++)
            {
                SetTimerColor(warningTimerColor);
                yield return new WaitForSeconds(flashTime);
                
                SetTimerColor(normalTimerColor);
                yield return new WaitForSeconds(flashTime);
            }
            
            isTimerFlashing = false;
        }
        
        /// <summary>
        /// Set timer text color
        /// </summary>
        private void SetTimerColor(Color color)
        {
            if (timerText != null)
                timerText.color = color;
                
            if (timerBackground != null)
                timerBackground.color = new Color(color.r, color.g, color.b, 0.3f);
        }
        
        /// <summary>
        /// Reset timer to normal color
        /// </summary>
        public void ResetTimerColor()
        {
            SetTimerColor(normalTimerColor);
        }
        
        #endregion
        
        #region Countdown System
        
        /// <summary>
        /// Show main countdown (3-2-1)
        /// </summary>
        public void ShowCountdown(int number)
        {
            if (countdownPanel == null || countdownText == null) return;
            
            SetPanelActive(countdownPanel, true);
            
            if (number > 0)
            {
                countdownText.text = number.ToString();
                countdownText.color = Color.white;
            }
            else
            {
                countdownText.text = "GO!";
                countdownText.color = Color.green;
            }
            
            // Scale animation
            if (hudAnimator != null)
            {
                hudAnimator.SetTrigger("CountdownPulse");
            }
        }
        
        /// <summary>
        /// Hide main countdown
        /// </summary>
        public void HideCountdown()
        {
            StartCoroutine(FadeOutCountdown());
        }
        
        /// <summary>
        /// Fade out countdown smoothly
        /// </summary>
        private System.Collections.IEnumerator FadeOutCountdown()
        {
            yield return new WaitForSeconds(countdownFadeTime);
            SetPanelActive(countdownPanel, false);
        }
        
        /// <summary>
        /// Show mini countdown for resuming play
        /// </summary>
        public void ShowMiniCountdown(string message)
        {
            if (miniCountdownPanel == null || miniCountdownText == null) return;
            
            SetPanelActive(miniCountdownPanel, true);
            miniCountdownText.text = message;
        }
        
        /// <summary>
        /// Hide mini countdown
        /// </summary>
        public void HideMiniCountdown()
        {
            SetPanelActive(miniCountdownPanel, false);
        }
        
        #endregion
        
        #region Match Status
        
        /// <summary>
        /// Show match started message
        /// </summary>
        public void ShowMatchStarted()
        {
            // Could show a brief "Match Started" message
            UnityEngine.Debug.Log("Match Started UI feedback");
        }
        
        /// <summary>
        /// Show goal celebration
        /// </summary>
        public void ShowGoalCelebration()
        {
            if (celebrationCoroutine != null)
                StopCoroutine(celebrationCoroutine);
                
            celebrationCoroutine = StartCoroutine(GoalCelebrationSequence());
        }
        
        /// <summary>
        /// Goal celebration coroutine
        /// </summary>
        private System.Collections.IEnumerator GoalCelebrationSequence()
        {
            SetPanelActive(goalCelebrationPanel, true);
            
            if (goalMessageText != null)
                goalMessageText.text = "GOAL!";
            
            if (hudAnimator != null)
                hudAnimator.SetTrigger("GoalCelebration");
            
            yield return new WaitForSeconds(celebrationDuration);
            
            SetPanelActive(goalCelebrationPanel, false);
        }
        
        /// <summary>
        /// Show golden goal mode
        /// </summary>
        public void ShowGoldenGoal()
        {
            SetPanelActive(goldenGoalPanel, true);
            
            if (goldenGoalText != null)
                goldenGoalText.text = "GOLDEN GOAL!";
            
            if (goldenGoalParticles != null)
                goldenGoalParticles.Play();
            
            // Change timer color
            SetTimerColor(goldenGoalColor);
            
            if (hudAnimator != null)
                hudAnimator.SetTrigger("GoldenGoal");
        }
        
        #endregion
        
        #region Match End
        
        /// <summary>
        /// Show match results
        /// </summary>
        public void ShowMatchResults(int teamAScore, int teamBScore, int winningTeam)
        {
            SetPanelActive(matchResultsPanel, true);
            
            // Update final score
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {teamAScore} - {teamBScore}";
            
            // Update winner text
            if (winnerText != null)
            {
                switch (winningTeam)
                {
                    case 0:
                        winnerText.text = "DRAW!";
                        winnerText.color = Color.yellow;
                        break;
                    case 1:
                        winnerText.text = "TEAM A WINS!";
                        winnerText.color = Color.blue;
                        break;
                    case 2:
                        winnerText.text = "TEAM B WINS!";
                        winnerText.color = Color.red;
                        break;
                }
            }
            
            if (hudAnimator != null)
                hudAnimator.SetTrigger("MatchEnd");
                
            // Auto-transition to post-match options after display duration
            StartCoroutine(AutoTransitionToPostMatch());
        }
        
        /// <summary>
        /// Automatically transition from results to post-match options
        /// </summary>
        private System.Collections.IEnumerator AutoTransitionToPostMatch()
        {
            yield return new WaitForSeconds(resultDisplayDuration);
            ShowPostMatchOptions();
        }
        
        /// <summary>
        /// Show post-match options
        /// </summary>
        public void ShowPostMatchOptions()
        {
            SetPanelActive(postMatchPanel, true);
            
            // Setup button callbacks
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
                
            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
        
        #endregion
        
        #region Stamina & Player Status
        
        /// <summary>
        /// Update stamina for a specific player
        /// </summary>
        public void UpdateStamina(PlushLeague.Gameplay.Player.PlayerController player, float current, float max)
        {
            // TODO: Implement when PlayerHUD is integrated
            UnityEngine.Debug.Log($"Updating stamina for {player.name}: {current}/{max}");
        }
        
        /// <summary>
        /// Set cooldown for a player's ability
        /// </summary>
        public void SetCooldown(PlushLeague.Gameplay.Player.PlayerController player, string ability, float cooldownTime)
        {
            // TODO: Implement when PlayerHUD is integrated
            UnityEngine.Debug.Log($"Setting {ability} cooldown for {player.name}: {cooldownTime}s");
        }
        
        /// <summary>
        /// Show role icon for a player
        /// </summary>
        public void ShowRoleIcon(PlushLeague.Gameplay.Player.PlayerController player, int roleType)
        {
            // TODO: Implement when PlayerHUD is integrated
            UnityEngine.Debug.Log($"Setting role icon for {player.name} to role {roleType}");
        }
        
        /// <summary>
        /// Flash action button when used
        /// </summary>
        public void FlashActionButton(PlushLeague.Gameplay.Player.PlayerController player, string actionName)
        {
            // TODO: Implement when PlayerHUD is integrated
            UnityEngine.Debug.Log($"Flashing {actionName} button for {player.name}");
        }
        
        /// <summary>
        /// Get PlayerHUD for specific player (placeholder)
        /// </summary>
        private GameObject GetPlayerHUD(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // TODO: Implement proper PlayerHUD lookup when integrated
            return null;
        }
        
        /// <summary>
        /// Get maximum cooldown time for an ability type
        /// </summary>
        private float GetMaxCooldownForAbility(string ability)
        {
            // Default cooldown times - these could be configurable
            switch (ability.ToLower())
            {
                case "superpower":
                case "y":
                    return 30f; // Superpower cooldown
                case "ability":
                case "x":
                    return 15f; // Special ability cooldown
                case "kick":
                case "b":
                    return 2f; // Kick cooldown
                case "action":
                case "a":
                    return 1f; // Basic action cooldown
                default:
                    return 10f; // Default cooldown
            }
        }
        
        #endregion
        
        #region Goal & Victory Banners
        
        /// <summary>
        /// Show goal banner for scoring team
        /// </summary>
        public void ShowGoalBanner(int scoringTeamId)
        {
            if (goalBannerPanel != null && goalBannerText != null)
            {
                goalBannerPanel.SetActive(true);
                goalBannerText.text = $"GOAL! Team {scoringTeamId}";
                
                // Auto-hide after celebration duration
                StartCoroutine(HideBannerAfterDelay(goalBannerPanel, celebrationDuration));
            }
            
            // Also show the standard goal celebration
            ShowGoalCelebration();
        }
        
        /// <summary>
        /// Show victory screen for winning team
        /// </summary>
        public void ShowVictoryScreen(int winningTeamId)
        {
            if (winningTeamId > 0)
            {
                // Specific team won
                if (victoryBannerPanel != null && victoryBannerText != null)
                {
                    victoryBannerPanel.SetActive(true);
                    victoryBannerText.text = $"TEAM {winningTeamId} WINS!";
                    victoryBannerText.color = winningTeamId == 1 ? Color.blue : Color.red;
                }
            }
            else
            {
                // Draw
                if (victoryBannerPanel != null && victoryBannerText != null)
                {
                    victoryBannerPanel.SetActive(true);
                    victoryBannerText.text = "DRAW!";
                    victoryBannerText.color = Color.yellow;
                }
            }
            
            // Also show match results
            ShowMatchResults(0, 0, winningTeamId); // Scores will be updated separately
        }
        
        /// <summary>
        /// Hide banner after specified delay
        /// </summary>
        private System.Collections.IEnumerator HideBannerAfterDelay(GameObject banner, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (banner != null)
                banner.SetActive(false);
        }
        
        #endregion
        
        #region Connection Status
        
        /// <summary>
        /// Show reconnecting overlay
        /// </summary>
        public void ShowReconnecting()
        {
            isReconnecting = true;
            
            if (reconnectingPanel != null)
                reconnectingPanel.SetActive(true);
                
            if (reconnectingText != null)
                reconnectingText.text = "Reconnecting...";
                
            // Start spinner animation
            if (spinnerCoroutine != null)
                StopCoroutine(spinnerCoroutine);
            spinnerCoroutine = StartCoroutine(SpinnerAnimation());
        }
        
        /// <summary>
        /// Hide reconnecting overlay
        /// </summary>
        public void HideReconnecting()
        {
            isReconnecting = false;
            
            if (reconnectingPanel != null)
                reconnectingPanel.SetActive(false);
                
            if (spinnerCoroutine != null)
            {
                StopCoroutine(spinnerCoroutine);
                spinnerCoroutine = null;
            }
        }
        
        /// <summary>
        /// Spinner animation coroutine
        /// </summary>
        private System.Collections.IEnumerator SpinnerAnimation()
        {
            if (reconnectingSpinner == null) yield break;
            
            while (isReconnecting)
            {
                reconnectingSpinner.transform.Rotate(0, 0, -spinnerRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        #endregion
        
        #region Responsive Layout
        
        /// <summary>
        /// Handle screen size changes and aspect ratio adjustments
        /// </summary>
        public void UpdateLayoutForScreen()
        {
            // Get current screen dimensions
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float aspectRatio = screenWidth / screenHeight;
            
            // Adjust UI layout based on aspect ratio
            if (aspectRatio > 2.0f) // Ultra-wide screens
            {
                AdjustForUltraWideScreen();
            }
            else if (aspectRatio < 1.5f) // Tall screens (mobile)
            {
                AdjustForTallScreen();
            }
            else // Standard screens
            {
                AdjustForStandardScreen();
            }
        }
        
        /// <summary>
        /// Adjust layout for ultra-wide screens
        /// </summary>
        private void AdjustForUltraWideScreen()
        {
            // Move score/timer more toward center
            // Adjust player HUD positions
        }
        
        /// <summary>
        /// Adjust layout for tall mobile screens
        /// </summary>
        private void AdjustForTallScreen()
        {
            // Move elements to avoid notch area
            // Adjust button sizes for touch
        }
        
        /// <summary>
        /// Adjust layout for standard screens
        /// </summary>
        private void AdjustForStandardScreen()
        {
            // Default layout
        }
        
        #endregion
        
        #region Public Getters (Enhanced)
        
        public bool IsReconnecting => isReconnecting;
        public bool IsCountdownActive => countdownPanel != null && countdownPanel.activeInHierarchy;
        public bool IsMatchResultsShowing => matchResultsPanel != null && matchResultsPanel.activeInHierarchy;
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Safely set panel active state
        /// </summary>
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
        
        #endregion
        
        #region Button Callbacks
        
        /// <summary>
        /// Handle play again button click
        /// </summary>
        private void OnPlayAgainClicked()
        {
            UnityEngine.Debug.Log("Play Again clicked");
            // Could trigger match restart or scene reload
            
            // Find and restart match
            var matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            if (matchManager != null)
            {
                // Could add restart functionality to MatchManager
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
        
        /// <summary>
        /// Handle return to menu button click
        /// </summary>
        private void OnReturnToMenuClicked()
        {
            UnityEngine.Debug.Log("Return to Menu clicked");
            // Load main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        #endregion
    }
}
