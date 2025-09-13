# Advanced Dodge Animation System

## Overview
The dodge skill now supports two different animations with custom visual effects based on your specific animation setup:
- **Dodge (Action 65)**: Ground dodge with scale interpolation effects
- **AirDodge (Action 70)**: Air dodge with complex scale transformations and different collision box

## Animation Effects Implementation

### Ground Dodge (Action 65)
- **Scale Effects**: Interpolates from (0.99, 0.99) → (0.99, 0.95) during animation
- **Collision Box**: Clsn2[0] = -9, -54, 5, 0 (14x54 units)
- **Duration**: Follows `dashDuration` setting (default: 0.3s)
- **Loop Behavior**: Smooth scale interpolation throughout dodge

### Air Dodge (Action 70) 
- **Initial Scale**: (1.1, 0.9) transformation
- **Final Scale**: Interpolates to (0.95, 1.05)
- **Collision Box**: Clsn2[0] = -20, -51, 18, 0 (38x51 units)
- **Sprite Change**: Uses different sprite (200,7)
- **Complex Animation**: Multi-phase scale effects

## Configuration Settings

### Inspector Settings
```csharp
[Header("Animation Settings")]
- useCustomAnimationEffects: Enable/disable custom scale effects
- dodgeScaleEffect: Final Y scale for ground dodge (default: 0.95)
- airDodgeScaleX: X scale for air dodge (default: 1.1)  
- airDodgeScaleY: Y scale for air dodge (default: 0.9)
- scaleAnimationDuration: Duration of scale effects (default: 0.2s)
```

## Technical Implementation

### Automatic Systems
1. **Collision Box Adjustment**: 
   - Automatically adjusts BoxCollider2D during dodge
   - Restores original collision box after dodge ends
   - Converts your animation coordinates to Unity units (scaled by 0.1)

2. **Scale Effects**:
   - Ground dodge: Smooth Y-scale reduction for "squash" effect
   - Air dodge: Complex X/Y scale transformations in phases
   - All effects restore to original scale automatically

3. **Animation Integration**:
   - Triggers appropriate Animator triggers ("Dodge" or "AirDodge")
   - Scale effects run simultaneously with animator animations
   - Collision adjustments happen immediately on dodge start

## Unity Animator Setup

### Required Trigger Parameters
- **Dodge** (Trigger): For ground dodge (Action 65)
- **AirDodge** (Trigger): For air dodge (Action 70)

### Animation Clips
- Create animation clips that match your sprite sequences
- Ground dodge: Use frames from your Action 65 setup
- Air dodge: Use frames from your Action 70 setup (including sprite 200,7)

## Usage
The system automatically:
1. Detects grounded state when L key is pressed
2. Applies appropriate collision box and scale effects
3. Triggers correct animation
4. Restores all values when dodge completes

No manual setup required - everything is handled by the DodgeSkill system!