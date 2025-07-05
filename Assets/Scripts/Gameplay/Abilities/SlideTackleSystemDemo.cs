using UnityEngine;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Test and setup script to demonstrate slide tackle system integration
    /// Shows how to properly configure slide tackle with input, UI, and abilities
    /// </summary>
    public class SlideTackleSystemDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController player;
        [SerializeField] private PlushLeague.Gameplay.Abilities.AbilityManager abilityManager;
        [SerializeField] private PlushLeague.Core.Input.InputManager inputManager;
        [SerializeField] private PlushLeague.UI.HUD.SlideTackleUI slideTackleUI;
        
        [Header("Abilities")]
        [SerializeField] private BaseAbility slideTackleAbility;
        
        [Header("Test Settings")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private KeyCode testSlideTackleKey = KeyCode.B;
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupSlideTackleSystem();
            }
        }
        
        private void Update()
        {
            // Manual test input
            if (Input.GetKeyDown(testSlideTackleKey))
            {
                TestSlideTackle();
            }
        }
        
        /// <summary>
        /// Automatically setup the complete slide tackle system
        /// </summary>
        [ContextMenu("Setup Slide Tackle System")]
        public void SetupSlideTackleSystem()
        {
            UnityEngine.Debug.Log("Setting up Slide Tackle System...");
            
            // Find components if not assigned
            if (player == null)
            {
                player = Object.FindFirstObjectByType<PlushLeague.Gameplay.Player.PlayerController>();
            }
            
            if (abilityManager == null)
            {
                abilityManager = player?.GetComponent<PlushLeague.Gameplay.Abilities.AbilityManager>();
            }
            
            if (inputManager == null)
            {
                inputManager = Object.FindFirstObjectByType<PlushLeague.Core.Input.InputManager>();
            }
            
            if (slideTackleUI == null)
            {
                slideTackleUI = Object.FindFirstObjectByType<PlushLeague.UI.HUD.SlideTackleUI>();
            }
            
            // Setup slide tackle ability if not assigned
            if (slideTackleAbility != null && abilityManager != null)
            {
                abilityManager.AddAbility(slideTackleAbility);
                UnityEngine.Debug.Log("Added slide tackle ability to player");
            }
            
            // Connect UI to ability manager
            if (slideTackleUI != null && abilityManager != null)
            {
                slideTackleUI.SetAbilityManager(abilityManager);
                UnityEngine.Debug.Log("Connected slide tackle UI to ability manager");
            }
            
            // Verify setup
            VerifySetup();
        }
        
        /// <summary>
        /// Test slide tackle ability programmatically
        /// </summary>
        [ContextMenu("Test Slide Tackle")]
        public void TestSlideTackle()
        {
            if (abilityManager == null)
            {
                UnityEngine.Debug.LogWarning("No ability manager found for testing");
                return;
            }
            
            // Try to use slide tackle ability
            bool success = abilityManager.UseAbility<SlideTackle>();
            
            if (success)
            {
                UnityEngine.Debug.Log("Test slide tackle successful!");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Test slide tackle failed - check cooldown or conditions");
            }
        }
        
        /// <summary>
        /// Verify that all components are properly connected
        /// </summary>
        private void VerifySetup()
        {
            UnityEngine.Debug.Log("=== Slide Tackle System Verification ===");
            
            bool allGood = true;
            
            if (player == null)
            {
                UnityEngine.Debug.LogError("✗ Player Controller not found");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Player Controller found");
            }
            
            if (abilityManager == null)
            {
                UnityEngine.Debug.LogError("✗ Ability Manager not found");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Ability Manager found");
                
                // Check if slide tackle ability is available
                bool hasSlideAbility = abilityManager.CanUseAbility<SlideTackle>();
                if (hasSlideAbility)
                {
                    UnityEngine.Debug.Log("✓ Slide Tackle ability available");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("⚠ Slide Tackle ability not found or on cooldown");
                }
            }
            
            if (inputManager == null)
            {
                UnityEngine.Debug.LogWarning("⚠ Input Manager not found (input may not work)");
            }
            else
            {
                UnityEngine.Debug.Log("✓ Input Manager found");
            }
            
            if (slideTackleUI == null)
            {
                UnityEngine.Debug.LogWarning("⚠ Slide Tackle UI not found (no UI feedback)");
            }
            else
            {
                UnityEngine.Debug.Log("✓ Slide Tackle UI found");
            }
            
            if (allGood)
            {
                UnityEngine.Debug.Log("✓ Slide Tackle System setup complete!");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Slide Tackle System has issues - check errors above");
            }
        }
        
        /// <summary>
        /// Reset all ability cooldowns for testing
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetCooldowns()
        {
            if (abilityManager != null)
            {
                abilityManager.ResetAllCooldowns();
                UnityEngine.Debug.Log("All ability cooldowns reset");
            }
        }
        
        /// <summary>
        /// Create a default slide tackle ability if none exists
        /// </summary>
        [ContextMenu("Create Default Slide Tackle Ability")]
        public void CreateDefaultSlideTackleAbility()
        {
            if (slideTackleAbility == null)
            {
                // This would typically be created as a ScriptableObject asset
                // For demo purposes, we'll note that it should be created in the asset menu
                UnityEngine.Debug.Log("Create a SlideTackle asset via: Assets > Create > Plush League > Abilities > Slide Tackle");
            }
        }
        
        #region Debug GUI
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 450, 300, 200));
            GUILayout.Label("=== Slide Tackle Demo ===");
            
            if (GUILayout.Button("Test Slide Tackle"))
            {
                TestSlideTackle();
            }
            
            if (GUILayout.Button("Reset Cooldowns"))
            {
                ResetCooldowns();
            }
            
            if (GUILayout.Button("Setup System"))
            {
                SetupSlideTackleSystem();
            }
            
            GUILayout.Label($"Test Key: {testSlideTackleKey}");
            GUILayout.Label($"Player: {(player != null ? player.name : "None")}");
            GUILayout.Label($"Ability Manager: {(abilityManager != null ? "Found" : "None")}");
            GUILayout.Label($"UI: {(slideTackleUI != null ? "Found" : "None")}");
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
