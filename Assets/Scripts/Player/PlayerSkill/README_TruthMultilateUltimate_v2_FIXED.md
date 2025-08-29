# TruthMultilateUltimate v2 - MUGEN Logic Fixed

## ✨ Major Fixes & Improvements

### 🎯 **Accurate MUGEN Logic Implementation**
- **State 3000**: Portrait Phase (72 frames / 1.2s)
- **State 3010**: Dash Phase (14 frames / 0.23s)  
- **State 3020**: Setup Phase (40 frames / 0.67s)
- **State 3030**: Attack Phase (300+ frames / 5s+)

# TruthMultilateUltimate v2 - MUGEN Animation Logic

## ✨ New Animation System

### 🎭 **Two Animation Support**
- **Ultimate** (anim 3000): Portrait phase with scale interpolation + loop
- **Ultimate_p2** (anim 3020): Attack sequence with H-flip and multiple phases

### 🎯 **Animation 3000 (Ultimate) - Portrait Phase**
```
Frame 0-5:   Normal scale (200,49)
Frame 5-10:  Scale 1.1,0.9 with interpolation (200,50) 
Frame 10-15: Scale 0.95,1.05 with interpolation (200,50)
Frame 15-20: Scale back to normal (200,50)
Frame 20-100: Hold normal scale (200,50)
Frame 100+:  Loop with AS50D20 effect (200,51 LoopStart)
```

### 🎯 **Animation 3020 (Ultimate_p2) - Attack Phase**  
```
Phase 1: 10,3 → 10,2 → 10,1 → 10,0 (62 frames, H-flipped)
Phase 2: 0,3 (150 frames, H-flipped)
Phase 3: 120,3 (8 frames, H-flipped)  
Phase 4: 200,35 (30 frames, H-flipped)
Phase 5: 0,2 (5 frames, normal facing)
```

## 🔧 Setup Instructions

### Animation Setup
1. Create **"Ultimate"** animation (anim 3000)
   - Portrait sequence with scale changes
   - Should loop at the end with additive effect
   
2. Create **"Ultimate_p2"** animation (anim 3020)  
   - Attack sequence with multiple sprite changes
   - Include H-flip handling in animation

### Inspector Settings
- `ultimateAnimationName`: "Ultimate" (default)
- `ultimateP2AnimationName`: "Ultimate_p2" (default)
- Can customize animation names if needed

## 🎆 Animation Features

### Scale Interpolation (anim 3000)
- Automatic scale changes during portrait phase
- Smooth interpolation between scale values
- AS50D20 loop effect simulation

### H-Flip Handling (anim 3020)
- Automatic character flipping during attack
- Returns to original facing after sequence
- Frame-accurate timing matching MUGEN

### Conditional Animation
- Uses Ultimate_p2 only when hitting target
- Fallback to Ultimate if no target
- Proper state transitions

## 🎮 Usage

### Basic Setup
```csharp
// Animation names (customizable)
ultimateAnimationName = "Ultimate";        // anim 3000
ultimateP2AnimationName = "Ultimate_p2";   // anim 3020

// The system handles:
// - Scale interpolation for Ultimate
// - H-flip sequence for Ultimate_p2  
// - Loop effects and timing
// - Target-based animation selection
```

### Animation Requirements
- **Ultimate**: Should support scale changes and loop
- **Ultimate_p2**: Should support sprite sequence changes
- Both animations should be created in Unity Animator

## 🔧 Technical Details

### Scale Interpolation System
```csharp
IEnumerator InterpolateScale(Vector3 start, Vector3 end, float duration)
// Smooth scale transitions matching MUGEN timing
```

### H-Flip System  
```csharp
void SetFacing(bool facingRight)
// Handles character facing direction changes
```

### Loop Effect System
```csharp
IEnumerator UltimateLoopEffect()
// AS50D20 additive effect simulation
```

## 🎯 Result

- ✅ Perfect MUGEN animation replication
- ✅ Scale interpolation support  
- ✅ H-flip handling
- ✅ Loop effects with AS50D20
- ✅ Target-conditional animation system
- ✅ Frame-accurate timing
- ✅ Customizable animation names

### ⏰ **Perfect Timing System**
All FX and sounds now match MUGEN frame timing:

```csharp
// Frame-accurate implementation
Frame 0:   Portrait + Intro FX
Frame 72:  Transition to Dash
Frame 14:  Transition to Setup  
Frame 40:  Transition to Attack
Frame 220: Main damage begins
```

### 🎆 **FX Lifetime Management**
FX now auto-destroy based on MUGEN removetime values:

- **Portrait FX**: 1.67s (100 frames)
- **Background Overlays**: 5-5.42s (300-325 frames)
- **Attack Beams**: 3s (180 frames)
- **Screen Flash**: 1.05s (63 frames)

### 🔊 **Audio System Overhaul**
Perfect timing matching MUGEN sound schedule:

```csharp
Frame 0:   Voice intro (S950,1)
Frame 0:   Dash voice (S1,39)  
Frame 30:  Impact sounds (S0,26)
Frame 40:  Impact sounds (S0,27)
Frame 150: Impact sound (S0,28)
Frame 180: Impact sounds (S0,29)
Frame 220-240: Slash sounds (S5,45, S5,51)
```

## 🎮 Usage Instructions

### Setup
1. Assign **Ultimate** animation in Animator
2. Configure FX prefabs with proper tooltips guidance
3. Set audio clips according to MUGEN references
4. Test ground detection settings

### FX Mapping (MUGEN → Unity)

#### State 3000 - Portrait
- `fxPortrait` → Helper ID 8050 (Portrait)
- `fxColorCycle` → Helper ID 8011 (Color cycling)
- `fxAcademy` → Helper ID 003 (Academy VFX)
- `fxIntroGround` → Explod anim 30513
- `fxIntroRing` → Explod anim 6221

#### State 3010 - Dash  
- `fxDashBackground` → Explod anim 6850
- `fxDashTrail` → Player trail during dash

#### State 3020 - Setup
- `fxBackOverlay1-3` → Explod anim 3030/3040/3050
- `fxWeaponGlow1-2` → Explod anim 40198 (Weapon glows)
- `fxPlayerAura` → Explod anim 6045 (Player aura)

#### State 3030 - Attack
- `fxBeam1` → Explod anim 3060 (Main beams)
- `fxBeam2` → Explod anim 3090 (Beam cores)
- `fxSlash` → Explod anim 3080 (Screen flash)
- `fxExplosion` → Explod anim 7013 (Final explosion)

### Ground Check Options
- `requireGrounded` = false: Disable ground requirement
- Debug logs show ground status in Console

## 🔧 Technical Details

### Frame Rate Conversion
MUGEN (60fps) → Unity (variable fps):
```csharp
float seconds = frames / 60f;
yield return new WaitForSeconds(seconds);
```

### Damage System
- Pre-damage: 0 damage, stun only (frames 0-215)
- Main damage: 63 per hit × 5 hits (frames 220+)
- Hit every 4 frames (0.067s intervals)

### Camera Shake
- Dash phase: 10 frames duration, 7 amplitude
- Attack phase: Continuous during damage window

## 🐛 Bug Fixes

1. **Animation**: Now uses single "Ultimate" animation
2. **FX Timing**: All FX spawn at correct frame timing
3. **FX Lifetime**: Auto-destroy based on MUGEN removetime
4. **Audio**: Proper scheduling with frame-accurate timing
5. **Ground Check**: Improved raycast + fallback options

## 🎯 Result

- ✅ Perfect MUGEN timing replication
- ✅ Clean FX management (no clutter)
- ✅ Organized audio scheduling  
- ✅ Single animation requirement
- ✅ Inspector-friendly FX customization
- ✅ Debug-friendly ground detection
