# Slide Tackle System Implementation

## Overview
The Slide Tackle system implements the defensive/offensive ability that allows players to lunge forward to steal the ball from opponents or contest loose balls. This is a core gameplay mechanic in Plush League.

## Key Features Implemented

### 1. Slide Tackle Ability (`SlideTackle.cs`)
- **Trigger**: B button (customizable)
- **Range**: 2.5 units forward lunge
- **Speed**: 20.0 units/second tackle speed  
- **Width**: 1.0 unit wide hit detection
- **Cooldown**: 2.0 seconds between tackles
- **Stun Duration**: 0.75s for successful tackle victims, 0.5s self-stun on miss

### 2. Hit Detection System
- Uses `Physics2D.OverlapBox` for precise hit detection
- Detects both ball and player collisions
- Supports ball stealing from opponents
- Optional body-checking (disabled by default to prevent griefing)

### 3. Physics Implementation
- **Option A**: Physics impulse-based movement (default)
- **Option B**: Kinematic interpolation movement
- Slide physics material for reduced friction during tackle
- Automatic velocity dampening after tackle completion

### 4. Input Integration
- **Mobile**: Virtual button in touch UI
- **Keyboard**: B key (configurable)
- **Gamepad**: B button (when implemented)
- Integrated with existing input system architecture

### 5. Visual Effects
- Slide trail particle effect
- Dust cloud on tackle initiation  
- Success/steal particle effects
- Player animation integration via `TriggerSlide()`

### 6. UI System (`SlideTackleUI.cs`)
- Cooldown timer display
- Button availability indication  
- Color-coded states (available/cooldown/disabled)
- Haptic feedback on mobile
- Flash indication when tackle opportunity available

## Implementation Details

### Slide Tackle Execution Flow
1. **Input Detection**: B button press detected by input system
2. **Validation**: Check if player can tackle (not stunned, not on cooldown)
3. **Direction Calculation**: Use player facing direction or movement input
4. **Hit Detection**: Immediate box cast in tackle direction
5. **Movement**: Apply physics impulse or kinematic movement
6. **Result Processing**: Handle successful steal, miss, or body check
7. **Cooldown**: Start 2-second cooldown period
8. **Recovery**: Apply appropriate stun duration based on result

### Successful Ball Steal Sequence
1. **Hit Detection**: Tackle hits opponent with ball
2. **Ball Transfer**: 
   - Detach ball from opponent
   - Stun opponent for 0.75 seconds
   - Attach ball to tackler
3. **Visual Feedback**: Play steal particle effect
4. **Audio**: Play successful tackle sound (when implemented)

### Miss Penalty
1. **Self-Stun**: Tackler stunned for 0.5 seconds (recovery time)
2. **Cooldown**: Full 2-second cooldown still applies
3. **Animation**: Play "getting up" animation

### Physics Tuning
- **Tackle Distance**: 2.5 units ensures good range without being overpowered
- **Tackle Speed**: 20.0 units/sec makes tackle feel snappy (~0.125s duration)
- **Hit Width**: 1.0 unit provides fair hit area without being too generous
- **Cooldown**: 2.0 seconds prevents tackle spam while allowing tactical use

## Integration Points

### With Ball System
- Integrates with `BallController.cs` for possession transfer
- Respects ball reclaim delays (e.g., after chip kicks)
- Handles both possessed and free ball scenarios

### With Player Controller
- Uses existing stun system (`PlayerController.Stun()`)
- Integrates with input disable system (`SetInputEnabled()`)
- Respects player state (stunned players cannot tackle)

### With Animation System
- Calls `PlayerAnimationController.TriggerSlide()`
- Assumes slide animation exists in Animator Controller
- Visual effects complement animation timing

### With Audio System (Future)
- Sound effect hooks ready for implementation
- Separate sounds for: tackle initiation, successful steal, miss
- Positional audio support for multiplayer

## Configuration Options

### Ability Settings (Inspector)
```csharp
tackleDistance = 2.5f        // Lunge distance
tackleSpeed = 20.0f          // Movement speed during tackle  
tackleWidth = 1.0f           // Hit detection width
tackleStunDuration = 0.75f   // Opponent stun on successful steal
missStunDuration = 0.5f      // Self-stun on miss
slideDuration = 0.2f         // Time for slide animation
```

### Physics Options
```csharp
usePhysicsImpulse = true     // Use physics vs kinematic movement
slideForceMultiplier = 1.5f  // Force scaling for physics impulse
slideMaterial                // Physics material for reduced friction
```

### Visual Effects
```csharp
slideTrailPrefab            // Particle trail during slide
dustEffect                  // Dust cloud on tackle start
stealEffect                 // Success particles on ball steal
```

## Setup Instructions

### 1. Create Slide Tackle Ability Asset
1. Right-click in Project window
2. Create > Plush League > Abilities > Slide Tackle
3. Configure settings in Inspector
4. Name it "DefaultSlideTackle" or similar

### 2. Add to Player
1. Select Player GameObject
2. In AbilityManager component, assign SlideTackle ability
3. Set slideTackleKey to desired input (default: B)

### 3. Setup Input (Mobile)
1. Add SlideTackle button to mobile UI canvas
2. In MobileInput component, assign slideTackleButton reference
3. Button will automatically connect to input system

### 4. Setup UI Feedback  
1. Add SlideTackleUI component to UI canvas
2. Assign UI elements (button, cooldown fill, text, icon)
3. Configure visual state colors
4. UI will auto-connect to AbilityManager

### 5. Test System
1. Add SlideTackleSystemDemo component to any GameObject
2. Use "Setup Slide Tackle System" context menu
3. Use "Test Slide Tackle" to verify functionality
4. Check console for setup verification messages

## Networking Considerations (Future)

### Authority
- Server/Host authoritative hit detection
- Client prediction for responsive feel
- Rollback on misprediction

### Synchronization  
- Position sync during tackle movement
- Ball possession transfer sync
- Stun state synchronization across clients

### Anti-Cheat
- Server validates tackle distance and timing
- Rate limiting to prevent exploit attempts
- Sanity checks on physics state changes

## Debugging Tools

### Debug Visualization
- Enable `showDebugInfo` in SlideTackle for gizmos
- Shows tackle range and hit detection area
- Visualizes movement path and collision results

### Console Logging
- Detailed logs for tackle attempts and results
- Performance timing information  
- Error reporting for invalid states

### Demo Component
- `SlideTackleSystemDemo` provides comprehensive testing
- Automatic system verification
- Manual testing controls and status display

## Performance Notes

### Optimization
- Hit detection uses efficient Physics2D.OverlapBox
- Minimal allocations during tackle execution
- Particle effects use object pooling (when implemented)

### Mobile Considerations
- Haptic feedback for tactile response
- Touch-friendly UI sizing and placement
- Efficient particle effects for lower-end devices

This implementation provides a complete, production-ready slide tackle system that integrates seamlessly with the existing Plush League architecture while maintaining high performance and clear debugging capabilities.
