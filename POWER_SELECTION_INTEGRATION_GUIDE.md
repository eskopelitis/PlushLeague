# Power Selection System Integration Guide

This guide explains how to integrate and use the complete Power Selection system in Plush League, including the pre-match superpower picker, role selection, and game flow management.

## System Overview

The Power Selection system consists of several interconnected components:

1. **GameManager** - Central game state management
2. **PowerSelectionManager** - Coordinates power selection process  
3. **PowerSelectionUI** - User interface for power/role selection
4. **PowerButton** - Individual power selection buttons
5. **MatchManager** - Handles match lifecycle
6. **SuperpowerData** - ScriptableObject assets for powers

## Quick Setup

### 1. Basic Scene Setup

```csharp
// Add these components to your scene:
// - GameManager (singleton, persists across scenes)
// - PowerSelectionManager 
// - PowerSelectionUI (with proper UI setup)
// - MatchManager
// - Canvas + EventSystem for UI
```

### 2. Create Superpower Assets

Use the `SuperpowerDataSetup` script to create sample powers:

```csharp
// Add SuperpowerDataSetup to any GameObject
// Use "Create Sample Superpowers" context menu
// Powers will be created in Resources/Superpowers folder
```

### 3. Integration Example

```csharp
// Use PowerSelectionIntegrationExample to validate setup
var integration = gameObject.AddComponent<PowerSelectionIntegrationExample>();
integration.SetupIntegration(); // Auto-setup all components
integration.ValidateIntegration(); // Check everything is working
```

## Core Components

### GameManager

Central coordinator for game flow:

```csharp
// Start a new game with power selection
GameManager.Instance.StartNewGame(isMultiplayer: false);

// Skip power selection for testing
GameManager.Instance.StartQuickMatch();

// Get current player configurations
var (player1, player2) = GameManager.Instance.GetPlayerConfigs();
```

**Game States:**
- `Menu` - Main menu state
- `PowerSelection` - Players choosing powers/roles
- `MatchSetup` - Applying configurations to match
- `MatchActive` - Match in progress
- `MatchEnded` - Match completed
- `PostMatch` - Showing results/statistics

### PowerSelectionManager

Manages the power selection process:

```csharp
// Start power selection
powerSelectionManager.StartPowerSelection(isMultiplayer: false);

// Get player configurations
var (p1, p2) = powerSelectionManager.GetPlayerConfigs();

// Handle remote player updates (multiplayer)
powerSelectionManager.UpdateRemotePlayerData(playerIndex, remoteData);
```

### PowerSelectionUI

Handles all UI interactions:

```csharp
// Show power selection screen
powerSelectionUI.ShowPowerSelection(availablePowers, isMultiplayer);

// Handle role conflicts (rock-paper-scissors)
powerSelectionUI.ShowRockPaperScissorsMiniGame();

// Update teammate status
powerSelectionUI.UpdateTeammateData(playerIndex, teammateData);
```

## Usage Examples

### Basic Single Player Flow

```csharp
public class GameFlow : MonoBehaviour
{
    private void Start()
    {
        // Initialize game
        var gameManager = GameManager.Instance;
        
        // Subscribe to events
        gameManager.OnGameStateChanged += OnGameStateChanged;
        gameManager.OnPlayersConfigured += OnPlayersConfigured;
        
        // Start game flow
        gameManager.StartNewGame(false);
    }
    
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.PowerSelection:
                Debug.Log("Show power selection UI");
                break;
                
            case GameManager.GameState.MatchActive:
                Debug.Log("Match started with configured players");
                break;
        }
    }
    
    private void OnPlayersConfigured(GameManager.PlayerGameConfig p1, GameManager.PlayerGameConfig p2)
    {
        Debug.Log($"P1: {p1.selectedPower?.displayName} ({p1.selectedRole})");
        Debug.Log($"P2: {p2.selectedPower?.displayName} ({p2.selectedRole})");
    }
}
```

### Custom Power Selection

```csharp
public class CustomPowerSelection : MonoBehaviour
{
    [SerializeField] private SuperpowerData[] customPowers;
    
    public void StartCustomSelection()
    {
        var manager = FindFirstObjectByType<PowerSelectionManager>();
        var ui = FindFirstObjectByType<PowerSelectionUI>();
        
        // Show UI with custom powers
        ui.ShowPowerSelection(customPowers.ToList(), false);
    }
}
```

### Multiplayer Integration

```csharp
public class MultiplayerPowerSelection : MonoBehaviour
{
    public void StartMultiplayerSelection()
    {
        // Start multiplayer selection
        GameManager.Instance.StartNewGame(true);
        
        // Listen for remote player updates
        // (integrate with your networking solution)
    }
    
    public void OnRemotePlayerUpdate(int playerId, PowerSelectionData data)
    {
        var manager = FindFirstObjectByType<PowerSelectionManager>();
        
        // Convert your network data to PowerSelectionData
        var selectionData = new PowerSelectionUI.PlayerSelectionData
        {
            playerName = data.name,
            selectedPower = data.power,
            selectedRole = data.role,
            isReady = data.ready
        };
        
        manager.UpdateRemotePlayerData(playerId, selectionData);
    }
}
```

## UI Customization

### Customizing Power Buttons

```csharp
public class CustomPowerButton : PowerButton
{
    [SerializeField] private Image rarityBorder;
    
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        
        // Add custom rarity border color
        if (PowerData != null)
        {
            rarityBorder.color = GetRarityColor(PowerData.rarity);
        }
    }
}
```

### Custom Role Selection

```csharp
public class CustomRoleSelector : MonoBehaviour
{
    public void OnCustomRoleSelected(string roleName)
    {
        var ui = FindFirstObjectByType<PowerSelectionUI>();
        
        // Convert custom role to system role
        var roleType = ConvertToRoleType(roleName);
        ui.OnRoleSelected(roleType);
    }
}
```

## Events and Callbacks

### GameManager Events

```csharp
GameManager.Instance.OnGameStateChanged += (state) => {
    // React to game state changes
};

GameManager.Instance.OnPlayersConfigured += (p1, p2) => {
    // Players have selected their powers/roles
};

GameManager.Instance.OnMatchStartRequested += () => {
    // Match start has been requested
};
```

### PowerSelectionUI Events

```csharp
powerSelectionUI.OnSelectionConfirmed += (power, role) => {
    // Player confirmed their selection
};

powerSelectionUI.OnAllPlayersReady += () => {
    // All players are ready to start
};

powerSelectionUI.OnBackToMenu += () => {
    // Player wants to return to menu
};
```

## Configuration

### Power Selection Settings

```csharp
[Header("Power Selection Configuration")]
public float selectionTimeLimit = 60f;        // Time limit for selection
public bool allowRoleConflicts = true;        // Enable rock-paper-scissors
public bool showPowerPreviews = true;         // Show power descriptions
public bool enableQuickSelect = false;        // Allow quick selection
public SuperpowerData defaultPower;           // Fallback power
```

### Role Conflict Resolution

When both players select the same role:

1. **Rock-Paper-Scissors Mini-Game** - Default resolution method
2. **Random Assignment** - Alternative quick resolution
3. **First-Come-First-Serve** - Based on selection order
4. **Manual Override** - Allow admin to resolve

## Testing and Debugging

### Debug Methods

```csharp
// Print current game state
GameManager.Instance.PrintCurrentState();

// Validate integration
integration.ValidateIntegration();

// Test power loading
setup.TestLoadSuperpowers();

// Force power selection
ui.ShowPowerSelection(powers, false);
```

### Common Issues

1. **No Powers Available**
   - Check Resources/Superpowers folder
   - Use SuperpowerDataSetup to create samples

2. **UI Not Responding**
   - Ensure Canvas and EventSystem exist
   - Check UI components are properly connected

3. **State Not Changing**
   - Verify GameManager events are connected
   - Check for null references in managers

4. **Multiplayer Issues**
   - Implement proper networking callbacks
   - Ensure data synchronization

## Advanced Features

### Custom Power Validation

```csharp
public bool ValidatePowerSelection(SuperpowerData power, PlayerConfig player)
{
    // Add custom validation logic
    if (power.requiredLevel > player.level)
        return false;
        
    if (power.cost > player.currency)
        return false;
        
    return true;
}
```

### Power Unlock System

```csharp
public class PowerUnlockSystem : MonoBehaviour
{
    public bool IsPowerUnlocked(SuperpowerData power, PlayerProfile profile)
    {
        // Check unlock conditions
        return profile.unlockedPowers.Contains(power.id);
    }
}
```

### Analytics Integration

```csharp
public void TrackPowerSelection(SuperpowerData power, RoleType role)
{
    // Send analytics data
    Analytics.CustomEvent("PowerSelected", new Dictionary<string, object>
    {
        {"power_name", power.displayName},
        {"role", role.ToString()},
        {"session_id", sessionId}
    });
}
```

## Performance Considerations

1. **Power Asset Loading** - Load powers asynchronously
2. **UI Pooling** - Pool power buttons for large lists
3. **Event Cleanup** - Unsubscribe from events properly
4. **Memory Management** - Release references when done

## Future Enhancements

- **Team-based Power Selection** - Multiple players per team
- **Power Synergies** - Bonus effects for power combinations
- **Dynamic Power Pools** - Different powers per match type
- **Tournament Mode** - Draft-style power selection
- **AI Recommendations** - Suggest powers based on playstyle

---

For more details, see the individual component documentation and example scripts.
