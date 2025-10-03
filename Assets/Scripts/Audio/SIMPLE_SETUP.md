# 🎵 Simple Audio Setup - Chỉ 5 Phút!

## 📋 Những gì bạn sẽ có:
- **MainMenuScene:** Nhạc nền + âm thanh hover/click buttons
- **SimpleScene:** Nhạc nền dungeon + âm thanh nhặt coin/items

---

## 🚀 Setup Cực Nhanh:

### Bước 1: Tạo SimpleAudioManager
1. **Trong MainMenuScene:**
   - Tạo empty GameObject tên `SimpleAudioManager`
   - Add component `SimpleAudioManager.cs`
   - Assign audio clips:
     ```
     Main Menu Music: [nhạc nền menu]
     Dungeon Music: [nhạc nền dungeon]
     Button Hover Sound: [âm hover]
     Button Click Sound: [âm click]
     Coin Pickup Sound: [âm nhặt coin]
     Item Pickup Sound: [âm nhặt item]
     ```

2. **Tạo Prefab:**
   - Drag SimpleAudioManager vào Project window
   - Save as `SimpleAudioManager.prefab`

### Bước 2: Setup UI Buttons (MainMenu)
1. **Chọn tất cả buttons** (Play, Settings, Quit, etc.)
2. **Add component:** `SimpleUIButton.cs`
3. **Done!** - Buttons sẽ tự động có âm thanh

### Bước 3: Setup Pickups (SimpleScene)
1. **Coin objects:**
   - Add component: `SimplePickup.cs`
   - Check `Is Coin = true`

2. **Item objects:**
   - Add component: `SimplePickup.cs`  
   - Leave `Is Coin = false`

### Bước 4: Room Prefabs Integration
**Nếu bạn có room prefabs trong RoomManager:**

1. **Mở từng room prefab**
2. **Tìm coin/item objects trong prefab**
3. **Add component `SimplePickup.cs`** vào chúng
4. **Save prefab**

**Hoặc làm hàng loạt:**
- Mở scene có room prefabs
- Select all coins: Add `SimplePickup` + check `Is Coin`
- Select all items: Add `SimplePickup` + leave unchecked
- Apply changes to prefabs

---

## ✅ Xong! Chỉ có thế thôi!

### 🎵 Auto Features:
- ✅ Nhạc tự động chơi khi load scene
- ✅ Volume được save/load tự động  
- ✅ Cross-scene persistence
- ✅ Buttons tự động có âm thanh
- ✅ Pickups tự động có âm thanh

### 🎮 Sử dụng trong code (optional):
```csharp
// Play sounds manually
SimpleAudioManager.Instance.PlayCoinPickup();
SimpleAudioManager.Instance.PlayItemPickup();
SimpleAudioManager.Instance.PlayButtonClick();

// Control volume
SimpleAudioManager.Instance.SetMusicVolume(0.5f);
SimpleAudioManager.Instance.SetSFXVolume(0.8f);
```

---

## 🔧 Nếu cần Volume Sliders:
```csharp
// Attach to sliders' OnValueChanged
public void OnMusicVolumeChanged(float value)
{
    SimpleAudioManager.Instance.SetMusicVolume(value);
}

public void OnSFXVolumeChanged(float value)
{
    SimpleAudioManager.Instance.SetSFXVolume(value);
}
```

**Thế là xong! Siêu đơn giản! 🎵**