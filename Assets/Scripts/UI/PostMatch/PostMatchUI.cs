using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PlushLeague.UI.PostMatch
{
    /// <summary>
    /// Post-match summary UI that displays match results, statistics, and provides options for rematch or return to menu.
    /// Integrates with GameManager for seamless game loop management.
    /// </summary>
    public class PostMatchUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private TextMeshProUGUI matchResultText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI matchDurationText;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private TextMeshProUGUI mvpText;
        
        [Header("Buttons")]
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Button shareResultsButton;
        
        [Header("Visual Effects")]
        [SerializeField] private Animator resultAnimator;
        [SerializeField] private ParticleSystem victoryEffect;
        [SerializeField] private ParticleSystem defeatEffect;
        [SerializeField] private GameObject mvpBadge;
        
        [Header("Audio")]
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip defeatSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float resultDisplayDelay = 1f;
        [SerializeField] private float statsAnimationDelay = 2f;
        
        // References
        private PlushLeague.Core.GameManager gameManager;
        private PlushLeague.Core.GameManager.MatchResult currentResult;
        private bool isInitialized = false;
        
        // Events
        public System.Action OnRematchRequested;
        public System.Action OnReturnToMenuRequested;
        public System.Action OnShareRequested;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupEventListeners();
            
            // Hide initially
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0f;
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            }
        }
        
        private void OnDestroy()
        {
            CleanupEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize components and references
        /// </summary>
        private void InitializeComponents()
        {
            // Find GameManager
            gameManager = PlushLeague.Core.GameManager.Instance;
            
            // Setup audio source
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            // Setup canvas group
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = GetComponent<CanvasGroup>();
                if (mainCanvasGroup == null)
                {
                    mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
        
        /// <summary>
        /// Setup event listeners for buttons and game manager
        /// </summary>
        private void SetupEventListeners()
        {
            // Button listeners
            if (rematchButton != null)
            {
                rematchButton.onClick.AddListener(OnRematchPressed);
            }
            
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(OnReturnToMenuPressed);
            }
            
            if (shareResultsButton != null)
            {
                shareResultsButton.onClick.AddListener(OnSharePressed);
            }
            
            // GameManager listeners
            if (gameManager != null)
            {
                gameManager.OnMatchCompleted += OnMatchCompleted;
            }
        }
        
        /// <summary>
        /// Clean up event listeners
        /// </summary>
        private void CleanupEventListeners()
        {
            // Button listeners
            if (rematchButton != null) rematchButton.onClick.RemoveAllListeners();
            if (returnToMenuButton != null) returnToMenuButton.onClick.RemoveAllListeners();
            if (shareResultsButton != null) shareResultsButton.onClick.RemoveAllListeners();
            
            // GameManager listeners
            if (gameManager != null)
            {
                gameManager.OnMatchCompleted -= OnMatchCompleted;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Show post-match summary with match result data
        /// </summary>
        public void ShowPostMatchSummary(PlushLeague.Core.GameManager.MatchResult result)
        {
            currentResult = result;
            StartCoroutine(ShowSummaryCoroutine(result));
        }
        
        /// <summary>
        /// Hide post-match summary
        /// </summary>
        public void HidePostMatchSummary()
        {
            StartCoroutine(HideSummaryCoroutine());
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle match completion from GameManager
        /// </summary>
        private void OnMatchCompleted(PlushLeague.Core.GameManager.MatchResult result)
        {
            ShowPostMatchSummary(result);
        }
        
        /// <summary>
        /// Handle rematch button press
        /// </summary>
        private void OnRematchPressed()
        {
            PlayButtonSound();
            
            // Check if rematch is available
            if (!gameManager?.WasLastMatchClean() ?? false)
            {
                UnityEngine.Debug.LogWarning("Rematch not available - previous match was not clean");
                ShowRematchUnavailableMessage();
                return;
            }
            
            OnRematchRequested?.Invoke();
            
            if (gameManager != null)
            {
                gameManager.OnRematchPressed();
            }
            
            HidePostMatchSummary();
        }
        
        /// <summary>
        /// Handle return to menu button press
        /// </summary>
        private void OnReturnToMenuPressed()
        {
            PlayButtonSound();
            OnReturnToMenuRequested?.Invoke();
            
            if (gameManager != null)
            {
                gameManager.OnReturnToMenuPressed();
            }
            
            HidePostMatchSummary();
        }
        
        /// <summary>
        /// Handle share results button press
        /// </summary>
        private void OnSharePressed()
        {
            PlayButtonSound();
            OnShareRequested?.Invoke();
            
            ShareMatchResults();
        }
        
        #endregion
        
        #region UI Animation and Display
        
        /// <summary>
        /// Show summary with animations
        /// </summary>
        private IEnumerator ShowSummaryCoroutine(PlushLeague.Core.GameManager.MatchResult result)
        {
            // Prepare UI elements
            PrepareUIElements(result);
            
            // Fade in main UI
            yield return StartCoroutine(FadeIn());
            
            // Delay before showing result
            yield return new WaitForSeconds(resultDisplayDelay);
            
            // Animate result text
            AnimateResultText(result);
            
            // Play result sound
            PlayResultSound(result.playerWon);
            
            // Show victory/defeat effects
            ShowResultEffects(result.playerWon);
            
            // Delay before showing stats
            yield return new WaitForSeconds(statsAnimationDelay);
            
            // Animate statistics
            AnimateStatistics(result);
            
            // Enable interaction
            EnableInteraction();
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Hide summary with animations
        /// </summary>
        private IEnumerator HideSummaryCoroutine()
        {
            // Disable interaction
            DisableInteraction();
            
            // Stop effects
            StopResultEffects();
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            isInitialized = false;
        }
        
        /// <summary>
        /// Prepare UI elements with match data
        /// </summary>
        private void PrepareUIElements(PlushLeague.Core.GameManager.MatchResult result)
        {
            // Set result text
            if (matchResultText != null)
            {
                matchResultText.text = result.playerWon ? "VICTORY!" : "DEFEAT";
                matchResultText.color = result.playerWon ? Color.green : Color.red;
            }
            
            // Set score text
            if (scoreText != null)
            {
                scoreText.text = $"{result.playerScore} - {result.opponentScore}";
            }
            
            // Set match duration
            if (matchDurationText != null)
            {
                int minutes = Mathf.FloorToInt(result.matchDuration / 60f);
                int seconds = Mathf.FloorToInt(result.matchDuration % 60f);
                matchDurationText.text = $"Match Time: {minutes:00}:{seconds:00}";
            }
            
            // Set player statistics
            if (playerStatsText != null)
            {
                playerStatsText.text = $"Goals: {result.playerGoals}\\nSaves: {result.playerSaves}\\nSuperpowers Used: {result.superpowerUsageCount}";
            }
            
            // Set MVP text
            if (mvpText != null)
            {
                mvpText.text = $"MVP: {result.mvpPlayerName}";
                if (mvpBadge != null)
                {
                    mvpBadge.SetActive(!string.IsNullOrEmpty(result.mvpPlayerName) && result.mvpPlayerName != "None");
                }
            }
            
            // Configure rematch button availability
            if (rematchButton != null)
            {
                rematchButton.interactable = result.wasCleanMatch;
                if (!result.wasCleanMatch && rematchButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                {
                    rematchButton.GetComponentInChildren<TextMeshProUGUI>().text = "Rematch\\n(Unavailable)";
                }
            }
        }
        
        /// <summary>
        /// Animate result text display
        /// </summary>
        private void AnimateResultText(PlushLeague.Core.GameManager.MatchResult result)
        {
            if (resultAnimator != null)
            {
                resultAnimator.SetTrigger(result.playerWon ? "Victory" : "Defeat");
            }
        }
        
        /// <summary>
        /// Animate statistics display
        /// </summary>
        private void AnimateStatistics(PlushLeague.Core.GameManager.MatchResult result)
        {
            if (resultAnimator != null)
            {
                resultAnimator.SetTrigger("ShowStats");
            }
        }
        
        #endregion
        
        #region Visual and Audio Effects
        
        /// <summary>
        /// Show victory or defeat effects
        /// </summary>
        private void ShowResultEffects(bool playerWon)
        {
            if (playerWon && victoryEffect != null)
            {
                victoryEffect.Play();
            }
            else if (!playerWon && defeatEffect != null)
            {
                defeatEffect.Play();
            }
        }
        
        /// <summary>
        /// Stop all result effects
        /// </summary>
        private void StopResultEffects()
        {
            if (victoryEffect != null) victoryEffect.Stop();
            if (defeatEffect != null) defeatEffect.Stop();
        }
        
        /// <summary>
        /// Play result sound
        /// </summary>
        private void PlayResultSound(bool playerWon)
        {
            if (audioSource == null) return;
            
            AudioClip clipToPlay = playerWon ? victorySound : defeatSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound()
        {
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Fade in the UI
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (mainCanvasGroup == null) yield break;
            
            mainCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                mainCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            
            mainCanvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// Fade out the UI
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (mainCanvasGroup == null) yield break;
            
            float elapsed = 0f;
            float startAlpha = mainCanvasGroup.alpha;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                mainCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeInDuration);
                yield return null;
            }
            
            mainCanvasGroup.alpha = 0f;
            mainCanvasGroup.blocksRaycasts = false;
        }
        
        /// <summary>
        /// Enable UI interaction
        /// </summary>
        private void EnableInteraction()
        {
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.interactable = true;
            }
        }
        
        /// <summary>
        /// Disable UI interaction
        /// </summary>
        private void DisableInteraction()
        {
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.interactable = false;
            }
        }
        
        /// <summary>
        /// Show message when rematch is unavailable
        /// </summary>
        private void ShowRematchUnavailableMessage()
        {
            if (rematchButton != null && rematchButton.GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                var buttonText = rematchButton.GetComponentInChildren<TextMeshProUGUI>();
                string originalText = buttonText.text;
                buttonText.text = "Match Error - No Rematch";
                buttonText.color = Color.red;
                
                StartCoroutine(ResetButtonText(buttonText, originalText, 3f));
            }
        }
        
        /// <summary>
        /// Reset button text after a delay
        /// </summary>
        private IEnumerator ResetButtonText(TextMeshProUGUI textComponent, string originalText, float delay)
        {
            yield return new WaitForSeconds(delay);
            textComponent.text = originalText;
            textComponent.color = Color.white;
        }
        
        /// <summary>
        /// Share match results (placeholder for future social features)
        /// </summary>
        private void ShareMatchResults()
        {
            string shareText = $"I just played Plush League! Score: {currentResult.playerScore}-{currentResult.opponentScore}. ";
            shareText += currentResult.playerWon ? "Victory! 🏆" : "Good game! 🎮";
            
            UnityEngine.Debug.Log($"Sharing results: {shareText}");
            
            // Future implementation: integrate with platform sharing APIs
            // For now, just copy to clipboard if possible
            
            #if UNITY_EDITOR || UNITY_STANDALONE
            GUIUtility.systemCopyBuffer = shareText;
            UnityEngine.Debug.Log("Results copied to clipboard!");
            #endif
        }
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool enableDebugGUI = false;
        
        private void OnGUI()
        {
            if (!enableDebugGUI || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
            GUILayout.Label("=== Post-Match UI Debug ===");
            GUILayout.Label($"Initialized: {isInitialized}");
            GUILayout.Label($"GameManager: {(gameManager != null ? "Found" : "Missing")}");
            
            if (GUILayout.Button("Test Victory"))
            {
                var testResult = new PlushLeague.Core.GameManager.MatchResult(
                    true, 3, 1, 300f, 2, 1, 3, "Player", true
                );
                ShowPostMatchSummary(testResult);
            }
            
            if (GUILayout.Button("Test Defeat"))
            {
                var testResult = new PlushLeague.Core.GameManager.MatchResult(
                    false, 1, 3, 300f, 1, 2, 2, "Opponent", true
                );
                ShowPostMatchSummary(testResult);
            }
            
            if (GUILayout.Button("Hide Summary"))
            {
                HidePostMatchSummary();
            }
            
            GUILayout.EndArea();
        }
        #endif
        
        #endregion
    }
}
