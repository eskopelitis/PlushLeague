using UnityEngine;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Demo system to test superpower functionality
    /// </summary>
    public class SuperpowerSystemDemo : MonoBehaviour
    {
        [Header("Test Players")]
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController[] testPlayers;
        
        [Header("Test Superpowers")]
        [SerializeField] private SuperShotPowerup superShotPowerup;
        [SerializeField] private FreezeShotPowerup freezeShotPowerup;
        [SerializeField] private CurveBoostPowerup curveBoostPowerup;
        [SerializeField] private SuperSavePowerup superSavePowerup;
        [SerializeField] private UltraDashPowerup ultraDashPowerup;
        [SerializeField] private FreezeZonePowerup freezeZonePowerup;
        
        [Header("Demo Settings")]
        [SerializeField] private bool autoAssignPowers = true;
        [SerializeField] private bool showDebugInfo = true;
        
        private void Start()
        {
            if (autoAssignPowers)
            {
                AssignSuperpowersToPlayers();
            }
        }
        
        private void AssignSuperpowersToPlayers()
        {
            if (testPlayers == null || testPlayers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("SuperpowerSystemDemo: No test players assigned!");
                return;
            }
            
            BasePowerup[] allPowerups = {
                superShotPowerup,
                freezeShotPowerup,
                curveBoostPowerup,
                superSavePowerup,
                ultraDashPowerup,
                freezeZonePowerup
            };
            
            for (int i = 0; i < testPlayers.Length; i++)
            {
                var player = testPlayers[i];
                if (player == null) continue;
                
                // Add PowerupController if not present
                var powerupController = player.GetComponent<PowerupController>();
                if (powerupController == null)
                {
                    powerupController = player.gameObject.AddComponent<PowerupController>();
                }
                
                // Assign a superpower (cycle through available ones)
                var powerupIndex = i % allPowerups.Length;
                var selectedPowerup = allPowerups[powerupIndex];
                
                if (selectedPowerup != null)
                {
                    powerupController.SetPowerup(selectedPowerup);
                    
                    if (showDebugInfo)
                    {
                        UnityEngine.Debug.Log($"Assigned {selectedPowerup.GetDisplayName()} to player {player.name}");
                    }
                }
            }
        }
        
        [ContextMenu("Test Super Shot")]
        private void TestSuperShot()
        {
            if (testPlayers.Length > 0 && superShotPowerup != null)
            {
                var player = testPlayers[0];
                var powerupController = player.GetComponent<PowerupController>();
                if (powerupController != null)
                {
                    powerupController.SetPowerup(superShotPowerup);
                    powerupController.TryActivatePowerup();
                }
            }
        }
        
        [ContextMenu("Test Freeze Zone")]
        private void TestFreezeZone()
        {
            if (testPlayers.Length > 0 && freezeZonePowerup != null)
            {
                var player = testPlayers[0];
                var powerupController = player.GetComponent<PowerupController>();
                if (powerupController != null)
                {
                    powerupController.SetPowerup(freezeZonePowerup);
                    powerupController.TryActivatePowerup();
                }
            }
        }
        
        [ContextMenu("Test Ultra Dash")]
        private void TestUltraDash()
        {
            if (testPlayers.Length > 0 && ultraDashPowerup != null)
            {
                var player = testPlayers[0];
                var powerupController = player.GetComponent<PowerupController>();
                if (powerupController != null)
                {
                    powerupController.SetPowerup(ultraDashPowerup);
                    powerupController.TryActivatePowerup();
                }
            }
        }
        
        private void Update()
        {
            // Debug key bindings for testing
            if (showDebugInfo)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
                {
                    TestSuperShot();
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
                {
                    TestFreezeZone();
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
                {
                    TestUltraDash();
                }
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUI.Label(new Rect(10, 10, 300, 20), "Superpower System Demo");
            GUI.Label(new Rect(10, 30, 300, 20), "Press 1: Super Shot");
            GUI.Label(new Rect(10, 50, 300, 20), "Press 2: Freeze Zone");
            GUI.Label(new Rect(10, 70, 300, 20), "Press 3: Ultra Dash");
            
            // Show powerup states for each player
            int yOffset = 100;
            foreach (var player in testPlayers)
            {
                if (player == null) continue;
                
                var powerupController = player.GetComponent<PowerupController>();
                if (powerupController != null)
                {
                    string status = $"{player.name}: Ready={powerupController.CanActivatePowerup()}, " +
                                  $"Cooldown={powerupController.GetRemainingCooldown():F1}s";
                    GUI.Label(new Rect(10, yOffset, 400, 20), status);
                    yOffset += 20;
                }
            }
        }
    }
}
