# 🦇 Enemy Spawning System Setup Guide

## Overview
Hệ thống Enemy Spawning cho Infinity Dungeon với random spawn dựa trên room type và difficulty scaling.

## 📁 Files Created
- `EnemySpawner.cs` - Main spawning logic
- `EnemyConfiguration.cs` - ScriptableObject config
- `RoomEnemySetup.cs` - Auto-setup helper
- `EnemyDifficultyScaling.cs` - Difficulty scaling component

## 🛠️ Setup Instructions

### 1. Create Enemy Configuration Asset
1. **Right-click in Project** → `Create` → `Shadowborn` → `Enemy Configuration`
2. **Name it**: `GlobalEnemyConfig`
3. **Configure enemy pools**:
   - Add normal enemy prefabs với spawn weights
   - Add boss enemy prefabs
   - Set difficulty requirements
   - Configure scaling parameters

### 2. Setup Room Prefabs
**For each room prefab:**
1. **Add RoomEnemySetup component** to root GameObject
2. **Configure settings**:
   - Room Type: "Normal" hoặc "Boss" (auto-detect từ tên)
   - Auto Create Spawn Points: ✅ 
   - Spawn Point Count: 6-10
3. **Add BoxCollider2D** cho room bounds (if not exist)
4. **Right-click component** → `Setup Enemy Spawner` để auto-config

### 3. Configure Global Settings
**Trong EnemyConfiguration asset:**
```csharp
Base Enemy Count: 3
Max Difficulty Bonus: 5  
Enemies Per Difficulty: 0.5
Min Spawn Distance: 2.0
Min Player Distance: 5.0
```

### 4. Add Enemy Prefabs
**Normal Enemies:**
- Set spawn weights (1.0 = normal, 2.0 = double chance)
- Set min difficulty requirement
- Enable/disable difficulty scaling

**Boss Enemies:**
- Usually spawn weight = 1.0
- Higher difficulty requirement
- Enable minion spawning

## 🎯 How It Works

### Room Spawning Flow
1. **RoomManager spawns new room**
2. **Calls OnNewRoomSpawned(difficulty)**
3. **Finds EnemySpawner in room**
4. **EnemySpawner.SpawnEnemiesForRoom(roomType, difficulty)**
5. **Spawns appropriate enemies with scaling**

### Enemy Selection
```csharp
// Normal rooms: 3-8 enemies based on difficulty
int enemyCount = baseCount + (difficulty * 0.5f);

// Boss rooms: 1 boss + optional minions
BossData boss = config.GetRandomBoss(difficulty);
+ minions if difficulty > 2
```

### Difficulty Scaling
```csharp
Health Multiplier = 1.0 + (difficulty - 1) * 0.1  // +10% per level
Damage Multiplier = 1.0 + (difficulty - 1) * 0.1  // +10% per level
Movement = slight increase optional
```

## 🎮 Usage Examples

### In Room Prefab
```csharp
// Auto-setup spawner
RoomEnemySetup setup = GetComponent<RoomEnemySetup>();
setup.roomType = "Normal";
setup.autoCreateSpawnPoints = true;
setup.autoSpawnPointCount = 8;
```

### Custom Enemy Pool Per Room
```csharp
// Override global config for specific room
RoomEnemySetup setup = GetComponent<RoomEnemySetup>();
setup.customEnemyPrefabs = new GameObject[] { 
    specialBatPrefab, 
    eliteSkeletonPrefab 
};
```

### Manual Spawn Testing
```csharp
// Test spawning in editor
EnemySpawner spawner = GetComponent<EnemySpawner>();
spawner.TestSpawnNormalEnemies(); // Context menu
spawner.TestClearEnemies(); // Clear all
```

## 🔧 Debug Features

### Inspector Tools
- **Test Spawn Buttons** trong context menu
- **Debug Gizmos** hiển thị spawn points và room bounds
- **Scaling Info** trên spawned enemies

### Console Logs
- Enemy spawn details với positions
- Scaling multipliers applied
- Spawn weight selections
- Error warnings for invalid configs

## 🚀 Advanced Features

### Custom Enemy Behaviors
```csharp
// In enemy prefab script
EnemyDifficultyScaling scaling = GetComponent<EnemyDifficultyScaling>();
if (scaling != null) {
    float healthBoost = scaling.healthMultiplier;
    float damageBoost = scaling.damageMultiplier;
    // Apply to your enemy stats
}
```

### Room-Specific Logic
```csharp
// Custom spawn logic per room
if (currentRoomType == "Boss") {
    // Boss room specific behavior
    SpawnBossWithMinions();
} else {
    // Normal room waves, ambushes, etc.
    SpawnNormalEnemyWaves();
}
```

## ⚠️ Common Issues

1. **No enemies spawning**: Check if EnemyConfiguration is assigned
2. **Spawn position errors**: Ensure room has BoxCollider2D bounds
3. **Scaling not working**: Check EnemyDifficultyScaling component on spawned enemies
4. **Boss rooms empty**: Make sure boss prefabs are assigned in config

## 🎯 Next Steps
- Create enemy prefabs và add to configuration
- Test spawning trong different room types
- Balance spawn rates và difficulty scaling
- Add special enemy behaviors for higher difficulties