using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Automated balance testing system for gameplay parameters.
    /// Runs automated tests to evaluate balance and suggest parameter adjustments.
    /// </summary>
    public class BalanceTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableAutomatedTesting = false;
        [SerializeField] private int testIterations = 100;
        [SerializeField] private float testSpeed = 10f; // Time scale for rapid testing
        [SerializeField] private bool logDetailedResults = true;
        
        [Header("Balance Metrics")]
        [SerializeField] private float targetAverageMatchDuration = 120f; // 2 minutes
        [SerializeField] private float targetWinRateBalance = 0.5f; // 50/50 split
        [SerializeField] private float acceptableWinRateDeviation = 0.1f; // ±10%
        [SerializeField] private int targetGoalsPerMatch = 3;
        [SerializeField] private float targetStaminaUsageRatio = 0.7f; // 70% stamina usage
        
        [Header("Parameter Ranges")]
        [SerializeField] private Vector2 playerSpeedRange = new Vector2(3f, 8f);
        [SerializeField] private Vector2 staminaCostRange = new Vector2(5f, 20f);
        [SerializeField] private Vector2 powerupCooldownRange = new Vector2(2f, 8f);
        [SerializeField] private Vector2 ballSpeedRange = new Vector2(5f, 12f);
        
        // Test results
        private List<TestResult> testResults = new List<TestResult>();
        private TestResult currentTest;
        private int currentIteration = 0;
        private bool testing = false;
        
        [System.Serializable]
        public class TestResult
        {
            public float playerSpeed;
            public float staminaCost;
            public float powerupCooldown;
            public float ballSpeed;
            public float averageMatchDuration;
            public float player1WinRate;
            public float player2WinRate;
            public float tieRate;
            public float averageGoalsPerMatch;
            public float averageStaminaUsage;
            public float balanceScore; // Overall balance rating
            public string notes;
        }
        
        private void Start()
        {
            if (enableAutomatedTesting)
            {
                StartCoroutine(RunAutomatedBalanceTests());
            }
        }
        
        /// <summary>
        /// Run automated balance tests
        /// </summary>
        public IEnumerator RunAutomatedBalanceTests()
        {
            if (testing)
            {
                UnityEngine.Debug.LogWarning("Balance testing already in progress");
                yield break;
            }
            
            testing = true;
            testResults.Clear();
            
            UnityEngine.Debug.Log($"Starting automated balance testing - {testIterations} iterations");
            
            // Save original time scale
            float originalTimeScale = Time.timeScale;
            Time.timeScale = testSpeed;
            
            for (currentIteration = 0; currentIteration < testIterations; currentIteration++)
            {
                yield return StartCoroutine(RunSingleBalanceTest());
                
                // Brief pause between tests
                yield return new WaitForSeconds(0.1f);
            }
            
            // Restore time scale
            Time.timeScale = originalTimeScale;
            
            // Analyze results
            AnalyzeTestResults();
            
            testing = false;
            UnityEngine.Debug.Log("Automated balance testing complete");
        }
        
        /// <summary>
        /// Run a single balance test
        /// </summary>
        private IEnumerator RunSingleBalanceTest()
        {
            // Generate random parameters within ranges
            currentTest = new TestResult
            {
                playerSpeed = Random.Range(playerSpeedRange.x, playerSpeedRange.y),
                staminaCost = Random.Range(staminaCostRange.x, staminaCostRange.y),
                powerupCooldown = Random.Range(powerupCooldownRange.x, powerupCooldownRange.y),
                ballSpeed = Random.Range(ballSpeedRange.x, ballSpeedRange.y)
            };
            
            // Apply parameters to game systems
            ApplyTestParameters(currentTest);
            
            // Run multiple matches with these parameters
            int matchesPerTest = 5;
            List<MatchData> matches = new List<MatchData>();
            
            for (int i = 0; i < matchesPerTest; i++)
            {
                yield return StartCoroutine(RunSingleMatch(matches));
            }
            
            // Calculate averages
            CalculateTestAverages(currentTest, matches);
            
            // Calculate balance score
            CalculateBalanceScore(currentTest);
            
            testResults.Add(currentTest);
            
            if (logDetailedResults)
            {
                UnityEngine.Debug.Log($"Test {currentIteration + 1}/{testIterations} - Balance Score: {currentTest.balanceScore:F2}");
            }
        }
        
        /// <summary>
        /// Apply test parameters to game systems
        /// </summary>
        private void ApplyTestParameters(TestResult testParams)
        {
            // Apply to debug tools if available
            var debugTools = DebugTools.Instance;
            if (debugTools != null)
            {
                // This would need to be implemented in DebugTools
                // debugTools.SetTestParameters(testParams);
            }
            
            // Apply directly to game systems
            var players = FindObjectsByType<PlushLeague.Gameplay.Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player != null)
                {
                    // Implementation depends on your player controller
                    // player.SetSpeed(testParams.playerSpeed);
                    // player.SetStaminaCost(testParams.staminaCost);
                }
            }
            
            var ballController = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallController>();
            if (ballController != null)
            {
                // Implementation depends on your ball controller
                // ballController.SetSpeed(testParams.ballSpeed);
            }
        }
        
        /// <summary>
        /// Run a single match and collect data
        /// </summary>
        private IEnumerator RunSingleMatch(List<MatchData> matches)
        {
            var matchData = new MatchData();
            float matchStartTime = Time.time;
            
            // Start match
            var gameManager = PlushLeague.Core.GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartQuickMatch();
            }
            
            // Wait for match to complete or timeout
            float maxMatchTime = 300f; // 5 minutes max
            float elapsedTime = 0f;
            
            while (elapsedTime < maxMatchTime)
            {
                elapsedTime = Time.time - matchStartTime;
                
                // Check if match is complete
                // This would need to be implemented based on your match system
                // if (matchManager.IsMatchComplete())
                //     break;
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // Collect match data
            matchData.duration = elapsedTime;
            // matchData.player1Score = matchManager.GetPlayer1Score();
            // matchData.player2Score = matchManager.GetPlayer2Score();
            // matchData.totalGoals = matchData.player1Score + matchData.player2Score;
            
            // Determine winner
            if (matchData.player1Score > matchData.player2Score)
                matchData.winner = 1;
            else if (matchData.player2Score > matchData.player1Score)
                matchData.winner = 2;
            else
                matchData.winner = 0; // Tie
            
            matches.Add(matchData);
        }
        
        /// <summary>
        /// Calculate test averages from match data
        /// </summary>
        private void CalculateTestAverages(TestResult testResult, List<MatchData> matches)
        {
            if (matches.Count == 0) return;
            
            float totalDuration = 0f;
            float totalGoals = 0f;
            int player1Wins = 0;
            int player2Wins = 0;
            int ties = 0;
            
            foreach (var match in matches)
            {
                totalDuration += match.duration;
                totalGoals += match.totalGoals;
                
                if (match.winner == 1) player1Wins++;
                else if (match.winner == 2) player2Wins++;
                else ties++;
            }
            
            testResult.averageMatchDuration = totalDuration / matches.Count;
            testResult.averageGoalsPerMatch = totalGoals / matches.Count;
            testResult.player1WinRate = (float)player1Wins / matches.Count;
            testResult.player2WinRate = (float)player2Wins / matches.Count;
            testResult.tieRate = (float)ties / matches.Count;
            
            // Placeholder for stamina usage - would need to be tracked during matches
            testResult.averageStaminaUsage = 0.7f; // Default value
        }
        
        /// <summary>
        /// Calculate balance score for test result
        /// </summary>
        private void CalculateBalanceScore(TestResult testResult)
        {
            float score = 100f; // Start with perfect score
            
            // Match duration balance
            float durationDiff = Mathf.Abs(testResult.averageMatchDuration - targetAverageMatchDuration);
            score -= (durationDiff / targetAverageMatchDuration) * 20f;
            
            // Win rate balance
            float winRateDeviation = Mathf.Abs(testResult.player1WinRate - targetWinRateBalance);
            if (winRateDeviation > acceptableWinRateDeviation)
            {
                score -= (winRateDeviation - acceptableWinRateDeviation) * 30f;
            }
            
            // Goals per match balance
            float goalsDiff = Mathf.Abs(testResult.averageGoalsPerMatch - targetGoalsPerMatch);
            score -= (goalsDiff / targetGoalsPerMatch) * 15f;
            
            // Stamina usage balance
            float staminaDiff = Mathf.Abs(testResult.averageStaminaUsage - targetStaminaUsageRatio);
            score -= staminaDiff * 10f;
            
            // Ensure score doesn't go below 0
            testResult.balanceScore = Mathf.Max(0f, score);
        }
        
        /// <summary>
        /// Analyze all test results and provide recommendations
        /// </summary>
        private void AnalyzeTestResults()
        {
            if (testResults.Count == 0) return;
            
            // Find best performing parameters
            TestResult bestResult = testResults[0];
            foreach (var result in testResults)
            {
                if (result.balanceScore > bestResult.balanceScore)
                {
                    bestResult = result;
                }
            }
            
            UnityEngine.Debug.Log("=== BALANCE TEST RESULTS ===");
            UnityEngine.Debug.Log($"Best Balance Score: {bestResult.balanceScore:F2}");
            UnityEngine.Debug.Log($"Recommended Parameters:");
            UnityEngine.Debug.Log($"  Player Speed: {bestResult.playerSpeed:F2}");
            UnityEngine.Debug.Log($"  Stamina Cost: {bestResult.staminaCost:F2}");
            UnityEngine.Debug.Log($"  Powerup Cooldown: {bestResult.powerupCooldown:F2}");
            UnityEngine.Debug.Log($"  Ball Speed: {bestResult.ballSpeed:F2}");
            UnityEngine.Debug.Log($"Performance Metrics:");
            UnityEngine.Debug.Log($"  Average Match Duration: {bestResult.averageMatchDuration:F1}s");
            UnityEngine.Debug.Log($"  Player 1 Win Rate: {bestResult.player1WinRate:P1}");
            UnityEngine.Debug.Log($"  Player 2 Win Rate: {bestResult.player2WinRate:P1}");
            UnityEngine.Debug.Log($"  Tie Rate: {bestResult.tieRate:P1}");
            UnityEngine.Debug.Log($"  Average Goals Per Match: {bestResult.averageGoalsPerMatch:F1}");
            
            // Generate recommendations
            GenerateRecommendations(bestResult);
        }
        
        /// <summary>
        /// Generate balance recommendations
        /// </summary>
        private void GenerateRecommendations(TestResult bestResult)
        {
            var recommendations = new List<string>();
            
            // Match duration recommendations
            if (bestResult.averageMatchDuration < targetAverageMatchDuration * 0.8f)
            {
                recommendations.Add("Matches are too short - consider reducing player speed or increasing field size");
            }
            else if (bestResult.averageMatchDuration > targetAverageMatchDuration * 1.2f)
            {
                recommendations.Add("Matches are too long - consider increasing player speed or reducing field size");
            }
            
            // Win rate recommendations
            float winRateDeviation = Mathf.Abs(bestResult.player1WinRate - targetWinRateBalance);
            if (winRateDeviation > acceptableWinRateDeviation)
            {
                recommendations.Add("Win rates are unbalanced - check for player advantages or asymmetries");
            }
            
            // Goals per match recommendations
            if (bestResult.averageGoalsPerMatch < targetGoalsPerMatch * 0.7f)
            {
                recommendations.Add("Too few goals per match - consider increasing ball speed or reducing defense effectiveness");
            }
            else if (bestResult.averageGoalsPerMatch > targetGoalsPerMatch * 1.3f)
            {
                recommendations.Add("Too many goals per match - consider decreasing ball speed or increasing defense effectiveness");
            }
            
            UnityEngine.Debug.Log("=== RECOMMENDATIONS ===");
            foreach (var recommendation in recommendations)
            {
                UnityEngine.Debug.Log($"• {recommendation}");
            }
        }
        
        /// <summary>
        /// Manual balance test with current parameters
        /// </summary>
        public void RunManualBalanceTest()
        {
            StartCoroutine(RunSingleBalanceTest());
        }
        
        /// <summary>
        /// Save test results to file
        /// </summary>
        public void SaveTestResults()
        {
            if (testResults.Count == 0) return;
            
            try
            {
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"balance_test_results_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
                
                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    // Write header
                    writer.WriteLine("PlayerSpeed,StaminaCost,PowerupCooldown,BallSpeed,AvgMatchDuration,Player1WinRate,Player2WinRate,TieRate,AvgGoalsPerMatch,AvgStaminaUsage,BalanceScore");
                    
                    // Write data
                    foreach (var result in testResults)
                    {
                        writer.WriteLine($"{result.playerSpeed:F2},{result.staminaCost:F2},{result.powerupCooldown:F2},{result.ballSpeed:F2},{result.averageMatchDuration:F2},{result.player1WinRate:F3},{result.player2WinRate:F3},{result.tieRate:F3},{result.averageGoalsPerMatch:F2},{result.averageStaminaUsage:F2},{result.balanceScore:F2}");
                    }
                }
                
                UnityEngine.Debug.Log($"Balance test results saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save balance test results: {e.Message}");
            }
        }
        
        [System.Serializable]
        private class MatchData
        {
            public float duration;
            public int player1Score;
            public int player2Score;
            public int totalGoals;
            public int winner; // 0 = tie, 1 = player1, 2 = player2
        }
        
        #region Debug GUI
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 150, 190, 140));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== BALANCE TESTER ===");
            
            if (testing)
            {
                GUILayout.Label($"Testing: {currentIteration + 1}/{testIterations}");
                GUILayout.Label("Please wait...");
            }
            else
            {
                if (GUILayout.Button("Run Manual Test"))
                {
                    RunManualBalanceTest();
                }
                
                if (GUILayout.Button("Run Automated Tests"))
                {
                    StartCoroutine(RunAutomatedBalanceTests());
                }
                
                if (GUILayout.Button("Save Results"))
                {
                    SaveTestResults();
                }
            }
            
            GUILayout.Label($"Results: {testResults.Count}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
