# Scene Management & Game Loop System Documentation

## Overview
The Scene Management & Game Loop system in Plush League provides a robust, centralized way to handle all scene transitions and game state management. This system ensures smooth gameplay flow from the main menu through power selection, match gameplay, and post-match summary.

## Architecture

### Core Components

#### 1. GameManager (PlushLeague.Core.GameManager)
- **Purpose**: Central coordinator for all game flow and scene management
- **Location**: `Assets/Scripts/Core/GameManager.cs`
- **Pattern**: Singleton with DontDestroyOnLoad

#### 2. Game States
```csharp
public enum GameState
{
    Menu,           // Main menu screen
    PowerSelection, // Power/role selection screen
    MatchSetup,     // Match initialization
    MatchActive,    // Match in progress
    MatchEnded,     // Match completed
    PostMatch       // Post-match summary
}
```

#### 3. Scene Management Fields
```csharp
[Header("Scene Management")]
[SerializeField] private string menuSceneName = "MainMenu";
[SerializeField] private string powerSelectionSceneName = "PowerSelection";
[SerializeField] private string matchSceneName = "GameArena";
[SerializeField] private string postMatchSceneName = "PostMatch";
```

### Key Features

#### 1. Scene Transitions
- **Fade Effects**: Smooth crossfade between scenes
- **Loading Screens**: Animated loading indicators for longer transitions
- **Audio Feedback**: Sound effects for scene transitions
- **Safety Checks**: Prevent multiple simultaneous loads

#### 2. Persistent Settings
```csharp
[System.Serializable]
public class PersistentPlayerSettings
{
    public string selectedPlushType = "Classic";
    public string selectedSuperpowerName = "";
    public float masterVolume = 1.0f;
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public int matchesWon = 0;
    public int matchesLost = 0;
    public int totalGoals = 0;
    public int totalSaves = 0;
}
```

#### 3. Match Result Tracking
```csharp
[System.Serializable]
public struct MatchResult
{
    public bool playerWon;
    public int playerScore;
    public int opponentScore;
    public float matchDuration;
    public int playerGoals;
    public int playerSaves;
    public int superpowerUsageCount;
    public string mvpPlayerName;
    public bool wasCleanMatch;
}
```

## Implementation Guide

### 1. Basic Scene Loading
```csharp
// Load a scene with state transition
gameManager.LoadScene("GameArena", GameState.MatchActive);

// Check if transitioning
if (gameManager.IsTransitioning())
{
    // Handle transition state
}
```

### 2. Game Flow Management
```csharp
// Start the game flow from menu
gameManager.OnPlayPressed();

// Handle power selection completion
gameManager.OnPowerSelectionComplete();

// Handle match completion
var matchResult = new MatchResult(
    playerWon: true,
    pScore: 3,
    oScore: 2,
    duration: 180f,
    goals: 2,
    saves: 5,
    powerUses: 8,
    mvp: "Player 1",
    clean: true
);
gameManager.OnMatchEnd(matchResult);
```

### 3. Post-Match Flow
```csharp
// Handle rematch request
gameManager.OnRematchPressed();

// Handle return to menu
gameManager.OnReturnToMenuPressed();
```

### 4. Persistent Settings
```csharp
// Save player preferences
gameManager.SavePlayerPrefs();

// Load player preferences
gameManager.LoadPlayerPrefs();

// Get persistent settings
var settings = gameManager.GetPersistentSettings();
```

## API Reference

### Public Methods

#### Scene Management
- `LoadScene(string sceneName, GameState targetState = GameState.Menu)`
- `GetCurrentSceneName()`
- `IsTransitioning()`
- `IsLoadingScene()`

#### Game Flow
- `OnPlayPressed()`
- `OnPowerSelectionComplete()`
- `OnMatchEnd(MatchResult result)`
- `OnRematchPressed()`
- `OnReturnToMenuPressed()`

#### Settings Management
- `SavePlayerPrefs()`
- `LoadPlayerPrefs()`
- `GetPersistentSettings()`

#### State Management
- `SetGameState(GameState newState)`
- `GetCurrentState()`

#### Utility Methods
- `GetLastMatchResult()`
- `WasLastMatchClean()`

### Events

#### State Events
- `OnGameStateChanged(GameState newState)`
- `OnMatchCompleted(MatchResult result)`

#### Scene Events
- `OnSceneTransitionStarted(string sceneName)`
- `OnSceneTransitionCompleted()`

## Integration Examples

### 1. Main Menu Integration
```csharp
// In MainMenuUI.cs
public void OnPlayButtonPressed()
{
    var gameManager = GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.OnPlayPressed();
    }
}
```

### 2. Power Selection Integration
```csharp
// In PowerSelectionManager.cs
public void OnSelectionsConfirmed()
{
    var gameManager = GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.OnPowerSelectionComplete();
    }
}
```

### 3. Match Manager Integration
```csharp
// In MatchManager.cs
public void OnMatchComplete(bool playerWon, int playerScore, int opponentScore)
{
    var result = new MatchResult(
        playerWon: playerWon,
        pScore: playerScore,
        oScore: opponentScore,
        duration: matchDuration,
        goals: playerGoals,
        saves: playerSaves,
        powerUses: superpowerUsageCount,
        mvp: GetMVPName(),
        clean: true
    );
    
    var gameManager = GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.OnMatchEnd(result);
    }
}
```

### 4. Post-Match UI Integration
```csharp
// In PostMatchUI.cs
public void OnRematchButtonPressed()
{
    var gameManager = GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.OnRematchPressed();
    }
}

public void OnReturnToMenuButtonPressed()
{
    var gameManager = GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.OnReturnToMenuPressed();
    }
}
```

## Edge Cases & Error Handling

### 1. Multiple Load Prevention
The system prevents multiple simultaneous scene loads:
```csharp
if (isLoadingScene && preventMultipleLoads)
{
    Debug.LogWarning($"Scene load blocked - already loading: {currentSceneName}");
    return;
}
```

### 2. Match Timeout Protection
Automatic timeout handling for stuck matches:
```csharp
private IEnumerator MatchTimeoutCoroutine()
{
    yield return new WaitForSeconds(matchTimeoutDuration);
    
    var timeoutResult = new MatchResult(
        playerWon: false,
        pScore: 0,
        oScore: 0,
        duration: matchTimeoutDuration,
        goals: 0,
        saves: 0,
        powerUses: 0,
        mvp: "None",
        clean: false
    );
    
    OnMatchEnd(timeoutResult);
}
```

### 3. Unclean Match Recovery
Handling matches that didn't complete properly:
```csharp
public void OnRematchPressed()
{
    if (!matchCompletedCleanly)
    {
        Debug.LogWarning("Cannot rematch - previous match did not complete cleanly");
        return;
    }
    
    // Continue with rematch logic
}
```

### 4. Scene Loading Failure
Automatic recovery from scene loading failures:
```csharp
private void OnSceneLoadFailed(string sceneName)
{
    Debug.LogError($"Failed to load scene: {sceneName}");
    
    if (autoReturnToMenuOnError)
    {
        LoadScene(menuSceneName, GameState.Menu);
    }
}
```

## Performance Considerations

### 1. Fade Transitions
- Configurable fade duration
- Option to disable for performance
- Async implementation to prevent blocking

### 2. Loading Screens
- Minimum duration to prevent flicker
- Animated indicators only when needed
- Automatic cleanup

### 3. Persistent Settings
- Lazy loading of settings
- Automatic saving on important changes
- Efficient serialization

## Testing

### 1. SceneManagementDemo
Comprehensive testing script that covers:
- Full game loop testing
- Scene transition testing
- Persistent settings testing
- Error handling testing
- Performance testing

### 2. Usage
```csharp
// Add to scene and run
var demoScript = GetComponent<SceneManagementDemo>();
StartCoroutine(demoScript.RunSceneManagementDemo());
```

## Best Practices

### 1. Scene Naming
- Use consistent naming conventions
- Match scene names with GameState enum values
- Include fallback scenes for error recovery

### 2. State Management
- Always use GameManager for state changes
- Subscribe to state change events for UI updates
- Handle state transitions gracefully

### 3. Error Handling
- Implement timeout protection for long operations
- Provide fallback mechanisms for failed operations
- Log errors appropriately for debugging

### 4. Performance
- Use async operations for scene loading
- Implement object pooling for frequently created objects
- Clean up resources on scene transitions

## Future Enhancements

### 1. Cloud Save Integration
- Sync persistent settings across devices
- Backup/restore functionality
- Conflict resolution

### 2. Advanced Analytics
- Track player behavior across scenes
- Performance metrics
- Error reporting

### 3. Tournament Mode
- Extended game flow for tournaments
- Bracket management
- Advanced match configurations

### 4. Replay System
- Scene-based replay functionality
- Integration with match results
- Sharing capabilities

## Troubleshooting

### Common Issues

1. **Scene Not Loading**
   - Check scene name spelling
   - Verify scene is in build settings
   - Check for conflicting load requests

2. **State Not Changing**
   - Verify GameManager instance exists
   - Check for null references
   - Ensure proper event subscriptions

3. **Persistent Settings Not Saving**
   - Check for sufficient storage permissions
   - Verify PlayerPrefs keys are valid
   - Ensure save is called at appropriate times

4. **Transitions Not Smooth**
   - Adjust fade duration settings
   - Check for performance bottlenecks
   - Verify audio source configuration

### Debug Information

The system provides extensive debug logging:
- Scene transition status
- State change notifications
- Error conditions
- Performance metrics

Enable debug logging in the GameManager inspector for detailed information.
