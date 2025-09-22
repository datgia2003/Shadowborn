using UnityEngine;
using System.Collections.Generic;

public enum UpgradeBranch { Power, Survivability, Agility }

[System.Serializable]
public class UpgradeNode
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public bool unlocked;
    public string prerequisiteId;
    public UpgradeBranch branch;
    public int tier;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public List<UpgradeNode> nodes = new List<UpgradeNode>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Nếu chưa có node thì khởi tạo mẫu
        if (nodes.Count == 0)
        {
            nodes = new List<UpgradeNode> {
            // Power Branch
            new UpgradeNode { id="power1", name="+5% Attack Damage", description="Tăng 5% sát thương đánh thường.", cost=100, branch=UpgradeBranch.Power, tier=1 },
            new UpgradeNode { id="power2", name="+5% Crit Chance", description="Tăng 5% tỉ lệ chí mạng.", cost=250, branch=UpgradeBranch.Power, tier=2, prerequisiteId="power1" },
            new UpgradeNode { id="power3", name="+10% Skill Damage", description="Tăng 10% sát thương kỹ năng.", cost=500, branch=UpgradeBranch.Power, tier=3, prerequisiteId="power2" },
            new UpgradeNode { id="power4", name="Shadow Burst", description="Mỗi lần giết quái hồi 2% máu.", cost=1000, branch=UpgradeBranch.Power, tier=4, prerequisiteId="power3" },
            // Survivability Branch
            new UpgradeNode { id="survive1", name="+50 Max HP", description="Tăng 50 máu tối đa.", cost=100, branch=UpgradeBranch.Survivability, tier=1 },
            new UpgradeNode { id="survive2", name="+5 HP khi nhặt Coin", description="Nhặt coin hồi 5 máu.", cost=250, branch=UpgradeBranch.Survivability, tier=2, prerequisiteId="survive1" },
            new UpgradeNode { id="survive3", name="+10% Defense", description="Giảm 10% sát thương nhận vào.", cost=500, branch=UpgradeBranch.Survivability, tier=3, prerequisiteId="survive2" },
            new UpgradeNode { id="survive4", name="Second Chance", description="Chết lần đầu sẽ hồi sinh với 30% máu.", cost=1000, branch=UpgradeBranch.Survivability, tier=4, prerequisiteId="survive3" },
            // Agility Branch
            new UpgradeNode { id="agility1", name="+10% Move Speed", description="Tăng 10% tốc độ di chuyển.", cost=100, branch=UpgradeBranch.Agility, tier=1 },
            new UpgradeNode { id="agility2", name="+10% Jump Force", description="Tăng 10% lực nhảy.", cost=250, branch=UpgradeBranch.Agility, tier=2, prerequisiteId="agility1" },
            new UpgradeNode { id="agility3", name="+15% Mana Regen", description="Tăng 15% hồi mana.", cost=500, branch=UpgradeBranch.Agility, tier=3, prerequisiteId="agility2" },
            new UpgradeNode { id="agility4", name="Perfect Evasion Boost", description="Né đúng lúc tăng tốc đánh 3s.", cost=1000, branch=UpgradeBranch.Agility, tier=4, prerequisiteId="agility3" },
        };
        }
        LoadUpgrades();
    }

    public bool CanUpgrade(string nodeId, int playerCoin)
    {
        var node = nodes.Find(n => n.id == nodeId);
        if (node == null || node.unlocked) return false;
        if (playerCoin < node.cost) return false;
        if (!string.IsNullOrEmpty(node.prerequisiteId))
        {
            var pre = nodes.Find(n => n.id == node.prerequisiteId);
            if (pre == null || !pre.unlocked) return false;
        }
        return true;
    }

    public bool UpgradeNode(string nodeId, ref int playerCoin)
    {
        var node = nodes.Find(n => n.id == nodeId);
        if (node == null || node.unlocked) return false;
        if (!CanUpgrade(nodeId, playerCoin)) return false;
        playerCoin -= node.cost;
        node.unlocked = true;
        SaveUpgrades();
        return true;
    }

    public void LoadUpgrades()
    {
        foreach (var node in nodes)
        {
            node.unlocked = PlayerPrefs.GetInt($"Upgrade_{node.id}", 0) == 1;
        }
    }
    public void SaveUpgrades()
    {
        foreach (var node in nodes)
        {
            PlayerPrefs.SetInt($"Upgrade_{node.id}", node.unlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void ResetUpgrades()
    {
        foreach (var node in nodes)
        {
            node.unlocked = false;
            PlayerPrefs.DeleteKey($"Upgrade_{node.id}");
        }
        PlayerPrefs.Save();
    }
}

// Khởi tạo dữ liệu mẫu cho 3 nhánh upgrade
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public class UpgradeDataInit
{
    static UpgradeDataInit()
    {
        UnityEditor.EditorApplication.update += Init;
    }
    static void Init()
    {
        UnityEditor.EditorApplication.update -= Init;
        var mgr = GameObject.FindObjectOfType<UpgradeManager>();
        if (mgr != null && mgr.nodes.Count == 0)
        {
            mgr.nodes = new List<UpgradeNode> {
                // Power Branch
                new UpgradeNode { id="power1", name="+5% Attack Damage", description="Tăng 5% sát thương đánh thường.", cost=100, branch=UpgradeBranch.Power, tier=1 },
                new UpgradeNode { id="power2", name="+5% Crit Chance", description="Tăng 5% tỉ lệ chí mạng.", cost=250, branch=UpgradeBranch.Power, tier=2, prerequisiteId="power1" },
                new UpgradeNode { id="power3", name="+10% Skill Damage", description="Tăng 10% sát thương kỹ năng.", cost=500, branch=UpgradeBranch.Power, tier=3, prerequisiteId="power2" },
                new UpgradeNode { id="power4", name="Shadow Burst", description="Mỗi lần giết quái hồi 2% máu.", cost=1000, branch=UpgradeBranch.Power, tier=4, prerequisiteId="power3" },
                // Survivability Branch
                new UpgradeNode { id="survive1", name="+50 Max HP", description="Tăng 50 máu tối đa.", cost=100, branch=UpgradeBranch.Survivability, tier=1 },
                new UpgradeNode { id="survive2", name="+5 HP khi nhặt Coin", description="Nhặt coin hồi 5 máu.", cost=250, branch=UpgradeBranch.Survivability, tier=2, prerequisiteId="survive1" },
                new UpgradeNode { id="survive3", name="+10% Defense", description="Giảm 10% sát thương nhận vào.", cost=500, branch=UpgradeBranch.Survivability, tier=3, prerequisiteId="survive2" },
                new UpgradeNode { id="survive4", name="Second Chance", description="Chết lần đầu sẽ hồi sinh với 30% máu.", cost=1000, branch=UpgradeBranch.Survivability, tier=4, prerequisiteId="survive3" },
                // Agility Branch
                new UpgradeNode { id="agility1", name="+10% Move Speed", description="Tăng 10% tốc độ di chuyển.", cost=100, branch=UpgradeBranch.Agility, tier=1 },
                new UpgradeNode { id="agility2", name="+10% Jump Force", description="Tăng 10% lực nhảy.", cost=250, branch=UpgradeBranch.Agility, tier=2, prerequisiteId="agility1" },
                new UpgradeNode { id="agility3", name="+15% Mana Regen", description="Tăng 15% hồi mana.", cost=500, branch=UpgradeBranch.Agility, tier=3, prerequisiteId="agility2" },
                new UpgradeNode { id="agility4", name="Perfect Evasion Boost", description="Né đúng lúc tăng tốc đánh 3s.", cost=1000, branch=UpgradeBranch.Agility, tier=4, prerequisiteId="agility3" },
            };
        }
    }
}
#endif
