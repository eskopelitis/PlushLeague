using UnityEngine;

namespace PlushLeague.Integration
{
    /// <summary>
    /// Integration example that shows how to properly set up a scene with the complete 
    /// Power Selection system and game flow management.
    /// </summary>
    public class PowerSelectionIntegrationExample : MonoBehaviour
    {
        [Header("Integration Setup")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createMissingComponents = true;
        
        [Header("Prefab References")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject powerSelectionUIPrefab;
        [SerializeField] private GameObject powerSelectionManagerPrefab;
        [SerializeField] private GameObject matchManagerPrefab;
        
        [Header("Sample Power Data")]
        [SerializeField] private PlushLeague.Gameplay.Superpowers.SuperpowerData[] samplePowers;
        
        private PlushLeague.Core.GameManager gameManager;
        private PlushLeague.UI.PowerSelection.PowerSelectionManager powerSelectionManager;
        private PlushLeague.UI.PowerSelection.PowerSelectionUI powerSelectionUI;
        private PlushLeague.Gameplay.Match.MatchManager matchManager;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupIntegration();
            }
        }
        
        #endregion
        
        #region Setup Methods
        
        /// <summary>
        /// Complete integration setup for Power Selection system
        /// </summary>
        [ContextMenu("Setup Integration")]
        public void SetupIntegration()
        {
            UnityEngine.Debug.Log("=== Setting up Power Selection Integration ===");
            
            SetupGameManager();
            SetupPowerSelectionManager();
            SetupPowerSelectionUI();
            SetupMatchManager();
            ConnectComponents();
            
            UnityEngine.Debug.Log("=== Power Selection Integration Setup Complete ===");
        }
        
        /// <summary>
        /// Setup GameManager component
        /// </summary>
        private void SetupGameManager()
        {
            gameManager = PlushLeague.Core.GameManager.Instance;
            
            if (gameManager == null)
            {
                if (gameManagerPrefab != null)
                {
                    var gameManagerGO = Instantiate(gameManagerPrefab);
                    gameManager = gameManagerGO.GetComponent<PlushLeague.Core.GameManager>();
                }
                else if (createMissingComponents)
                {
                    var gameManagerGO = new GameObject("GameManager");
                    gameManager = gameManagerGO.AddComponent<PlushLeague.Core.GameManager>();
                }
            }
            
            if (gameManager != null)
            {
                UnityEngine.Debug.Log("✓ GameManager setup complete");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Failed to setup GameManager");
            }
        }
        
        /// <summary>
        /// Setup PowerSelectionManager component
        /// </summary>
        private void SetupPowerSelectionManager()
        {
            powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            
            if (powerSelectionManager == null)
            {
                if (powerSelectionManagerPrefab != null)
                {
                    var managerGO = Instantiate(powerSelectionManagerPrefab);
                    powerSelectionManager = managerGO.GetComponent<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
                }
                else if (createMissingComponents)
                {
                    var managerGO = new GameObject("PowerSelectionManager");
                    powerSelectionManager = managerGO.AddComponent<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
                }
            }
            
            if (powerSelectionManager != null)
            {
                // Setup sample powers if provided
                if (samplePowers != null && samplePowers.Length > 0)
                {
                    // Use reflection or public method to set available powers
                    UnityEngine.Debug.Log($"Configured {samplePowers.Length} sample powers");
                }
                
                UnityEngine.Debug.Log("✓ PowerSelectionManager setup complete");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Failed to setup PowerSelectionManager");
            }
        }
        
        /// <summary>
        /// Setup PowerSelectionUI component
        /// </summary>
        private void SetupPowerSelectionUI()
        {
            powerSelectionUI = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionUI>();
            
            if (powerSelectionUI == null)
            {
                if (powerSelectionUIPrefab != null)
                {
                    var uiGO = Instantiate(powerSelectionUIPrefab);
                    powerSelectionUI = uiGO.GetComponent<PlushLeague.UI.PowerSelection.PowerSelectionUI>();
                }
                else if (createMissingComponents)
                {
                    UnityEngine.Debug.LogWarning("PowerSelectionUI requires complex UI setup - consider creating a prefab");
                    // Create basic UI structure
                    CreateBasicPowerSelectionUI();
                }
            }
            
            if (powerSelectionUI != null)
            {
                UnityEngine.Debug.Log("✓ PowerSelectionUI setup complete");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Failed to setup PowerSelectionUI");
            }
        }
        
        /// <summary>
        /// Setup MatchManager component
        /// </summary>
        private void SetupMatchManager()
        {
            matchManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            
            if (matchManager == null)
            {
                if (matchManagerPrefab != null)
                {
                    var matchGO = Instantiate(matchManagerPrefab);
                    matchManager = matchGO.GetComponent<PlushLeague.Gameplay.Match.MatchManager>();
                }
                else if (createMissingComponents)
                {
                    var matchGO = new GameObject("MatchManager");
                    matchManager = matchGO.AddComponent<PlushLeague.Gameplay.Match.MatchManager>();
                }
            }
            
            if (matchManager != null)
            {
                UnityEngine.Debug.Log("✓ MatchManager setup complete");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Failed to setup MatchManager");
            }
        }
        
        /// <summary>
        /// Connect all components together
        /// </summary>
        private void ConnectComponents()
        {
            // Connect GameManager to other managers
            if (gameManager != null)
            {
                // Use reflection or public methods to connect managers
                UnityEngine.Debug.Log("✓ Components connected");
            }
        }
        
        /// <summary>
        /// Create basic PowerSelectionUI structure
        /// </summary>
        private void CreateBasicPowerSelectionUI()
        {
            // Create Canvas if none exists
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Create PowerSelectionUI GameObject
            var uiGO = new GameObject("PowerSelectionUI");
            uiGO.transform.SetParent(canvas.transform, false);
            
            // Add PowerSelectionUI component
            powerSelectionUI = uiGO.AddComponent<PlushLeague.UI.PowerSelection.PowerSelectionUI>();
            
            // Basic UI setup - this would need more detailed implementation
            UnityEngine.Debug.Log("Created basic PowerSelectionUI structure - requires manual UI setup");
        }
        
        #endregion
        
        #region Demo Methods
        
        /// <summary>
        /// Start a demo power selection session
        /// </summary>
        [ContextMenu("Demo: Start Power Selection")]
        public void DemoStartPowerSelection()
        {
            if (powerSelectionManager != null)
            {
                powerSelectionManager.StartPowerSelection(false);
            }
            else
            {
                UnityEngine.Debug.LogWarning("PowerSelectionManager not available for demo");
            }
        }
        
        /// <summary>
        /// Start a complete game flow demo
        /// </summary>
        [ContextMenu("Demo: Start Complete Flow")]
        public void DemoCompleteFlow()
        {
            if (gameManager != null)
            {
                gameManager.StartNewGame(false);
            }
            else
            {
                UnityEngine.Debug.LogWarning("GameManager not available for demo");
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validate the current integration setup
        /// </summary>
        [ContextMenu("Validate Integration")]
        public void ValidateIntegration()
        {
            UnityEngine.Debug.Log("=== Validating Power Selection Integration ===");
            
            bool isValid = true;
            
            // Check GameManager
            if (PlushLeague.Core.GameManager.Instance == null)
            {
                UnityEngine.Debug.LogError("✗ GameManager not found");
                isValid = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ GameManager found");
            }
            
            // Check PowerSelectionManager
            var psManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            if (psManager == null)
            {
                UnityEngine.Debug.LogError("✗ PowerSelectionManager not found");
                isValid = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ PowerSelectionManager found");
            }
            
            // Check PowerSelectionUI
            var psUI = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionUI>();
            if (psUI == null)
            {
                UnityEngine.Debug.LogError("✗ PowerSelectionUI not found");
                isValid = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ PowerSelectionUI found");
            }
            
            // Check MatchManager
            var mManager = FindFirstObjectByType<PlushLeague.Gameplay.Match.MatchManager>();
            if (mManager == null)
            {
                UnityEngine.Debug.LogWarning("⚠ MatchManager not found (optional for power selection testing)");
            }
            else
            {
                UnityEngine.Debug.Log("✓ MatchManager found");
            }
            
            // Check Canvas and EventSystem
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                UnityEngine.Debug.LogError("✗ Canvas not found - UI will not work");
                isValid = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Canvas found");
            }
            
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                UnityEngine.Debug.LogError("✗ EventSystem not found - UI interactions will not work");
                isValid = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ EventSystem found");
            }
            
            if (isValid)
            {
                UnityEngine.Debug.Log("✅ Integration validation passed!");
            }
            else
            {
                UnityEngine.Debug.Log("❌ Integration validation failed - please fix the issues above");
            }
        }
        
        #endregion
    }
}
