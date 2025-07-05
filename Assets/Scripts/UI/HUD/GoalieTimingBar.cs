using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// Timing bar UI for goalie save mini-game
    /// Shows color-coded zones and handles player input timing
    /// </summary>
    public class GoalieTimingBar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject timingBarPanel;
        [SerializeField] private Image timingBarFill;
        [SerializeField] private Image timingBarBackground;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Timing Zones")]
        [SerializeField] private float perfectZoneSize = 0.15f; // Last 15% is perfect (green)
        [SerializeField] private float goodZoneSize = 0.35f;    // 35% before perfect is good (yellow)
        [SerializeField] private Color perfectColor = Color.green;
        [SerializeField] private Color goodColor = Color.yellow;
        [SerializeField] private Color failColor = Color.red;
        
        [Header("Visual Effects")]
        [SerializeField] private float pulseSpeed = 2.0f;
        [SerializeField] private bool showInstructions = true;
        [SerializeField] private string instructionMessage = "Press A to Save!";
        
        [Header("Audio")]
        [SerializeField] private AudioClip countdownSound;
        [SerializeField] private AudioClip perfectSaveSound;
        [SerializeField] private AudioClip goodSaveSound;
        [SerializeField] private AudioClip failSound;
        
        // State
        private bool isActive = false;
        private float currentProgress = 1.0f; // Starts full, decreases to 0
        private float totalDuration = 0.5f;
        private float startTime;
        private System.Action<SaveResult> onSaveResult;
        
        // Input
        private bool inputPressed = false;
        private SaveResult finalResult = SaveResult.Fail;
        
        public enum SaveResult
        {
            Perfect,    // Green zone
            Good,       // Yellow zone  
            Fail        // Red zone or no input
        }
        
        // Events
        public System.Action<SaveResult> OnSaveCompleted;
        
        private void Start()
        {
            HideTimingBar();
        }
        
        private void Update()
        {
            if (isActive)
            {
                UpdateTimingBar();
                HandleInput();
            }
        }
        
        /// <summary>
        /// Show timing bar and start countdown
        /// </summary>
        public void ShowTimingBar(float duration, System.Action<SaveResult> resultCallback = null)
        {
            isActive = true;
            totalDuration = duration;
            currentProgress = 1.0f;
            startTime = Time.time;
            inputPressed = false;
            onSaveResult = resultCallback;
            
            // Show UI
            if (timingBarPanel != null)
            {
                timingBarPanel.SetActive(true);
            }
            
            if (instructionText != null && showInstructions)
            {
                instructionText.text = instructionMessage;
                instructionText.gameObject.SetActive(true);
            }
            
            // Play countdown sound
            PlaySound(countdownSound);
            
            UnityEngine.Debug.Log($"Goalie timing bar activated - Duration: {duration:F2}s");
            
            // Start timeout coroutine
            StartCoroutine(TimingBarTimeout());
        }
        
        /// <summary>
        /// Hide timing bar
        /// </summary>
        public void HideTimingBar()
        {
            isActive = false;
            
            if (timingBarPanel != null)
            {
                timingBarPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update timing bar progress and colors
        /// </summary>
        private void UpdateTimingBar()
        {
            if (!isActive) return;
            
            // Update progress (decreasing from 1 to 0)
            float elapsed = Time.time - startTime;
            currentProgress = 1.0f - (elapsed / totalDuration);
            currentProgress = Mathf.Clamp01(currentProgress);
            
            // Update fill amount
            if (timingBarFill != null)
            {
                timingBarFill.fillAmount = currentProgress;
                
                // Update color based on zone
                Color currentColor = GetCurrentZoneColor();
                timingBarFill.color = currentColor;
                
                // Add pulsing effect for urgency
                float pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
                timingBarFill.transform.localScale = Vector3.one * pulse;
            }
        }
        
        /// <summary>
        /// Get color for current timing zone
        /// </summary>
        private Color GetCurrentZoneColor()
        {
            if (currentProgress <= perfectZoneSize)
            {
                return perfectColor; // Green zone (perfect)
            }
            else if (currentProgress <= perfectZoneSize + goodZoneSize)
            {
                return goodColor; // Yellow zone (good)
            }
            else
            {
                return failColor; // Red zone (fail)
            }
        }
        
        /// <summary>
        /// Get save result based on current progress
        /// </summary>
        private SaveResult GetCurrentResult()
        {
            if (currentProgress <= perfectZoneSize)
            {
                return SaveResult.Perfect;
            }
            else if (currentProgress <= perfectZoneSize + goodZoneSize)
            {
                return SaveResult.Good;
            }
            else
            {
                return SaveResult.Fail;
            }
        }
        
        /// <summary>
        /// Handle player input during timing window
        /// </summary>
        private void HandleInput()
        {
            if (inputPressed) return;
            
            // Check for save input (A button or touch)
            bool savePressed = Input.GetKeyDown(KeyCode.A) || Input.GetMouseButtonDown(0);
            
            if (savePressed)
            {
                inputPressed = true;
                finalResult = GetCurrentResult();
                CompleteSave();
            }
        }
        
        /// <summary>
        /// Complete the save attempt with result
        /// </summary>
        private void CompleteSave()
        {
            isActive = false;
            
            // Play appropriate sound
            switch (finalResult)
            {
                case SaveResult.Perfect:
                    PlaySound(perfectSaveSound);
                    break;
                case SaveResult.Good:
                    PlaySound(goodSaveSound);
                    break;
                case SaveResult.Fail:
                    PlaySound(failSound);
                    break;
            }
            
            UnityEngine.Debug.Log($"Goalie save completed: {finalResult} (Progress: {currentProgress:F2})");
            
            // Notify callbacks
            onSaveResult?.Invoke(finalResult);
            OnSaveCompleted?.Invoke(finalResult);
            
            // Hide UI after brief delay
            StartCoroutine(HideAfterDelay(0.5f));
        }
        
        /// <summary>
        /// Timeout coroutine - triggers fail if no input
        /// </summary>
        private IEnumerator TimingBarTimeout()
        {
            yield return new WaitForSeconds(totalDuration);
            
            if (isActive && !inputPressed)
            {
                // Time's up - automatic fail
                inputPressed = true;
                finalResult = SaveResult.Fail;
                CompleteSave();
            }
        }
        
        /// <summary>
        /// Hide UI after delay
        /// </summary>
        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideTimingBar();
        }
        
        /// <summary>
        /// Play audio clip if available
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        /// <summary>
        /// Set save input externally (for mobile or other input systems)
        /// </summary>
        public void TriggerSaveInput()
        {
            if (isActive && !inputPressed)
            {
                inputPressed = true;
                finalResult = GetCurrentResult();
                CompleteSave();
            }
        }
        
        /// <summary>
        /// Check if timing bar is currently active
        /// </summary>
        public bool IsActive => isActive;
        
        /// <summary>
        /// Get current progress (0-1, where 1 is start, 0 is end)
        /// </summary>
        public float CurrentProgress => currentProgress;
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying || !isActive) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
            GUILayout.Label("=== Timing Bar Debug ===");
            GUILayout.Label($"Active: {isActive}");
            GUILayout.Label($"Progress: {currentProgress:F3}");
            GUILayout.Label($"Current Zone: {GetCurrentResult()}");
            GUILayout.Label($"Input Pressed: {inputPressed}");
            GUILayout.Label($"Time Remaining: {(totalDuration - (Time.time - startTime)):F2}s");
            
            GUILayout.Label("Zone Breakdown:");
            GUILayout.Label($"Perfect: 0.00 - {perfectZoneSize:F2}");
            GUILayout.Label($"Good: {perfectZoneSize:F2} - {(perfectZoneSize + goodZoneSize):F2}");
            GUILayout.Label($"Fail: {(perfectZoneSize + goodZoneSize):F2} - 1.00");
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
