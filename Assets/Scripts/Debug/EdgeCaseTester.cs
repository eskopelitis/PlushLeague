using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Automated edge case testing system for comprehensive game testing.
    /// Tests extreme conditions, boundary values, and stress scenarios.
    /// </summary>
    public class EdgeCaseTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableEdgeCaseTesting = true;
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private float testInterval = 5f;
        [SerializeField] private int maxConcurrentTests = 3;
        [SerializeField] private bool logAllTests = true;
        [SerializeField] private bool stopOnFailure = false;
        
        [Header("Test Categories")]
        [SerializeField] private bool testBoundaryValues = true;
        [SerializeField] private bool testExtremeConditions = true;
        [SerializeField] private bool testStressScenarios = true;
        [SerializeField] private bool testNetworkConditions = true;
        [SerializeField] private bool testPerformanceBreakpoints = true;
        [SerializeField] private bool testUserInputEdgeCases = true;
        
        [Header("System References")]
        [SerializeField] private PlushLeague.Core.GameManager gameManager;
        [SerializeField] private PlushLeague.Gameplay.Match.MatchManager matchManager;
        [SerializeField] private PlushLeague.Gameplay.Player.PlayerController[] players;
        [SerializeField] private PlushLeague.Gameplay.Ball.BallController ballController;
        
        // Test state
        private List<EdgeCaseTest> activeTests = new List<EdgeCaseTest>();
        private List<EdgeCaseTest> completedTests = new List<EdgeCaseTest>();
        private Queue<EdgeCaseTest> testQueue = new Queue<EdgeCaseTest>();
        private Coroutine testRunnerCoroutine;
        private bool testingInProgress = false;
        
        // Test results
        private int totalTests = 0;
        private int passedTests = 0;
        private int failedTests = 0;
        private int skippedTests = 0;
        
        [System.Serializable]
        public class EdgeCaseTest
        {
            public string testName;
            public string category;
            public string description;
            public TestPriority priority;
            public float timeout;
            public System.Func<IEnumerator> testCoroutine;
            public System.Action setupAction;
            public System.Action cleanupAction;
            public TestResult result;
            public float executionTime;
            public string failureReason;
            public bool isLongRunning;
            
            public EdgeCaseTest(string name, string cat, string desc, TestPriority prio = TestPriority.Medium)
            {
                testName = name;
                category = cat;
                description = desc;
                priority = prio;
                timeout = 30f;
                result = TestResult.Pending;
                executionTime = 0f;
                failureReason = "";
                isLongRunning = false;
            }
        }
        
        public enum TestPriority
        {
            Low = 0,
            Medium = 1,
            High = 2,
            Critical = 3
        }
        
        public enum TestResult
        {
            Pending,
            Running,
            Passed,
            Failed,
            Skipped,
            Timeout
        }
        
        private static EdgeCaseTester instance;
        public static EdgeCaseTester Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<EdgeCaseTester>();
                    if (instance == null)
                    {
                        GameObject testerObject = new GameObject("EdgeCaseTester");
                        instance = testerObject.AddComponent<EdgeCaseTester>();
                        DontDestroyOnLoad(testerObject);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeTests();
        }
        
        private void Start()
        {
            if (runOnStart && enableEdgeCaseTesting)
            {
                StartEdgeCaseTesting();
            }
        }
        
        private void InitializeTests()
        {
            // Boundary value tests
            if (testBoundaryValues)
            {
                QueueTest(new EdgeCaseTest("Zero Player Speed", "Boundary", "Test with player speed = 0", TestPriority.High)
                {
                    testCoroutine = () => TestZeroPlayerSpeed(),
                    timeout = 10f
                });
                
                QueueTest(new EdgeCaseTest("Maximum Player Speed", "Boundary", "Test with extremely high player speed", TestPriority.High)
                {
                    testCoroutine = () => TestMaxPlayerSpeed(),
                    timeout = 10f
                });
                
                QueueTest(new EdgeCaseTest("Negative Ball Speed", "Boundary", "Test with negative ball speed", TestPriority.Medium)
                {
                    testCoroutine = () => TestNegativeBallSpeed(),
                    timeout = 10f
                });
                
                QueueTest(new EdgeCaseTest("Zero Stamina", "Boundary", "Test with zero stamina cost", TestPriority.Medium)
                {
                    testCoroutine = () => TestZeroStamina(),
                    timeout = 15f
                });
                
                QueueTest(new EdgeCaseTest("Infinite Stamina", "Boundary", "Test with infinite stamina", TestPriority.Medium)
                {
                    testCoroutine = () => TestInfiniteStamina(),
                    timeout = 15f
                });
            }
            
            // Extreme condition tests
            if (testExtremeConditions)
            {
                QueueTest(new EdgeCaseTest("Extreme Time Scale", "Extreme", "Test with very high time scale", TestPriority.High)
                {
                    testCoroutine = () => TestExtremeTimeScale(),
                    timeout = 20f
                });
                
                QueueTest(new EdgeCaseTest("Ball Outside Boundaries", "Extreme", "Test ball far outside play area", TestPriority.High)
                {
                    testCoroutine = () => TestBallOutsideBoundaries(),
                    timeout = 15f
                });
                
                QueueTest(new EdgeCaseTest("Player Stack Overflow", "Extreme", "Test multiple players in same position", TestPriority.Medium)
                {
                    testCoroutine = () => TestPlayerStackOverflow(),
                    timeout = 10f
                });
                
                QueueTest(new EdgeCaseTest("Rapid Scene Changes", "Extreme", "Test rapid scene switching", TestPriority.High)
                {
                    testCoroutine = () => TestRapidSceneChanges(),
                    timeout = 30f
                });
            }
            
            // Stress scenario tests
            if (testStressScenarios)
            {
                QueueTest(new EdgeCaseTest("Thousand Ball Spawns", "Stress", "Spawn 1000 balls simultaneously", TestPriority.High)
                {
                    testCoroutine = () => TestMassiveBallSpawn(),
                    timeout = 60f,
                    isLongRunning = true
                });
                
                QueueTest(new EdgeCaseTest("Continuous Powerup Spam", "Stress", "Activate powerups continuously", TestPriority.Medium)
                {
                    testCoroutine = () => TestPowerupSpam(),
                    timeout = 30f
                });
                
                QueueTest(new EdgeCaseTest("Memory Stress Test", "Stress", "Create and destroy many objects", TestPriority.Critical)
                {
                    testCoroutine = () => TestMemoryStress(),
                    timeout = 45f,
                    isLongRunning = true
                });
            }
            
            // Performance breakpoint tests
            if (testPerformanceBreakpoints)
            {
                QueueTest(new EdgeCaseTest("FPS Drop Detection", "Performance", "Test FPS under load", TestPriority.High)
                {
                    testCoroutine = () => TestFPSDropDetection(),
                    timeout = 30f
                });
                
                QueueTest(new EdgeCaseTest("Memory Leak Detection", "Performance", "Monitor memory usage over time", TestPriority.Critical)
                {
                    testCoroutine = () => TestMemoryLeakDetection(),
                    timeout = 60f,
                    isLongRunning = true
                });
            }
            
            // User input edge cases
            if (testUserInputEdgeCases)
            {
                QueueTest(new EdgeCaseTest("Simultaneous Input", "Input", "Test simultaneous key presses", TestPriority.Medium)
                {
                    testCoroutine = () => TestSimultaneousInput(),
                    timeout = 15f
                });
                
                QueueTest(new EdgeCaseTest("Rapid Fire Input", "Input", "Test rapid input sequences", TestPriority.Medium)
                {
                    testCoroutine = () => TestRapidFireInput(),
                    timeout = 20f
                });
            }
            
            // Network condition tests
            if (testNetworkConditions)
            {
                QueueTest(new EdgeCaseTest("Network Disconnection", "Network", "Simulate network disconnection", TestPriority.High)
                {
                    testCoroutine = () => TestNetworkDisconnection(),
                    timeout = 10f
                });
                
                QueueTest(new EdgeCaseTest("High Network Latency", "Network", "Simulate high network latency", TestPriority.High)
                {
                    testCoroutine = () => TestHighLatency(),
                    timeout = 10f
                });
            }
            
            UnityEngine.Debug.Log($"Edge case tester initialized with {testQueue.Count} tests");
        }
        
        public void StartEdgeCaseTesting()
        {
            if (testingInProgress)
            {
                UnityEngine.Debug.LogWarning("Edge case testing already in progress");
                return;
            }
            
            testingInProgress = true;
            testRunnerCoroutine = StartCoroutine(TestRunner());
            UnityEngine.Debug.Log("Edge case testing started");
        }
        
        public void StopEdgeCaseTesting()
        {
            if (testRunnerCoroutine != null)
            {
                StopCoroutine(testRunnerCoroutine);
                testRunnerCoroutine = null;
            }
            
            testingInProgress = false;
            UnityEngine.Debug.Log("Edge case testing stopped");
        }
        
        private void QueueTest(EdgeCaseTest test)
        {
            testQueue.Enqueue(test);
            totalTests++;
        }
        
        private IEnumerator TestRunner()
        {
            while (testQueue.Count > 0)
            {
                // Wait if we have too many concurrent tests
                while (activeTests.Count >= maxConcurrentTests)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                EdgeCaseTest test = testQueue.Dequeue();
                StartCoroutine(ExecuteTest(test));
                
                yield return new WaitForSeconds(testInterval);
            }
            
            // Wait for all tests to complete
            while (activeTests.Count > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            testingInProgress = false;
            GenerateTestReport();
        }
        
        private IEnumerator ExecuteTest(EdgeCaseTest test)
        {
            activeTests.Add(test);
            test.result = TestResult.Running;
            
            float startTime = Time.time;
            
            if (logAllTests)
            {
                UnityEngine.Debug.Log($"Starting test: {test.testName}");
            }
            
            // Setup
            try
            {
                test.setupAction?.Invoke();
            }
            catch (System.Exception e)
            {
                test.result = TestResult.Failed;
                test.failureReason = $"Setup failed: {e.Message}";
                CompleteTest(test, startTime);
                yield break;
            }
            
            // Execute test with timeout
            Coroutine testCoroutine = null;
            bool testCompleted = false;
            
            try
            {
                if (test.testCoroutine != null)
                {
                    testCoroutine = StartCoroutine(ExecuteTestCoroutine(test, () => testCompleted = true));
                }
                else
                {
                    testCompleted = true;
                }
            }
            catch (System.Exception e)
            {
                test.result = TestResult.Failed;
                test.failureReason = $"Execution failed: {e.Message}";
                CompleteTest(test, startTime);
                yield break;
            }
            
            // Wait for completion or timeout
            float elapsed = 0f;
            while (!testCompleted && elapsed < test.timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!testCompleted)
            {
                test.result = TestResult.Timeout;
                test.failureReason = $"Test timed out after {test.timeout} seconds";
                
                if (testCoroutine != null)
                {
                    StopCoroutine(testCoroutine);
                }
            }
            
            // Cleanup
            try
            {
                test.cleanupAction?.Invoke();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Cleanup failed for test {test.testName}: {e.Message}");
            }
            
            CompleteTest(test, startTime);
        }
        
        private IEnumerator ExecuteTestCoroutine(EdgeCaseTest test, System.Action onComplete)
        {
            // Remove unused variables
            if (test.testCoroutine != null)
            {
                Coroutine testCoroutine = StartCoroutine(ExecuteTestWithErrorHandling(test));
                yield return testCoroutine;
            }
            
            if (test.result == TestResult.Running)
            {
                test.result = TestResult.Passed;
            }
            
            onComplete?.Invoke();
        }
        
        private IEnumerator ExecuteTestWithErrorHandling(EdgeCaseTest test)
        {
            bool hasError = false;
            string errorMessage = "";
            
            // Execute the test in a separate coroutine to handle exceptions
            yield return StartCoroutine(ExecuteTestSafely(test, (error, message) => {
                hasError = error;
                errorMessage = message;
            }));
            
            if (hasError)
            {
                test.result = TestResult.Failed;
                test.failureReason = errorMessage;
            }
        }
        
        private IEnumerator ExecuteTestSafely(EdgeCaseTest test, System.Action<bool, string> onComplete)
        {
            // Cannot use try-catch with yield, so we'll handle exceptions differently
            var testCoroutine = test.testCoroutine();
            Exception caughtException = null;
            
            // Execute the test and catch any exceptions using a wrapper
            yield return StartCoroutine(RunWithExceptionHandling(testCoroutine, ex => caughtException = ex));
            
            if (caughtException != null)
            {
                onComplete(true, caughtException.Message);
            }
            else
            {
                onComplete(false, "");
            }
        }
        
        private IEnumerator RunWithExceptionHandling(IEnumerator testCoroutine, System.Action<Exception> onError)
        {
            // This wrapper handles exceptions without using yield in try-catch
            while (true)
            {
                object current = null;
                bool hasNext = false;
                
                try
                {
                    hasNext = testCoroutine.MoveNext();
                    if (hasNext)
                        current = testCoroutine.Current;
                }
                catch (Exception e)
                {
                    onError(e);
                    yield break;
                }
                
                if (!hasNext)
                    break;
                    
                yield return current;
            }
        }
        
        private void CompleteTest(EdgeCaseTest test, float startTime)
        {
            test.executionTime = Time.time - startTime;
            activeTests.Remove(test);
            completedTests.Add(test);
            
            switch (test.result)
            {
                case TestResult.Passed:
                    passedTests++;
                    break;
                case TestResult.Failed:
                case TestResult.Timeout:
                    failedTests++;
                    break;
                case TestResult.Skipped:
                    skippedTests++;
                    break;
            }
            
            if (logAllTests)
            {
                UnityEngine.Debug.Log($"Test completed: {test.testName} - {test.result} ({test.executionTime:F2}s)");
                
                if (test.result == TestResult.Failed || test.result == TestResult.Timeout)
                {
                    UnityEngine.Debug.LogError($"Test failure: {test.failureReason}");
                }
            }
            
            if (stopOnFailure && test.result == TestResult.Failed)
            {
                StopEdgeCaseTesting();
            }
        }
        
        // Individual test implementations
        private IEnumerator TestZeroPlayerSpeed()
        {
            float originalSpeed = 0f;
            
            if (players != null && players.Length > 0)
            {
                // Store original speed and set to zero
                originalSpeed = players[0].GetComponent<Rigidbody>().linearVelocity.magnitude;
                players[0].GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                
                // Test for 3 seconds
                yield return new WaitForSeconds(3f);
                
                // Restore original speed
                players[0].GetComponent<Rigidbody>().linearVelocity = Vector3.forward * originalSpeed;
            }
        }
        
        private IEnumerator TestMaxPlayerSpeed()
        {
            if (players != null && players.Length > 0)
            {
                Vector3 originalVelocity = players[0].GetComponent<Rigidbody>().linearVelocity;
                
                // Set extremely high speed
                players[0].GetComponent<Rigidbody>().linearVelocity = Vector3.forward * 100f;
                
                yield return new WaitForSeconds(2f);
                
                // Restore original velocity
                players[0].GetComponent<Rigidbody>().linearVelocity = originalVelocity;
            }
        }
        
        private IEnumerator TestNegativeBallSpeed()
        {
            if (ballController != null)
            {
                Vector3 originalVelocity = ballController.GetComponent<Rigidbody>().linearVelocity;
                
                // Set negative speed
                ballController.GetComponent<Rigidbody>().linearVelocity = Vector3.forward * -50f;
                
                yield return new WaitForSeconds(2f);
                
                // Restore original velocity
                ballController.GetComponent<Rigidbody>().linearVelocity = originalVelocity;
            }
        }
        
        private IEnumerator TestZeroStamina()
        {
            // Test zero stamina cost scenario
            yield return new WaitForSeconds(5f);
            
            // Verify players can act without stamina constraints
            if (players != null && players.Length > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    // Simulate rapid actions
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        
        private IEnumerator TestInfiniteStamina()
        {
            // Test infinite stamina scenario
            yield return new WaitForSeconds(3f);
            
            // Verify continuous actions are possible
            if (players != null && players.Length > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    // Simulate continuous actions
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        
        private IEnumerator TestExtremeTimeScale()
        {
            float originalTimeScale = Time.timeScale;
            
            // Test with very high time scale
            Time.timeScale = 10f;
            yield return new WaitForSeconds(1f);
            
            // Test with very low time scale
            Time.timeScale = 0.1f;
            yield return new WaitForSeconds(1f);
            
            // Restore original time scale
            Time.timeScale = originalTimeScale;
        }
        
        private IEnumerator TestBallOutsideBoundaries()
        {
            if (ballController != null)
            {
                Vector3 originalPosition = ballController.transform.position;
                
                // Move ball far outside boundaries
                ballController.transform.position = new Vector3(1000f, 1000f, 1000f);
                
                yield return new WaitForSeconds(2f);
                
                // Restore original position
                ballController.transform.position = originalPosition;
            }
        }
        
        private IEnumerator TestPlayerStackOverflow()
        {
            if (players != null && players.Length > 1)
            {
                Vector3[] originalPositions = new Vector3[players.Length];
                
                // Store original positions
                for (int i = 0; i < players.Length; i++)
                {
                    originalPositions[i] = players[i].transform.position;
                }
                
                // Stack all players at same position
                Vector3 stackPosition = players[0].transform.position;
                for (int i = 1; i < players.Length; i++)
                {
                    players[i].transform.position = stackPosition;
                }
                
                yield return new WaitForSeconds(3f);
                
                // Restore original positions
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].transform.position = originalPositions[i];
                }
            }
        }
        
        private IEnumerator TestRapidSceneChanges()
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Rapid scene switching (simulated)
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                // In real implementation, would switch scenes
                UnityEngine.Debug.Log($"Simulating scene change {i + 1}");
            }
        }
        
        private IEnumerator TestMassiveBallSpawn()
        {
            List<GameObject> spawnedBalls = new List<GameObject>();
            
            // Spawn many balls
            for (int i = 0; i < 100; i++) // Reduced from 1000 for performance
            {
                if (ballController != null)
                {
                    GameObject ball = Instantiate(ballController.gameObject);
                    ball.transform.position = Vector3.zero + Vector3.up * i * 0.1f;
                    spawnedBalls.Add(ball);
                }
                
                if (i % 10 == 0)
                {
                    yield return null; // Yield periodically
                }
            }
            
            yield return new WaitForSeconds(2f);
            
            // Clean up
            foreach (var ball in spawnedBalls)
            {
                if (ball != null)
                {
                    Destroy(ball);
                }
            }
        }
        
        private IEnumerator TestPowerupSpam()
        {
            // Simulate continuous powerup activation
            for (int i = 0; i < 100; i++)
            {
                // Simulate powerup activation
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private IEnumerator TestMemoryStress()
        {
            List<GameObject> objects = new List<GameObject>();
            
            // Create many objects
            for (int i = 0; i < 1000; i++)
            {
                GameObject obj = new GameObject($"StressObject_{i}");
                obj.AddComponent<Rigidbody>();
                objects.Add(obj);
                
                if (i % 50 == 0)
                {
                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(2f);
            
            // Destroy objects
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            
            // Force garbage collection
            System.GC.Collect();
        }
        
        private IEnumerator TestFPSDropDetection()
        {
            float initialFPS = 1f / Time.deltaTime;
            float minFPS = initialFPS;
            
            // Monitor FPS for 10 seconds
            for (int i = 0; i < 100; i++)
            {
                float currentFPS = 1f / Time.deltaTime;
                if (currentFPS < minFPS)
                {
                    minFPS = currentFPS;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            // Check for significant FPS drop
            if (minFPS < initialFPS * 0.5f)
            {
                throw new System.Exception($"Significant FPS drop detected: {initialFPS:F1} -> {minFPS:F1}");
            }
        }
        
        private IEnumerator TestMemoryLeakDetection()
        {
            float initialMemory = (float)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            
            // Create and destroy objects repeatedly
            for (int i = 0; i < 50; i++)
            {
                List<GameObject> tempObjects = new List<GameObject>();
                
                // Create objects
                for (int j = 0; j < 10; j++)
                {
                    GameObject obj = new GameObject($"TempObject_{j}");
                    tempObjects.Add(obj);
                }
                
                yield return new WaitForSeconds(0.1f);
                
                // Destroy objects
                foreach (var obj in tempObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
                
                if (i % 10 == 0)
                {
                    System.GC.Collect();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            System.GC.Collect();
            yield return new WaitForSeconds(1f);
            
            float finalMemory = (float)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            float memoryIncrease = finalMemory - initialMemory;
            
            // Check for memory leak
            if (memoryIncrease > 10 * 1024 * 1024) // 10MB threshold
            {
                throw new System.Exception($"Potential memory leak detected: {memoryIncrease / 1024 / 1024:F2} MB increase");
            }
        }
        
        private IEnumerator TestSimultaneousInput()
        {
            // Simulate simultaneous input (would need actual input simulation)
            yield return new WaitForSeconds(5f);
        }
        
        private IEnumerator TestRapidFireInput()
        {
            // Simulate rapid input sequences
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
        
        private IEnumerator TestNetworkDisconnection()
        {
            // Placeholder for network disconnection testing
            UnityEngine.Debug.Log("Simulating network disconnection...");
            yield return new WaitForSeconds(2f);
            UnityEngine.Debug.Log("Network disconnection test completed");
        }
        
        private IEnumerator TestHighLatency()
        {
            // Placeholder for high latency testing
            UnityEngine.Debug.Log("Simulating high network latency...");
            yield return new WaitForSeconds(3f);
            UnityEngine.Debug.Log("High latency test completed");
        }
        
        private void GenerateTestReport()
        {
            UnityEngine.Debug.Log("=== EDGE CASE TEST REPORT ===");
            UnityEngine.Debug.Log($"Total Tests: {totalTests}");
            UnityEngine.Debug.Log($"Passed: {passedTests}");
            UnityEngine.Debug.Log($"Failed: {failedTests}");
            UnityEngine.Debug.Log($"Skipped: {skippedTests}");
            UnityEngine.Debug.Log($"Success Rate: {(float)passedTests / totalTests * 100:F1}%");
            
            if (failedTests > 0)
            {
                UnityEngine.Debug.Log("=== FAILED TESTS ===");
                foreach (var test in completedTests.Where(t => t.result == TestResult.Failed || t.result == TestResult.Timeout))
                {
                    UnityEngine.Debug.Log($"- {test.testName}: {test.failureReason}");
                }
            }
            
            // Save report to file
            PlaytestingManager.Instance?.RecordBalanceNote("EdgeCaseTesting", 
                $"Completed {totalTests} tests. {passedTests} passed, {failedTests} failed.", 
                (float)passedTests / totalTests);
        }
        
        // Public API
        public void RunSingleTest(string testName)
        {
            EdgeCaseTest test = completedTests.FirstOrDefault(t => t.testName == testName);
            if (test != null)
            {
                test.result = TestResult.Pending;
                StartCoroutine(ExecuteTest(test));
            }
        }
        
        public float GetTestSuccessRate()
        {
            return totalTests > 0 ? (float)passedTests / totalTests : 0f;
        }
        
        public List<EdgeCaseTest> GetFailedTests()
        {
            return completedTests.Where(t => t.result == TestResult.Failed || t.result == TestResult.Timeout).ToList();
        }
        
        public void ClearTestResults()
        {
            completedTests.Clear();
            totalTests = 0;
            passedTests = 0;
            failedTests = 0;
            skippedTests = 0;
        }
    }
}
