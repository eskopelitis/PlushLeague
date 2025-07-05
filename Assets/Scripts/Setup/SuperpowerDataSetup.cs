using UnityEngine;

namespace PlushLeague.Setup
{
    /// <summary>
    /// Helper script to create sample superpower data for testing the Power Selection system.
    /// This can be used to quickly populate the system with test powers.
    /// </summary>
    public class SuperpowerDataSetup : MonoBehaviour
    {
        [Header("Sample Superpower Creation")]
        [SerializeField] private bool createSamplePowersOnStart = false;
        [SerializeField] private string resourceFolderPath = "Assets/Resources/Superpowers";
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (createSamplePowersOnStart)
            {
                CreateSampleSuperpowers();
            }
        }
        
        #endregion
        
        #region Sample Creation
        
        /// <summary>
        /// Create sample superpower data assets for testing
        /// </summary>
        [ContextMenu("Create Sample Superpowers")]
        public void CreateSampleSuperpowers()
        {
            UnityEngine.Debug.Log("Creating sample superpower data...");
            
            CreateSuperpowerData("Super Shot", "Powerful shot that breaks through defenses", 30f, true);
            CreateSuperpowerData("Freeze Zone", "Creates a frozen area that slows enemies", 45f, false);
            CreateSuperpowerData("Ultra Dash", "Lightning-fast movement boost", 25f, true);
            CreateSuperpowerData("Super Save", "Enhanced goalie reflexes and reach", 40f, false);
            CreateSuperpowerData("Curve Boost", "Ball curves dramatically around obstacles", 35f, true);
            CreateSuperpowerData("Freeze Shot", "Ball freezes in mid-air, then continues", 50f, true);
            
            UnityEngine.Debug.Log("Sample superpowers created!");
        }
        
        /// <summary>
        /// Create a single superpower data asset
        /// </summary>
        private void CreateSuperpowerData(string powerName, string description, float cooldown, bool isOffensive)
        {
            #if UNITY_EDITOR
            // Create the ScriptableObject
            var powerData = ScriptableObject.CreateInstance<PlushLeague.Gameplay.Superpowers.SuperpowerData>();
            
            // Set basic properties
            powerData.displayName = powerName;
            powerData.description = description;
            powerData.cooldownDuration = cooldown;
            
            // Create the asset
            string assetPath = $"{resourceFolderPath}/{powerName.Replace(" ", "")}.asset";
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists(resourceFolderPath))
            {
                System.IO.Directory.CreateDirectory(resourceFolderPath);
            }
            
            UnityEditor.AssetDatabase.CreateAsset(powerData, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            
            UnityEngine.Debug.Log($"Created superpower: {powerName} at {assetPath}");
            #else
            UnityEngine.Debug.LogWarning("Superpower creation only available in Editor");
            #endif
        }
        
        #endregion
        
        #region Runtime Testing
        
        /// <summary>
        /// Load and test sample superpowers at runtime
        /// </summary>
        [ContextMenu("Test Load Superpowers")]
        public void TestLoadSuperpowers()
        {
            var powers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            
            UnityEngine.Debug.Log($"Found {powers.Length} superpowers:");
            
            foreach (var power in powers)
            {
                UnityEngine.Debug.Log($"- {power.displayName}: {power.description} (Cooldown: {power.cooldownTime}s)");
            }
        }
        
        /// <summary>
        /// Test power selection system with loaded powers
        /// </summary>
        [ContextMenu("Test Power Selection System")]
        public void TestPowerSelectionSystem()
        {
            var powerSelectionManager = FindFirstObjectByType<PlushLeague.UI.PowerSelection.PowerSelectionManager>();
            
            if (powerSelectionManager != null)
            {
                var powers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
                
                if (powers.Length > 0)
                {
                    UnityEngine.Debug.Log($"Starting power selection with {powers.Length} available powers");
                    powerSelectionManager.StartPowerSelection(false);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("No superpowers found! Create some sample powers first.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("PowerSelectionManager not found in scene!");
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validate that all necessary superpower components exist
        /// </summary>
        [ContextMenu("Validate Superpower System")]
        public void ValidateSuperpowerSystem()
        {
            UnityEngine.Debug.Log("=== Validating Superpower System ===");
            
            // Check for SuperpowerData assets
            var powers = Resources.LoadAll<PlushLeague.Gameplay.Superpowers.SuperpowerData>("Superpowers");
            if (powers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("⚠ No SuperpowerData assets found in Resources/Superpowers");
            }
            else
            {
                UnityEngine.Debug.Log($"✓ Found {powers.Length} SuperpowerData assets");
            }
            
            // Check for PowerupController components
            var powerupControllers = FindObjectsByType<PlushLeague.Gameplay.Superpowers.PowerupController>(FindObjectsSortMode.None);
            if (powerupControllers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("⚠ No PowerupController components found in scene");
            }
            else
            {
                UnityEngine.Debug.Log($"✓ Found {powerupControllers.Length} PowerupController components");
            }
            
            // Check for PlayerController components
            var playerControllers = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            if (playerControllers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("⚠ No PlayerController components found in scene");
            }
            else
            {
                UnityEngine.Debug.Log($"✓ Found {playerControllers.Length} PlayerController components");
            }
            
            // Check that PlayerControllers have PowerupControllers
            int playersWithPowerups = 0;
            foreach (var player in playerControllers)
            {
                if (player.GetComponent<PlushLeague.Gameplay.Superpowers.PowerupController>() != null)
                {
                    playersWithPowerups++;
                }
            }
            
            if (playersWithPowerups != playerControllers.Length)
            {
                UnityEngine.Debug.LogWarning($"⚠ Only {playersWithPowerups}/{playerControllers.Length} players have PowerupController components");
            }
            else if (playerControllers.Length > 0)
            {
                UnityEngine.Debug.Log($"✓ All {playerControllers.Length} players have PowerupController components");
            }
            
            UnityEngine.Debug.Log("=== Superpower System Validation Complete ===");
        }
        
        #endregion
    }
}
