using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Dungeon generator (prototype) dùng Tile màu, phong cách Solo Leveling:
/// - Entrance có cổng + đuốc + rune
/// - Hành lang tối có đuốc thưa
/// - Combat room có rune rải + tượng canh + decal vết nứt/máu
/// - Chest room có rương giữa, tượng canh + rune bệ
/// - Boss room to, có vòng triệu hồi (sigil), đuốc 4 góc, tượng trên
/// - Exit portal
/// Tất cả vị trí prop đã "vẽ" ra bằng tile màu. Sau này chỉ cần swap tile màu → art thật.
///
/// Cách dùng:
/// - Tạo 1 Grid + 1 Tilemap (gán vào "tilemap").
/// - Tạo 8 tile màu placeholder và assign vào Inspector.
/// - Chạy Play để spawn.
/// </summary>
public class DungeonAutoGeneratorRandom : MonoBehaviour
{
    [Header("Target Tilemap")]
    public Tilemap tilemap;

    [Header("Placeholder Tiles")]
    public TileBase wallTile;        // Tường (xám)
    public TileBase floorTile;       // Nền (đen)
    public TileBase gateTile;        // Cổng/Portal (xanh dương)
    public TileBase torchTile;       // Đuốc (vàng)
    public TileBase runeTile;        // Rune (đỏ)
    public TileBase chestTile;       // Rương (xanh lá)
    public TileBase statueTile;      // Tượng (tím)
    public TileBase decalTile;       // Vết nứt/máu (trắng)
    public TileBase bossSigilTile;   // Vòng triệu hồi (đỏ tươi; có thể reuse runeTile)

    [Header("Global Config")]
    public bool useRandomSeed = false;
    public int fixedSeed = 20250820;
    public int tilePPU = 32; // chỉ để nhắc nhở đồng bộ import
    [Tooltip("Khoảng cách dọc giữa các node (phòng/hành lang)")]
    public int verticalStep = 6;

    [Header("Room Size Ranges")]
    public Vector2Int roomSizeMin = new Vector2Int(18, 12);
    public Vector2Int roomSizeMax = new Vector2Int(26, 18);
    public Vector2Int bossSizeMin = new Vector2Int(28, 20);
    public Vector2Int bossSizeMax = new Vector2Int(36, 26);
    public Vector2Int corridorSize = new Vector2Int(8, 10); // width x length

    [Header("Prop Densities / Probabilities")]
    [Range(0f, 1f)] public float torchAlongWallChance = 0.18f;
    [Range(0f, 1f)] public float torchAlongCorridorChance = 0.22f;
    [Range(0f, 1f)] public float runeScatterChance = 0.08f;
    [Range(0f, 1f)] public float decalScatterChance = 0.06f;
    [Range(0f, 1f)] public float extraStatueChance = 0.35f;

    [Header("Clamps to avoid emptiness/overcrowding")]
    public int minTorchesPerRoom = 2;
    public int maxTorchesPerRoom = 8;
    public int minRunesPerRoom = 2;
    public int maxRunesPerRoom = 12;
    public int minDecalsPerRoom = 1;
    public int maxDecalsPerRoom = 10;

    // working RNG
    System.Random rng;

    void Reset()
    {
        // sensible defaults in inspector if added
        fixedSeed = 20250820;
        roomSizeMin = new Vector2Int(18, 12);
        roomSizeMax = new Vector2Int(26, 18);
        bossSizeMin = new Vector2Int(28, 20);
        bossSizeMax = new Vector2Int(36, 26);
        corridorSize = new Vector2Int(8, 10);
    }

    void Start()
    {
        rng = useRandomSeed ? new System.Random() : new System.Random(fixedSeed);
        GenerateDungeon();
    }

    // ===== Core generation pipeline =====

    void GenerateDungeon()
    {
        tilemap.ClearAllTiles();

        Vector3Int cursor = Vector3Int.zero;

        // 1) Entrance (room)
        Vector2Int entSize = RandSize(roomSizeMin, roomSizeMax);
        RectI entRect = RoomAt(cursor, entSize);
        DrawHollow(entRect, wallTile, floorTile);
        DecorEntrance(entRect);
        cursor = entRect.bottomMid + new Vector3Int(-corridorSize.x / 2, -verticalStep, 0);

        // 2) Corridor
        RectI cor1Rect = CorridorAt(cursor, corridorSize);
        DrawHollow(cor1Rect, wallTile, floorTile);
        DecorCorridor(cor1Rect, torchAlongCorridorChance);
        cursor = cor1Rect.bottomMid + new Vector3Int(-(RandRange(0, 2)), -verticalStep, 0);

        // 3) Combat room
        Vector2Int combSize = RandSize(roomSizeMin, roomSizeMax);
        RectI combRect = RoomAt(cursor, combSize);
        DrawHollow(combRect, wallTile, floorTile);
        DecorCombat(combRect);
        cursor = combRect.bottomMid + new Vector3Int(-corridorSize.x / 2, -verticalStep, 0);

        // 4) Chest/Mid room
        RectI cor2Rect = CorridorAt(cursor, corridorSize);
        DrawHollow(cor2Rect, wallTile, floorTile);
        DecorCorridor(cor2Rect, torchAlongCorridorChance * 0.8f);
        cursor = cor2Rect.bottomMid + new Vector3Int(-RandRange(0, 2), -verticalStep, 0);

        Vector2Int midSize = RandSize(roomSizeMin, roomSizeMax);
        RectI midRect = RoomAt(cursor, midSize);
        DrawHollow(midRect, wallTile, floorTile);
        DecorChest(midRect);
        cursor = midRect.bottomMid + new Vector3Int(-corridorSize.x / 2, -verticalStep, 0);

        // 5) Boss room (bigger)
        RectI cor3Rect = CorridorAt(cursor, corridorSize + new Vector2Int(0, RandRange(-2, 3)));
        DrawHollow(cor3Rect, wallTile, floorTile);
        DecorCorridor(cor3Rect, torchAlongCorridorChance * 1.2f);
        cursor = cor3Rect.bottomMid + new Vector3Int(-RandRange(0, 2), -verticalStep, 0);

        Vector2Int bossSize = RandSize(bossSizeMin, bossSizeMax);
        RectI bossRect = RoomAt(cursor, bossSize);
        DrawHollow(bossRect, wallTile, floorTile);
        DecorBoss(bossRect);

        // 6) Exit room (small/normal) placed after a short corridor
        cursor = bossRect.bottomMid + new Vector3Int(-corridorSize.x / 2, -verticalStep, 0);
        RectI corExitRect = CorridorAt(cursor, corridorSize + new Vector2Int(0, RandRange(-2, 2)));
        DrawHollow(corExitRect, wallTile, floorTile);
        DecorCorridor(corExitRect, torchAlongCorridorChance * 0.6f);
        cursor = corExitRect.bottomMid + new Vector3Int(-RandRange(0, 2), -verticalStep, 0);

        Vector2Int exitSize = RandSize(roomSizeMin, roomSizeMax);
        RectI exitRect = RoomAt(cursor, exitSize);
        DrawHollow(exitRect, wallTile, floorTile);
        DecorExit(exitRect);
    }

    // ===== Rect & drawing helpers =====
    struct RectI
    {
        public Vector3Int min; // inclusive
        public Vector3Int size;
        public Vector3Int max => new Vector3Int(min.x + size.x - 1, min.y + size.y - 1, 0);

        public Vector3Int center => new Vector3Int(min.x + size.x / 2, min.y + size.y / 2, 0);
        public Vector3Int topMid => new Vector3Int(min.x + size.x / 2, max.y, 0);
        public Vector3Int bottomMid => new Vector3Int(min.x + size.x / 2, min.y, 0);
    }

    RectI RoomAt(Vector3Int anchorBottomMid, Vector2Int size)
    {
        // anchor là đáy-giữa; dựng phòng lên trên (y+)
        Vector3Int min = new Vector3Int(anchorBottomMid.x - size.x / 2, anchorBottomMid.y, 0);
        return new RectI { min = min, size = new Vector3Int(size.x, size.y, 0) };
    }

    RectI CorridorAt(Vector3Int anchorBottomMid, Vector2Int size)
    {
        // corridor dựng lên trên, hẹp theo width, dài theo height
        Vector3Int min = new Vector3Int(anchorBottomMid.x - size.x / 2, anchorBottomMid.y, 0);
        return new RectI { min = min, size = new Vector3Int(size.x, size.y, 0) };
    }

    void DrawHollow(RectI r, TileBase wall, TileBase floor)
    {
        // Fill walls
        for (int x = 0; x < r.size.x; x++)
        {
            for (int y = 0; y < r.size.y; y++)
            {
                Vector3Int p = new Vector3Int(r.min.x + x, r.min.y + y, 0);
                bool border = (x == 0 || y == 0 || x == r.size.x - 1 || y == r.size.y - 1);
                tilemap.SetTile(p, border ? wall : floor);
            }
        }
    }

    // ===== Decorators =====

    void DecorEntrance(RectI r)
    {
        // Gate ở giữa đáy
        Place(r.bottomMid, gateTile);

        // 2 đuốc 2 bên (khoảng 1/3 chiều cao)
        int torchY = r.min.y + r.size.y / 3;
        Place(new Vector3Int(r.min.x + 2, torchY, 0), torchTile);
        Place(new Vector3Int(r.max.x - 2, torchY, 0), torchTile);

        // Rune giữa phòng
        PlaceSafe(r, r.center, runeTile);

        // Một vài decal xung quanh
        ScatterInside(r, decalTile, decalScatterChance * 0.5f, 0, 4);
    }

    void DecorCorridor(RectI r, float torchChance)
    {
        int placedTorches = 0;
        for (int y = r.min.y + 2; y <= r.max.y - 2; y++)
        {
            // mỗi hàng có thể đặt 0/1 đuốc
            if (Chance(torchChance))
            {
                // trái hoặc phải
                int x = Chance(0.5f) ? r.min.x + 1 : r.max.x - 1;
                Place(new Vector3Int(x, y, 0), torchTile);
                placedTorches++;
            }
        }
        // clamp tối thiểu
        if (placedTorches < 2)
        {
            Place(new Vector3Int(r.min.x + 1, r.center.y, 0), torchTile);
            Place(new Vector3Int(r.max.x - 1, r.center.y + 2, 0), torchTile);
        }
    }

    void DecorCombat(RectI r)
    {
        // Runes theo pattern + scatter
        DrawRuneCross(r, step: 4);
        int runes = ScatterInside(r, runeTile, runeScatterChance, minRunesPerRoom, maxRunesPerRoom);

        // Tượng ở hai góc trên
        Place(new Vector3Int(r.min.x + 2, r.max.y - 1, 0), statueTile);
        Place(new Vector3Int(r.max.x - 2, r.max.y - 1, 0), statueTile);

        // Đuốc rải dọc tường
        int torches = ScatterAlongWalls(r, torchTile, torchAlongWallChance, minTorchesPerRoom, maxTorchesPerRoom);

        // Vết nứt/máu lác đác
        ScatterInside(r, decalTile, decalScatterChance, minDecalsPerRoom, maxDecalsPerRoom);
    }

    void DecorChest(RectI r)
    {
        // Rương giữa, rune dưới chân như bệ
        Place(r.center, chestTile);
        PlaceSafe(r, r.center + new Vector3Int(1, 0, 0), runeTile);
        PlaceSafe(r, r.center + new Vector3Int(-1, 0, 0), runeTile);

        // Hai tượng canh bên
        Place(new Vector3Int(r.center.x - 3, r.center.y, 0), statueTile);
        Place(new Vector3Int(r.center.x + 3, r.center.y, 0), statueTile);

        // Đuốc 2 bên tường giữa phòng
        Place(new Vector3Int(r.min.x + 1, r.center.y, 0), torchTile);
        Place(new Vector3Int(r.max.x - 1, r.center.y, 0), torchTile);

        // Runes + Decals nhẹ
        ScatterInside(r, runeTile, runeScatterChance * 0.5f, 1, 6);
        ScatterInside(r, decalTile, decalScatterChance * 0.8f, 1, 6);

        // Statues phụ (random)
        if (Chance(extraStatueChance))
            Place(new Vector3Int(r.center.x, r.max.y - 2, 0), statueTile);
    }

    void DecorBoss(RectI r)
    {
        // Đuốc 4 góc trong
        Place(new Vector3Int(r.min.x + 2, r.min.y + 2, 0), torchTile);
        Place(new Vector3Int(r.max.x - 2, r.min.y + 2, 0), torchTile);
        Place(new Vector3Int(r.min.x + 2, r.max.y - 2, 0), torchTile);
        Place(new Vector3Int(r.max.x - 2, r.max.y - 2, 0), torchTile);

        // Tượng đôi phía trên
        Place(new Vector3Int(r.center.x - 5, r.max.y - 2, 0), statueTile);
        Place(new Vector3Int(r.center.x + 5, r.max.y - 2, 0), statueTile);

        // Vòng triệu hồi Boss (sigil) ở giữa
        DrawBossSigil(r, radiusX: Mathf.Max(6, r.size.x / 4), radiusY: Mathf.Max(4, r.size.y / 4));

        // Runes rải thưa ở vành ngoài
        ScatterInside(r, runeTile, runeScatterChance * 0.6f, 2, 10);

        // Decal nứt vỡ
        ScatterInside(r, decalTile, decalScatterChance * 1.2f, 2, 12);
    }

    void DecorExit(RectI r)
    {
        // Portal ở giữa đỉnh
        Place(new Vector3Int(r.center.x, r.max.y, 0), gateTile);

        // Rune triền thấp
        DrawRuneArcBottom(r, spacing: 3);

        // Đuốc dẫn đường
        Place(new Vector3Int(r.center.x - 4, r.min.y + 1, 0), torchTile);
        Place(new Vector3Int(r.center.x + 4, r.min.y + 1, 0), torchTile);

        // Đá vỡ (decal) gần tâm
        ScatterInside(r, decalTile, decalScatterChance * 0.7f, 1, 6);
    }

    // ===== Shape drawing helpers =====

    void DrawRuneCross(RectI r, int step)
    {
        for (int x = r.min.x + 2; x <= r.max.x - 2; x += step)
            PlaceSafe(r, new Vector3Int(x, r.center.y, 0), runeTile);
        for (int y = r.min.y + 2; y <= r.max.y - 2; y += step)
            PlaceSafe(r, new Vector3Int(r.center.x, y, 0), runeTile);
    }

    void DrawRuneArcBottom(RectI r, int spacing)
    {
        int baseY = r.min.y + 2;
        for (int dx = -(r.size.x / 2 - 3); dx <= (r.size.x / 2 - 3); dx += spacing)
        {
            int x = r.center.x + dx;
            int y = baseY + Mathf.RoundToInt(Mathf.Lerp(0, 3, Mathf.InverseLerp(-(r.size.x / 2), (r.size.x / 2), dx)));
            PlaceSafe(r, new Vector3Int(x, y, 0), runeTile);
        }
    }

    void DrawBossSigil(RectI r, int radiusX, int radiusY)
    {
        // ellipse discrete
        for (int i = 0; i < 360; i += 6)
        {
            float rad = i * Mathf.Deg2Rad;
            int x = r.center.x + Mathf.RoundToInt(Mathf.Cos(rad) * radiusX);
            int y = r.center.y + Mathf.RoundToInt(Mathf.Sin(rad) * radiusY);
            PlaceSafe(r, new Vector3Int(x, y, 0), bossSigilTile ? bossSigilTile : runeTile);
        }
        // thêm dấu cộng ở tâm
        for (int dx = -2; dx <= 2; dx++)
            PlaceSafe(r, new Vector3Int(r.center.x + dx, r.center.y, 0), bossSigilTile ? bossSigilTile : runeTile);
        for (int dy = -2; dy <= 2; dy++)
            PlaceSafe(r, new Vector3Int(r.center.x, r.center.y + dy, 0), bossSigilTile ? bossSigilTile : runeTile);
    }

    int ScatterInside(RectI r, TileBase tile, float chance, int minClamp, int maxClamp)
    {
        int placed = 0;
        for (int x = r.min.x + 2; x <= r.max.x - 2; x++)
        {
            for (int y = r.min.y + 2; y <= r.max.y - 2; y++)
            {
                if (Chance(chance))
                {
                    Place(new Vector3Int(x, y, 0), tile);
                    placed++;
                }
            }
        }
        // clamp: thêm/bớt để đạt min
        while (placed < minClamp)
        {
            Vector3Int p = RandPointInside(r, margin: 2);
            if (tilemap.GetTile(p) == floorTile)
            {
                Place(p, tile);
                placed++;
            }
        }
        // không cần remove nếu > max (chấp nhận hơi dày), nhưng có thể cap mềm
        return placed;
    }

    int ScatterAlongWalls(RectI r, TileBase tile, float chance, int minClamp, int maxClamp)
    {
        int placed = 0;
        // trái/phải
        for (int y = r.min.y + 2; y <= r.max.y - 2; y++)
        {
            if (Chance(chance)) { Place(new Vector3Int(r.min.x + 1, y, 0), tile); placed++; }
            if (Chance(chance)) { Place(new Vector3Int(r.max.x - 1, y, 0), tile); placed++; }
        }
        // trên/dưới
        for (int x = r.min.x + 2; x <= r.max.x - 2; x++)
        {
            if (Chance(chance * 0.7f)) { Place(new Vector3Int(x, r.min.y + 1, 0), tile); placed++; }
            if (Chance(chance * 0.7f)) { Place(new Vector3Int(x, r.max.y - 1, 0), tile); placed++; }
        }
        // đảm bảo tối thiểu
        while (placed < minClamp)
        {
            Vector3Int p = new Vector3Int(
                Chance(0.5f) ? r.min.x + 1 : r.max.x - 1,
                RandRange(r.min.y + 2, r.max.y - 2),
                0
            );
            Place(p, tile);
            placed++;
        }
        return placed;
    }

    // ===== Low-level helpers =====

    void Place(Vector3Int p, TileBase t)
    {
        if (t == null) return;
        tilemap.SetTile(p, t);
    }

    void PlaceSafe(RectI r, Vector3Int p, TileBase t)
    {
        if (t == null) return;
        if (p.x <= r.min.x + 0 || p.x >= r.max.x - 0) return;
        if (p.y <= r.min.y + 0 || p.y >= r.max.y - 0) return;
        tilemap.SetTile(p, t);
    }

    Vector2Int RandSize(Vector2Int min, Vector2Int max)
    {
        return new Vector2Int(RandRange(min.x, max.x), RandRange(min.y, max.y));
    }

    Vector3Int RandPointInside(RectI r, int margin = 1)
    {
        int x = RandRange(r.min.x + margin, r.max.x - margin);
        int y = RandRange(r.min.y + margin, r.max.y - margin);
        return new Vector3Int(x, y, 0);
    }

    int RandRange(int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive) { var t = minInclusive; minInclusive = maxInclusive; maxInclusive = t; }
        return rng.Next(minInclusive, maxInclusive + 1);
    }

    bool Chance(float p) => rng.NextDouble() < Mathf.Clamp01(p);
}
