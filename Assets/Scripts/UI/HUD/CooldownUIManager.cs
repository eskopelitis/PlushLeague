using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PlushLeague.UI.HUD
{
    /// <summary>
    /// Manages cooldown displays for all abilities and actions
    /// Provides centralized cooldown UI management with visual feedback
    /// </summary>
    public class CooldownUIManager : MonoBehaviour
    {
        [Header("Cooldown Display Settings")]
        [SerializeField] private GameObject cooldownUIPrefab;
        [SerializeField] private Transform cooldownContainer;
        [SerializeField] private float cooldownUISpacing = 60f;
        [SerializeField] private Vector2 cooldownUISize = new Vector2(50f, 50f);
        
        [Header("Visual Settings")]
        [SerializeField] private Color activeCooldownColor = Color.red;
        [SerializeField] private Color completedCooldownColor = Color.green;
        [SerializeField] private Color unavailableColor = Color.gray;
        [SerializeField] private float pulseSpeed = 2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip cooldownCompleteSound;
        [SerializeField] private AudioClip cooldownStartSound;
        
        // Runtime tracking
        private Dictionary<string, CooldownDisplay> activeCooldowns = new Dictionary<string, CooldownDisplay>();
        private AudioSource audioSource;
        
        // Cooldown display structure
        [System.Serializable]
        public class CooldownDisplay
        {
            public GameObject uiObject;
            public Image fillImage;
            public TextMeshProUGUI timeText;
            public Image iconImage;
            public string abilityName;
            public float totalTime;
            public float remainingTime;
            public bool isActive;
            
            public float Progress => totalTime > 0 ? (totalTime - remainingTime) / totalTime : 1f;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeManager();
        }
        
        private void Update()
        {
            UpdateActiveCooldowns();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the cooldown manager
        /// </summary>
        private void InitializeManager()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            if (cooldownContainer == null)
                cooldownContainer = transform;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start a cooldown for an ability
        /// </summary>
        public void StartCooldown(string abilityName, float cooldownTime, Sprite icon = null)
        {
            if (cooldownTime <= 0f) return;
            
            // Stop existing cooldown if any
            if (activeCooldowns.ContainsKey(abilityName))
            {
                StopCooldown(abilityName);
            }
            
            // Create new cooldown display
            CooldownDisplay cooldown = CreateCooldownDisplay(abilityName, cooldownTime, icon);
            activeCooldowns[abilityName] = cooldown;
            
            // Play start sound
            PlaySound(cooldownStartSound);
            
            // Position the UI element
            PositionCooldownUI(cooldown);
        }
        
        /// <summary>
        /// Stop a cooldown manually
        /// </summary>
        public void StopCooldown(string abilityName)
        {
            if (!activeCooldowns.ContainsKey(abilityName)) return;
            
            CooldownDisplay cooldown = activeCooldowns[abilityName];
            
            // Play completion sound
            PlaySound(cooldownCompleteSound);
            
            // Remove UI
            if (cooldown.uiObject != null)
                Destroy(cooldown.uiObject);
                
            activeCooldowns.Remove(abilityName);
            
            // Reposition remaining cooldowns
            RepositionCooldowns();
        }
        
        /// <summary>
        /// Check if an ability is on cooldown
        /// </summary>
        public bool IsOnCooldown(string abilityName)
        {
            return activeCooldowns.ContainsKey(abilityName) && activeCooldowns[abilityName].isActive;
        }
        
        /// <summary>
        /// Get remaining cooldown time
        /// </summary>
        public float GetRemainingCooldown(string abilityName)
        {
            if (!activeCooldowns.ContainsKey(abilityName)) return 0f;
            return activeCooldowns[abilityName].remainingTime;
        }
        
        /// <summary>
        /// Get cooldown progress (0-1)
        /// </summary>
        public float GetCooldownProgress(string abilityName)
        {
            if (!activeCooldowns.ContainsKey(abilityName)) return 1f;
            return activeCooldowns[abilityName].Progress;
        }
        
        /// <summary>
        /// Clear all cooldowns
        /// </summary>
        public void ClearAllCooldowns()
        {
            foreach (var cooldown in activeCooldowns.Values)
            {
                if (cooldown.uiObject != null)
                    Destroy(cooldown.uiObject);
            }
            
            activeCooldowns.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Create a new cooldown display UI element
        /// </summary>
        private CooldownDisplay CreateCooldownDisplay(string abilityName, float cooldownTime, Sprite icon)
        {
            GameObject uiObject = CreateCooldownUIObject();
            
            CooldownDisplay cooldown = new CooldownDisplay
            {
                uiObject = uiObject,
                abilityName = abilityName,
                totalTime = cooldownTime,
                remainingTime = cooldownTime,
                isActive = true
            };
            
            // Get UI components
            cooldown.fillImage = uiObject.transform.Find("Fill")?.GetComponent<Image>();
            cooldown.timeText = uiObject.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            cooldown.iconImage = uiObject.transform.Find("Icon")?.GetComponent<Image>();
            
            // Set initial values
            if (cooldown.fillImage != null)
            {
                cooldown.fillImage.fillAmount = 0f;
                cooldown.fillImage.color = activeCooldownColor;
            }
                
            if (cooldown.iconImage != null && icon != null)
                cooldown.iconImage.sprite = icon;
                
            if (cooldown.timeText != null)
                cooldown.timeText.text = cooldownTime.ToString("F1");
            
            return cooldown;
        }
        
        /// <summary>
        /// Create the UI GameObject for cooldown display
        /// </summary>
        private GameObject CreateCooldownUIObject()
        {
            if (cooldownUIPrefab != null)
            {
                return Instantiate(cooldownUIPrefab, cooldownContainer);
            }
            
            // Create basic UI if no prefab is assigned
            GameObject obj = new GameObject("CooldownUI");
            obj.transform.SetParent(cooldownContainer);
            
            // Add Image component
            Image bgImage = obj.AddComponent<Image>();
            bgImage.color = Color.black;
            
            // Add fill image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(obj.transform);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            
            // Add text
            GameObject textObj = new GameObject("TimeText");
            textObj.transform.SetParent(obj.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "0";
            text.fontSize = 12;
            text.alignment = TextAlignmentOptions.Center;
            
            // Add icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(obj.transform);
            Image iconImage = iconObj.AddComponent<Image>();
            
            // Set RectTransforms
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.sizeDelta = cooldownUISize;
            objRect.anchoredPosition = Vector2.zero;
            
            return obj;
        }
        
        /// <summary>
        /// Position a cooldown UI element
        /// </summary>
        private void PositionCooldownUI(CooldownDisplay cooldown)
        {
            if (cooldown.uiObject == null) return;
            
            RectTransform rect = cooldown.uiObject.GetComponent<RectTransform>();
            if (rect == null) return;
            
            // Position based on number of active cooldowns
            int index = 0;
            foreach (var kvp in activeCooldowns)
            {
                if (kvp.Value == cooldown) break;
                index++;
            }
            
            Vector2 position = new Vector2(index * cooldownUISpacing, 0);
            rect.anchoredPosition = position;
        }
        
        /// <summary>
        /// Reposition all cooldown UI elements
        /// </summary>
        private void RepositionCooldowns()
        {
            int index = 0;
            foreach (var cooldown in activeCooldowns.Values)
            {
                if (cooldown.uiObject != null)
                {
                    RectTransform rect = cooldown.uiObject.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        Vector2 position = new Vector2(index * cooldownUISpacing, 0);
                        rect.anchoredPosition = position;
                    }
                }
                index++;
            }
        }
        
        /// <summary>
        /// Update all active cooldowns
        /// </summary>
        private void UpdateActiveCooldowns()
        {
            List<string> completedCooldowns = new List<string>();
            
            foreach (var kvp in activeCooldowns)
            {
                CooldownDisplay cooldown = kvp.Value;
                
                if (cooldown.isActive)
                {
                    cooldown.remainingTime -= Time.deltaTime;
                    
                    if (cooldown.remainingTime <= 0f)
                    {
                        // Cooldown completed
                        cooldown.remainingTime = 0f;
                        cooldown.isActive = false;
                        completedCooldowns.Add(kvp.Key);
                    }
                    
                    // Update UI
                    UpdateCooldownUI(cooldown);
                }
            }
            
            // Remove completed cooldowns
            foreach (string abilityName in completedCooldowns)
            {
                StopCooldown(abilityName);
            }
        }
        
        /// <summary>
        /// Update a cooldown's UI display
        /// </summary>
        private void UpdateCooldownUI(CooldownDisplay cooldown)
        {
            if (cooldown.uiObject == null) return;
            
            // Update fill amount
            if (cooldown.fillImage != null)
            {
                cooldown.fillImage.fillAmount = cooldown.Progress;
                
                // Update color based on progress
                if (cooldown.Progress >= 1f)
                    cooldown.fillImage.color = completedCooldownColor;
                else
                    cooldown.fillImage.color = activeCooldownColor;
            }
            
            // Update time text
            if (cooldown.timeText != null)
            {
                if (cooldown.remainingTime > 0.1f)
                    cooldown.timeText.text = cooldown.remainingTime.ToString("F1");
                else
                    cooldown.timeText.text = "";
            }
            
            // Pulse effect for nearly completed cooldowns
            if (cooldown.remainingTime <= 3f && cooldown.remainingTime > 0f)
            {
                ApplyPulseEffect(cooldown);
            }
        }
        
        /// <summary>
        /// Apply pulse effect to cooldown UI
        /// </summary>
        private void ApplyPulseEffect(CooldownDisplay cooldown)
        {
            if (cooldown.uiObject == null) return;
            
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.1f + 1f;
            cooldown.uiObject.transform.localScale = Vector3.one * pulse;
        }
        
        /// <summary>
        /// Play audio feedback
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            ClearAllCooldowns();
        }
        
        #endregion
    }
}
