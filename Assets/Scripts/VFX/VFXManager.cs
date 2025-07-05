using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PlushLeague.VFX
{
    /// <summary>
    /// Visual effects manager for Plush League.
    /// Handles all particle effects, animations, and visual feedback with a plush toy theme.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("Goal Effects")]
        [SerializeField] private ParticleSystem goalConfetti;
        [SerializeField] private ParticleSystem goalFireworks;
        [SerializeField] private ParticleSystem goalSparkles;
        [SerializeField] private GameObject goalTextEffect;
        
        [Header("Gameplay Effects")]
        [SerializeField] private ParticleSystem tackleCloud;
        [SerializeField] private ParticleSystem ballTrail;
        [SerializeField] private ParticleSystem bounceEffect;
        [SerializeField] private ParticleSystem kickEffect;
        [SerializeField] private ParticleSystem saveEffect;
        
        [Header("Superpower Effects")]
        [SerializeField] private ParticleSystem[] superpowerActivation;
        [SerializeField] private ParticleSystem[] superpowerImpact;
        [SerializeField] private ParticleSystem superpowerCharge;
        [SerializeField] private ParticleSystem superpowerAura;
        
        [Header("UI Effects")]
        [SerializeField] private ParticleSystem buttonClickEffect;
        [SerializeField] private ParticleSystem menuTransitionEffect;
        [SerializeField] private ParticleSystem powerSelectionGlow;
        [SerializeField] private ParticleSystem victoryEffect;
        [SerializeField] private ParticleSystem defeatEffect;
        
        [Header("Ambient Effects")]
        [SerializeField] private ParticleSystem crowdCheerEffect;
        [SerializeField] private ParticleSystem windEffect;
        [SerializeField] private ParticleSystem dustMotes;
        [SerializeField] private ParticleSystem fieldSparkles;
        
        [Header("Effect Settings")]
        [SerializeField] private bool enableVFX = true;
        [SerializeField] private float effectDuration = 2f;
        [SerializeField] private int maxActiveEffects = 10;
        [SerializeField] private bool poolEffects = true;
        
        // Singleton instance
        private static VFXManager _instance;
        public static VFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<VFXManager>();
                    if (_instance == null)
                    {
                        var vfxManagerObject = new GameObject("VFXManager");
                        _instance = vfxManagerObject.AddComponent<VFXManager>();
                    }
                }
                return _instance;
            }
        }
        
        // Effect management
        private Dictionary<string, ParticleSystem> effectsPool = new Dictionary<string, ParticleSystem>();
        private List<ParticleSystem> activeEffects = new List<ParticleSystem>();
        private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeEffects();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            RegisterEffectPrefabs();
        }
        
        private void Update()
        {
            UpdateActiveEffects();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize effect systems
        /// </summary>
        private void InitializeEffects()
        {
            // Create effect pools if needed
            if (poolEffects)
            {
                CreateEffectPools();
            }
        }
        
        /// <summary>
        /// Register effect prefabs for easy access
        /// </summary>
        private void RegisterEffectPrefabs()
        {
            // Goal effects
            if (goalConfetti != null) effectPrefabs["goal_confetti"] = goalConfetti.gameObject;
            if (goalFireworks != null) effectPrefabs["goal_fireworks"] = goalFireworks.gameObject;
            if (goalSparkles != null) effectPrefabs["goal_sparkles"] = goalSparkles.gameObject;
            if (goalTextEffect != null) effectPrefabs["goal_text"] = goalTextEffect;
            
            // Gameplay effects
            if (tackleCloud != null) effectPrefabs["tackle_cloud"] = tackleCloud.gameObject;
            if (ballTrail != null) effectPrefabs["ball_trail"] = ballTrail.gameObject;
            if (bounceEffect != null) effectPrefabs["bounce_effect"] = bounceEffect.gameObject;
            if (kickEffect != null) effectPrefabs["kick_effect"] = kickEffect.gameObject;
            if (saveEffect != null) effectPrefabs["save_effect"] = saveEffect.gameObject;
            
            // UI effects
            if (buttonClickEffect != null) effectPrefabs["button_click"] = buttonClickEffect.gameObject;
            if (menuTransitionEffect != null) effectPrefabs["menu_transition"] = menuTransitionEffect.gameObject;
            if (powerSelectionGlow != null) effectPrefabs["power_glow"] = powerSelectionGlow.gameObject;
            if (victoryEffect != null) effectPrefabs["victory"] = victoryEffect.gameObject;
            if (defeatEffect != null) effectPrefabs["defeat"] = defeatEffect.gameObject;
            
            // Ambient effects
            if (crowdCheerEffect != null) effectPrefabs["crowd_cheer"] = crowdCheerEffect.gameObject;
            if (windEffect != null) effectPrefabs["wind"] = windEffect.gameObject;
            if (dustMotes != null) effectPrefabs["dust_motes"] = dustMotes.gameObject;
            if (fieldSparkles != null) effectPrefabs["field_sparkles"] = fieldSparkles.gameObject;
        }
        
        /// <summary>
        /// Create effect pools for better performance
        /// </summary>
        private void CreateEffectPools()
        {
            foreach (var effectPair in effectPrefabs)
            {
                if (effectPair.Value != null)
                {
                    var pooledEffect = Instantiate(effectPair.Value, transform);
                    pooledEffect.SetActive(false);
                    
                    var particleSystem = pooledEffect.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        effectsPool[effectPair.Key] = particleSystem;
                    }
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Trigger a visual effect at a specific position
        /// </summary>
        public void TriggerVFX(string vfxName, Vector3 position)
        {
            if (!enableVFX) return;
            
            ParticleSystem effect = GetEffect(vfxName);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.gameObject.SetActive(true);
                effect.Play();
                
                AddToActiveEffects(effect);
                StartCoroutine(DisableEffectAfterDuration(effect, effectDuration));
            }
            else
            {
                UnityEngine.Debug.LogWarning($"VFXManager: Effect '{vfxName}' not found");
            }
        }
        
        /// <summary>
        /// Trigger a visual effect at a specific position with custom duration
        /// </summary>
        public void TriggerVFX(string vfxName, Vector3 position, float duration)
        {
            if (!enableVFX) return;
            
            ParticleSystem effect = GetEffect(vfxName);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.gameObject.SetActive(true);
                effect.Play();
                
                AddToActiveEffects(effect);
                StartCoroutine(DisableEffectAfterDuration(effect, duration));
            }
        }
        
        /// <summary>
        /// Trigger a visual effect on a specific GameObject
        /// </summary>
        public void TriggerVFX(string vfxName, GameObject target)
        {
            if (!enableVFX || target == null) return;
            
            TriggerVFX(vfxName, target.transform.position);
        }
        
        /// <summary>
        /// Trigger goal celebration effects
        /// </summary>
        public void TriggerGoalEffect(Vector3 goalPosition)
        {
            if (!enableVFX) return;
            
            // Play multiple effects for big celebration
            TriggerVFX("goal_confetti", goalPosition);
            TriggerVFX("goal_fireworks", goalPosition + Vector3.up * 2f);
            TriggerVFX("goal_sparkles", goalPosition);
            
            // Play goal text effect
            if (goalTextEffect != null)
            {
                var textEffect = Instantiate(goalTextEffect, goalPosition, Quaternion.identity);
                StartCoroutine(DisableGameObjectAfterDuration(textEffect, 3f));
            }
        }
        
        /// <summary>
        /// Trigger tackle effect
        /// </summary>
        public void TriggerTackleEffect(Vector3 tacklePosition)
        {
            TriggerVFX("tackle_cloud", tacklePosition);
        }
        
        /// <summary>
        /// Trigger ball kick effect
        /// </summary>
        public void TriggerKickEffect(Vector3 kickPosition)
        {
            TriggerVFX("kick_effect", kickPosition);
        }
        
        /// <summary>
        /// Trigger ball bounce effect
        /// </summary>
        public void TriggerBounceEffect(Vector3 bouncePosition)
        {
            TriggerVFX("bounce_effect", bouncePosition);
        }
        
        /// <summary>
        /// Trigger save effect
        /// </summary>
        public void TriggerSaveEffect(Vector3 savePosition)
        {
            TriggerVFX("save_effect", savePosition);
        }
        
        /// <summary>
        /// Trigger superpower activation effect
        /// </summary>
        public void TriggerSuperpowerActivation(Vector3 playerPosition, int powerId = 0)
        {
            if (!enableVFX) return;
            
            if (superpowerActivation != null && superpowerActivation.Length > 0)
            {
                int effectIndex = Mathf.Clamp(powerId, 0, superpowerActivation.Length - 1);
                var effect = superpowerActivation[effectIndex];
                
                if (effect != null)
                {
                    effect.transform.position = playerPosition;
                    effect.gameObject.SetActive(true);
                    effect.Play();
                    
                    AddToActiveEffects(effect);
                    StartCoroutine(DisableEffectAfterDuration(effect, effectDuration));
                }
            }
        }
        
        /// <summary>
        /// Trigger superpower impact effect
        /// </summary>
        public void TriggerSuperpowerImpact(Vector3 impactPosition, int powerId = 0)
        {
            if (!enableVFX) return;
            
            if (superpowerImpact != null && superpowerImpact.Length > 0)
            {
                int effectIndex = Mathf.Clamp(powerId, 0, superpowerImpact.Length - 1);
                var effect = superpowerImpact[effectIndex];
                
                if (effect != null)
                {
                    effect.transform.position = impactPosition;
                    effect.gameObject.SetActive(true);
                    effect.Play();
                    
                    AddToActiveEffects(effect);
                    StartCoroutine(DisableEffectAfterDuration(effect, effectDuration));
                }
            }
        }
        
        /// <summary>
        /// Trigger UI button click effect
        /// </summary>
        public void TriggerButtonClickEffect(Vector3 buttonPosition)
        {
            TriggerVFX("button_click", buttonPosition, 0.5f);
        }
        
        /// <summary>
        /// Trigger menu transition effect
        /// </summary>
        public void TriggerMenuTransition()
        {
            if (menuTransitionEffect != null)
            {
                menuTransitionEffect.gameObject.SetActive(true);
                menuTransitionEffect.Play();
            }
        }
        
        /// <summary>
        /// Trigger victory effect
        /// </summary>
        public void TriggerVictoryEffect(Vector3 centerPosition)
        {
            TriggerVFX("victory", centerPosition, 5f);
        }
        
        /// <summary>
        /// Trigger defeat effect
        /// </summary>
        public void TriggerDefeatEffect(Vector3 centerPosition)
        {
            TriggerVFX("defeat", centerPosition, 3f);
        }
        
        /// <summary>
        /// Start ambient field effects
        /// </summary>
        public void StartAmbientEffects()
        {
            if (!enableVFX) return;
            
            if (dustMotes != null)
            {
                dustMotes.gameObject.SetActive(true);
                dustMotes.Play();
            }
            
            if (fieldSparkles != null)
            {
                fieldSparkles.gameObject.SetActive(true);
                fieldSparkles.Play();
            }
        }
        
        /// <summary>
        /// Stop ambient field effects
        /// </summary>
        public void StopAmbientEffects()
        {
            if (dustMotes != null)
            {
                dustMotes.Stop();
                dustMotes.gameObject.SetActive(false);
            }
            
            if (fieldSparkles != null)
            {
                fieldSparkles.Stop();
                fieldSparkles.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Stop all effects
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                {
                    effect.Stop();
                    effect.gameObject.SetActive(false);
                }
            }
            
            activeEffects.Clear();
        }
        
        /// <summary>
        /// Toggle VFX on/off
        /// </summary>
        public void ToggleVFX(bool enabled)
        {
            enableVFX = enabled;
            if (!enabled)
            {
                StopAllEffects();
            }
        }
        
        #endregion
        
        #region Effect Management
        
        /// <summary>
        /// Get an effect from the pool or create a new one
        /// </summary>
        private ParticleSystem GetEffect(string effectName)
        {
            if (poolEffects && effectsPool.ContainsKey(effectName))
            {
                return effectsPool[effectName];
            }
            
            if (effectPrefabs.ContainsKey(effectName))
            {
                var effectObject = Instantiate(effectPrefabs[effectName], transform);
                return effectObject.GetComponent<ParticleSystem>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Add effect to active effects list
        /// </summary>
        private void AddToActiveEffects(ParticleSystem effect)
        {
            if (activeEffects.Count >= maxActiveEffects)
            {
                // Remove oldest effect
                var oldestEffect = activeEffects[0];
                if (oldestEffect != null)
                {
                    oldestEffect.Stop();
                    oldestEffect.gameObject.SetActive(false);
                }
                activeEffects.RemoveAt(0);
            }
            
            activeEffects.Add(effect);
        }
        
        /// <summary>
        /// Update active effects and remove finished ones
        /// </summary>
        private void UpdateActiveEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] == null || (!activeEffects[i].isPlaying && !activeEffects[i].gameObject.activeInHierarchy))
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Disable effect after specified duration
        /// </summary>
        private IEnumerator DisableEffectAfterDuration(ParticleSystem effect, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (effect != null)
            {
                effect.Stop();
                
                // Wait for particles to finish
                yield return new WaitForSeconds(effect.main.startLifetime.constantMax);
                
                effect.gameObject.SetActive(false);
                
                // Remove from active effects
                activeEffects.Remove(effect);
            }
        }
        
        /// <summary>
        /// Disable GameObject after specified duration
        /// </summary>
        private IEnumerator DisableGameObjectAfterDuration(GameObject obj, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
        
        #endregion
    }
}
