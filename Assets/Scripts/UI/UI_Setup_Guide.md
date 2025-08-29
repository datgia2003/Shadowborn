# 🎮 UI SETUP GUIDE - STEP BY STEP

## 📋 **BƯỚC 1: TẠO CANVAS**

1. Trong Unity Hierarchy:
   - Right-click → UI → Canvas
   - Đặt tên: "GameHUD"

2. Canvas Settings:
   - Canvas Scaler → UI Scale Mode: "Scale With Screen Size"
   - Reference Resolution: 1920 x 1080
   - Screen Match Mode: "Match Width Or Height"
   - Match: 0.5

## 📋 **BƯỚC 2: TẠO PLAYER HUD CONTAINER**

1. Right-click Canvas → UI → Empty (tạo empty GameObject)
2. Đặt tên: "PlayerHUD"
3. Add Component: PlayerHUD.cs
4. Tạo các child objects:

### 🩸 **A. HEALTH BAR**
```
PlayerHUD
├── HealthBar (UI → Slider)
│   ├── Background (có sẵn)
│   ├── Fill Area (có sẵn)
│   │   └── Fill (Image - màu đỏ/xanh)
│   └── HealthText (UI → Text - TextMeshPro)
```

**Health Bar Setup:**
- Slider: Min=0, Max=1, Value=1
- Background: Dark color
- Fill: Use Gradient (green→yellow→red)
- Text: "100/100"

### 💙 **B. MANA BAR**
```
PlayerHUD
├── ManaBar (UI → Slider)
│   ├── Background (có sẵn)
│   ├── Fill Area (có sẵn)
│   │   └── Fill (Image - màu xanh dương)
│   └── ManaText (UI → Text - TextMeshPro)
```

**Mana Bar Setup:**
- Slider: Min=0, Max=1, Value=1
- Fill: Blue color (#4A9EFF)
- Text: "100/100"

### ⚡ **C. ENERGY BAR**
```
PlayerHUD
├── EnergyBar (UI → Slider)
│   ├── Background (có sẵn)
│   ├── Fill Area (có sẵn)
│   │   └── Fill (Image - màu vàng)
│   └── EnergyText (UI → Text - TextMeshPro)
```

**Energy Bar Setup:**
- Slider: Min=0, Max=1, Value=1
- Fill: Yellow color (#FFD700)
- Text: "50/50"

### 👤 **D. PLAYER AVATAR**
```
PlayerHUD
├── PlayerAvatar (UI → Image)
│   └── AvatarFrame (UI → Image)
```

**Avatar Setup:**
- PlayerAvatar: Your player portrait sprite
- AvatarFrame: Border image around avatar

## 📋 **BƯỚC 3: TẠO SKILL BAR**

1. Right-click Canvas → UI → Empty
2. Đặt tên: "SkillBar"
3. Tạo 4 skill slots:

```
SkillBar
├── SkillSlot1 (UI → Image)
│   ├── SkillIcon (UI → Image)
│   ├── CooldownOverlay (UI → Image)
│   ├── KeybindText (UI → Text)
│   └── CooldownText (UI → Text)
├── SkillSlot2 (copy của SkillSlot1)
├── SkillSlot3 (copy của SkillSlot1)
└── SkillSlot4 (copy của SkillSlot1)
```

**Skill Slot Setup:**
- Background: Dark frame
- SkillIcon: Skill sprite
- CooldownOverlay: Semi-transparent black, Image Type: Filled
- KeybindText: "U", "I", "O", "L"
- CooldownText: "3.2s"

## 📋 **BƯỚC 4: ADD COMPONENTS**

### **A. Player GameObject:**
1. Add PlayerResources.cs
2. Add SkillCooldownManager.cs
3. Add SkillUIIntegration.cs

### **B. Canvas GameObject:**
1. Add UIManager.cs

### **C. PlayerHUD GameObject:**
1. Đã có PlayerHUD.cs

### **D. Mỗi SkillSlot:**
1. Add SkillUISlot.cs

## 📋 **BƯỚC 5: LINK REFERENCES**

### **A. PlayerHUD Component:**
- Health Bar → HealthBar Slider
- Health Text → HealthText 
- Health Fill → Fill Image
- Mana Bar → ManaBar Slider
- Mana Text → ManaText
- Mana Fill → Fill Image
- Energy Bar → EnergyBar Slider
- Energy Text → EnergyText
- Energy Fill → Fill Image
- Player Avatar → PlayerAvatar Image
- Avatar Frame → AvatarFrame Image

### **B. SkillUISlot Components:**
- Skill Icon → SkillIcon Image
- Cooldown Overlay → CooldownOverlay Image
- Keybind Text → KeybindText 
- Cooldown Text → CooldownText

### **C. UIManager Component:**
- Player HUD → PlayerHUD GameObject
- Skill Slots → Array of 4 SkillUISlot components
- Main Canvas → Canvas component

## 📋 **BƯỚC 6: CONFIGURE SETTINGS**

### **A. PlayerResources:**
- Max Health: 100
- Max Mana: 100
- Max Energy: 50
- SliceUp Mana Cost: 20
- Summon Mana Cost: 30
- Ultimate Mana Cost: 50

### **B. SkillCooldownManager:**
- Skill Array (4 items):
  1. SliceUp: 5s cooldown, U key, 20 mana
  2. SummonSkill: 8s cooldown, I key, 30 mana
  3. Ultimate: 15s cooldown, O key, 50 mana
  4. Dodge: 2s cooldown, L key, 0 mana

## 📋 **BƯỚC 7: POSITIONING**

### **Health Bar Position:**
- Anchor: Top Left
- Position: X=20, Y=-20
- Size: 300x30

### **Mana Bar Position:**
- Below Health Bar
- Position: X=20, Y=-60
- Size: 250x25

### **Energy Bar Position:**
- Below Mana Bar
- Position: X=20, Y=-95
- Size: 200x20

### **Player Avatar Position:**
- Top Left corner
- Position: X=350, Y=-20
- Size: 80x80

### **Skill Bar Position:**
- Bottom Center
- Anchor: Bottom Center
- Position: X=0, Y=50
- Each slot: 80x80, spacing 10

## 🎮 **BƯỚC 8: TEST**

1. Play scene
2. Health/Mana/Energy bars should show current values
3. Press U, I, O keys to test skills
4. Check cooldown animations
5. Watch resource consumption

## 🎨 **STYLING TIPS:**

### **Colors:**
- Health: Green (#4CAF50) → Yellow (#FFC107) → Red (#F44336)
- Mana: Blue (#2196F3)
- Energy: Yellow (#FFD700)
- Background: Dark gray (#333333)
- Text: White (#FFFFFF)

### **Fonts:**
- Use TextMeshPro for crisp text
- Bold font for important numbers
- Size 16-20 for UI text

### **Effects:**
- Subtle drop shadows
- Glow effects for low health
- Pulse animation for ready skills
