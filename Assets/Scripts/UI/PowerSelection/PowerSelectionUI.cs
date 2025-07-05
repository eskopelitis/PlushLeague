using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace PlushLeague.UI.PowerSelection
{
    /// <summary>
    /// Manages the pre-match power selection screen where players choose their superpowers and roles.
    /// Handles role conflicts with mini-games and ensures both players are ready before match start.
    /// </summary>
    public class PowerSelectionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject powerSelectionPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backToMenuButton;
        
        [Header("Power Selection")]
        [SerializeField] private Transform powerGridParent;
        [SerializeField] private GameObject powerButtonPrefab;
        [SerializeField] private TextMeshProUGUI powerDescriptionText;
        [SerializeField] private TextMeshProUGUI powerCooldownText;
        [SerializeField] private Image powerPreviewImage;
        
        [Header("Role Selection")]
        [SerializeField] private Button strikerButton;
        [SerializeField] private Button goalieButton;
        [SerializeField] private TextMeshProUGUI selectedRoleText;
        [SerializeField] private Image strikerIcon;
        [SerializeField] private Image goalieIcon;
        [SerializeField] private Color selectedRoleColor = Color.green;
        [SerializeField] private Color unselectedRoleColor = Color.gray;
        
        [Header("Player Ready States")]
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        [SerializeField] private GameObject player1ReadyIndicator;
        [SerializeField] private GameObject player2ReadyIndicator;
        [SerializeField] private TextMeshProUGUI teammate1StatusText;
        [SerializeField] private TextMeshProUGUI teammate2StatusText;
        
        [Header("Rock Paper Scissors Mini-Game")]
        [SerializeField] private GameObject rpsPanel;
        [SerializeField] private Button rockButton;
        [SerializeField] private Button paperButton;
        [SerializeField] private Button scissorsButton;
        [SerializeField] private TextMeshProUGUI rpsInstructionText;
        [SerializeField] private TextMeshProUGUI rpsResultText;
        [SerializeField] private Image player1ChoiceImage;
        [SerializeField] private Image player2ChoiceImage;
        
        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private float selectionTimeLimit = 60f;
        [SerializeField] private Color warningTimerColor = Color.red;
        [SerializeField] private Color normalTimerColor = Color.white;
        
        [Header("Audio")]
        [SerializeField] private AudioClip powerSelectSound;
        [SerializeField] private AudioClip roleSelectSound;
        [SerializeField] private AudioClip readySound;
        [SerializeField] private AudioClip teamReadySound;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem selectionEffect;
        [SerializeField] private ParticleSystem teamReadyEffect;
        [SerializeField] private Animator panelAnimator;
        
        // Selection state
        private List<PlushLeague.Gameplay.Superpowers.SuperpowerData> availablePowers;
        private PlushLeague.Gameplay.Superpowers.SuperpowerData selectedPower;
        private RoleType selectedRole = RoleType.Striker;
        private bool isReady = false;
        private float timeRemaining;
        private bool timerActive = false;
        
        // Multiplayer state
        private bool isMultiplayer = false;
        private PlayerSelectionData teammate1Data;
        private PlayerSelectionData teammate2Data;
        private bool allPlayersReady = false;
        
        // Rock Paper Scissors state
        private bool rpsInProgress = false;
        private RPSChoice localPlayerChoice = RPSChoice.None;
        private RPSChoice remotePlayerChoice = RPSChoice.None;
        
        // Power button pool
        private List<PowerButton> powerButtons = new List<PowerButton>();
        
        // Events
        public System.Action<PlushLeague.Gameplay.Superpowers.SuperpowerData, RoleType> OnSelectionConfirmed;
        public System.Action OnAllPlayersReady;
        public System.Action OnBackToMenu;
        
        public enum RoleType
        {
            Striker,
            Goalkeeper
        }
        
        public enum RPSChoice
        {
            None,
            Rock,
            Paper,
            Scissors
        }
        
        [System.Serializable]
        public class PlayerSelectionData
        {
            public string playerName;
            public PlushLeague.Gameplay.Superpowers.SuperpowerData selectedPower;
            public RoleType selectedRole;
            public bool isReady;
            public bool isConnected;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            SetupEventListeners();
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void Update()
        {
            if (timerActive)
            {
                UpdateTimer();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize UI components and references
        /// </summary>
        private void InitializeComponents()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            if (panelAnimator == null)
                panelAnimator = GetComponent<Animator>();
                
            // Initialize player data
            teammate1Data = new PlayerSelectionData();
            teammate2Data = new PlayerSelectionData();
        }
        
        /// <summary>
        /// Setup button event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (strikerButton != null)
                strikerButton.onClick.AddListener(() => OnRoleSelected(RoleType.Striker));
                
            if (goalieButton != null)
                goalieButton.onClick.AddListener(() => OnRoleSelected(RoleType.Goalkeeper));
                
            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyPressed);
                
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(() => OnBackToMenu?.Invoke());
                
            // Rock Paper Scissors buttons
            if (rockButton != null)
                rockButton.onClick.AddListener(() => OnRPSChoice(RPSChoice.Rock));
            if (paperButton != null)
                paperButton.onClick.AddListener(() => OnRPSChoice(RPSChoice.Paper));
            if (scissorsButton != null)
                scissorsButton.onClick.AddListener(() => OnRPSChoice(RPSChoice.Scissors));
        }
        
        /// <summary>
        /// Initialize UI to default state
        /// </summary>
        private void InitializeUI()
        {
            SetPanelActive(powerSelectionPanel, false);
            SetPanelActive(rpsPanel, false);
            
            // Set default selections
            selectedRole = RoleType.Striker;
            UpdateRoleSelection();
            UpdateReadyButton();
            UpdatePlayerStatusDisplay();
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Show power selection screen with available powers
        /// </summary>
        public void ShowPowerSelection(List<PlushLeague.Gameplay.Superpowers.SuperpowerData> powers, bool multiplayer = false)
        {
            availablePowers = powers;
            isMultiplayer = multiplayer;
            
            SetPanelActive(powerSelectionPanel, true);
            CreatePowerButtons();
            StartSelectionTimer();
            
            if (panelAnimator != null)
                panelAnimator.SetTrigger("ShowPanel");
                
            UnityEngine.Debug.Log($"Power selection shown with {powers.Count} available powers (Multiplayer: {multiplayer})");
        }
        
        /// <summary>
        /// Hide power selection screen
        /// </summary>
        public void HidePowerSelection()
        {
            if (panelAnimator != null)
                panelAnimator.SetTrigger("HidePanel");
            else
                SetPanelActive(powerSelectionPanel, false);
                
            StopSelectionTimer();
        }
        
        /// <summary>
        /// Update teammate selection data (for multiplayer)
        /// </summary>
        public void UpdateTeammateData(int playerIndex, PlayerSelectionData data)
        {
            if (playerIndex == 1)
                teammate1Data = data;
            else if (playerIndex == 2)
                teammate2Data = data;
                
            UpdatePlayerStatusDisplay();
            CheckForRoleConflict();
        }
        
        /// <summary>
        /// Get current local player selection
        /// </summary>
        public PlayerSelectionData GetLocalPlayerData()
        {
            return new PlayerSelectionData
            {
                playerName = "Local Player", // Could be set from PlayerPrefs or input
                selectedPower = selectedPower,
                selectedRole = selectedRole,
                isReady = isReady,
                isConnected = true
            };
        }
        
        #endregion
        
        #region Power Selection
        
        /// <summary>
        /// Create power selection buttons from available powers
        /// </summary>
        private void CreatePowerButtons()
        {
            // Clear existing buttons
            ClearPowerButtons();
            
            // Create new buttons
            for (int i = 0; i < availablePowers.Count; i++)
            {
                var powerData = availablePowers[i];
                var buttonObj = Instantiate(powerButtonPrefab, powerGridParent);
                var powerButton = buttonObj.GetComponent<PowerButton>();
                
                if (powerButton != null)
                {
                    powerButton.Initialize(powerData, () => OnPowerSelected(powerData));
                    powerButtons.Add(powerButton);
                }
            }
            
            // Select first power by default
            if (availablePowers.Count > 0)
            {
                OnPowerSelected(availablePowers[0]);
            }
        }
        
        /// <summary>
        /// Clear all power selection buttons
        /// </summary>
        private void ClearPowerButtons()
        {
            foreach (var button in powerButtons)
            {
                if (button != null && button.gameObject != null)
                    Destroy(button.gameObject);
            }
            powerButtons.Clear();
        }
        
        /// <summary>
        /// Handle power selection
        /// </summary>
        public void OnPowerSelected(PlushLeague.Gameplay.Superpowers.SuperpowerData power)
        {
            if (power == null) return;
            
            selectedPower = power;
            
            // Update UI
            UpdatePowerDetails(power);
            UpdatePowerButtonSelection(power);
            UpdateReadyButton();
            
            // Play audio feedback
            PlaySound(powerSelectSound);
            
            // Visual effect
            if (selectionEffect != null)
            {
                selectionEffect.Play();
            }
            
            UnityEngine.Debug.Log($"Power selected: {power.name}");
        }
        
        /// <summary>
        /// Update power details display
        /// </summary>
        private void UpdatePowerDetails(PlushLeague.Gameplay.Superpowers.SuperpowerData power)
        {
            if (powerDescriptionText != null)
                powerDescriptionText.text = power.description;
                
            if (powerCooldownText != null)
                powerCooldownText.text = $"Cooldown: {power.cooldownTime:F1}s";
                
            if (powerPreviewImage != null && power.icon != null)
                powerPreviewImage.sprite = power.icon;
        }
        
        /// <summary>
        /// Update visual selection state of power buttons
        /// </summary>
        private void UpdatePowerButtonSelection(PlushLeague.Gameplay.Superpowers.SuperpowerData selectedPower)
        {
            foreach (var button in powerButtons)
            {
                button.SetSelected(button.PowerData == selectedPower);
            }
        }
        
        #endregion
        
        #region Role Selection
        
        /// <summary>
        /// Handle role selection
        /// </summary>
        public void OnRoleSelected(RoleType role)
        {
            selectedRole = role;
            UpdateRoleSelection();
            UpdateReadyButton();
            
            PlaySound(roleSelectSound);
            
            UnityEngine.Debug.Log($"Role selected: {role}");
            
            // Check for conflicts in multiplayer
            if (isMultiplayer)
            {
                CheckForRoleConflict();
            }
        }
        
        /// <summary>
        /// Update role selection UI
        /// </summary>
        private void UpdateRoleSelection()
        {
            if (selectedRoleText != null)
                selectedRoleText.text = $"Role: {selectedRole}";
                
            // Update button colors
            if (strikerIcon != null)
                strikerIcon.color = selectedRole == RoleType.Striker ? selectedRoleColor : unselectedRoleColor;
                
            if (goalieIcon != null)
                goalieIcon.color = selectedRole == RoleType.Goalkeeper ? selectedRoleColor : unselectedRoleColor;
        }
        
        /// <summary>
        /// Check for role conflicts between players
        /// </summary>
        private void CheckForRoleConflict()
        {
            if (!isMultiplayer) return;
            
            // Check if multiple players selected the same role
            bool hasConflict = (teammate1Data.selectedRole == selectedRole && teammate1Data.isConnected) ||
                              (teammate2Data.selectedRole == selectedRole && teammate2Data.isConnected);
                              
            if (hasConflict && !rpsInProgress)
            {
                ShowRockPaperScissorsMiniGame();
            }
        }
        
        #endregion
        
        #region Ready State
        
        /// <summary>
        /// Handle ready button press
        /// </summary>
        public void OnReadyPressed()
        {
            if (selectedPower == null)
            {
                UnityEngine.Debug.LogWarning("Cannot ready up without selecting a power");
                return;
            }
            
            isReady = !isReady;
            UpdateReadyButton();
            UpdatePlayerStatusDisplay();
            
            PlaySound(readySound);
            
            UnityEngine.Debug.Log($"Player ready state: {isReady}");
            
            // Check if all players are ready
            CheckAllPlayersReady();
        }
        
        /// <summary>
        /// Update ready button appearance and text
        /// </summary>
        private void UpdateReadyButton()
        {
            if (readyButton == null) return;
            
            bool canReady = selectedPower != null;
            readyButton.interactable = canReady;
            
            if (readyButtonText != null)
            {
                if (!canReady)
                    readyButtonText.text = "Select Power First";
                else if (isReady)
                    readyButtonText.text = "READY!";
                else
                    readyButtonText.text = "Ready Up";
            }
            
            // Update button color
            var colors = readyButton.colors;
            colors.normalColor = isReady ? selectedRoleColor : Color.white;
            readyButton.colors = colors;
        }
        
        /// <summary>
        /// Check if all players are ready to start
        /// </summary>
        private void CheckAllPlayersReady()
        {
            if (!isMultiplayer)
            {
                // Single player - just local ready state
                allPlayersReady = isReady;
            }
            else
            {
                // Multiplayer - check all connected players
                allPlayersReady = isReady && 
                                 (!teammate1Data.isConnected || teammate1Data.isReady) &&
                                 (!teammate2Data.isConnected || teammate2Data.isReady);
            }
            
            if (allPlayersReady)
            {
                StartCoroutine(AllPlayersReadySequence());
            }
        }
        
        /// <summary>
        /// Handle when all players are ready
        /// </summary>
        private IEnumerator AllPlayersReadySequence()
        {
            PlaySound(teamReadySound);
            
            if (teamReadyEffect != null)
                teamReadyEffect.Play();
                
            // Show "Team Ready!" message
            if (titleText != null)
            {
                titleText.text = "TEAM READY!";
                titleText.color = selectedRoleColor;
            }
            
            yield return new WaitForSeconds(2f);
            
            ConfirmSelectionsAndStartMatch();
        }
        
        #endregion
        
        #region Rock Paper Scissors Mini-Game
        
        /// <summary>
        /// Show rock paper scissors mini-game for role conflicts
        /// </summary>
        public void ShowRockPaperScissorsMiniGame()
        {
            if (rpsInProgress) return;
            
            rpsInProgress = true;
            SetPanelActive(rpsPanel, true);
            
            if (rpsInstructionText != null)
                rpsInstructionText.text = "Role conflict! Rock, Paper, Scissors to decide!";
                
            if (rpsResultText != null)
                rpsResultText.text = "";
                
            // Reset choices
            localPlayerChoice = RPSChoice.None;
            remotePlayerChoice = RPSChoice.None;
            
            UnityEngine.Debug.Log("Rock Paper Scissors mini-game started for role conflict");
        }
        
        /// <summary>
        /// Handle RPS choice selection
        /// </summary>
        private void OnRPSChoice(RPSChoice choice)
        {
            localPlayerChoice = choice;
            
            // Update UI to show choice
            UpdateRPSChoiceDisplay();
            
            // In real multiplayer, this would be sent to other players
            // For now, simulate opponent choice
            if (isMultiplayer)
            {
                // TODO: Send choice to network
                SimulateOpponentRPSChoice();
            }
            else
            {
                SimulateOpponentRPSChoice();
            }
        }
        
        /// <summary>
        /// Simulate opponent RPS choice (for testing/AI)
        /// </summary>
        private void SimulateOpponentRPSChoice()
        {
            remotePlayerChoice = (RPSChoice)Random.Range(1, 4); // 1-3 for Rock, Paper, Scissors
            
            StartCoroutine(ProcessRPSResult());
        }
        
        /// <summary>
        /// Process RPS result and determine winner
        /// </summary>
        private IEnumerator ProcessRPSResult()
        {
            yield return new WaitForSeconds(1f); // Dramatic pause
            
            UpdateRPSChoiceDisplay();
            
            // Determine winner
            RPSResult result = DetermineRPSWinner(localPlayerChoice, remotePlayerChoice);
            
            yield return new WaitForSeconds(1f);
            
            // Show result
            string resultText = "";
            switch (result)
            {
                case RPSResult.LocalWin:
                    resultText = "You win! You keep your role choice.";
                    break;
                case RPSResult.RemoteWin:
                    resultText = "You lose! Switching to other role.";
                    // Switch to opposite role
                    selectedRole = selectedRole == RoleType.Striker ? RoleType.Goalkeeper : RoleType.Striker;
                    UpdateRoleSelection();
                    break;
                case RPSResult.Tie:
                    resultText = "Tie! Let's try again.";
                    // Reset for another round
                    localPlayerChoice = RPSChoice.None;
                    remotePlayerChoice = RPSChoice.None;
                    yield return new WaitForSeconds(2f);
                    if (rpsInstructionText != null)
                        rpsInstructionText.text = "Tie! Rock, Paper, Scissors again!";
                    yield break; // Don't end the mini-game yet
            }
            
            if (rpsResultText != null)
                rpsResultText.text = resultText;
                
            yield return new WaitForSeconds(3f);
            
            // End mini-game
            EndRockPaperScissorsMiniGame();
        }
        
        /// <summary>
        /// Determine RPS winner
        /// </summary>
        private RPSResult DetermineRPSWinner(RPSChoice local, RPSChoice remote)
        {
            if (local == remote) return RPSResult.Tie;
            
            if ((local == RPSChoice.Rock && remote == RPSChoice.Scissors) ||
                (local == RPSChoice.Paper && remote == RPSChoice.Rock) ||
                (local == RPSChoice.Scissors && remote == RPSChoice.Paper))
            {
                return RPSResult.LocalWin;
            }
            
            return RPSResult.RemoteWin;
        }
        
        /// <summary>
        /// Update RPS choice display
        /// </summary>
        private void UpdateRPSChoiceDisplay()
        {
            // This would show the actual choice icons
            // For now, just update text
            string localText = localPlayerChoice == RPSChoice.None ? "?" : localPlayerChoice.ToString();
            string remoteText = remotePlayerChoice == RPSChoice.None ? "?" : remotePlayerChoice.ToString();
            
            if (rpsInstructionText != null)
                rpsInstructionText.text = $"You: {localText} vs Opponent: {remoteText}";
        }
        
        /// <summary>
        /// End rock paper scissors mini-game
        /// </summary>
        private void EndRockPaperScissorsMiniGame()
        {
            rpsInProgress = false;
            SetPanelActive(rpsPanel, false);
            
            UnityEngine.Debug.Log("Rock Paper Scissors mini-game ended");
        }
        
        private enum RPSResult
        {
            LocalWin,
            RemoteWin,
            Tie
        }
        
        #endregion
        
        #region Timer
        
        /// <summary>
        /// Start selection timer
        /// </summary>
        private void StartSelectionTimer()
        {
            timeRemaining = selectionTimeLimit;
            timerActive = true;
            UpdateTimerDisplay();
        }
        
        /// <summary>
        /// Stop selection timer
        /// </summary>
        private void StopSelectionTimer()
        {
            timerActive = false;
        }
        
        /// <summary>
        /// Update timer each frame
        /// </summary>
        private void UpdateTimer()
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                HandleTimeExpired();
            }
            
            UpdateTimerDisplay();
        }
        
        /// <summary>
        /// Update timer display
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;
            
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color when time is running low
            if (timeRemaining <= 10f)
            {
                timerText.color = warningTimerColor;
                
                // Play warning sound every second in final 5 seconds
                if (timeRemaining <= 5f && Mathf.FloorToInt(timeRemaining) != Mathf.FloorToInt(timeRemaining + Time.deltaTime))
                {
                    PlaySound(warningSound);
                }
            }
            else
            {
                timerText.color = normalTimerColor;
            }
        }
        
        /// <summary>
        /// Handle when selection time expires
        /// </summary>
        private void HandleTimeExpired()
        {
            timerActive = false;
            
            // Auto-select defaults if nothing selected
            if (selectedPower == null && availablePowers.Count > 0)
            {
                OnPowerSelected(availablePowers[0]);
            }
            
            // Auto-ready up
            if (!isReady)
            {
                OnReadyPressed();
            }
            
            UnityEngine.Debug.Log("Selection time expired - auto-selected defaults");
        }
        
        #endregion
        
        #region Final Confirmation
        
        /// <summary>
        /// Confirm selections and start match
        /// </summary>
        public void ConfirmSelectionsAndStartMatch()
        {
            StopSelectionTimer();
            
            // Fire selection confirmed event
            OnSelectionConfirmed?.Invoke(selectedPower, selectedRole);
            
            // Fire all players ready event
            OnAllPlayersReady?.Invoke();
            
            UnityEngine.Debug.Log($"Match starting with Power: {selectedPower?.name}, Role: {selectedRole}");
            
            // Hide UI
            HidePowerSelection();
        }
        
        #endregion
        
        #region Player Status Display
        
        /// <summary>
        /// Update player status indicators
        /// </summary>
        private void UpdatePlayerStatusDisplay()
        {
            // Update ready indicators
            SetPanelActive(player1ReadyIndicator, teammate1Data.isReady);
            SetPanelActive(player2ReadyIndicator, teammate2Data.isReady);
            
            // Update status text
            if (teammate1StatusText != null)
            {
                if (teammate1Data.isConnected)
                    teammate1StatusText.text = $"Player 1: {(teammate1Data.isReady ? "Ready" : "Selecting...")}";
                else
                    teammate1StatusText.text = "Player 1: Disconnected";
            }
            
            if (teammate2StatusText != null)
            {
                if (teammate2Data.isConnected)
                    teammate2StatusText.text = $"Player 2: {(teammate2Data.isReady ? "Ready" : "Selecting...")}";
                else
                    teammate2StatusText.text = "Player 2: Disconnected";
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Set panel active state safely
        /// </summary>
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
        
        /// <summary>
        /// Play audio clip safely
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
        
        #endregion
        
        #region Editor/Debug
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Label("=== Power Selection Debug ===");
            GUILayout.Label($"Selected Power: {(selectedPower != null ? selectedPower.name : "None")}");
            GUILayout.Label($"Selected Role: {selectedRole}");
            GUILayout.Label($"Is Ready: {isReady}");
            GUILayout.Label($"Time Remaining: {timeRemaining:F1}s");
            GUILayout.Label($"All Players Ready: {allPlayersReady}");
            GUILayout.Label($"RPS In Progress: {rpsInProgress}");
            
            if (GUILayout.Button("Force Time Expire"))
            {
                timeRemaining = 0;
            }
            
            if (GUILayout.Button("Test Role Conflict"))
            {
                ShowRockPaperScissorsMiniGame();
            }
            
            GUILayout.EndArea();
        }
        #endif
        
        #endregion
    }
}
