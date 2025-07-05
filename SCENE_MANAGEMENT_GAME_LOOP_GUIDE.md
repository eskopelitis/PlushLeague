# Scene Management & Game Loop Implementation Guide

## Overview

The Scene Management & Game Loop system provides comprehensive control over transitions between all scenes in Plush League, ensuring a smooth, bug-free experience across the entire game cycle: **Menu → Power Selection → Match → Post-Match Summary → Rematch/Return to Menu**.

## Key Components

### 1. Enhanced GameManager.cs
- **Location**: `Assets/Scripts/Core/GameManager.cs`
- **Purpose**: Central coordinator for all scene transitions and game state management
- **New Features**:
  - Complete scene transition system with fade effects
  - Persistent player settings and statistics
  - Match timeout and error handling
  - Loading screens with progress indication
  - Audio feedback for transitions
  - Prevention of multiple simultaneous loads

### 2. PostMatchUI.cs
- **Location**: `Assets/Scripts/UI/PostMatch/PostMatchUI.cs`
- **Purpose**: Displays match results and handles post-match user choices
- **Features**:
  - Animated result display (Victory/Defeat)
  - Player statistics and match summary
  - Rematch and Return to Menu options
  - Victory/defeat visual and audio effects
  - Share results functionality (future feature)

### 3. CompleteGameLoopDemo.cs
- **Location**: `Assets/Scripts/Examples/CompleteGameLoopDemo.cs`
- **Purpose**: Comprehensive demonstration and testing of the complete game loop
- **Features**:
  - Multiple test scenarios (victory, defeat, rematch, error handling)
  - Continuous loop testing capability
  - System status monitoring
  - Debug GUI with manual controls

## Game Flow States

### State Transition Diagram
```
Menu → Power Selection → Match Setup → Match Active → Match Ended → Post Match → [Rematch OR Return to Menu]
```

### Detailed State Descriptions

#### 1. **Menu State**
- **Scene**: MainMenu.unity
- **UI**: MainMenuUI.cs
- **Purpose**: Entry point, player can start new game or access settings
- **Transitions**: To PowerSelection on "Play" button

#### 2. **Power Selection State**
- **Scene**: PowerSelection.unity (or integrated in game scene)
- **UI**: PowerSelectionUI.cs, PowerSelectionManager.cs
- **Purpose**: Players choose superpowers and roles
- **Transitions**: To MatchSetup when all players ready

#### 3. **Match Setup State**
- **Scene**: GameArena.unity
- **Purpose**: Initialize match environment, spawn players, configure superpowers
- **Transitions**: To MatchActive when setup complete

#### 4. **Match Active State**
- **Scene**: GameArena.unity
- **UI**: GameHUD.cs, PlayerHUD.cs
- **Purpose**: Actual gameplay with superpowers, goals, and match mechanics
- **Transitions**: To MatchEnded when match concludes

#### 5. **Match Ended State**
- **Purpose**: Process match results, update statistics
- **Transitions**: To PostMatch for result display

#### 6. **Post Match State**
- **Scene**: PostMatch.unity (or overlay in game scene)
- **UI**: PostMatchUI.cs
- **Purpose**: Show results, offer rematch or return to menu
- **Transitions**: To MatchSetup (rematch) or Menu (return)

## Scene Management Features

### Loading System
```csharp
// Load scene with proper transitions
gameManager.LoadScene("GameArena", GameState.MatchActive);

// Scene loading includes:
// - Fade out current scene
// - Show loading screen with progress
// - Load scene asynchronously
// - Initialize systems in new scene
// - Fade in new scene
// - Set appropriate game state
```

### Transition Effects
- **Fade Transitions**: Smooth black fade between scenes
- **Loading Screens**: Progress indication with spinning loader
- **Audio Feedback**: Transition sounds and scene-specific music
- **Minimum Loading Time**: Ensures users see loading feedback

### Error Handling
```csharp
// Handle match errors gracefully
gameManager.HandleMatchError("Network disconnection");

// Error handling includes:
// - Mark match as "unclean"
// - Disable rematch option
// - Auto-return to menu if configured
// - Preserve player statistics
```

### Persistence System
```csharp
// Save player preferences
gameManager.SavePlayerPrefs();

// Persistent data includes:
// - Player name and preferred plush type
// - Preferred superpower and role
// - Audio settings (master, music, SFX volume)
// - Match statistics (wins, losses, goals, saves)
// - Feature preferences (vibration, etc.)
```

## Public API

### GameManager Scene Methods
```csharp
// Core scene management
public void LoadScene(string sceneName, GameState targetState = GameState.Menu);
public void OnMatchEnd(MatchResult result);
public void OnRematchPressed();
public void OnReturnToMenuPressed();

// Persistence
public void SavePlayerPrefs();
public void LoadPlayerPrefs();
public void ApplyPreferencesToPlayerConfigs();

// Error handling
public void HandleMatchError(string errorMessage);

// State queries
public bool IsTransitioning();
public bool IsLoadingScene();
public string GetCurrentSceneName();
public MatchResult GetLastMatchResult();
public bool WasLastMatchClean();
```

### PostMatchUI Methods
```csharp
// Display results
public void ShowPostMatchSummary(MatchResult result);
public void HidePostMatchSummary();

// Events
public System.Action OnRematchRequested;
public System.Action OnReturnToMenuRequested;
public System.Action OnShareRequested;
```

## Configuration Options

### Scene References
```csharp
[Header("Scene Management")]
[SerializeField] private string menuSceneName = "MainMenu";
[SerializeField] private string powerSelectionSceneName = "PowerSelection";
[SerializeField] private string matchSceneName = "GameArena";
[SerializeField] private string postMatchSceneName = "PostMatch";
```

### Transition Settings
```csharp
[Header("Scene Transitions")]
[SerializeField] private float sceneTransitionDuration = 1f;
[SerializeField] private bool enableSceneFadeTransitions = true;
[SerializeField] private bool showLoadingScreen = true;
[SerializeField] private float loadingScreenMinDuration = 2f;
```

### Safety Features
```csharp
[Header("Game Loop Control")]
[SerializeField] private bool preventMultipleLoads = true;
[SerializeField] private bool autoReturnToMenuOnError = true;
[SerializeField] private float matchTimeoutDuration = 600f; // 10 minutes
```

## Match Result Data Structure

```csharp
public struct MatchResult
{
    public bool playerWon;          // Did the player win?
    public int playerScore;         // Player's final score
    public int opponentScore;       // Opponent's final score
    public float matchDuration;     // Match time in seconds
    public int playerGoals;         // Goals scored by player
    public int playerSaves;         // Saves made by player
    public int superpowerUsageCount; // Superpowers used
    public string mvpPlayerName;    // Most Valuable Player
    public bool wasCleanMatch;      // No errors or disconnects
}
```

## Integration with Existing Systems

### MainMenuUI Integration
```csharp
// MainMenuUI connects to GameManager
private void OnPlayPressed()
{
    if (gameManager != null)
    {
        gameManager.StartNewGame(false); // Start single-player
    }
}
```

### PowerSelectionManager Integration
```csharp
// Power selection triggers match start
powerSelectionManager.OnMatchStartRequested += () => {
    gameManager.SetGameState(GameState.MatchSetup);
};
```

### MatchManager Integration
```csharp
// Match completion triggers post-match
matchManager.OnMatchEnded += (result) => {
    gameManager.OnMatchEnd(result);
};
```

## Testing and Debugging

### CompleteGameLoopDemo Features
- **Automated Testing**: Run complete game loops automatically
- **Scenario Testing**: Test victory, defeat, rematch, error scenarios
- **Continuous Loop**: Stress test with repeated cycles
- **Manual Controls**: Step through individual phases
- **System Monitoring**: Real-time status of all components

### Debug GUI Usage
```csharp
// Enable debug GUI in CompleteGameLoopDemo
[SerializeField] private bool enableDebugGUI = true;

// Available controls:
// - Start/Stop demo
// - Test specific scenarios
// - Monitor system status
// - Adjust timing parameters
```

## Edge Cases and Safety Features

### Multiple Load Prevention
```csharp
if (preventMultipleLoads && isLoadingScene)
{
    Debug.LogWarning("Scene loading already in progress");
    return;
}
```

### Match Timeout Handling
```csharp
// Automatic timeout after 10 minutes
private IEnumerator MatchTimeoutCoroutine()
{
    yield return new WaitForSeconds(matchTimeoutDuration);
    OnMatchEnd(timeoutResult);
}
```

### Error Recovery
```csharp
// Safe return to menu on errors
private IEnumerator SafeReturnToMenu()
{
    CleanupMatchState();
    LoadScene(menuSceneName, GameState.Menu);
}
```

### Rematch Validation
```csharp
// Only allow rematch if previous match was clean
public void OnRematchPressed()
{
    if (!matchCompletedCleanly)
    {
        ShowRematchUnavailableMessage();
        return;
    }
    // Proceed with rematch...
}
```

## Performance Optimization

### Async Scene Loading
- Non-blocking scene transitions
- Progress feedback during loading
- Minimum loading time for UX consistency
- Resource cleanup between scenes

### Memory Management
- Proper cleanup of event listeners
- Destruction of temporary objects
- Garbage collection optimization
- Resource pooling for frequent objects

### Audio Optimization
- Audio source pooling
- Compressed audio formats
- Volume fade-outs during transitions
- Memory-efficient sound management

## Future Enhancements

### Planned Features
1. **Cloud Save Integration**: Sync player progress across devices
2. **Replay System**: Save and replay match highlights
3. **Tournament Mode**: Multi-match progression system
4. **Social Features**: Share results, leaderboards, achievements
5. **Advanced Analytics**: Detailed match statistics and player insights

### Extension Points
- Custom scene transition effects
- Platform-specific sharing APIs
- External tournament integration
- Streaming/spectator mode support
- Custom game modes and rulesets

## Setup Checklist

### GameManager Configuration
- [ ] Scene names configured in inspector
- [ ] Transition settings configured
- [ ] Audio clips assigned
- [ ] UI references set up
- [ ] Safety features enabled

### Scene Setup
- [ ] All scenes added to Build Settings
- [ ] Scene names match GameManager configuration
- [ ] Required UI components present in each scene
- [ ] Proper scene hierarchy and organization

### Component Integration
- [ ] MainMenuUI connected to GameManager
- [ ] PowerSelectionManager event integration
- [ ] PostMatchUI configured and connected
- [ ] MatchManager result reporting set up

### Testing
- [ ] CompleteGameLoopDemo configured and tested
- [ ] All transition scenarios verified
- [ ] Error handling tested
- [ ] Performance validated on target platforms

## Troubleshooting

### Common Issues

1. **Scene Not Loading**
   - Check scene name spelling
   - Verify scene is in Build Settings
   - Check for multiple simultaneous loads

2. **Transitions Not Working**
   - Verify UI references are assigned
   - Check event listener setup
   - Ensure proper scene hierarchy

3. **Persistent Data Not Saving**
   - Check PlayerPrefs permissions
   - Verify save/load method calls
   - Test on target platform

4. **Match Results Not Displaying**
   - Check PostMatchUI setup
   - Verify MatchResult data structure
   - Test event system integration

## Related Documentation

- [Main Menu Integration Guide](MAIN_MENU_INTEGRATION_GUIDE.md)
- [Power Selection Integration Guide](POWER_SELECTION_INTEGRATION_GUIDE.md)
- [Superpower System Documentation](SUPERPOWER_SYSTEM_DOCUMENTATION.md)
- [Match Management System](MATCH_MANAGEMENT_GUIDE.md)
- [UI/HUD Integration Guide](HUD_INTEGRATION_GUIDE.md)
