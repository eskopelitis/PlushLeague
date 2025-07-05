using UnityEngine;

namespace PlushLeague.Gameplay.Goal
{
    /// <summary>
    /// Demo and setup script for the complete goalie save system
    /// Shows integration of shot detection, timing UI, save manager, and abilities
    /// </summary>
    public class GoalieSaveSystemDemo : MonoBehaviour
    {
        [Header("System Components")]
        [SerializeField] private ShotDetector shotDetector;
        [SerializeField] private GoalieSaveManager saveManager;
        [SerializeField] private PlushLeague.UI.HUD.GoalieTimingBar timingBar;
        [SerializeField] private PlushLeague.Gameplay.Ball.BallController ballController;
        
        [Header("Test Settings")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private Transform goalPosition;
        [SerializeField] private Transform testShotPosition;
        [SerializeField] private Vector2 testShotVelocity = new Vector2(0, 8);
        [SerializeField] private float testSaveWindow = 0.5f;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode testShotKey = KeyCode.T;
        [SerializeField] private KeyCode testSaveKey = KeyCode.G;
        [SerializeField] private KeyCode resetBallKey = KeyCode.R;
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupGoalieSaveSystem();
            }
        }
        
        private void Update()
        {
            HandleTestInput();
        }
        
        /// <summary>
        /// Setup the complete goalie save system
        /// </summary>
        [ContextMenu("Setup Goalie Save System")]
        public void SetupGoalieSaveSystem()
        {
            UnityEngine.Debug.Log("Setting up Goalie Save System...");
            
            // Find components if not assigned
            if (shotDetector == null)
            {
                shotDetector = Object.FindFirstObjectByType<ShotDetector>();
            }
            
            if (saveManager == null)
            {
                saveManager = Object.FindFirstObjectByType<GoalieSaveManager>();
            }
            
            if (timingBar == null)
            {
                timingBar = Object.FindFirstObjectByType<PlushLeague.UI.HUD.GoalieTimingBar>();
            }
            
            if (ballController == null)
            {
                var ballManager = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
                if (ballManager != null)
                {
                    ballController = ballManager.CurrentBall;
                }
                else
                {
                    ballController = Object.FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
                }
            }
            
            // Setup goal position if not set
            if (goalPosition == null && shotDetector != null)
            {
                goalPosition = shotDetector.transform;
            }
            
            // Setup test shot position
            if (testShotPosition == null && goalPosition != null)
            {
                var testObj = new GameObject("Test Shot Position");
                testObj.transform.position = goalPosition.position + Vector3.down * 5f;
                testShotPosition = testObj.transform;
            }
            
            VerifySetup();
        }
        
        /// <summary>
        /// Handle test input
        /// </summary>
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(testShotKey))
            {
                TestShot();
            }
            
            if (Input.GetKeyDown(testSaveKey))
            {
                TestSaveOpportunity();
            }
            
            if (Input.GetKeyDown(resetBallKey))
            {
                ResetBall();
            }
        }
        
        /// <summary>
        /// Test shot on goal
        /// </summary>
        [ContextMenu("Test Shot")]
        public void TestShot()
        {
            if (ballController == null || testShotPosition == null)
            {
                UnityEngine.Debug.LogWarning("Cannot test shot - missing ball controller or shot position");
                return;
            }
            
            // Position ball at test position
            ballController.SetPosition(testShotPosition.position, true);
            
            // Apply shot velocity
            ballController.ApplyForce(testShotVelocity, ForceMode2D.Impulse);
            
            UnityEngine.Debug.Log($"Test shot fired from {testShotPosition.position} with velocity {testShotVelocity}");
        }
        
        /// <summary>
        /// Test save opportunity directly
        /// </summary>
        [ContextMenu("Test Save Opportunity")]
        public void TestSaveOpportunity()
        {
            if (saveManager == null)
            {
                UnityEngine.Debug.LogWarning("Cannot test save - no save manager found");
                return;
            }
            
            Vector3 ballPos = ballController != null ? ballController.Position : testShotPosition.position;
            saveManager.TriggerSaveOpportunity(ballPos, testShotVelocity, testSaveWindow);
            
            UnityEngine.Debug.Log("Test save opportunity triggered");
        }
        
        /// <summary>
        /// Reset ball to test position
        /// </summary>
        [ContextMenu("Reset Ball")]
        public void ResetBall()
        {
            if (ballController == null || testShotPosition == null)
            {
                UnityEngine.Debug.LogWarning("Cannot reset ball - missing components");
                return;
            }
            
            ballController.SetPosition(testShotPosition.position, true);
            UnityEngine.Debug.Log("Ball reset to test position");
        }
        
        /// <summary>
        /// Verify system setup
        /// </summary>
        private void VerifySetup()
        {
            UnityEngine.Debug.Log("=== Goalie Save System Verification ===");
            
            bool allGood = true;
            
            if (shotDetector == null)
            {
                UnityEngine.Debug.LogError("✗ Shot Detector not found");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Shot Detector found");
            }
            
            if (saveManager == null)
            {
                UnityEngine.Debug.LogError("✗ Save Manager not found");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Save Manager found");
            }
            
            if (timingBar == null)
            {
                UnityEngine.Debug.LogWarning("⚠ Timing Bar UI not found (create UI prefab)");
            }
            else
            {
                UnityEngine.Debug.Log("✓ Timing Bar UI found");
            }
            
            if (ballController == null)
            {
                UnityEngine.Debug.LogError("✗ Ball Controller not found");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("✓ Ball Controller found");
            }
            
            if (goalPosition == null)
            {
                UnityEngine.Debug.LogWarning("⚠ Goal position not set");
            }
            else
            {
                UnityEngine.Debug.Log("✓ Goal position set");
            }
            
            if (allGood)
            {
                UnityEngine.Debug.Log("✓ Goalie Save System setup complete!");
                UnityEngine.Debug.Log($"Test Controls: Shot={testShotKey}, Save={testSaveKey}, Reset={resetBallKey}");
            }
            else
            {
                UnityEngine.Debug.LogError("✗ Goalie Save System has issues - check errors above");
            }
        }
        
        /// <summary>
        /// Create missing components
        /// </summary>
        [ContextMenu("Create Missing Components")]
        public void CreateMissingComponents()
        {
            if (shotDetector == null)
            {
                var shotDetectorObj = new GameObject("Shot Detector");
                if (goalPosition != null)
                {
                    shotDetectorObj.transform.position = goalPosition.position;
                }
                shotDetector = shotDetectorObj.AddComponent<ShotDetector>();
                UnityEngine.Debug.Log("Created Shot Detector");
            }
            
            if (saveManager == null)
            {
                var saveManagerObj = new GameObject("Goalie Save Manager");
                saveManager = saveManagerObj.AddComponent<GoalieSaveManager>();
                UnityEngine.Debug.Log("Created Save Manager");
            }
        }
        
        /// <summary>
        /// Test perfect save result
        /// </summary>
        [ContextMenu("Test Perfect Save")]
        public void TestPerfectSave()
        {
            if (saveManager != null)
            {
                // This would normally be called by the timing bar
                var method = saveManager.GetType().GetMethod("HandlePerfectSave", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(saveManager, null);
                UnityEngine.Debug.Log("Tested perfect save");
            }
        }
        
        /// <summary>
        /// Test block save result
        /// </summary>
        [ContextMenu("Test Block Save")]
        public void TestBlockSave()
        {
            if (saveManager != null)
            {
                var method = saveManager.GetType().GetMethod("HandleGoodSave", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(saveManager, null);
                UnityEngine.Debug.Log("Tested block save");
            }
        }
        
        #region Debug GUI
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 650, 350, 200));
            GUILayout.Label("=== Goalie Save System Demo ===");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Test Shot ({testShotKey})"))
            {
                TestShot();
            }
            if (GUILayout.Button($"Test Save ({testSaveKey})"))
            {
                TestSaveOpportunity();
            }
            if (GUILayout.Button($"Reset Ball ({resetBallKey})"))
            {
                ResetBall();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Perfect Save"))
            {
                TestPerfectSave();
            }
            if (GUILayout.Button("Block Save"))
            {
                TestBlockSave();
            }
            if (GUILayout.Button("Setup System"))
            {
                SetupGoalieSaveSystem();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("System Status:");
            GUILayout.Label($"Shot Detector: {(shotDetector != null ? "✓" : "✗")}");
            GUILayout.Label($"Save Manager: {(saveManager != null ? "✓" : "✗")}");
            GUILayout.Label($"Timing Bar: {(timingBar != null ? "✓" : "✗")}");
            GUILayout.Label($"Ball Controller: {(ballController != null ? "✓" : "✗")}");
            
            if (saveManager != null)
            {
                GUILayout.Label($"Save In Progress: {saveManager.IsSaveInProgress}");
                GUILayout.Label($"Current Goalie: {(saveManager.CurrentGoalie?.name ?? "None")}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
