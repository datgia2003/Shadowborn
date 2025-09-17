# 🌊 Enemy Wave Spawning System

## Overview
System spawn enemies theo waves/zones cho room dài, thay vì spawn tất cả enemies cùng lúc. Player trigger waves khi chạy qua các zones khác nhau trong room.

## 📁 Wave System Components

### Core Components
- **`EnemyWaveZone.cs`** - Individual wave trigger zones
- **`EnemyWaveManager.cs`** - Manages multiple waves in room
- **`EnemyWaveInfo.cs`** - Component attached to wave enemies
- **`RoomWaveSetup.cs`** - Auto-setup helper for room prefabs

## 🛠️ Setup Guide

### Method 1: Auto Setup (Recommended)
1. **Add RoomWaveSetup** to room prefab root
2. **Configure settings**:
   ```
   Use Wave Spawning: ✅
   Auto Create Wave Zones: ✅
   Number of Waves: 3-5
   Progression Mode: Player Triggered
   ```
3. **Right-click component** → `Setup Wave System`
4. **Done!** - Wave zones auto-created based on room size

### Method 2: Manual Setup
1. **Add EnemyWaveManager** to room root
2. **Create wave zones manually**:
   - Add child GameObjects for each wave zone
   - Add **EnemyWaveZone** component
   - Add **BoxCollider2D** (trigger = true)
   - Configure wave settings
3. **Add EnemySpawner** for enemy selection

## 🎯 Wave Progression Modes

### Player Triggered (Default)
```csharp
// Waves trigger when player enters trigger zones
triggerOnPlayerEnter = true;
triggerDuration = 0.1f; // Delay before trigger
```

### Sequential
```csharp
// Waves trigger automatically after previous wave cleared
progressionMode = Sequential;
requirePreviousWaveCleared = true;
timeBetweenWaves = 3f;
```

### Timed
```csharp
// Waves trigger on timer regardless of player position
progressionMode = Timed;
timeBetweenWaves = 5f;
```

### Mixed
```csharp
// Custom combination of triggers
progressionMode = Mixed;
// Implement custom logic
```

## 🌊 Wave Zone Configuration

### Basic Settings
```csharp
waveId = 1;                    // Wave order
waveName = "Wave 1";           // Display name
enemyCount = 3;                // Enemies to spawn
difficultyMultiplier = 1.2f;   // Difficulty boost
```

### Trigger Settings
```csharp
triggerOnPlayerEnter = true;   // Auto-trigger on player enter
triggerDuration = 0.5f;        // Delay before spawn
triggerOnce = true;            // Only trigger once per room visit
showWaveWarning = true;        // Show UI warning
```

### Spawn Configuration
```csharp
randomSpawnInBounds = true;    // Random positions in zone
waveSpawnPoints = [];          // Specific spawn points
customEnemyPrefabs = [];       // Override enemy types
spawnBossEnemies = false;      // Spawn boss instead
```

## 📊 Wave Flow Example

### Long Room với 4 Waves
```
[Entry] ──→ [Zone1] ──→ [Zone2] ──→ [Zone3] ──→ [Zone4] ──→ [Exit]
           Wave 1     Wave 2     Wave 3     Boss Wave
           2 enemies  3 enemies  4 enemies  1 boss
```

### Trigger Sequence
1. **Player enters Zone1** → Wave 1 spawns (2 enemies)
2. **Player kills all** → Can proceed to Zone2
3. **Player enters Zone2** → Wave 2 spawns (3 enemies)
4. **Continue pattern** until all waves cleared
5. **All waves complete** → Room exit opens

## 🎮 Player Experience

### Visual Flow
1. **Player approaches zone** → Optional warning UI
2. **Enter trigger area** → Brief delay (0.5s)
3. **Enemies spawn** → Combat phase
4. **Clear all enemies** → Progress to next zone
5. **Complete all waves** → Exit unlocked + rewards

### Difficulty Progression
```csharp
Wave 1: difficultyMultiplier = 1.0f (normal)
Wave 2: difficultyMultiplier = 1.1f (+10% stats)
Wave 3: difficultyMultiplier = 1.2f (+20% stats)
Boss Wave: difficultyMultiplier = 1.5f (+50% stats)
```

## ⚙️ Advanced Configuration

### Custom Wave Logic
```csharp
// In EnemyWaveZone
public override void TriggerWave() {
    // Custom pre-spawn logic
    if (SpecialConditionMet()) {
        enemyCount *= 2; // Double enemies
    }
    base.TriggerWave();
}
```

### Wave Manager Events
```csharp
waveManager.OnWaveStarted += (wave) => {
    ShowWaveUI($"Wave {wave.waveId} incoming!");
};

waveManager.OnWaveCleared += (wave) => {
    GiveRewards(wave.waveId);
};

waveManager.OnAllWavesCompleted += () => {
    ShowVictoryScreen();
    OpenSecretArea();
};
```

### Custom Completion Actions
```csharp
// In EnemyWaveManager
completionAction = TriggerEvent;

void TriggerCompletionEvent() {
    // Custom room completion logic
    SpawnBossChest();
    UnlockSecretPassage();
    TriggerCutscene();
}
```

## 🔧 Debug Features

### Inspector Tools
- **Test Trigger Wave** - Force trigger individual waves
- **Reset Wave** - Reset wave to untriggered state  
- **Clear Wave Enemies** - Remove all spawned enemies
- **Wave Gizmos** - Visual wave zones in scene view

### Console Information
```
🌊 Wave started: Wave 1 (1 active waves)
🦇 Spawned Bat_01 at (10.5, 2.3) for wave Wave 1
✅ Wave completed: Wave 1
🏆 Wave cleared: Wave 1 (0 waves remaining)
🎉 All waves completed! Executing completion action: OpenExit
```

## 🎯 Best Practices

### Room Design
- **Zone spacing**: Leave enough space between waves for combat
- **Visual cues**: Use environment to hint at wave boundaries
- **Safe zones**: Small areas between waves for player recovery
- **Boss placement**: Final wave should be in largest/special area

### Balance Guidelines
```
Short Room (2-3 waves):  2-3-Boss enemies
Medium Room (3-4 waves): 2-3-4-Boss enemies  
Long Room (4-5 waves):   2-3-4-5-Boss enemies
Difficulty scaling: +10% per wave, +50% for boss
```

### Performance Notes
- Wave zones only active when player nearby
- Enemies cleaned up automatically when wave cleared
- Max 1-2 concurrent waves recommended
- Use object pooling for frequently spawned enemies

## 🚀 Integration với Existing Systems

### RoomManager Integration
```csharp
// RoomManager automatically detects wave vs instant spawning
// No changes needed to existing room prefabs
// Both systems can coexist in same dungeon
```

### EnemyConfiguration Support
```csharp
// Wave system uses same enemy config as instant spawning
// Same difficulty scaling and enemy selection
// Maintains consistency across spawn methods
```

## 📈 Example Implementation

### 1. Create Long Room Prefab
- BoxCollider2D for room bounds
- Environment art stretched horizontally
- Entry/Exit points at ends

### 2. Add Wave System
```csharp
// Add RoomWaveSetup component
useWaveSpawning = true;
numberOfWaves = 4;
baseEnemiesPerWave = 2;
progressionMode = PlayerTriggered;
```

### 3. Configure Waves
```csharp
Wave 1: 2 basic enemies
Wave 2: 3 mixed enemies  
Wave 3: 4 elite enemies
Wave 4: 1 boss + minions
```

### 4. Test & Balance
- Adjust zone sizes based on combat space needed
- Balance enemy counts for pacing
- Test progression flow and difficulty curve

System này cho phép tạo long rooms với encounters staged properly, thay vì overwhelming player với tất cả enemies cùng lúc! 🎮