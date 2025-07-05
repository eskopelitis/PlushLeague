using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Examples
{
    /// <summary>
    /// Comprehensive demonstration of all polish systems including Audio, VFX, Animation, and UI Theme.
    /// This script showcases the complete Step 12 implementation with interactive testing.
    /// </summary>
    public class PolishSystemDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoStartDemo = false;
        [SerializeField] private float demoStepDelay = 3f;
        [SerializeField] private bool enableDebugGUI = true;
        [SerializeField] private bool loopDemo = false;
        
        [Header("Test Objects")]
        [SerializeField] private GameObject testPlayer1;
        [SerializeField] private GameObject testPlayer2;
        [SerializeField] private GameObject testBall;
        [SerializeField] private Transform goalPosition;
        [SerializeField] private Transform[] testPositions;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.Audio.AudioManager audioManager;
        [SerializeField] private PlushLeague.VFX.VFXManager vfxManager;
        [SerializeField] private PlushLeague.UI.Theme.UIThemeManager uiThemeManager;
        [SerializeField] private PlushLeague.Polish.PolishIntegrationManager polishManager;
        
        // Demo state
        private bool demoRunning = false;
        private int currentDemoStep = 0;
        private List<string> demoLog = new List<string>();
        
        // Test scenarios
        private enum DemoScenario
        {
            AudioDemo,
            VFXDemo,
            AnimationDemo,
            UIThemeDemo,
            IntegratedGameplayDemo,
            FullPolishDemo
        }
        
        private DemoScenario currentScenario = DemoScenario.FullPolishDemo;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeSystems();
            
            if (autoStartDemo)
            {
                StartCoroutine(RunPolishDemo());
            }
        }
        
        private void Update()
        {
            HandleInputCommands();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize all polish systems
        /// </summary>
        private void InitializeSystems()
        {
            Log("Initializing Polish Systems...");
            
            // Get or create system managers
            if (audioManager == null)
            {
                audioManager = PlushLeague.Audio.AudioManager.Instance;
            }
            
            if (vfxManager == null)
            {
                vfxManager = PlushLeague.VFX.VFXManager.Instance;
            }
            
            if (uiThemeManager == null)
            {
                uiThemeManager = PlushLeague.UI.Theme.UIThemeManager.Instance;
            }
            
            if (polishManager == null)
            {
                polishManager = PlushLeague.Polish.PolishIntegrationManager.Instance;
            }
            
            // Create test objects if not assigned
            CreateTestObjectsIfNeeded();
            
            Log("Polish systems initialized successfully");
        }
        
        /// <summary>
        /// Create test objects for demonstration
        /// </summary>
        private void CreateTestObjectsIfNeeded()
        {
            // Create test players
            if (testPlayer1 == null)
            {
                testPlayer1 = CreateTestPlayer("TestPlayer1", Vector3.left * 2f, Color.blue);
            }
            
            if (testPlayer2 == null)
            {
                testPlayer2 = CreateTestPlayer("TestPlayer2", Vector3.right * 2f, Color.red);
            }
            
            // Create test ball
            if (testBall == null)
            {
                testBall = CreateTestBall("TestBall", Vector3.zero);
            }
            
            // Create goal position marker
            if (goalPosition == null)
            {
                var goalObject = new GameObject("GoalPosition");
                goalPosition = goalObject.transform;
                goalPosition.position = Vector3.forward * 5f;
            }
            
            // Create test positions
            if (testPositions == null || testPositions.Length == 0)
            {
                testPositions = new Transform[6];
                for (int i = 0; i < testPositions.Length; i++)
                {
                    var posObj = new GameObject($"TestPosition{i}");
                    testPositions[i] = posObj.transform;
                    
                    // Arrange in circle
                    float angle = (i * 60f) * Mathf.Deg2Rad;
                    testPositions[i].position = new Vector3(Mathf.Cos(angle) * 3f, 0, Mathf.Sin(angle) * 3f);
                }
            }
        }
        
        /// <summary>
        /// Create a test player with animation controller
        /// </summary>
        private GameObject CreateTestPlayer(string name, Vector3 position, Color color)
        {
            // Create player object
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = name;
            player.transform.position = position;
            
            // Add plush animation controller
            var animController = player.AddComponent<PlushLeague.Animation.PlushAnimationController>();
            
            // Color the player
            var renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
            
            Log($"Created test player: {name}");
            return player;
        }
        
        /// <summary>
        /// Create a test ball
        /// </summary>
        private GameObject CreateTestBall(string name, Vector3 position)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = name;
            ball.transform.position = position;
            ball.transform.localScale = Vector3.one * 0.5f;
            
            // Make it orange like a soccer ball
            var renderer = ball.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
            
            Log($"Created test ball: {name}");
            return ball;
        }
        
        #endregion
        
        #region Demo Execution
        
        /// <summary>
        /// Run the complete polish system demonstration
        /// </summary>
        public IEnumerator RunPolishDemo()
        {
            if (demoRunning)
            {
                Log("Demo already running!");
                yield break;
            }
            
            do
            {
                demoRunning = true;
                currentDemoStep = 0;
                demoLog.Clear();
                
                Log("=== STARTING POLISH SYSTEM DEMO ===");
                Log($"Scenario: {currentScenario}");
                
                switch (currentScenario)
                {
                    case DemoScenario.AudioDemo:
                        yield return StartCoroutine(RunAudioDemo());
                        break;
                    case DemoScenario.VFXDemo:
                        yield return StartCoroutine(RunVFXDemo());
                        break;
                    case DemoScenario.AnimationDemo:
                        yield return StartCoroutine(RunAnimationDemo());
                        break;
                    case DemoScenario.UIThemeDemo:
                        yield return StartCoroutine(RunUIThemeDemo());
                        break;
                    case DemoScenario.IntegratedGameplayDemo:
                        yield return StartCoroutine(RunIntegratedGameplayDemo());
                        break;
                    case DemoScenario.FullPolishDemo:
                        yield return StartCoroutine(RunFullPolishDemo());
                        break;
                }
                
                Log("=== POLISH DEMO COMPLETE ===");
                demoRunning = false;
                
                if (loopDemo)
                {
                    yield return new WaitForSeconds(2f);
                }
                
            } while (loopDemo);
        }
        
        /// <summary>
        /// Demonstrate audio system features
        /// </summary>
        private IEnumerator RunAudioDemo()
        {
            Log("=== AUDIO SYSTEM DEMO ===");
            
            // Music demo
            yield return StartCoroutine(DemoStep("Menu Music", () => {
                if (audioManager != null) audioManager.StartMenuMusic();
            }));
            
            yield return StartCoroutine(DemoStep("Gameplay Music", () => {
                if (audioManager != null) audioManager.StartGameplayMusic();
            }));
            
            // SFX demo
            yield return StartCoroutine(DemoStep("Kick Sound", () => {
                if (audioManager != null) audioManager.PlayKickSound();
            }));
            
            yield return StartCoroutine(DemoStep("Bounce Sound", () => {
                if (audioManager != null) audioManager.PlayBounceSound();
            }));
            
            yield return StartCoroutine(DemoStep("Tackle Sound", () => {
                if (audioManager != null) audioManager.PlayTackleSound();
            }));
            
            yield return StartCoroutine(DemoStep("Goal Sound", () => {
                if (audioManager != null) audioManager.PlaySFX("goal_scored");
            }));
            
            // UI sounds
            yield return StartCoroutine(DemoStep("Button Click", () => {
                if (audioManager != null) audioManager.PlayUISound("button_click");
            }));
            
            yield return StartCoroutine(DemoStep("Countdown", () => {
                if (audioManager != null) 
                {
                    StartCoroutine(PlayCountdownSequence());
                }
            }));
        }
        
        /// <summary>
        /// Demonstrate VFX system features
        /// </summary>
        private IEnumerator RunVFXDemo()
        {
            Log("=== VFX SYSTEM DEMO ===");
            
            // Goal effects
            yield return StartCoroutine(DemoStep("Goal Celebration", () => {
                if (vfxManager != null) vfxManager.TriggerGoalEffect(goalPosition.position);
            }));
            
            // Gameplay effects
            yield return StartCoroutine(DemoStep("Kick Effect", () => {
                if (vfxManager != null) vfxManager.TriggerKickEffect(testBall.transform.position);
            }));
            
            yield return StartCoroutine(DemoStep("Bounce Effect", () => {
                if (vfxManager != null) vfxManager.TriggerBounceEffect(testBall.transform.position);
            }));
            
            yield return StartCoroutine(DemoStep("Tackle Effect", () => {
                if (vfxManager != null) vfxManager.TriggerTackleEffect(testPlayer1.transform.position);
            }));
            
            // Superpower effects
            yield return StartCoroutine(DemoStep("Superpower Activation", () => {
                if (vfxManager != null) vfxManager.TriggerSuperpowerActivation(testPlayer2.transform.position, 0);
            }));
            
            yield return StartCoroutine(DemoStep("Superpower Impact", () => {
                if (vfxManager != null) vfxManager.TriggerSuperpowerImpact(testBall.transform.position, 0);
            }));
            
            // UI effects
            yield return StartCoroutine(DemoStep("Victory Effect", () => {
                if (vfxManager != null) vfxManager.TriggerVictoryEffect(Vector3.zero);
            }));
            
            // Ambient effects
            yield return StartCoroutine(DemoStep("Ambient Effects Start", () => {
                if (vfxManager != null) vfxManager.StartAmbientEffects();
            }));
            
            yield return new WaitForSeconds(2f);
            
            yield return StartCoroutine(DemoStep("Ambient Effects Stop", () => {
                if (vfxManager != null) vfxManager.StopAmbientEffects();
            }));
        }
        
        /// <summary>
        /// Demonstrate animation system features
        /// </summary>
        private IEnumerator RunAnimationDemo()
        {
            Log("=== ANIMATION SYSTEM DEMO ===");
            
            var player1Anim = testPlayer1.GetComponent<PlushLeague.Animation.PlushAnimationController>();
            var player2Anim = testPlayer2.GetComponent<PlushLeague.Animation.PlushAnimationController>();
            
            // Idle animation
            yield return StartCoroutine(DemoStep("Idle Bounce", () => {
                if (player1Anim != null) player1Anim.StartIdleAnimation();
            }));
            
            // Celebration animation
            yield return StartCoroutine(DemoStep("Celebration", () => {
                if (player1Anim != null) player1Anim.StartCelebrationAnimation();
            }));
            
            // Tackle animation
            yield return StartCoroutine(DemoStep("Tackle Flop", () => {
                if (player2Anim != null) player2Anim.StartTackleAnimation();
            }));
            
            // Squash and stretch
            yield return StartCoroutine(DemoStep("Bounce Animation", () => {
                if (player1Anim != null) player1Anim.StartBounceAnimation();
            }));
            
            yield return StartCoroutine(DemoStep("Squash Animation", () => {
                if (player2Anim != null) player2Anim.StartSquashAnimation();
            }));
            
            yield return StartCoroutine(DemoStep("Stretch Animation", () => {
                if (player1Anim != null) player1Anim.StartStretchAnimation();
            }));
        }
        
        /// <summary>
        /// Demonstrate UI theme system features
        /// </summary>
        private IEnumerator RunUIThemeDemo()
        {
            Log("=== UI THEME SYSTEM DEMO ===");
            
            // Create test UI elements
            var testCanvas = CreateTestCanvas();
            
            yield return StartCoroutine(DemoStep("Create Test Button", () => {
                var testButton = CreateTestButton(testCanvas);
                if (uiThemeManager != null) uiThemeManager.ApplyButtonTheme(testButton);
            }));
            
            yield return StartCoroutine(DemoStep("Create Test Panel", () => {
                var testPanel = CreateTestPanel(testCanvas);
                if (uiThemeManager != null) uiThemeManager.ApplyPanelTheme(testPanel);
            }));
            
            yield return StartCoroutine(DemoStep("Create Test Slider", () => {
                var testSlider = CreateTestSlider(testCanvas);
                if (uiThemeManager != null) uiThemeManager.ApplySliderTheme(testSlider);
            }));
            
            yield return StartCoroutine(DemoStep("Color Theme Change", () => {
                if (uiThemeManager != null) 
                {
                    uiThemeManager.SetColorTheme(Color.cyan, Color.blue, Color.yellow);
                }
            }));
            
            // Clean up test UI
            yield return new WaitForSeconds(2f);
            if (testCanvas != null)
            {
                Destroy(testCanvas.gameObject);
            }
        }
        
        /// <summary>
        /// Demonstrate integrated gameplay with all polish systems
        /// </summary>
        private IEnumerator RunIntegratedGameplayDemo()
        {
            Log("=== INTEGRATED GAMEPLAY DEMO ===");
            
            // Simulated match start
            yield return StartCoroutine(DemoStep("Match Start", () => {
                if (polishManager != null) 
                {
                    StartCoroutine(PlayCountdownSequence());
                }
            }));
            
            yield return new WaitForSeconds(3f);
            
            // Simulated gameplay events
            yield return StartCoroutine(DemoStep("Ball Kick", () => {
                if (polishManager != null) 
                {
                    polishManager.OnBallKicked(testBall.transform.position, 10f);
                }
            }));
            
            yield return StartCoroutine(DemoStep("Ball Bounce", () => {
                if (polishManager != null) 
                {
                    polishManager.OnBallBounced(testBall.transform.position + Vector3.right);
                }
            }));
            
            yield return StartCoroutine(DemoStep("Player Tackle", () => {
                if (polishManager != null) 
                {
                    polishManager.OnPlayerTackle(testPlayer1.transform.position, testPlayer1);
                }
            }));
            
            yield return StartCoroutine(DemoStep("Superpower Activation", () => {
                if (polishManager != null) 
                {
                    polishManager.OnSuperpowerActivated(testPlayer2.transform.position, "TestPower", 0);
                }
            }));
            
            yield return StartCoroutine(DemoStep("Goal Save", () => {
                if (polishManager != null) 
                {
                    polishManager.OnGoalSaved(goalPosition.position, testPlayer2);
                }
            }));
            
            yield return StartCoroutine(DemoStep("Goal Scored", () => {
                if (polishManager != null) 
                {
                    polishManager.OnGoalScored(goalPosition.position, true);
                }
            }));
        }
        
        /// <summary>
        /// Run the complete polish demo with all systems
        /// </summary>
        private IEnumerator RunFullPolishDemo()
        {
            Log("=== FULL POLISH SYSTEM DEMO ===");
            
            // Run each sub-demo
            yield return StartCoroutine(RunAudioDemo());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(RunVFXDemo());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(RunAnimationDemo());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(RunUIThemeDemo());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(RunIntegratedGameplayDemo());
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Execute a demo step with logging
        /// </summary>
        private IEnumerator DemoStep(string stepName, System.Action stepAction)
        {
            currentDemoStep++;
            Log($"STEP {currentDemoStep}: {stepName}");
            
            stepAction?.Invoke();
            yield return new WaitForSeconds(demoStepDelay);
        }
        
        /// <summary>
        /// Play countdown sequence
        /// </summary>
        private IEnumerator PlayCountdownSequence()
        {
            for (int i = 3; i >= 1; i--)
            {
                if (audioManager != null) audioManager.PlayCountdownBeep(i);
                yield return new WaitForSeconds(1f);
            }
            
            if (audioManager != null) audioManager.PlaySFX("countdown_final");
        }
        
        /// <summary>
        /// Create test canvas for UI demo
        /// </summary>
        private Canvas CreateTestCanvas()
        {
            var canvasObject = new GameObject("TestCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            return canvas;
        }
        
        /// <summary>
        /// Create test button
        /// </summary>
        private UnityEngine.UI.Button CreateTestButton(Canvas parentCanvas)
        {
            var buttonObject = new GameObject("TestButton");
            buttonObject.transform.SetParent(parentCanvas.transform, false);
            
            var image = buttonObject.AddComponent<UnityEngine.UI.Image>();
            var button = buttonObject.AddComponent<UnityEngine.UI.Button>();
            
            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchoredPosition = Vector2.zero;
            
            return button;
        }
        
        /// <summary>
        /// Create test panel
        /// </summary>
        private UnityEngine.UI.Image CreateTestPanel(Canvas parentCanvas)
        {
            var panelObject = new GameObject("TestPanel");
            panelObject.transform.SetParent(parentCanvas.transform, false);
            
            var image = panelObject.AddComponent<UnityEngine.UI.Image>();
            
            var rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 200);
            rectTransform.anchoredPosition = new Vector2(0, 100);
            
            return image;
        }
        
        /// <summary>
        /// Create test slider
        /// </summary>
        private UnityEngine.UI.Slider CreateTestSlider(Canvas parentCanvas)
        {
            var sliderObject = new GameObject("TestSlider");
            sliderObject.transform.SetParent(parentCanvas.transform, false);
            
            var slider = sliderObject.AddComponent<UnityEngine.UI.Slider>();
            
            var rectTransform = sliderObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 20);
            rectTransform.anchoredPosition = new Vector2(0, -100);
            
            return slider;
        }
        
        /// <summary>
        /// Handle keyboard input for manual testing
        /// </summary>
        private void HandleInputCommands()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) TriggerAudioTest();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TriggerVFXTest();
            if (Input.GetKeyDown(KeyCode.Alpha3)) TriggerAnimationTest();
            if (Input.GetKeyDown(KeyCode.Alpha4)) TriggerUITest();
            if (Input.GetKeyDown(KeyCode.Alpha5)) TriggerIntegratedTest();
            
            if (Input.GetKeyDown(KeyCode.Space)) TriggerRandomEffect();
        }
        
        /// <summary>
        /// Trigger audio test
        /// </summary>
        private void TriggerAudioTest()
        {
            if (audioManager != null)
            {
                string[] testSounds = { "button_click", "goal_scored", "crowd_cheer" };
                string randomSound = testSounds[Random.Range(0, testSounds.Length)];
                audioManager.PlaySFX(randomSound);
                Log($"Played audio: {randomSound}");
            }
        }
        
        /// <summary>
        /// Trigger VFX test
        /// </summary>
        private void TriggerVFXTest()
        {
            if (vfxManager != null && testPositions.Length > 0)
            {
                Vector3 randomPosition = testPositions[Random.Range(0, testPositions.Length)].position;
                string[] testEffects = { "kick_effect", "bounce_effect", "tackle_cloud" };
                string randomEffect = testEffects[Random.Range(0, testEffects.Length)];
                vfxManager.TriggerVFX(randomEffect, randomPosition);
                Log($"Triggered VFX: {randomEffect} at {randomPosition}");
            }
        }
        
        /// <summary>
        /// Trigger animation test
        /// </summary>
        private void TriggerAnimationTest()
        {
            var players = new[] { testPlayer1, testPlayer2 };
            var randomPlayer = players[Random.Range(0, players.Length)];
            var animController = randomPlayer.GetComponent<PlushLeague.Animation.PlushAnimationController>();
            
            if (animController != null)
            {
                string[] testAnims = { "bounce", "celebrate", "squash", "stretch" };
                string randomAnim = testAnims[Random.Range(0, testAnims.Length)];
                animController.AnimatePlush(randomAnim);
                Log($"Triggered animation: {randomAnim} on {randomPlayer.name}");
            }
        }
        
        /// <summary>
        /// Trigger UI test
        /// </summary>
        private void TriggerUITest()
        {
            if (polishManager != null)
            {
                polishManager.OnUIButtonInteraction("click", Vector3.zero);
                Log("Triggered UI button interaction");
            }
        }
        
        /// <summary>
        /// Trigger integrated test
        /// </summary>
        private void TriggerIntegratedTest()
        {
            if (polishManager != null)
            {
                var testEvents = new System.Action[]
                {
                    () => polishManager.OnBallKicked(testBall.transform.position, 10f),
                    () => polishManager.OnPlayerTackle(testPlayer1.transform.position, testPlayer1),
                    () => polishManager.OnGoalScored(goalPosition.position, true),
                    () => polishManager.OnSuperpowerActivated(testPlayer2.transform.position, "TestPower", 0)
                };
                
                var randomEvent = testEvents[Random.Range(0, testEvents.Length)];
                randomEvent.Invoke();
                Log("Triggered random integrated event");
            }
        }
        
        /// <summary>
        /// Trigger random effect for fun
        /// </summary>
        private void TriggerRandomEffect()
        {
            int randomTest = Random.Range(1, 6);
            switch (randomTest)
            {
                case 1: TriggerAudioTest(); break;
                case 2: TriggerVFXTest(); break;
                case 3: TriggerAnimationTest(); break;
                case 4: TriggerUITest(); break;
                case 5: TriggerIntegratedTest(); break;
            }
        }
        
        /// <summary>
        /// Log demo messages
        /// </summary>
        private void Log(string message)
        {
            string timestampedMessage = $"[{Time.time:F2}] [POLISH DEMO] {message}";
            UnityEngine.Debug.Log(timestampedMessage);
            demoLog.Add(timestampedMessage);
            
            // Keep log size manageable
            if (demoLog.Count > 100)
            {
                demoLog.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Debug GUI
        
        private Vector2 logScrollPosition = Vector2.zero;
        
        private void OnGUI()
        {
            if (!enableDebugGUI || !Application.isPlaying) return;
            
            // Main control panel
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.Label("=== Polish System Demo ===");
            
            GUILayout.Label($"Demo Running: {demoRunning}");
            GUILayout.Label($"Current Step: {currentDemoStep}");
            GUILayout.Label($"Scenario: {currentScenario}");
            
            GUILayout.Space(10);
            
            // Scenario selection
            GUILayout.Label("Select Demo Scenario:");
            foreach (DemoScenario scenario in System.Enum.GetValues(typeof(DemoScenario)))
            {
                if (GUILayout.Button(scenario.ToString()))
                {
                    currentScenario = scenario;
                }
            }
            
            GUILayout.Space(10);
            
            // Demo controls
            if (!demoRunning)
            {
                if (GUILayout.Button("Start Demo"))
                {
                    StartCoroutine(RunPolishDemo());
                }
                
                loopDemo = GUILayout.Toggle(loopDemo, "Loop Demo");
            }
            else
            {
                if (GUILayout.Button("Stop Demo"))
                {
                    StopAllCoroutines();
                    demoRunning = false;
                }
            }
            
            GUILayout.Space(10);
            
            // Manual test buttons
            GUILayout.Label("Manual Tests (or use number keys 1-5):");
            
            if (GUILayout.Button("Audio Test (1)"))
            {
                TriggerAudioTest();
            }
            
            if (GUILayout.Button("VFX Test (2)"))
            {
                TriggerVFXTest();
            }
            
            if (GUILayout.Button("Animation Test (3)"))
            {
                TriggerAnimationTest();
            }
            
            if (GUILayout.Button("UI Test (4)"))
            {
                TriggerUITest();
            }
            
            if (GUILayout.Button("Integrated Test (5)"))
            {
                TriggerIntegratedTest();
            }
            
            if (GUILayout.Button("Random Effect (Space)"))
            {
                TriggerRandomEffect();
            }
            
            GUILayout.Space(10);
            
            // System status
            GUILayout.Label("=== System Status ===");
            GUILayout.Label($"Audio Manager: {(audioManager != null ? "✓" : "✗")}");
            GUILayout.Label($"VFX Manager: {(vfxManager != null ? "✓" : "✗")}");
            GUILayout.Label($"UI Theme Manager: {(uiThemeManager != null ? "✓" : "✗")}");
            GUILayout.Label($"Polish Manager: {(polishManager != null ? "✓" : "✗")}");
            
            GUILayout.EndArea();
            
            // Log window
            GUILayout.BeginArea(new Rect(420, 10, 400, 600));
            GUILayout.Label("=== Demo Log ===");
            
            logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Width(390), GUILayout.Height(550));
            
            for (int i = Mathf.Max(0, demoLog.Count - 50); i < demoLog.Count; i++)
            {
                GUILayout.Label(demoLog[i]);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
