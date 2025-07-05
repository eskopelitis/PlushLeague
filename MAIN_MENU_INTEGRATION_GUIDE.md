# Main Menu System Integration Guide

## Overview

The Main Menu system provides a polished entry point for Plush League, featuring a modern UI with smooth transitions, audio feedback, and seamless integration with the game flow. This guide explains how to set up and use the Main Menu system.

## Key Components

### 1. MainMenuUI.cs
- **Location**: `Assets/Scripts/UI/Menu/MainMenuUI.cs`
- **Purpose**: Core main menu controller with navigation, transitions, and audio
- **Features**:
  - Play, Customize, Settings, Quit buttons
  - Smooth fade-in/out transitions
  - Audio feedback (music, hover sounds, click sounds)
  - Integration with GameManager for seamless game flow
  - Feature flags for MVP control
  - Debug mode for testing

### 2. MainMenuSetupExample.cs
- **Location**: `Assets/Scripts/Examples/MainMenuSetupExample.cs`
- **Purpose**: Example script showing how to programmatically create a main menu
- **Features**:
  - Auto-setup functionality
  - Canvas creation and configuration
  - Button creation with proper styling
  - Audio setup
  - Event subscription examples

## Setup Instructions

### Method 1: Using Existing Scene (Recommended)
1. Open the `MainMenu.unity` scene in `Assets/Scenes/`
2. Find the MainMenu GameObject
3. Attach the `MainMenuUI` component
4. Configure the inspector fields:
   - **UI References**: Assign buttons, title text, logo image
   - **Scene Configuration**: Set scene names for navigation
   - **Visual Effects**: Setup canvas group, animator, particles
   - **Audio**: Assign music clips and sound effects
   - **Feature Flags**: Enable/disable buttons as needed

### Method 2: Programmatic Setup
1. Create an empty GameObject in your scene
2. Attach the `MainMenuSetupExample` script
3. Configure the settings in the inspector
4. Enable `autoSetupOnStart` or call `SetupMainMenu()` manually
5. The script will automatically create all required UI elements

## Configuration Options

### Scene Names
```csharp
[SerializeField] private string gameSceneName = "GameArena";
[SerializeField] private string settingsSceneName = "Settings";
[SerializeField] private string customizeSceneName = "Customize";
```

### Feature Flags (MVP Control)
```csharp
[SerializeField] private bool enableCustomizeButton = false; // MVP: Can be disabled
[SerializeField] private bool enableSettingsButton = true;
[SerializeField] private bool enableQuitButton = true;
```

### Game Flow Integration
```csharp
[SerializeField] private bool useIntegratedGameFlow = true; // Use GameManager for flow
```
- **True**: Uses GameManager for seamless transitions (recommended)
- **False**: Direct scene loading (fallback)

### Visual Effects
```csharp
[SerializeField] private CanvasGroup menuCanvasGroup; // For fading
[SerializeField] private Animator menuAnimator; // For animations
[SerializeField] private ParticleSystem backgroundEffect; // Background particles
[SerializeField] private GameObject plushAvatar; // Animated character
```

### Audio Configuration
```csharp
[SerializeField] private AudioClip menuMusicClip; // Background music
[SerializeField] private AudioClip buttonHoverSound; // Hover feedback
[SerializeField] private AudioClip buttonClickSound; // Click feedback
[SerializeField] private float menuMusicVolume = 0.5f; // Music volume
```

## Integration with GameManager

The MainMenuUI integrates seamlessly with the GameManager for proper game flow:

### Starting the Game
```csharp
// When Play button is pressed:
if (useIntegratedGameFlow && gameManager != null)
{
    gameManager.StartNewGame(false); // Start single-player by default
}
```

### Game Manager Events
The GameManager handles the transition from main menu to power selection to match:
1. **Menu State**: Player sees main menu
2. **Power Selection State**: Player chooses superpower and role
3. **Match Setup State**: Game prepares the match
4. **Match Active State**: Gameplay begins

## Events and Callbacks

### Public Events
```csharp
public System.Action OnPlayRequested;
public System.Action OnCustomizeRequested;
public System.Action OnSettingsRequested;
public System.Action OnQuitRequested;
```

### Usage Example
```csharp
var mainMenu = FindObjectOfType<MainMenuUI>();
mainMenu.OnPlayRequested += () => Debug.Log("Player wants to play!");
mainMenu.OnQuitRequested += () => Debug.Log("Player wants to quit!");
```

## Public API

### Core Methods
```csharp
public void ShowMenu(); // Show the menu (fade in)
public void HideMenu(); // Hide the menu (fade out)
public bool IsTransitioning { get; } // Check if transitioning
```

### Button Handlers (can be called directly)
```csharp
public void OnPlayPressed();
public void OnCustomizePressed();
public void OnSettingsPressed();
public void OnQuitPressed();
```

## Audio Management

### Background Music
- Plays automatically when menu initializes
- Fades out smoothly during transitions
- Loops continuously while menu is active

### Sound Effects
- **Hover Sound**: Plays when mouse enters button
- **Click Sound**: Plays when button is pressed
- **Volume Control**: Configurable via inspector

### Audio Setup
```csharp
// Music will start automatically, or manually:
private void StartMenuMusic()
{
    if (audioSource != null && menuMusicClip != null)
    {
        audioSource.clip = menuMusicClip;
        audioSource.volume = menuMusicVolume;
        audioSource.loop = true;
        audioSource.Play();
    }
}
```

## Visual Effects and Animations

### Fade Transitions
- Smooth fade-in when menu appears
- Smooth fade-out when transitioning to game
- Configurable transition duration

### Animations (Optional)
- Title text animation on show
- Button hover effects
- Plush avatar idle animations
- Background particle effects

### Animation Setup
```csharp
// Trigger animations via Animator component:
if (menuAnimator != null)
{
    menuAnimator.SetTrigger("ShowTitle");
    menuAnimator.SetTrigger("FadeOut");
}
```

## Testing and Debug Features

### Debug Mode
Enable `debugMode` in the inspector to access debug GUI:
- Test play flow
- Reset menu state
- Monitor transition status
- Check music playback

### Debug GUI Features
```csharp
#if UNITY_EDITOR
private void OnGUI()
{
    if (!debugMode) return;
    
    // Show debug information
    GUILayout.Label($"Initialized: {isInitialized}");
    GUILayout.Label($"Transitioning: {isTransitioning}");
    GUILayout.Label($"Music Playing: {audioSource?.isPlaying}");
    
    // Debug buttons
    if (GUILayout.Button("Test Play Flow"))
        OnPlayPressed();
}
#endif
```

## Common Issues and Solutions

### Issue: Buttons Not Responding
**Solution**: Check that:
- Canvas has a GraphicRaycaster component
- EventSystem exists in the scene
- Buttons are not disabled during transition

### Issue: Audio Not Playing
**Solution**: Check that:
- AudioSource component is present
- Audio clips are assigned
- Volume is not set to 0
- Audio Listener exists in the scene

### Issue: GameManager Not Found
**Solution**: Check that:
- GameManager exists in the scene as a singleton
- `useIntegratedGameFlow` is enabled
- GameManager scene references are set correctly

### Issue: Scene Transitions Not Working
**Solution**: Check that:
- Scene names are correct in Build Settings
- Scenes are added to Build Settings
- Scene names match the configured strings

## Performance Considerations

### Optimization Tips
1. **Particle Systems**: Limit particle count for background effects
2. **Audio**: Use compressed audio formats for music
3. **UI**: Use UI object pooling for complex animations
4. **Transitions**: Cache references to avoid FindObjectOfType calls

### Memory Management
- MainMenuUI automatically cleans up event listeners on destroy
- Audio clips are managed by Unity's resource system
- UI elements are destroyed when scene changes

## Future Enhancements

### Planned Features
1. **Profile System**: User profiles with avatars and stats
2. **Social Features**: Friends list, leaderboards
3. **Store Integration**: Cosmetic purchases and unlocks
4. **Advanced Animations**: 3D character previews, dynamic backgrounds
5. **Accessibility**: Screen reader support, colorblind options

### Extension Points
- `OnPlayRequested` event for custom game modes
- `ShowFeatureNotAvailable()` method for placeholder features
- Animation triggers for custom visual effects
- Audio callback system for dynamic music

## Integration Checklist

- [ ] MainMenuUI component added to scene
- [ ] All button references assigned
- [ ] Scene names configured in inspector
- [ ] Audio clips assigned (music, sounds)
- [ ] Canvas and EventSystem present
- [ ] GameManager integration enabled
- [ ] Feature flags configured for MVP
- [ ] Debug mode tested
- [ ] Scene transitions tested
- [ ] Audio playback tested

## Related Documentation

- [Power Selection System Integration Guide](POWER_SELECTION_INTEGRATION_GUIDE.md)
- [Game Manager Documentation](GAME_MANAGER_DOCUMENTATION.md)
- [Superpower System Documentation](SUPERPOWER_SYSTEM_DOCUMENTATION.md)
- [UI/HUD Integration Guide](HUD_INTEGRATION_GUIDE.md)

## Support

For issues with the Main Menu system:
1. Check the debug mode output
2. Verify all inspector references are set
3. Test with the MainMenuSetupExample script
4. Review the console for error messages
5. Ensure proper scene setup and Build Settings
