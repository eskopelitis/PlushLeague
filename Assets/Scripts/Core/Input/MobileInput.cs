using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PlushLeague.Core.Input
{
    /// <summary>
    /// Mobile input implementation using virtual joystick and touch buttons
    /// </summary>
    public class MobileInput : MonoBehaviour, IInputProvider, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Virtual Joystick")]
        [SerializeField] private RectTransform joystickArea;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRadius = 50f;
        [SerializeField] private bool showJoystick = true;
        
        [Header("Action Buttons")]
        [SerializeField] private Button sprintButton;
        [SerializeField] private Button actionButton;
        [SerializeField] private Button chipKickButton;
        [SerializeField] private Button slideTackleButton;
        [SerializeField] private Button goalieSaveButton;
        [SerializeField] private Button superpowerButton;
        
        [Header("Settings")]
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private bool snapToCenter = true;
        
        // Input state
        private Vector2 movementInput;
        private bool sprintHeld;
        private bool actionPressed;
        private bool actionHeld;
        private bool chipKickPressed;
        private bool slideTacklePressed;
        private bool goalieSavePressed;
        private bool superpowerPressed;
        
        // Joystick state
        private Vector2 joystickCenter;
        private bool isDragging;
        private int joystickPointerId = -1;
        
        // Properties
        public Vector2 MovementInput => movementInput;
        public bool SprintHeld => sprintHeld;
        public bool ActionPressed => actionPressed;
        public bool ActionHeld => actionHeld;
        public bool ChipKickPressed => chipKickPressed;
        public bool SlideTacklePressed => slideTacklePressed;
        public bool GoalieSavePressed => goalieSavePressed;
        public bool SuperpowerPressed => superpowerPressed;
        
        public void Initialize()
        {
            SetupJoystick();
            SetupButtons();
        }
        
        public void UpdateInput()
        {
            // Reset frame-based inputs
            actionPressed = false;
            chipKickPressed = false;
            slideTacklePressed = false;
            goalieSavePressed = false;
            superpowerPressed = false;
            
            // Update joystick visuals
            UpdateJoystickVisuals();
        }
        
        public void Cleanup()
        {
            // Cleanup if needed
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            UpdateInput();
        }
        
        #region Joystick Setup
        
        private void SetupJoystick()
        {
            if (joystickArea == null)
            {
                UnityEngine.Debug.LogWarning("MobileInput: Joystick area not assigned!");
                return;
            }
            
            // Store center position
            joystickCenter = joystickArea.anchoredPosition;
            
            // Hide joystick initially if snap to center is enabled
            if (snapToCenter && joystickHandle != null)
            {
                joystickHandle.gameObject.SetActive(showJoystick && !snapToCenter);
            }
        }
        
        private void SetupButtons()
        {
            // Setup sprint button
            if (sprintButton != null)
            {
                sprintButton.onClick.AddListener(() => { }); // Handled by pointer events
                
                var sprintEventTrigger = sprintButton.gameObject.GetComponent<EventTrigger>();
                if (sprintEventTrigger == null)
                    sprintEventTrigger = sprintButton.gameObject.AddComponent<EventTrigger>();
                
                // Sprint button down
                var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener((data) => { sprintHeld = true; });
                sprintEventTrigger.triggers.Add(pointerDown);
                
                // Sprint button up
                var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener((data) => { sprintHeld = false; });
                sprintEventTrigger.triggers.Add(pointerUp);
                
                // Sprint button exit (in case finger drags off)
                var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener((data) => { sprintHeld = false; });
                sprintEventTrigger.triggers.Add(pointerExit);
            }
            
            // Setup action button
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(() => { actionPressed = true; });
                
                var actionEventTrigger = actionButton.gameObject.GetComponent<EventTrigger>();
                if (actionEventTrigger == null)
                    actionEventTrigger = actionButton.gameObject.AddComponent<EventTrigger>();
                
                // Action button down
                var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener((data) => { 
                    actionPressed = true; 
                    actionHeld = true; 
                });
                actionEventTrigger.triggers.Add(pointerDown);
                
                // Action button up
                var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener((data) => { actionHeld = false; });
                actionEventTrigger.triggers.Add(pointerUp);
                
                // Action button exit
                var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener((data) => { actionHeld = false; });
                actionEventTrigger.triggers.Add(pointerExit);
            }
            
            // Setup chip kick button
            if (chipKickButton != null)
            {
                chipKickButton.onClick.AddListener(() => { chipKickPressed = true; });
            }
            
            // Setup slide tackle button
            if (slideTackleButton != null)
            {
                slideTackleButton.onClick.AddListener(() => { slideTacklePressed = true; });
            }
            
            // Setup goalie save button
            if (goalieSaveButton != null)
            {
                goalieSaveButton.onClick.AddListener(() => { goalieSavePressed = true; });
            }
            
            // Setup superpower button
            if (superpowerButton != null)
            {
                superpowerButton.onClick.AddListener(() => { superpowerPressed = true; });
            }
        }
        
        #endregion
        
        #region Joystick Input Handling
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (joystickArea == null) return;
            
            // Check if touch is within joystick area
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                isDragging = true;
                joystickPointerId = eventData.pointerId;
                
                if (snapToCenter)
                {
                    // Move joystick to touch position
                    joystickArea.anchoredPosition = localPoint;
                    joystickCenter = localPoint;
                    
                    if (joystickHandle != null)
                        joystickHandle.gameObject.SetActive(showJoystick);
                }
                
                UpdateJoystickInput(eventData);
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || eventData.pointerId != joystickPointerId) return;
            
            UpdateJoystickInput(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDragging || eventData.pointerId != joystickPointerId) return;
            
            isDragging = false;
            joystickPointerId = -1;
            movementInput = Vector2.zero;
            
            if (snapToCenter && joystickHandle != null)
            {
                joystickHandle.gameObject.SetActive(false);
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }
        
        private void UpdateJoystickInput(PointerEventData eventData)
        {
            if (joystickArea == null) return;
            
            // Convert screen point to local point in joystick area
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                // Calculate offset from center
                Vector2 offset = localPoint - joystickCenter;
                
                // Clamp to joystick radius
                float distance = offset.magnitude;
                if (distance > joystickRadius)
                {
                    offset = offset.normalized * joystickRadius;
                }
                
                // Calculate normalized input
                Vector2 normalizedInput = offset / joystickRadius;
                
                // Apply dead zone
                if (normalizedInput.magnitude < deadZone)
                {
                    movementInput = Vector2.zero;
                }
                else
                {
                    // Remap from deadzone to 1
                    float remappedMagnitude = (normalizedInput.magnitude - deadZone) / (1f - deadZone);
                    movementInput = normalizedInput.normalized * remappedMagnitude;
                }
                
                // Update handle position
                if (joystickHandle != null)
                {
                    joystickHandle.anchoredPosition = offset;
                }
            }
        }
        
        private void UpdateJoystickVisuals()
        {
            if (!showJoystick || joystickHandle == null) return;
            
            // Smooth return to center when not dragging
            if (!isDragging && !snapToCenter)
            {
                joystickHandle.anchoredPosition = Vector2.Lerp(
                    joystickHandle.anchoredPosition, Vector2.zero, Time.deltaTime * 10f);
            }
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(430, 10, 200, 150));
            GUILayout.Label("=== Mobile Input Debug ===");
            GUILayout.Label($"Movement: {movementInput.ToString("F2")}");
            GUILayout.Label($"Sprint: {sprintHeld}");
            GUILayout.Label($"Action Pressed: {actionPressed}");
            GUILayout.Label($"Action Held: {actionHeld}");
            GUILayout.Label($"Chip Kick: {chipKickPressed}");
            GUILayout.Label($"Slide Tackle: {slideTacklePressed}");
            GUILayout.Label($"Goalie Save: {goalieSavePressed}");
            GUILayout.Label($"Dragging: {isDragging}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
