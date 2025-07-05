using UnityEngine;

namespace PlushLeague.Core.Input
{
    /// <summary>
    /// Manages input routing to player controllers
    /// Bridges the gap between input providers and game logic
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Provider")]
        [SerializeField] private GameObject inputProviderPrefab;
        [SerializeField] private InputType inputType = InputType.Mobile;
        
        [Header("Player Assignment")]
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private GameObject targetPlayer;
        
        private IInputProvider inputProvider;
        private PlushLeague.Gameplay.Player.PlayerController playerController;
        private PlushLeague.Gameplay.Abilities.AbilityManager abilityManager;
        
        public enum InputType
        {
            Mobile,
            Keyboard,
            Gamepad
        }
        
        private void Start()
        {
            InitializeInputProvider();
            FindTargetPlayer();
        }
        
        private void Update()
        {
            if (inputProvider == null || playerController == null) return;
            
            // Update input provider
            inputProvider.UpdateInput();
            
            // Route input to player controller
            playerController.SetMovementInput(inputProvider.MovementInput);
            playerController.SetSprintInput(inputProvider.SprintHeld);
            
            // Handle action input (for abilities like chip kick)
            if (inputProvider.ActionPressed)
            {
                HandleActionInput();
            }
            
            // Handle superpower input (Y button)
            if (inputProvider.SuperpowerPressed)
            {
                playerController.SetSuperpowerInput(true);
            }
            else
            {
                playerController.SetSuperpowerInput(false);
            }
            
            // Route ability inputs to ability manager if available
            if (abilityManager != null)
            {
                // Handle chip kick input
                if (inputProvider.ChipKickPressed)
                {
                    abilityManager.SetChipKickInput(true);
                }
                else if (!inputProvider.ActionHeld)
                {
                    abilityManager.SetChipKickInput(false);
                }
                
                // Handle slide tackle input
                if (inputProvider.SlideTacklePressed)
                {
                    abilityManager.SetSlideTackleInput(true);
                }
                else
                {
                    abilityManager.SetSlideTackleInput(false);
                }
                
                // Handle goalie save input
                if (inputProvider.GoalieSavePressed)
                {
                    abilityManager.SetGoalieSaveInput(true);
                }
                else
                {
                    abilityManager.SetGoalieSaveInput(false);
                }
            }
        }
        
        private void OnDestroy()
        {
            if (inputProvider != null)
            {
                inputProvider.Cleanup();
            }
        }
        
        private void InitializeInputProvider()
        {
            switch (inputType)
            {
                case InputType.Mobile:
                    InitializeMobileInput();
                    break;
                case InputType.Keyboard:
                    InitializeKeyboardInput();
                    break;
                case InputType.Gamepad:
                    InitializeGamepadInput();
                    break;
            }
            
            inputProvider?.Initialize();
        }
        
        private void InitializeMobileInput()
        {
            if (inputProviderPrefab != null)
            {
                GameObject inputObj = Instantiate(inputProviderPrefab, transform);
                inputProvider = inputObj.GetComponent<IInputProvider>();
            }
            else
            {
                // Try to find existing mobile input in scene
                var mobileInput = Object.FindFirstObjectByType<MobileInput>();
                if (mobileInput != null)
                {
                    inputProvider = mobileInput;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("InputManager: No mobile input provider found!");
                }
            }
        }
        
        private void InitializeKeyboardInput()
        {
            // TODO: Implement keyboard input provider
            UnityEngine.Debug.LogWarning("Keyboard input not yet implemented");
        }
        
        private void InitializeGamepadInput()
        {
            // TODO: Implement gamepad input provider
            UnityEngine.Debug.LogWarning("Gamepad input not yet implemented");
        }
        
        private void FindTargetPlayer()
        {
            if (autoFindPlayer)
            {
                // Try to find player controller in scene
                var player = Object.FindFirstObjectByType<PlushLeague.Gameplay.Player.PlayerController>();
                if (player != null)
                {
                    playerController = player;
                    targetPlayer = player.gameObject;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("InputManager: No PlayerController found in scene!");
                }
            }
            else if (targetPlayer != null)
            {
                playerController = targetPlayer.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
                if (playerController == null)
                {
                    UnityEngine.Debug.LogWarning("InputManager: Target player doesn't have PlayerController component!");
                }
                else
                {
                    // Also get ability manager if available
                    abilityManager = targetPlayer.GetComponent<PlushLeague.Gameplay.Abilities.AbilityManager>();
                }
            }
        }
        
        private void HandleActionInput()
        {
            // This will be expanded later for abilities, tackles, etc.
            UnityEngine.Debug.Log("Action input received!");
        }
        
        /// <summary>
        /// Manually assign a player to receive input
        /// </summary>
        /// <param name="player">Player GameObject with PlayerController</param>
        public void AssignPlayer(GameObject player)
        {
            targetPlayer = player;
            playerController = player?.GetComponent<PlushLeague.Gameplay.Player.PlayerController>();
        }
        
        /// <summary>
        /// Switch input type at runtime
        /// </summary>
        /// <param name="newInputType">New input type to use</param>
        public void SwitchInputType(InputType newInputType)
        {
            inputType = newInputType;
            
            // Cleanup current provider
            if (inputProvider != null)
            {
                inputProvider.Cleanup();
            }
            
            // Initialize new provider
            InitializeInputProvider();
        }
        
        /// <summary>
        /// Get current input state for debugging
        /// </summary>
        public Vector2 GetMovementInput()
        {
            return inputProvider?.MovementInput ?? Vector2.zero;
        }
        
        /// <summary>
        /// Get current sprint state for debugging
        /// </summary>
        public bool GetSprintState()
        {
            return inputProvider?.SprintHeld ?? false;
        }
    }
}
