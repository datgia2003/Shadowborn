# Truth Multilate Ultimate System

## Overview
A faithful Unity implementation of the M.U.G.E.N Truth Multilate Ultimate attack (Statedefs 3000-3090). This system recreates the complex cinematic ultimate with precise timing, visual effects, and audio synchronization.

## Features
- **Frame-perfect timing** - All MUGEN frame timings converted to Unity seconds (60 FPS base)
- **Cinematic sequence** - Multi-phase ultimate with intro, dash, setup, and main attack
- **Complex VFX system** - 20+ different visual effects with proper layering and positioning
- **Audio synchronization** - Precise SFX timing matching MUGEN specification
- **Target interaction** - Enemy-centered positioning and cinematic lock system
- **Camera integration** - Screen shake, follow lock, and freeze effects

## System Architecture

### Core Components
```csharp
TruthMultilateUltimate : MonoBehaviour
├── ICameraController - Camera shake and follow control
├── IGameTimer - Time freeze functionality  
├── IEnemy - Target interaction and damage system
└── Audio/VFX Management - Prefab and sound coordination
```

### Phase Breakdown

#### **Phase 3000: Intro (Statedef 3000)**
- Duration: ~0.75 seconds
- Portrait display with color cycling
- Academy special VFX
- Power consumption: -1000
- Sound: S950,1 (voice)

#### **Phase 3010: Dash (Statedef 3010)**  
- Duration: 14 frames or until near target
- High-speed dash (velocity: 30 units/sec)
- Screen shake effect
- Background trail VFX (fx_6850)
- Power consumption: -2000

#### **Phase 3020: Setup (Statedef 3020)**
- Duration: 40 frames (0.67 seconds)
- Time freeze activation
- Background overlay setup (fx_3030, 3040, 3050)
- Target cinematic lock (270 frames)
- Sound: S1,39 + S0,26 (double)

#### **Phase 3030: Main Attack (Statedef 3030)**
- Duration: ~5 seconds
- Complex multi-hit sequence
- Massive VFX display with 15+ effects
- Environmental screen shake
- Damage: 5 hits × 63 damage each
- Target state transition to 6000

## Setup Instructions

### 1. Component Assignment
```csharp
[Header("Core")]
public Animator animator;           // Player animator
public Rigidbody2D rb;             // Player physics
public Transform fxRoot;           // Foreground FX parent
public Transform backRoot;         // Background FX parent

[Header("Systems")]
public MonoBehaviour cameraControllerBehaviour;  // ICameraController implementation
public MonoBehaviour gameTimerBehaviour;         // IGameTimer implementation
```

### 2. Audio Setup
Assign all audio clips in Inspector:
- **S950_1** - Ultimate voice line
- **S1_39** - Setup voice
- **S0_26, S0_27, S0_28, S0_29** - Impact sounds
- **S2_9, S3_3** - Looping ambient
- **S5_45, S5_51** - Attack sounds

### 3. VFX Prefabs
Create and assign 22 VFX prefabs:

#### Essential Effects
- **portrait_8050** - Character portrait
- **fx_3030/3040/3050** - Background overlays
- **fx_3060/3090** - Main attack beams
- **fx_6850** - Dash trail

#### Color Cycling
- **colorLight_50903** - Light color variant
- **colorNormal_50904** - Normal color
- **colorDark_50905** - Dark color variant

#### Special Effects
- **fx_40198** - Character overlay (2 variants)
- **fx_6045** - Base character effect
- **fx_30854** - Small impact
- **fx_3080** - Side slash effect
- **fx_7013** - Final explosion

### 4. Interface Implementation

#### ICameraController
```csharp
public interface ICameraController
{
    void LockFollow(bool locked);           // Camera follow control
    void Shake(float duration, float magnitude, float frequency);  // Screen shake
}
```

#### IGameTimer  
```csharp
public interface IGameTimer
{
    void Freeze(bool freeze);              // Time freeze toggle
}
```

#### IEnemy
```csharp
public interface IEnemy
{
    Transform transform { get; }
    void CinematicLock(int frames);         // Lock enemy for duration
    void ReleaseCinematicLock();           // Release lock
    void TakeHit(int damage, Vector2 knockback, float hitstopAttacker, float hitstopVictim);
}
```

## Usage

### Basic Activation
```csharp
var ultimate = GetComponent<TruthMultilateUltimate>();
var target = FindObjectOfType<Enemy>(); // Your enemy implementation
ultimate.ActivateUltimate(target);
```

### Advanced Integration
```csharp
// Check if ultimate is available
if (!ultimate.isActive && playerPower >= 3000)
{
    ultimate.ActivateUltimate(selectedTarget);
}
```

## Technical Specifications

### Timing System
- **Base Rate**: 60 FPS (MUGEN standard)
- **Conversion**: `ToSec(frames) = frames / 60f`
- **Total Duration**: ~7.5 seconds

### Damage System
- **Fake Hits**: Frames 0-214, every 20 frames (0 damage)
- **Real Hits**: Frames 220+, every 4 frames (63 damage × 5)
- **Total Damage**: 315 + knockback

### VFX Layering (Sprite Priority)
```
Layer Hierarchy (back to front):
-50: fx_3050 (darkest background)
-10: fx_30513 (character base)
-5:  fx_3030 (background overlay)
 0:  Default effects
 5:  fx_6221 (mid-layer)
 6:  fx_6850 (dash trail)
50:  fx_3040 (bright overlay)  
70:  fx_3080 (slash effects)
100+: ontop effects
```

## Troubleshooting

### Common Issues

#### Ultimate Not Starting
- Check `isActive` flag isn't stuck true
- Verify target is not null
- Ensure component is properly initialized

#### Missing VFX
- Assign all prefabs in Inspector
- Check `fxRoot` and `backRoot` transforms
- Verify prefab instantiation in console

#### Audio Problems
- Confirm all AudioClips are assigned
- Check AudioSource components exist
- Verify volume levels and mixing

#### Timing Issues
- Test `ToSec()` conversion accuracy
- Check coroutine execution order
- Verify Time.timeScale settings

### Performance Optimization
- Use object pooling for frequently spawned FX
- Implement LOD system for distant effects
- Cache component references
- Limit particle system counts

### Integration Notes
- Replace mock interfaces with your actual implementations
- Adapt coordinate system scaling for your game's units
- Customize damage values and effects as needed
- Add proper state machine integration for end transitions

## Dependencies
- Unity 2021.3+ (Coroutine system)
- 2D Sprite rendering pipeline
- Audio system with multiple sources
- Physics2D (for Rigidbody2D)

## Version History
- **v1.0** - Initial MUGEN conversion
- **v1.1** - Added sprite priority system
- **v1.2** - Fixed timing and positioning issues
- **v1.3** - Enhanced VFX layering and audio sync

---
*Created for Shadowborn Unity Project - Faithful M.U.G.E.N Ultimate Implementation*
