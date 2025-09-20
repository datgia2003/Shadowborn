using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int Coin { get; private set; }
    public int HpPotion { get; private set; }
    public int MpPotion { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadInventory();
    }

    public void AddCoin(int amount)
    {
        Coin += amount;
        SaveInventory();
    }
    public bool SpendCoin(int amount)
    {
        if (Coin >= amount)
        {
            Coin -= amount;
            SaveInventory();
            return true;
        }
        return false;
    }
    public void AddHpPotion(int amount)
    {
        HpPotion += amount;
        SaveInventory();
    }
    public void AddMpPotion(int amount)
    {
        MpPotion += amount;
        SaveInventory();
    }

    public void SetHpPotion(int amount)
    {
        HpPotion = amount;
        SaveInventory();
    }
    public void SetMpPotion(int amount)
    {
        MpPotion = amount;
        SaveInventory();
    }
    public void LoadInventory()
    {
        Coin = PlayerPrefs.GetInt("Inventory_Coin", 0);
        HpPotion = PlayerPrefs.GetInt("Inventory_HpPotion", 0);
        MpPotion = PlayerPrefs.GetInt("Inventory_MpPotion", 0);
    }
    public void SaveInventory()
    {
        PlayerPrefs.SetInt("Inventory_Coin", Coin);
        PlayerPrefs.SetInt("Inventory_HpPotion", HpPotion);
        PlayerPrefs.SetInt("Inventory_MpPotion", MpPotion);
        PlayerPrefs.Save();
    }
    public void ResetInventory()
    {
        PlayerPrefs.DeleteKey("Inventory_Coin");
        PlayerPrefs.DeleteKey("Inventory_HpPotion");
        PlayerPrefs.DeleteKey("Inventory_MpPotion");
        PlayerPrefs.Save();
        LoadInventory(); // Đọc lại giá trị mặc định
    }
}
