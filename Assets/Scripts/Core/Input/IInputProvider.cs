using UnityEngine;

namespace PlushLeague.Core.Input
{
    /// <summary>
    /// Interface for input systems (mobile touch, keyboard, gamepad, etc.)
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Movement input as a normalized Vector2 (-1 to 1 on each axis)
        /// </summary>
        Vector2 MovementInput { get; }
        
        /// <summary>
        /// Whether the sprint button/action is currently held
        /// </summary>
        bool SprintHeld { get; }
        
        /// <summary>
        /// Whether the action button was pressed this frame (for kicks, etc.)
        /// </summary>
        bool ActionPressed { get; }
        
        /// <summary>
        /// Whether the action button is currently held
        /// </summary>
        bool ActionHeld { get; }
        
        /// <summary>
        /// Whether the chip kick button was pressed this frame
        /// </summary>
        bool ChipKickPressed { get; }
        
        /// <summary>
        /// Whether the slide tackle button was pressed this frame
        /// </summary>
        bool SlideTacklePressed { get; }
        
        /// <summary>
        /// Whether the goalie save button was pressed this frame
        /// </summary>
        bool GoalieSavePressed { get; }
        
        /// <summary>
        /// Whether the superpower button was pressed this frame (Y button)
        /// </summary>
        bool SuperpowerPressed { get; }
        
        /// <summary>
        /// Initialize the input provider
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Update input state (called each frame)
        /// </summary>
        void UpdateInput();
        
        /// <summary>
        /// Clean up resources
        /// </summary>
        void Cleanup();
    }
}
