using UnityEngine;
using System.Collections;

namespace PlushLeague.Polish
{
    /// <summary>
    /// Integration manager for all polish systems (Audio, VFX, Animation, UI Theme).
    /// Coordinates feedback between gameplay events and polish systems.
    /// </summary>
    public class PolishIntegrationManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PlushLeague.Audio.AudioManager audioManager;
        [SerializeField] private PlushLeague.VFX.VFXManager vfxManager;
        [SerializeField] private PlushLeague.UI.Theme.UIThemeManager uiThemeManager;
        
        [Header("Polish Settings")]
        [SerializeField] private bool enablePolishSystems = true;
        [SerializeField] private bool enableAudioFeedback = true;
        [SerializeField] private bool enableVisualFeedback = true;
        [SerializeField] private bool enableHapticFeedback = false;
        
        // Singleton instance
        private static PolishIntegrationManager _instance;
        public static PolishIntegrationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PolishIntegrationManager>();
                    if (_instance == null)
                    {
                        var polishManagerObject = new GameObject("PolishIntegrationManager");
                        _instance = polishManagerObject.AddComponent<PolishIntegrationManager>();
                    }
                }
                return _instance;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystems();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ConnectToGameSystems();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize all polish systems
        /// </summary>
        private void InitializeSystems()
        {
            if (!enablePolishSystems) return;
            
            // Get or create AudioManager
            if (audioManager == null)
            {
                audioManager = PlushLeague.Audio.AudioManager.Instance;
            }
            
            // Get or create VFXManager
            if (vfxManager == null)
            {
                vfxManager = PlushLeague.VFX.VFXManager.Instance;
            }
            
            // Get or create UIThemeManager
            if (uiThemeManager == null)
            {
                uiThemeManager = PlushLeague.UI.Theme.UIThemeManager.Instance;
            }
        }
        
        /// <summary>
        /// Connect to game systems for event-driven feedback
        /// </summary>
        private void ConnectToGameSystems()
        {
            // Connect to GameManager events
            var gameManager = PlushLeague.Core.GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                gameManager.OnMatchCompleted += OnMatchCompleted;
                gameManager.OnSceneTransitionStarted += OnSceneTransitionStarted;
            }
        }
        
        #endregion
        
        #region Gameplay Feedback Methods
        
        /// <summary>
        /// Handle ball kick event with audio and visual feedback
        /// </summary>
        public void OnBallKicked(Vector3 kickPosition, float kickForce)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlayKickSound();
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerKickEffect(kickPosition);
            }
            
            // Haptic feedback
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticType.Light);
            }
        }
        
        /// <summary>
        /// Handle ball bounce event
        /// </summary>
        public void OnBallBounced(Vector3 bouncePosition)
        {
            if (!enablePolishSystems) return;
            
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlayBounceSound();
            }
            
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerBounceEffect(bouncePosition);
            }
        }
        
        /// <summary>
        /// Handle player tackle event
        /// </summary>
        public void OnPlayerTackle(Vector3 tacklePosition, GameObject tackledPlayer)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlayTackleSound();
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerTackleEffect(tacklePosition);
            }
            
            // Animation feedback
            if (tackledPlayer != null)
            {
                var animController = tackledPlayer.GetComponent<PlushLeague.Animation.PlushAnimationController>();
                if (animController != null)
                {
                    animController.StartTackleAnimation();
                }
            }
            
            // Haptic feedback
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticType.Medium);
            }
        }
        
        /// <summary>
        /// Handle goal scored event
        /// </summary>
        public void OnGoalScored(Vector3 goalPosition, bool playerScored)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlaySFX("goal_scored");
                if (playerScored)
                {
                    audioManager.PlaySFX("crowd_cheer");
                }
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerGoalEffect(goalPosition);
            }
            
            // Animation feedback for all players
            var players = FindObjectsByType<PlushLeague.Animation.PlushAnimationController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (playerScored)
                {
                    player.StartCelebrationAnimation();
                }
            }
            
            // Haptic feedback
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticType.Heavy);
            }
        }
        
        /// <summary>
        /// Handle goal save event
        /// </summary>
        public void OnGoalSaved(Vector3 savePosition, GameObject goalkeeper)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlaySFX("goal_saved");
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerSaveEffect(savePosition);
            }
            
            // Animation feedback
            if (goalkeeper != null)
            {
                var animController = goalkeeper.GetComponent<PlushLeague.Animation.PlushAnimationController>();
                if (animController != null)
                {
                    animController.StartCelebrationAnimation();
                }
            }
        }
        
        /// <summary>
        /// Handle superpower activation
        /// </summary>
        public void OnSuperpowerActivated(Vector3 playerPosition, string superpowerName, int superpowerId)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlaySuperpowerActivation();
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerSuperpowerActivation(playerPosition, superpowerId);
            }
            
            // Haptic feedback
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticType.Medium);
            }
        }
        
        /// <summary>
        /// Handle superpower impact
        /// </summary>
        public void OnSuperpowerImpact(Vector3 impactPosition, string superpowerName, int superpowerId)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlaySuperpowerImpact();
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerSuperpowerImpact(impactPosition, superpowerId);
            }
        }
        
        /// <summary>
        /// Handle UI button interaction
        /// </summary>
        public void OnUIButtonInteraction(string buttonType, Vector3 buttonPosition)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                switch (buttonType.ToLower())
                {
                    case "click":
                        audioManager.PlayUISound("button_click");
                        break;
                    case "hover":
                        audioManager.PlayUISound("button_hover");
                        break;
                    case "confirm":
                        audioManager.PlayUISound("confirm");
                        break;
                    case "cancel":
                        audioManager.PlayUISound("cancel");
                        break;
                    case "error":
                        audioManager.PlayUISound("error");
                        break;
                }
            }
            
            // Visual feedback
            if (enableVisualFeedback && vfxManager != null && buttonType.ToLower() == "click")
            {
                vfxManager.TriggerButtonClickEffect(buttonPosition);
            }
            
            // Light haptic feedback for UI
            if (enableHapticFeedback && buttonType.ToLower() == "click")
            {
                TriggerHapticFeedback(HapticType.Light);
            }
        }
        
        /// <summary>
        /// Handle match countdown
        /// </summary>
        public void OnCountdown(int count, bool isFinal = false)
        {
            if (!enablePolishSystems) return;
            
            // Audio feedback
            if (enableAudioFeedback && audioManager != null)
            {
                if (isFinal)
                {
                    audioManager.PlaySFX("countdown_final");
                }
                else
                {
                    audioManager.PlayCountdownBeep(count);
                }
            }
        }
        
        #endregion
        
        #region Game State Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(PlushLeague.Core.GameManager.GameState newState)
        {
            if (!enablePolishSystems) return;
            
            switch (newState)
            {
                case PlushLeague.Core.GameManager.GameState.Menu:
                    if (audioManager != null)
                    {
                        audioManager.StartMenuMusic();
                    }
                    break;
                    
                case PlushLeague.Core.GameManager.GameState.PowerSelection:
                    if (audioManager != null)
                    {
                        audioManager.StartPowerSelectionMusic();
                    }
                    break;
                    
                case PlushLeague.Core.GameManager.GameState.MatchActive:
                    if (audioManager != null)
                    {
                        audioManager.StartGameplayMusic();
                        audioManager.PlaySFX("whistle_start");
                    }
                    if (vfxManager != null)
                    {
                        vfxManager.StartAmbientEffects();
                    }
                    break;
                    
                case PlushLeague.Core.GameManager.GameState.MatchEnded:
                    if (audioManager != null)
                    {
                        audioManager.PlaySFX("whistle_end");
                    }
                    if (vfxManager != null)
                    {
                        vfxManager.StopAmbientEffects();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Handle match completion
        /// </summary>
        private void OnMatchCompleted(PlushLeague.Core.GameManager.MatchResult result)
        {
            if (!enablePolishSystems) return;
            
            if (result.playerWon)
            {
                // Victory feedback
                if (audioManager != null)
                {
                    audioManager.PlayVictoryMusic();
                }
                if (vfxManager != null)
                {
                    vfxManager.TriggerVictoryEffect(Vector3.zero);
                }
            }
            else
            {
                // Defeat feedback
                if (audioManager != null)
                {
                    audioManager.PlayDefeatMusic();
                }
                if (vfxManager != null)
                {
                    vfxManager.TriggerDefeatEffect(Vector3.zero);
                }
            }
        }
        
        /// <summary>
        /// Handle scene transitions
        /// </summary>
        private void OnSceneTransitionStarted()
        {
            if (!enablePolishSystems) return;
            
            if (vfxManager != null)
            {
                vfxManager.TriggerMenuTransition();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Haptic feedback types
        /// </summary>
        public enum HapticType
        {
            Light,
            Medium,
            Heavy
        }
        
        /// <summary>
        /// Trigger haptic feedback (mobile devices)
        /// </summary>
        private void TriggerHapticFeedback(HapticType type)
        {
            if (!enableHapticFeedback) return;
            
            #if UNITY_ANDROID || UNITY_IOS
            // Simple haptic feedback for mobile devices
            Handheld.Vibrate();
            #endif
        }
        
        /// <summary>
        /// Enable/disable all polish systems
        /// </summary>
        public void SetPolishEnabled(bool enabled)
        {
            enablePolishSystems = enabled;
            
            if (!enabled)
            {
                // Stop all active effects
                if (audioManager != null)
                {
                    audioManager.StopMusic();
                }
                if (vfxManager != null)
                {
                    vfxManager.StopAllEffects();
                }
            }
        }
        
        /// <summary>
        /// Set specific feedback types
        /// </summary>
        public void SetFeedbackEnabled(bool audio, bool visual, bool haptic)
        {
            enableAudioFeedback = audio;
            enableVisualFeedback = visual;
            enableHapticFeedback = haptic;
        }
        
        #endregion
        
        #region Public API for Manual Triggering
        
        /// <summary>
        /// Manually trigger audio feedback
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            if (enableAudioFeedback && audioManager != null)
            {
                audioManager.PlaySFX(sfxName);
            }
        }
        
        /// <summary>
        /// Manually trigger visual effect
        /// </summary>
        public void TriggerVFX(string vfxName, Vector3 position)
        {
            if (enableVisualFeedback && vfxManager != null)
            {
                vfxManager.TriggerVFX(vfxName, position);
            }
        }
        
        /// <summary>
        /// Manually trigger player animation
        /// </summary>
        public void AnimatePlush(GameObject player, string animationName)
        {
            if (player != null)
            {
                var animController = player.GetComponent<PlushLeague.Animation.PlushAnimationController>();
                if (animController != null)
                {
                    animController.AnimatePlush(animationName);
                }
            }
        }
        
        /// <summary>
        /// Manually update UI sprite
        /// </summary>
        public void UpdateUISprite(string uiElement, Sprite newSprite)
        {
            if (uiThemeManager != null)
            {
                uiThemeManager.UpdateUISprite(uiElement, newSprite);
            }
        }
        
        #endregion
    }
}
