# Goalie Jump Save System Implementation

## Overview
The Goalie Jump Save system implements a skill-based timing mini-game that allows players to make dramatic saves when defending their goal. This creates exciting moments and rewards skilled defensive play in Plush League.

## Key Features Implemented

### 1. Shot Detection System (`ShotDetector.cs`)
- **Automatic Detection**: Monitors ball velocity and trajectory toward goal
- **Speed Threshold**: 3.0 units/second minimum to trigger save opportunity  
- **Direction Analysis**: Uses dot product to ensure ball is aimed at goal
- **Goal Intersection**: Calculates if trajectory will hit goal area
- **Cooldown System**: 1-second cooldown prevents multiple triggers per shot

### 2. Timing Bar Mini-Game (`GoalieTimingBar.cs`)
- **Color-Coded Zones**:
  - **Green (Perfect)**: Last 15% of timing window - spectacular catch
  - **Yellow (Good)**: 35% before perfect zone - successful block/deflection  
  - **Red (Fail)**: Remaining 50% - missed save, goal likely scored
- **Shrinking Timer**: Bar decreases from full to empty during save window
- **Dynamic Duration**: 0.5 second standard window (adjustable based on shot distance)
- **Visual Feedback**: Pulsing effects, color changes, and instruction text

### 3. Save Manager (`GoalieSaveManager.cs`)
- **Goalie Assignment**: Automatically finds closest player to goal for save opportunity
- **Result Processing**: Handles perfect saves, blocks, and failures
- **Ball Physics**: Modifies ball trajectory based on save result
- **Animation Integration**: Triggers appropriate goalie animations
- **Visual Effects**: Manages particles, slow motion, and audio feedback

### 4. Save Outcomes

#### Perfect Save (Green Zone)
- **Ball Control**: Ball is caught and attached to goalie immediately
- **Stamina Bonus**: +50 stamina awarded to goalie for excellent timing
- **Visual Effects**: Slow motion effect, bright glow, crowd cheer sound
- **Animation**: Spectacular diving catch or jump save animation

#### Good Save (Yellow Zone)  
- **Ball Deflection**: Ball bounced away from goal at 70% original speed
- **Trajectory**: Reflected with 30° upward angle away from goal
- **Result**: Goal prevented but ball remains in play (scramble opportunity)
- **Visual Effects**: Impact particles, deflection sound effects

#### Failed Save (Red Zone/No Input)
- **Ball Continues**: Original trajectory maintained (likely goal scored)
- **Goalie Penalty**: 0.5 second stun from failed dive attempt
- **Visual Effects**: Fumble animation, failure sound effects
- **Consequence**: Goal scored if ball was on target

### 5. Input Integration
- **Primary Input**: A button during save opportunities only
- **Contextual Activation**: Button only responds when player is designated goalie
- **Mobile Support**: Touch button integration with haptic feedback
- **Network Ready**: RPC system for multiplayer save synchronization

## Technical Implementation

### Shot Detection Flow
1. **Continuous Monitoring**: ShotDetector monitors ball velocity each frame
2. **Threshold Check**: Ball speed must exceed 3.0 units/second
3. **Direction Validation**: Ball velocity must point toward goal (70% accuracy)
4. **Range Check**: Ball must be within detection radius (5.0 units default)
5. **Trajectory Calculation**: Project ball path to determine goal intersection
6. **Trigger Event**: If all conditions met, trigger save opportunity

### Save Opportunity Sequence
1. **Goalie Selection**: Find closest player to goal within max distance (8.0 units)
2. **UI Activation**: Show timing bar with calculated save window
3. **Input Monitoring**: Listen for A button press during timing window
4. **Result Calculation**: Determine save quality based on timing bar progress
5. **Outcome Processing**: Apply appropriate ball physics and player effects
6. **Cleanup**: Hide UI, reset state, apply cooldowns

### Timing Calculation
```csharp
// Standard save window with distance scaling
float timeToGoal = distanceToGoal / ballVelocity.magnitude;
float saveWindow = Mathf.Min(standardSaveWindow, timeToGoal * 0.8f);

// Zone boundaries (as percentage of total window)
Perfect Zone: 0.00 - 0.15 (15%)
Good Zone:    0.15 - 0.50 (35%) 
Fail Zone:    0.50 - 1.00 (50%)
```

### Ball Physics Modification

#### Perfect Save
```csharp
ballController.SetPosition(goaliePosition + Vector3.up * 0.5f);
ballController.AttachToPlayer(goalie);
// Ball velocity = Vector2.zero (caught)
```

#### Block Save
```csharp
Vector2 deflection = CalculateDeflectionDirection();
ballController.ApplyForce(deflection * originalSpeed * 0.7f);
// Ball bounces away from goal with upward angle
```

## Configuration Options

### Shot Detection Settings
```csharp
minimumShotSpeed = 3.0f         // Speed threshold for save trigger
detectionRadius = 5.0f          // Range around goal to monitor
goalWidth = 3.0f               // Goal area width for intersection
standardSaveWindow = 0.5f       // Default timing window duration
maxSaveDistance = 8.0f          // Max distance for goalie eligibility
```

### Timing Bar Zones
```csharp
perfectZoneSize = 0.15f         // Perfect zone (15% of window)
goodZoneSize = 0.35f           // Good zone (35% of window)
// Fail zone is remaining 50%
```

### Save Effects
```csharp
perfectSaveStaminaBonus = 50f   // Stamina reward for perfect saves
blockReflectionSpeed = 0.7f     // Speed multiplier for blocks (70%)
blockUpwardAngle = 30f          // Upward deflection angle
slowMotionScale = 0.3f          // Time scale for slow motion effect
slowMotionDuration = 0.5f       // Duration of slow motion
```

## Setup Instructions

### 1. Create Goal with Shot Detection
1. Create empty GameObject at goal location
2. Add `ShotDetector` component
3. Configure goal width, detection radius, minimum shot speed
4. Set goal direction (usually Vector2.up for upward-facing goals)

### 2. Setup Save Manager
1. Create empty GameObject in scene
2. Add `GoalieSaveManager` component  
3. Assign particle effect prefabs for different save outcomes
4. Configure audio clips for save sounds
5. Set physics parameters (reflection speed, angles)

### 3. Create Timing Bar UI
1. Create Canvas if not exists
2. Create UI Panel for timing bar
3. Add Image components for background and fill
4. Add TextMeshPro for instructions
5. Add `GoalieTimingBar` component to panel
6. Assign UI references and configure zones/colors

### 4. Add Goalie Save Ability
1. Right-click in Project: Create > Plush League > Abilities > Goalie Save
2. Configure ability settings (max goalie distance, shot-only mode)
3. Add ability to player's AbilityManager component
4. Set goalieSaveKey input (default: A)

### 5. Test System
1. Add `GoalieSaveSystemDemo` component to scene
2. Use "Setup Goalie Save System" context menu
3. Test with T key (shot), G key (save opportunity), R key (reset)
4. Verify all components are connected in console

## Animation Integration

The system calls these animation methods (to be implemented in PlayerAnimationController):

```csharp
// When save opportunity begins
animController.TriggerGoaliePrep();

// Save result animations
animController.TriggerPerfectSave();  // Spectacular catch
animController.TriggerBlockSave();    // Diving punch/deflection  
animController.TriggerFailSave();     // Fumble/wrong direction
```

## Audio System Integration

Required audio clips for full experience:
- **countdownSound**: Heartbeat/tension sound during timing window
- **perfectSaveSound**: Dramatic save completion sound
- **goodSaveSound**: Impact/deflection sound
- **failSound**: Miss/fumble sound effect
- **crowdCheerSound**: Crowd reaction to spectacular saves

## Networking Considerations (Future)

### Authority Model
- **Host Authority**: Server decides save opportunities and outcomes
- **Client Input**: Goalie client sends timing input to host
- **Prediction**: Local timing bar for responsive feel
- **Reconciliation**: Adjust for network latency in timing calculations

### RPC Messages
```csharp
[Rpc(RpcSources.All, RpcTargets.All)]
void RPC_TriggerSaveOpportunity(NetworkId goalieId, float saveWindow);

[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]  
void RPC_GoalieSaveInput(float timingProgress);

[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
void RPC_SaveResult(SaveResult result, Vector3 ballPosition, Vector3 ballVelocity);
```

## Performance Optimization

### Efficient Monitoring
- Shot detection only runs when ball is moving above threshold
- Uses distance squared for goalie selection (avoids sqrt)
- Timing bar only updates during active save opportunities
- Particle effects use object pooling

### Mobile Optimization
- Reduced particle complexity on lower-end devices
- Simplified physics calculations for deflections
- Touch-optimized timing bar sizing and responsiveness
- Efficient GUI rendering for debug displays

## Visual Effects Hierarchy

### Perfect Save
1. Slow motion effect (0.3x speed for 0.5 seconds)
2. Ball glow effect (attached to ball)
3. Perfect save particle burst at goalie position
4. Camera shake/zoom effect (if implemented)
5. Crowd cheer audio with reverb

### Block Save  
1. Impact particles at contact point
2. Ball trail effect during deflection
3. Dust/debris particles on ground impact
4. Sharp impact sound with echo

### Failed Save
1. Fumble particles around goalie
2. Goal explosion effect (if ball enters goal)
3. Disappointed crowd sound
4. Goalie stumble animation with dust

## Debugging Tools

### Visual Debug Information
- Shot detection gizmos (range, goal area, trajectory)
- Timing bar progress and zone indicators
- Save manager state display
- Ball velocity and goal distance readouts

### Console Logging
- Shot detection triggers with detailed parameters
- Save opportunity assignments and timing windows
- Save results with precise timing measurements
- System setup verification messages

### Test Controls
- Manual shot triggering (T key)
- Direct save opportunity testing (G key)
- Ball position reset (R key) 
- Individual save result testing (Perfect/Block buttons)

This implementation creates a thrilling, skill-based goalie system that rewards precise timing while maintaining the arcade-style fun of Plush League. The modular design allows for easy customization and extension while providing robust debugging and testing capabilities.
