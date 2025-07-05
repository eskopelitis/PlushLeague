using UnityEngine;
using System.Collections;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// Example integration showing how to use all HUD components together
    /// Demonstrates complete in-game UI system with player feedback, cooldowns, and match flow
    /// </summary>
    public class HUDIntegrationExample : MonoBehaviour
    {
        [Header("HUD Components")]
        [SerializeField] private GameHUD gameHUD;
        [SerializeField] private PlayerHUD[] playerHUDs;
        [SerializeField] private CooldownUIManager cooldownManager;
        [SerializeField] private ActionButtonUI[] actionButtons;
        
        [Header("Test Settings")]
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private float testCooldownTime = 10f;
        [SerializeField] private KeyCode testKey = KeyCode.T;
        
        private PlushLeague.Gameplay.Player.PlayerController[] players;
        private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeIntegration();
            
            if (enableTestMode)
            {
                StartCoroutine(RunUITests());
            }
        }
        
        private void Update()
        {
            if (enableTestMode && Input.GetKeyDown(testKey))
            {
                TestRandomUIFeature();
            }
            
            UpdatePlayerHUDs();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize HUD integration and find components
        /// </summary>
        private void InitializeIntegration()
        {
            // Find components if not assigned
            if (gameHUD == null)
                gameHUD = FindFirstObjectByType<GameHUD>();
                
            if (cooldownManager == null)
                cooldownManager = FindFirstObjectByType<CooldownUIManager>();
                
            if (matchManager == null)
                matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            
            // Find all players
            players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            
            // Subscribe to player events
            SubscribeToPlayerEvents();
            
            // Subscribe to match events
            SubscribeToMatchEvents();
            
            // Initialize player HUDs
            InitializePlayerHUDs();
            
            UnityEngine.Debug.Log($"HUD Integration initialized with {players.Length} players");
        }
        
        /// <summary>
        /// Initialize player HUDs with player references
        /// </summary>
        private void InitializePlayerHUDs()
        {
            for (int i = 0; i < playerHUDs.Length && i < players.Length; i++)
            {
                if (playerHUDs[i] != null && players[i] != null)
                {
                    playerHUDs[i].SetPlayer(players[i]);
                    
                    // Set role based on player index (example)
                    PlayerHUD.PlayerRole role = (i == 0) ? PlayerHUD.PlayerRole.Goalkeeper : PlayerHUD.PlayerRole.Striker;
                    playerHUDs[i].SetPlayerRole(role);
                }
            }
        }
        
        #endregion
        
        #region Event Subscription
        
        /// <summary>
        /// Subscribe to player events for UI updates
        /// </summary>
        private void SubscribeToPlayerEvents()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    // Subscribe to stamina changes
                    player.OnStaminaChanged += (current, max) => OnPlayerStaminaChanged(player, current, max);
                    player.OnSprintStateChanged += (sprinting) => OnPlayerSprintChanged(player, sprinting);
                    
                    // Subscribe to superpower events
                    var powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
                    if (powerupController != null)
                    {
                        // Would subscribe to powerup events here if they existed
                        // powerupController.OnPowerupUsed += (powerup) => OnPlayerUsedPowerup(player, powerup);
                        // powerupController.OnCooldownStarted += (cooldown) => OnPlayerCooldownStarted(player, cooldown);
                    }
                }
            }
        }
        
        /// <summary>
        /// Subscribe to match events for UI updates
        /// </summary>
        private void SubscribeToMatchEvents()
        {
            if (matchManager != null)
            {
                matchManager.OnGoalScored += OnGoalScored;
                matchManager.OnScoreUpdated += OnScoreUpdated;
                matchManager.OnGoldenGoalStarted += OnGoldenGoalStarted;
                matchManager.OnMatchEnded += OnMatchEnded;
                matchManager.OnTimerUpdated += OnTimerUpdated;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle player stamina changes
        /// </summary>
        private void OnPlayerStaminaChanged(PlushLeague.Gameplay.Player.PlayerController player, float current, float max)
        {
            // Update GameHUD with stamina info
            if (gameHUD != null)
            {
                gameHUD.UpdateStamina(player, current, max);
            }
            
            // Flash stamina bar if low
            if (current / max < 0.2f)
            {
                FlashLowStaminaWarning(player);
            }
        }
        
        /// <summary>
        /// Handle player sprint state changes
        /// </summary>
        private void OnPlayerSprintChanged(PlushLeague.Gameplay.Player.PlayerController player, bool isSprinting)
        {
            // Could add sprint indicator effects here
            UnityEngine.Debug.Log($"{player.name} sprint state: {isSprinting}");
        }
        
        /// <summary>
        /// Handle goal scored events
        /// </summary>
        private void OnGoalScored(int teamId)
        {
            if (gameHUD != null)
            {
                gameHUD.ShowGoalBanner(teamId);
            }
        }
        
        /// <summary>
        /// Handle score updates
        /// </summary>
        private void OnScoreUpdated(int teamAScore, int teamBScore)
        {
            if (gameHUD != null)
            {
                gameHUD.UpdateScore(teamAScore, teamBScore);
            }
        }
        
        /// <summary>
        /// Handle golden goal start
        /// </summary>
        private void OnGoldenGoalStarted()
        {
            if (gameHUD != null)
            {
                gameHUD.ShowGoldenGoal();
            }
        }
        
        /// <summary>
        /// Handle match end
        /// </summary>
        private void OnMatchEnded(int winningTeamId)
        {
            if (gameHUD != null)
            {
                gameHUD.ShowVictoryScreen(winningTeamId);
            }
        }
        
        /// <summary>
        /// Handle timer updates
        /// </summary>
        private void OnTimerUpdated(float currentTime)
        {
            if (gameHUD != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
                gameHUD.SetTimerText(timeString);
            }
        }
        
        #endregion
        
        #region UI Updates
        
        /// <summary>
        /// Update all player HUDs with current data
        /// </summary>
        private void UpdatePlayerHUDs()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    UpdatePlayerCooldowns(player);
                    UpdatePlayerActionAvailability(player);
                }
            }
        }
        
        /// <summary>
        /// Update cooldowns for a specific player
        /// </summary>
        private void UpdatePlayerCooldowns(PlushLeague.Gameplay.Player.PlayerController player)
        {
            var powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
            if (powerupController != null)
            {
                float cooldownRemaining = powerupController.GetCooldownRemaining();
                if (cooldownRemaining > 0)
                {
                    // Update superpower cooldown
                    if (gameHUD != null)
                    {
                        gameHUD.SetCooldown(player, "superpower", cooldownRemaining);
                    }
                    
                    if (cooldownManager != null && !cooldownManager.IsOnCooldown("superpower"))
                    {
                        cooldownManager.StartCooldown("superpower", cooldownRemaining);
                    }
                }
            }
        }
        
        /// <summary>
        /// Update action button availability for a player
        /// </summary>
        private void UpdatePlayerActionAvailability(PlushLeague.Gameplay.Player.PlayerController player)
        {
            PlayerHUD playerHUD = GetPlayerHUD(player);
            if (playerHUD != null)
            {
                // Update kick availability
                bool canKick = player.HasBall();
                playerHUD.SetActionAvailable("kick", canKick);
                
                // Update action availability
                bool hasAction = player.HasBall() || player.GetNearbyBall() != null;
                playerHUD.SetActionAvailable("action", hasAction);
                
                // Update superpower availability
                var powerupController = player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>();
                bool canUsePowerup = powerupController != null && powerupController.CanUsePowerup();
                playerHUD.SetActionAvailable("superpower", canUsePowerup);
            }
        }
        
        /// <summary>
        /// Flash low stamina warning
        /// </summary>
        private void FlashLowStaminaWarning(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Could implement stamina warning flash here
            UnityEngine.Debug.Log($"Low stamina warning for {player.name}");
        }
        
        #endregion
        
        #region Test Functions
        
        /// <summary>
        /// Run automated UI tests
        /// </summary>
        private System.Collections.IEnumerator RunUITests()
        {
            yield return new WaitForSeconds(2f);
            
            UnityEngine.Debug.Log("Running HUD UI Tests...");
            
            // Test countdown
            if (gameHUD != null)
            {
                for (int i = 3; i > 0; i--)
                {
                    gameHUD.ShowCountdown(i);
                    yield return new WaitForSeconds(1f);
                }
                gameHUD.ShowCountdown(0); // "GO!"
                yield return new WaitForSeconds(0.5f);
                gameHUD.HideCountdown();
            }
            
            yield return new WaitForSeconds(2f);
            
            // Test cooldowns
            if (cooldownManager != null)
            {
                cooldownManager.StartCooldown("Test Ability 1", 5f);
                cooldownManager.StartCooldown("Test Ability 2", 8f);
                cooldownManager.StartCooldown("Test Ability 3", 3f);
            }
            
            yield return new WaitForSeconds(3f);
            
            // Test goal celebration
            if (gameHUD != null)
            {
                gameHUD.ShowGoalBanner(1);
            }
            
            yield return new WaitForSeconds(3f);
            
            UnityEngine.Debug.Log("HUD UI Tests completed!");
        }
        
        /// <summary>
        /// Test random UI feature
        /// </summary>
        private void TestRandomUIFeature()
        {
            int randomTest = Random.Range(0, 4);
            
            switch (randomTest)
            {
                case 0:
                    // Test cooldown
                    if (cooldownManager != null)
                        cooldownManager.StartCooldown("Random Test", testCooldownTime);
                    break;
                    
                case 1:
                    // Test goal banner
                    if (gameHUD != null)
                        gameHUD.ShowGoalBanner(Random.Range(1, 3));
                    break;
                    
                case 2:
                    // Test action button flash
                    if (players.Length > 0 && gameHUD != null)
                        gameHUD.FlashActionButton(players[0], "superpower");
                    break;
                    
                case 3:
                    // Test reconnecting overlay
                    if (gameHUD != null)
                    {
                        gameHUD.ShowReconnecting();
                        StartCoroutine(HideReconnectingAfterDelay(3f));
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Hide reconnecting overlay after delay
        /// </summary>
        private System.Collections.IEnumerator HideReconnectingAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (gameHUD != null)
                gameHUD.HideReconnecting();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get PlayerHUD for specific player
        /// </summary>
        private PlayerHUD GetPlayerHUD(PlushLeague.Gameplay.Player.PlayerController player)
        {
            foreach (var hud in playerHUDs)
            {
                if (hud != null && hud.GetPlayer() == player)
                    return hud;
            }
            return null;
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (matchManager != null)
            {
                matchManager.OnGoalScored -= OnGoalScored;
                matchManager.OnScoreUpdated -= OnScoreUpdated;
                matchManager.OnGoldenGoalStarted -= OnGoldenGoalStarted;
                matchManager.OnMatchEnded -= OnMatchEnded;
                matchManager.OnTimerUpdated -= OnTimerUpdated;
            }
        }
        
        #endregion
    }
}
