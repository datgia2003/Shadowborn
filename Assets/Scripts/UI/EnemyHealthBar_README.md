# Enemy Health Bar System Documentation

## Tổng quan
Hệ thống Enemy Health Bar cung cấp floating health bars cho các enemy thường như Bat và Skeleton. Health bars xuất hiện phía trên enemy và hiển thị health theo thời gian thực.

## Các Component chính

### 1. EnemyHealthBar.cs
- Component chính quản lý health bar UI
- Tự động tạo Canvas và UI elements
- Hỗ trợ animation và color transitions
- Tự động ẩn/hiện khi cần thiết

### 2. EnemyHealthBarSetup.cs
- Utility script để config health bar settings
- Tự động thêm EnemyHealthBar component
- Preset settings cho từng loại enemy

### 3. EnemyHealthBarTools.cs (Editor)
- Editor tools để bulk setup health bars
- Menu: Tools > Shadowborn > Enemy Health Bar Tools
- Hỗ trợ add/remove health bars cho nhiều enemies

### 4. EnemyHealthBarDemo.cs
- Demo script để test health bar system
- Tạo demo enemies và test controls

## Cách sử dụng

### Setup tự động (Recommended)
1. Mở menu `Tools > Shadowborn > Add Health Bars to All Enemies`
2. Tool sẽ tự động tìm tất cả Bat và Skeleton trong scene
3. Thêm health bar cho từng enemy với settings phù hợp

### Setup thủ công
1. Thêm component `EnemyHealthBarSetup` vào enemy prefab
2. Configure settings trong Inspector:
   - `enableHealthBar`: Bật/tắt health bar
   - `showOnlyWhenDamaged`: Chỉ hiện khi bị damage
   - `alwaysVisible`: Luôn hiển thị
   - `hideDelay`: Thời gian ẩn sau khi không combat
   - `healthBarOffset`: Vị trí health bar
   - `healthBarScale`: Kích thước health bar
   - Colors và animation settings

### Integration với Enemy Controllers
Health bar system đã được integrate với:
- `BatController.cs`: Getter methods và health updates
- `SkeletonController.cs`: Getter methods và health updates

## Các tính năng

### Visual Features
- ✅ Floating health bar phía trên enemy
- ✅ Color transitions (Green → Yellow → Red)
- ✅ Smooth animations khi health thay đổi
- ✅ Auto-hide khi không combat
- ✅ Always face camera
- ✅ Scale với distance (optional)

### Behavior Features
- ✅ Chỉ hiện khi enemy bị damage
- ✅ Tự động ẩn sau một thời gian
- ✅ Ẩn khi enemy chết
- ✅ Real-time health updates

### Editor Features
- ✅ Bulk add/remove health bars
- ✅ Settings window với preview
- ✅ Context menu actions
- ✅ Preset configs cho từng enemy type

## Customization

### Health Bar Settings
```csharp
// Offset từ enemy position
public Vector3 offset = new Vector3(0, 1.5f, 0);

// Thời gian ẩn health bar
public float hideDelay = 3f;

// Colors
public Color fullHealthColor = Color.green;
public Color midHealthColor = Color.yellow;
public Color lowHealthColor = Color.red;

// Animation speed
public float animationSpeed = 5f;
public float colorTransitionSpeed = 3f;
```

### Per-Enemy Type Settings
```csharp
// Bat settings
healthBarOffset = new Vector3(0, 1.2f, 0);
healthBarScale = Vector3.one * 0.8f;
hideDelay = 2f;

// Skeleton settings  
healthBarOffset = new Vector3(0, 1.8f, 0);
healthBarScale = Vector3.one;
hideDelay = 3f;
```

## API Reference

### EnemyHealthBar Public Methods
```csharp
// Hiển thị health bar
public void ShowHealthBar()

// Ẩn health bar  
public void HideHealthBar()

// Set health value
public void SetHealth(int current, int max)

// Apply damage
public void TakeDamage(int damage)
```

### Required Enemy Controller Methods
```csharp
// Getter methods cần có trong BatController/SkeletonController
public int GetCurrentHealth()
public int GetMaxHealth()
```

## Testing

### Demo Scene Setup
1. Tạo GameObject và attach `EnemyHealthBarDemo`
2. Configure số lượng enemies
3. Chạy scene và dùng controls:
   - `Space`: Damage all enemies
   - `H`: Heal all enemies

### Manual Testing
1. Tạo enemy trong scene
2. Add `EnemyHealthBarSetup` component
3. Play scene và test bằng cách damage enemy

## Troubleshooting

### Common Issues
1. **Health bar không xuất hiện**
   - Check `enableHealthBar = true`
   - Check enemy có BatController/SkeletonController
   - Check Console for errors

2. **Health bar không update**
   - Check enemy controller có getter methods
   - Check TakeDamage method có call health bar update

3. **Health bar position sai**
   - Adjust `healthBarOffset` trong settings
   - Check camera reference

### Debug Logs
System có debug logs để track:
- Health bar creation
- Health updates
- Show/hide events

## Performance Notes

- Health bars chỉ active khi cần thiết
- Auto-pooling cho WorldSpace Canvas
- Optimized update loops
- Memory cleanup khi enemy destroyed

## Future Enhancements

Có thể thêm:
- [ ] Health bar pooling system
- [ ] More animation effects
- [ ] Shield/armor indicators
- [ ] Status effect icons
- [ ] Boss health bars integration