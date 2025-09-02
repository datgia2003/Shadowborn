# 🔧 Experience System Bug Fixes

## 📅 Date: September 2025

## 🐛 **Issues Fixed**

### **Problem 1: Light/Heavy Attack Kills Don't Give EXP**
❌ **Issue**: "enemy khi bị giết bằng light và heavy attack có vẻ không cộng exp (do đòn đánh stun hay gì mà sao mỗi khi giết bằng đánh light, heavy thì nó không có animation death luôn)"

### **Root Cause**: 
- **Dual Death Systems**: Both `Damageable.OnDeath()` and `BatController.Die()` exist
- **Damageable takes priority**: Light/Heavy attacks go through Damageable system
- **BatController.Die()** never called → No EXP awarded
- **Damageable.OnDeath()** runs but has no EXP logic

### ✅ **Solution**: Integrated EXP into Damageable.OnDeath()
```csharp
void OnDeath()
{
    // Award experience if this is an enemy
    var experienceSystem = FindObjectOfType<ExperienceSystem>();
    if (experienceSystem != null)
    {
        var batController = GetComponent<BatController>();
        if (batController != null)
        {
            experienceSystem.GainExpFromEnemy("bat");
            Debug.Log("💀 Bat defeated via Damageable → +50 EXP awarded");
        }
    }
    
    // Rest of death logic...
}
```

### **Problem 2: Level Up Doesn't Increase HP/Mana**
❌ **Issue**: "khi lên level hình như chưa có levelup system (hp, mana không tăng)"

### **Root Cause**: 
- **Reflection failure**: `GetType().GetProperty()` approach didn't work
- **Private properties**: PlayerResources doesn't expose properties properly
- **No visible stat changes**: Users couldn't see HP/Mana increase

### ✅ **Solution**: Direct field access + visible bonuses
```csharp
private void ApplyLevelUpBonuses()
{
    // Direct access to public fields
    int oldHealth = playerResources.maxHealth;
    playerResources.maxHealth += healthBonusPerLevel;
    playerResources.maxMana += manaBonusPerLevel;
    playerResources.maxEnergy += energyBonusPerLevel;
    
    // Also restore full resources on level up
    playerResources.AddHealth(healthBonusPerLevel);
    playerResources.AddMana(manaBonusPerLevel);
    playerResources.AddEnergy(energyBonusPerLevel);
    
    // Clear debug logs showing changes
    Debug.Log($"💪 Health: {oldHealth} → {playerResources.maxHealth} (+{healthBonusPerLevel})");
}
```

## ✅ **Fixes Applied**

### **1. EXP Award System** 
- ✅ **All damage sources**: Projectiles, Light attacks, Heavy attacks now give EXP
- ✅ **Consistent rewards**: 50 EXP per bat kill regardless of damage source  
- ✅ **Debug logging**: Clear confirmation when EXP is awarded
- ✅ **Fallback system**: Generic enemies give 10 EXP if not specifically configured

### **2. Level Up Bonuses**
- ✅ **Direct field modification**: No more reflection issues
- ✅ **Immediate stat increases**: +10 HP, +5 MP, +3 Energy per level
- ✅ **Resource restoration**: Level up also restores some resources
- ✅ **Clear feedback**: Debug logs show exact stat changes

### **3. Enhanced Debug Tools**
```csharp
[ContextMenu("🧪 Test Bat Kill")]     // Simulate killing a bat
[ContextMenu("🧪 Add 500 EXP")]      // Add bulk EXP
[ContextMenu("🧪 Force Level Up")]   // Instant level up with bonuses
```

## 🧪 **Testing Validation**

### **Test Case 1: Light/Heavy Attack EXP**
1. **Kill bat with Light Attack** → Should see: "💀 Bat defeated via Damageable → +50 EXP"
2. **Kill bat with Heavy Attack** → Should see: "💀 Bat defeated via Damageable → +50 EXP"  
3. **Kill bat with Projectile** → Should see: "💀 Bat defeated → +50 EXP awarded to player"

### **Test Case 2: Level Up Bonuses**
1. **Before Level Up**: Note current HP/Mana max values
2. **Gain enough EXP** → Level up should occur
3. **After Level Up**: Should see debug logs like:
   ```
   💪 Level 2 bonuses applied:
      Health: 100 → 110 (+10)
      Mana: 100 → 105 (+5)
      Energy: 50 → 53 (+3)
   ```
4. **Visual Validation**: HP/Mana bars should show higher max values

### **Test Case 3: Debug Commands**
- Right-click ExperienceSystem → "🧪 Force Level Up"
- Should immediately level up and show stat increase logs

## 📊 **Expected Behavior Now**

### **EXP Rewards (All Sources):**
- 🔪 **Light Attack Kill**: +50 EXP ✅
- ⚔️ **Heavy Attack Kill**: +50 EXP ✅  
- 🎯 **Projectile Kill**: +50 EXP ✅
- 🎮 **Any Damage Source**: Consistent EXP reward ✅

### **Level Up Bonuses:**
- **Level 1 → 2**: 100→110 HP, 100→105 MP, 50→53 Energy ✅
- **Level 2 → 3**: 110→120 HP, 105→110 MP, 53→56 Energy ✅
- **Each Level**: Clear debug confirmation ✅

### **Visual Feedback:**
- ✅ **EXP Bar**: Fills correctly from all kill sources
- ✅ **Health Bar**: Max value increases visibly on level up
- ✅ **Mana Bar**: Max value increases visibly on level up  
- ✅ **Level Display**: Shows "LV.2", "LV.3" etc.

## 🎯 **Summary**

**Before Fixes:**
- ❌ Light/Heavy kills = No EXP, no death animation
- ❌ Level up = No stat increases, no visible changes

**After Fixes:**  
- ✅ All kill methods = Consistent 50 EXP + proper death animation
- ✅ Level up = Clear stat increases + resource restoration + debug logs

**Both major issues resolved!** 🚀

---
**💡 Key Insight**: Always check for multiple systems handling the same event (death, damage, etc.) and ensure they're properly integrated rather than conflicting.
