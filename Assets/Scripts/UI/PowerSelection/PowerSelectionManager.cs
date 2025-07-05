using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PlushLeague.UI.PowerSelection
{
    /// <summary>
    /// Manages the power selection process, coordinates between UI and game systems.
    /// Handles the transition from menu to power selection to match start.
    /// </summary>
    public class PowerSelectionManager : MonoBehaviour
    {
        [Header("Power Selection")]
        [SerializeField] private PowerSelectionUI powerSelectionUI;
        [SerializeField] private List<PlushLeague.Gameplay.Superpowers.SuperpowerData> availablePowers;
        [SerializeField] private bool autoLoadPowersFromResources = true;
        [SerializeField] private string powersResourcePath = "Superpowers";
        
        [Header("Game Integration")]
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private string matchSceneName = "GameArena";
        [SerializeField] private float transitionDelay = 1f;
        
        [Header("Player Data")]
        [SerializeField] private PlayerSelectionConfig player1Config;
        [SerializeField] private PlayerSelectionConfig player2Config;
        
        [Header("Networking")]
        [SerializeField] private bool isMultiplayer = false;
        // Note: isHost field removed as it's not currently used
        
        [Header("Default Selections")]
        [SerializeField] private PlushLeague.Gameplay.Superpowers.SuperpowerData defaultPower;
        [SerializeField] private PowerSelectionUI.RoleType defaultRole = PowerSelectionUI.RoleType.Striker;
        
        // Events
        public System.Action<PlayerSelectionConfig, PlayerSelectionConfig> OnSelectionsConfirmed;
        public System.Action OnMatchStartRequested;
        
        [System.Serializable]
        public class PlayerSelectionConfig
        {
            public string playerName = "Player";
            public PlushLeague.Gameplay.Superpowers.SuperpowerData selectedPower;
            public PowerSelectionUI.RoleType selectedRole = PowerSelectionUI.RoleType.Striker;
            public bool isReady = false;
            public bool isConnected = true;
            public bool isLocalPlayer = true;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            LoadAvailablePowers();
        }
        
        private void Start()
        {
            SetupEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize components and references
        /// </summary>
        private void InitializeComponents()
        {
            if (powerSelectionUI == null)
                powerSelectionUI = FindFirstObjectByType<PowerSelectionUI>();
                
            if (gameManager == null)
                gameManager = FindFirstObjectByType<PlushLeague.Core.GameManager>();
                
            // Initialize player configs
            if (player1Config == null)
                player1Config = new PlayerSelectionConfig { playerName = "Player 1", isLocalPlayer = true };
                
            if (player2Config == null)
                player2Config = new PlayerSelectionConfig { playerName = "Player 2", isLocalPlayer = !isMultiplayer };
        }
        
        /// <summary>
        /// Load available powers from resources or assigned list
        /// </summary>
        private void LoadAvailablePowers()
        {
            if (autoLoadPowersFromResources)
            {
                var loadedPowers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>(powersResourcePath);
                if (loadedPowers.Length > 0)
                {
                    availablePowers = new List<PlushLeague.Gameplay.Superpowers.SuperpowerData>(loadedPowers);
                    UnityEngine.Debug.Log($"Loaded {availablePowers.Count} powers from resources");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"No powers found in Resources/{powersResourcePath}");
                }
            }
            
            // Ensure we have at least the default power
            if (availablePowers.Count == 0 && defaultPower != null)
            {
                availablePowers.Add(defaultPower);
                UnityEngine.Debug.Log("Using default power as fallback");
            }
        }
        
        /// <summary>
        /// Setup event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (powerSelectionUI != null)
            {
                powerSelectionUI.OnSelectionConfirmed += HandleLocalPlayerSelection;
                powerSelectionUI.OnAllPlayersReady += HandleAllPlayersReady;
                powerSelectionUI.OnBackToMenu += HandleBackToMenu;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Start power selection process
        /// </summary>
        public void StartPowerSelection(bool multiplayer = false)
        {
            isMultiplayer = multiplayer;
            
            UnityEngine.Debug.Log($"Starting power selection (Multiplayer: {multiplayer})");
            
            // Reset player states
            ResetPlayerConfigs();
            
            // Show power selection UI
            if (powerSelectionUI != null)
            {
                powerSelectionUI.ShowPowerSelection(availablePowers, isMultiplayer);
            }
            else
            {
                UnityEngine.Debug.LogError("PowerSelectionUI not found!");
            }
        }
        
        /// <summary>
        /// Cancel power selection and return to menu
        /// </summary>
        public void CancelPowerSelection()
        {
            if (powerSelectionUI != null)
            {
                powerSelectionUI.HidePowerSelection();
            }
            
            UnityEngine.Debug.Log("Power selection cancelled");
        }
        
        /// <summary>
        /// Update remote player data (for multiplayer)
        /// </summary>
        public void UpdateRemotePlayerData(int playerIndex, PowerSelectionUI.PlayerSelectionData remoteData)
        {
            if (!isMultiplayer) return;
            
            // Convert to local config format
            var config = playerIndex == 1 ? player1Config : player2Config;
            config.playerName = remoteData.playerName;
            config.selectedPower = remoteData.selectedPower;
            config.selectedRole = remoteData.selectedRole;
            config.isReady = remoteData.isReady;
            config.isConnected = remoteData.isConnected;
            config.isLocalPlayer = false;
            
            // Update UI
            if (powerSelectionUI != null)
            {
                powerSelectionUI.UpdateTeammateData(playerIndex, remoteData);
            }
            
            UnityEngine.Debug.Log($"Updated remote player {playerIndex} data: {config.playerName}, Power: {config.selectedPower?.name}, Role: {config.selectedRole}, Ready: {config.isReady}");
        }
        
        /// <summary>
        /// Get current player configurations
        /// </summary>
        public (PlayerSelectionConfig player1, PlayerSelectionConfig player2) GetPlayerConfigs()
        {
            return (player1Config, player2Config);
        }
        
        /// <summary>
        /// Force apply default selections for testing
        /// </summary>
        public void ApplyDefaultSelections()
        {
            player1Config.selectedPower = defaultPower;
            player1Config.selectedRole = defaultRole;
            player1Config.isReady = true;
            
            if (!isMultiplayer)
            {
                player2Config.selectedPower = defaultPower;
                player2Config.selectedRole = defaultRole == PowerSelectionUI.RoleType.Striker ? 
                    PowerSelectionUI.RoleType.Goalkeeper : PowerSelectionUI.RoleType.Striker;
                player2Config.isReady = true;
            }
            
            UnityEngine.Debug.Log("Applied default selections for testing");
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle local player selection confirmation
        /// </summary>
        private void HandleLocalPlayerSelection(PlushLeague.Gameplay.Superpowers.SuperpowerData power, PowerSelectionUI.RoleType role)
        {
            // Update local player config (assuming player 1 is local)
            player1Config.selectedPower = power;
            player1Config.selectedRole = role;
            player1Config.isReady = true;
            
            UnityEngine.Debug.Log($"Local player selected: Power: {power?.name}, Role: {role}");
            
            // In multiplayer, send this data to other players
            if (isMultiplayer)
            {
                SendPlayerDataToNetwork();
            }
        }
        
        /// <summary>
        /// Handle when all players are ready
        /// </summary>
        private void HandleAllPlayersReady()
        {
            UnityEngine.Debug.Log("All players ready - starting match transition");
            
            // Validate selections
            if (!ValidateSelections())
            {
                UnityEngine.Debug.LogError("Invalid selections detected - cannot start match");
                return;
            }
            
            // Apply selections to game systems
            ApplySelectionsToGame();
            
            // Fire events
            OnSelectionsConfirmed?.Invoke(player1Config, player2Config);
            OnMatchStartRequested?.Invoke();
            
            // Start match transition
            StartCoroutine(TransitionToMatch());
        }
        
        /// <summary>
        /// Handle back to menu request
        /// </summary>
        private void HandleBackToMenu()
        {
            UnityEngine.Debug.Log("Returning to main menu");
            
            // Could fire event for menu system to handle
            // For now, just hide the power selection
            CancelPowerSelection();
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validate that all selections are valid
        /// </summary>
        private bool ValidateSelections()
        {
            // Check player 1
            if (player1Config.selectedPower == null)
            {
                UnityEngine.Debug.LogError("Player 1 has no power selected");
                return false;
            }
            
            // Check player 2 (if playing)
            if (!isMultiplayer || (player2Config.isConnected && player2Config.selectedPower == null))
            {
                UnityEngine.Debug.LogError("Player 2 has no power selected");
                return false;
            }
            
            // Check role conflicts (both players can't have same role)
            if (player1Config.selectedRole == player2Config.selectedRole && 
                player2Config.isConnected)
            {
                UnityEngine.Debug.LogError("Both players have same role - conflict not resolved");
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Game Integration
        
        /// <summary>
        /// Apply selections to game systems
        /// </summary>
        private void ApplySelectionsToGame()
        {
            // Store selections for match start
            if (gameManager != null)
            {
                // This would integrate with GameManager to set up player loadouts
                // For now, we'll store them in PlayerPrefs or a persistent data system
                
                StorePlayerSelection(1, player1Config);
                
                if (player2Config.isConnected)
                {
                    StorePlayerSelection(2, player2Config);
                }
            }
            
            UnityEngine.Debug.Log("Applied selections to game systems");
        }
        
        /// <summary>
        /// Store player selection data
        /// </summary>
        private void StorePlayerSelection(int playerIndex, PlayerSelectionConfig config)
        {
            string prefix = $"Player{playerIndex}";
            
            // Store power selection
            if (config.selectedPower != null)
            {
                PlayerPrefs.SetString($"{prefix}_Power", config.selectedPower.name);
            }
            
            // Store role selection
            PlayerPrefs.SetString($"{prefix}_Role", config.selectedRole.ToString());
            
            // Store other data as needed
            PlayerPrefs.SetString($"{prefix}_Name", config.playerName);
            
            UnityEngine.Debug.Log($"Stored selection data for {prefix}: Power: {config.selectedPower?.name}, Role: {config.selectedRole}");
        }
        
        /// <summary>
        /// Transition to match scene
        /// </summary>
        private IEnumerator TransitionToMatch()
        {
            yield return new WaitForSeconds(transitionDelay);
            
            // Load match scene
            if (!string.IsNullOrEmpty(matchSceneName))
            {
                UnityEngine.Debug.Log($"Loading match scene: {matchSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(matchSceneName);
            }
            else
            {
                UnityEngine.Debug.LogWarning("No match scene name specified");
            }
        }
        
        #endregion
        
        #region Multiplayer Integration
        
        /// <summary>
        /// Send player data to network (placeholder for networking)
        /// </summary>
        private void SendPlayerDataToNetwork()
        {
            if (!isMultiplayer) return;
            
            var localData = new PowerSelectionUI.PlayerSelectionData
            {
                playerName = player1Config.playerName,
                selectedPower = player1Config.selectedPower,
                selectedRole = player1Config.selectedRole,
                isReady = player1Config.isReady,
                isConnected = player1Config.isConnected
            };
            
            // TODO: Implement actual networking
            // NetworkManager.SendPlayerSelection(localData);
            
            UnityEngine.Debug.Log("Sent player data to network (placeholder)");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Reset player configurations to defaults
        /// </summary>
        private void ResetPlayerConfigs()
        {
            player1Config.selectedPower = null;
            player1Config.selectedRole = PowerSelectionUI.RoleType.Striker;
            player1Config.isReady = false;
            player1Config.isConnected = true;
            player1Config.isLocalPlayer = true;
            
            player2Config.selectedPower = null;
            player2Config.selectedRole = PowerSelectionUI.RoleType.Striker;
            player2Config.isReady = false;
            player2Config.isConnected = !isMultiplayer; // Only connected in local play
            player2Config.isLocalPlayer = !isMultiplayer;
        }
        
        #endregion
        
        #region Debug/Editor
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 400, 300, 300));
            GUILayout.Label("=== Power Selection Manager ===");
            GUILayout.Label($"Available Powers: {availablePowers.Count}");
            GUILayout.Label($"Is Multiplayer: {isMultiplayer}");
            
            GUILayout.Space(10);
            GUILayout.Label("Player 1:");
            GUILayout.Label($"  Power: {(player1Config.selectedPower != null ? player1Config.selectedPower.name : "None")}");
            GUILayout.Label($"  Role: {player1Config.selectedRole}");
            GUILayout.Label($"  Ready: {player1Config.isReady}");
            
            GUILayout.Space(5);
            GUILayout.Label("Player 2:");
            GUILayout.Label($"  Power: {(player2Config.selectedPower != null ? player2Config.selectedPower.name : "None")}");
            GUILayout.Label($"  Role: {player2Config.selectedRole}");
            GUILayout.Label($"  Ready: {player2Config.isReady}");
            GUILayout.Label($"  Connected: {player2Config.isConnected}");
            
            GUILayout.Space(10);
            if (GUILayout.Button("Start Selection (Single)"))
            {
                StartPowerSelection(false);
            }
            
            if (GUILayout.Button("Start Selection (Multi)"))
            {
                StartPowerSelection(true);
            }
            
            if (GUILayout.Button("Apply Defaults"))
            {
                ApplyDefaultSelections();
            }
            
            if (GUILayout.Button("Force Start Match"))
            {
                HandleAllPlayersReady();
            }
            
            GUILayout.EndArea();
        }
        #endif
        
        #endregion
    }
}
