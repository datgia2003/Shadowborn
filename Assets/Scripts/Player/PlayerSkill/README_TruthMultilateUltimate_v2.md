# Truth Multilate Ultimate v2 - Simplified Version

## Overview
Phiên bản đơn giản hóa của Truth Multilate Ultimate, được viết lại theo format của các skill khác (`SliceUpSkill`, `SummonSkill`) để dễ sử dụng và maintain hơn.

## Key Improvements
✅ **Đơn giản hóa setup** - Chỉ cần assign prefabs và audio clips  
✅ **Input System integration** - Kích hoạt bằng `OnUltimate(InputValue)`  
✅ **Automatic target detection** - Tự động tìm enemy gần nhất  
✅ **Built-in validation** - Kiểm tra điều kiện trước khi activate  
✅ **Easy animation control** - Tương thích với Animator controller  
✅ **Automatic cleanup** - Tự động dọn dẹp FX và restore state  

## Setup Instructions

### 1. Basic Setup
```csharp
// Gán references cơ bản
[Header("References")]
public Animator animator;           // Player animator
public Rigidbody2D rb;             // Player rigidbody  
public AudioSource audioSource;    // Main audio source
public AudioSource voiceSource;    // Voice audio source (optional)
public CameraShake camShake;       // Camera shake component
public Transform fxSpawnPoint;     // FX spawn point (default: transform)
```

### 2. Audio Setup
Assign 10 audio clips trong Inspector:
```csharp
[Header("Audio Clips")]
public AudioClip voiceIntro;       // S950,1 - Voice intro
public AudioClip voiceDash;        // S1,39 - Voice dash
public AudioClip sndImpact1;       // S0,26 - Impact sound 1
public AudioClip sndImpact2;       // S0,27 - Impact sound 2
public AudioClip sndImpact3;       // S0,28 - Impact sound 3
public AudioClip sndImpact4;       // S0,29 - Impact sound 4
public AudioClip sndSlash1;        // S5,45 - Slash sound 1
public AudioClip sndSlash2;        // S5,51 - Slash sound 2
public AudioClip sndAmbient1;      // S2,9 - Ambient loop
public AudioClip sndAmbient2;      // S3,3 - Ambient loop 2
```

### 3. FX Prefabs Setup
Assign FX prefabs với các settings có thể customize:
```csharp
[Header("FX Prefabs")]
public UltimateFXSettings fxPortrait;      // Portrait effect
public UltimateFXSettings fxColorCycle;    // Color cycling effect
public UltimateFXSettings fxAcademy;       // Academy special effect
public UltimateFXSettings fxDashTrail;     // Dash trail effect
// ... and more
```

#### UltimateFXSettings Properties:
- **Prefab**: GameObject FX prefab
- **Offset**: Position offset from spawn point (Vector3)
- **Scale**: FX scale multiplier (Vector3)
- **Rotation**: Z-axis rotation in degrees (float)
- **Lifetime**: Auto-destroy time in seconds (float)
- **Follow Player**: Whether FX follows player transform (bool)

#### Setup in Inspector:
1. Drag FX prefab vào field `Prefab`
2. Adjust `Offset` để đặt vị trí FX
3. Modify `Scale` để resize FX
4. Set `Rotation` để xoay FX
5. Configure `Lifetime` cho auto-destroy
6. Check `Follow Player` nếu muốn FX theo player

### 4. Animation Setup
Tạo 4 animation clips trong Animator:
- **Ultimate_Intro** - Intro phase animation
- **Ultimate_Dash** - Dash phase animation  
- **Ultimate_Setup** - Setup phase animation
- **Ultimate_Attack** - Main attack animation

### 5. Input System Setup
Trong Input Actions, tạo action `Ultimate` và bind với script:
```csharp
public void OnUltimate(InputValue value)
{
    if (value.isPressed && !isUltimateActive)
    {
        StartUltimate();
    }
}
```

## Usage

### Basic Activation
Ultimate tự động kiểm tra điều kiện và kích hoạt:
```csharp
// Qua Input System (recommend)
// Player nhấn phím -> OnUltimate() được gọi

// Hoặc code trực tiếp
var ultimate = GetComponent<TruthMultilateUltimate>();
ultimate.StartUltimate();
```

### Customization Settings
```csharp
[Header("Ultimate Settings")]
public float dashDistance = 8f;        // Khoảng cách dash
public float dashDuration = 0.3f;      // Thời gian dash
public int totalDamage = 315;          // Tổng damage
public int hitCount = 5;               // Số hit
public LayerMask enemyLayers;          // Layer của enemy
public float hitRadius = 2f;           // Bán kính hit

[Header("Timing Settings")]
public float introDuration = 1.2f;     // Thời gian intro
public float dashPhaseDuration = 0.5f; // Thời gian dash
public float setupDuration = 1.0f;     // Thời gian setup
public float attackDuration = 4.0f;    // Thời gian attack
```

## Ultimate Sequence

### Phase 0: Intro (1.2s)
- Spawn portrait và color effects
- Play intro voice và animation
- Setup cinematic elements

### Phase 1: Dash (0.5s)  
- Dash towards target enemy
- Spawn dash trail FX
- Play dash voice và camera shake

### Phase 2: Setup (1.0s)
- Spawn background overlays
- Start ambient sound loops
- Play setup sounds

### Phase 3: Attack (4.0s)
- Main attack sequence với 5 hits
- Complex FX spawning timeline
- Camera shake sequence
- Final explosion

## Features

### Customizable FX System
```csharp
// Mỗi FX có settings riêng có thể chỉnh trong Inspector
[System.Serializable]
public class UltimateFXSettings
{
    public GameObject prefab;       // FX prefab
    public Vector3 offset;          // Position offset
    public Vector3 scale;           // Scale multiplier  
    public float rotation;          // Z rotation (degrees)
    public float lifetime;          // Auto-destroy time
    public bool followPlayer;       // Follow player transform
}
```

### Advanced FX Spawn System
```csharp
// Spawn FX với settings từ Inspector
SpawnFX(fxPortrait);

// Override settings at runtime
SpawnFX(fxBeam1, additionalOffset: Vector3.up, 
        scaleOverride: Vector3.one * 2f, 
        rotationOverride: 45f, 
        lifetimeOverride: 3f);
```

### Smart Validation
```csharp
bool CanActivateUltimate()
{
    // Check grounded
    if (!IsGrounded()) return false;
    
    // Check target exists and in range
    if (requiresTarget && FindNearestEnemy() == null) return false;
    
    return true;
}
```

### Automatic Cleanup
```csharp
void EndUltimate()
{
    // Restore player control
    if (playerController != null)
        playerController.enabled = true;
        
    // Return to appropriate animation
    if (IsGrounded())
        animator.Play("Player_Idle");
    else
        animator.Play("Player_Falling");
        
    // Clean up FX
    StartCoroutine(CleanupFX());
}
```

## Comparison với Version Cũ

| Feature | Old Version | New Version |
|---------|-------------|-------------|
| **Setup** | Complex interfaces + mocks | Simple prefab assignment |
| **Activation** | Manual ActivateUltimate(target) | Input System integration |
| **Target** | Manual target required | Auto-detection |
| **Animation** | Manual animation names | Standard naming convention |
| **FX Management** | Complex SpawnFX with many params | Simple SpawnFX method |
| **Audio** | Complex audio routing | Straightforward AudioSource |
| **Cleanup** | Manual cleanup required | Automatic cleanup |
| **Integration** | Hard to integrate | Drop-in compatible |

## Troubleshooting

### Ultimate Not Activating
```csharp
// Check these conditions:
Debug.Log($"Is Active: {isUltimateActive}");
Debug.Log($"Is Grounded: {IsGrounded()}");
Debug.Log($"Target Found: {FindNearestEnemy() != null}");
```

### FX Not Spawning
- Ensure FX prefabs have `AutoDestroyOnAnimationEnd` component
- Check `fxSpawnPoint` is assigned
- Verify prefabs are not null

### Audio Issues
- Assign both `audioSource` and `voiceSource`
- Check audio clip assignments
- Verify AudioSource components are active

### Animation Problems
- Create 4 required animation clips
- Use standard naming convention
- Ensure Animator Controller is properly set up

## Best Practices

1. **FX Prefabs**: Add `AutoDestroyOnAnimationEnd` to all FX prefabs
2. **Enemy Detection**: Set up proper `enemyLayers` LayerMask  
3. **Audio Separation**: Use separate AudioSource for voice lines
4. **Ground Check**: Implement proper ground detection
5. **Testing**: Use Gizmos for visual debugging in Scene view

---
*Tạo để thay thế version phức tạp trước đó - Đơn giản, dễ dùng, dễ maintain!*
