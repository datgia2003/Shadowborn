using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    private Coroutine panelAnimCoroutine;
    [Header("UI References")]
    public GameObject upgradePanel;
    public Button closeButton;
    public Transform powerBranchRoot;
    public Transform surviveBranchRoot;
    public Transform agilityBranchRoot;
    public GameObject nodeButtonPrefab;
    public GameObject nodeLinePrefab;
    public RectTransform tooltipPanel;
    public TMP_Text tooltipText;
    public TMP_Text coinText;

    private List<UpgradeNodeButton> nodeButtons = new List<UpgradeNodeButton>();

    void Start()
    {
        RefreshUI();
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
        panelAnimCoroutine = StartCoroutine(FadePanel(false));
    }

    public void RefreshUI()
    {
        if (UpgradeManager.Instance == null) return;
        if (coinText != null) coinText.text = $"Vàng: {InventoryManager.Instance?.Coin ?? 0}";
        nodeButtons.Clear();
        // Xóa node cũ
        foreach (Transform t in powerBranchRoot) Destroy(t.gameObject);
        foreach (Transform t in surviveBranchRoot) Destroy(t.gameObject);
        foreach (Transform t in agilityBranchRoot) Destroy(t.gameObject);

        // Tạo node và line cho từng nhánh
        SpawnBranch(UpgradeBranch.Power, powerBranchRoot);
        SpawnBranch(UpgradeBranch.Survivability, surviveBranchRoot);
        SpawnBranch(UpgradeBranch.Agility, agilityBranchRoot);
        // Khi mở panel, chạy hiệu ứng fade in
        if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
        panelAnimCoroutine = StartCoroutine(FadePanel(true));
    }
    private System.Collections.IEnumerator FadePanel(bool fadeIn)
    {
        if (upgradePanel == null) yield break;
        CanvasGroup cg = upgradePanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = upgradePanel.AddComponent<CanvasGroup>();
        float startScale = fadeIn ? 0.8f : 1f;
        float endScale = fadeIn ? 1f : 0.8f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float duration = 0.18f;
        float t = 0f;
        if (fadeIn) upgradePanel.SetActive(true);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, k);
            upgradePanel.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
            yield return null;
        }
        cg.alpha = endAlpha;
        upgradePanel.transform.localScale = Vector3.one * endScale;
        if (!fadeIn) upgradePanel.SetActive(false);
    }


    private void SpawnBranch(UpgradeBranch branch, Transform parent)
    {
        var branchNodes = UpgradeManager.Instance.nodes.FindAll(n => n.branch == branch);
        branchNodes.Sort((a, b) => a.tier.CompareTo(b.tier));
        bool first = true;
        foreach (var node in branchNodes)
        {
            if (!first && nodeLinePrefab != null)
            {
                Instantiate(nodeLinePrefab, parent);
            }
            var btnObj = Instantiate(nodeButtonPrefab, parent);
            var btn = btnObj.GetComponent<UpgradeNodeButton>();
            btn.Setup(node, this);
            // Pass branch info to tooltip
            btn.onHover = (rect) => ShowTooltip(node, rect, branch);
            btn.onExit = HideTooltip;
            nodeButtons.Add(btn);
            first = false;
        }
    }

    public void ShowTooltip(UpgradeNode node, RectTransform nodeRect, UpgradeBranch branch)
    {
        if (tooltipText != null && tooltipPanel != null && nodeRect != null)
        {
            string cond = string.IsNullOrEmpty(node.prerequisiteId) ? "" : $"Yêu cầu: Đã mở node trước";
            string cost = node.unlocked ? "Đã mở" : $"Cost: {node.cost} vàng";
            tooltipText.text = $"<b>{node.name}</b>\n{node.description}\n{cost}\n{cond}";
            tooltipPanel.gameObject.SetActive(true);

            Vector3[] corners = new Vector3[4];
            nodeRect.GetWorldCorners(corners);
            Vector3 leftPos = corners[0]; // bottom left
            Vector3 rightPos = corners[3]; // bottom right
            Vector3 abovePos = (corners[1] + corners[2]) * 0.5f; // top center
            float padding = 20f;
            Vector3 tooltipPos;

            // For Power branch, always show tooltip to the right and above to avoid overlap
            if (branch == UpgradeBranch.Power)
            {
                tooltipPos = rightPos + new Vector3(tooltipPanel.rect.width * 0.5f, tooltipPanel.rect.height * 0.7f, 0);
            }
            else
            {
                tooltipPos = leftPos + new Vector3(-tooltipPanel.rect.width * 0.7f, tooltipPanel.rect.height * 0.5f, 0);
            }

            Canvas canvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Vector2 screenSize = canvas.pixelRect.size;
                Vector2 tooltipSize = tooltipPanel.rect.size;
                Vector2 pos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, tooltipPos);

                // If still off right, show above node
                if (pos.x + tooltipSize.x > screenSize.x - padding)
                {
                    tooltipPos = abovePos + new Vector3(-tooltipPanel.rect.width * 0.5f, tooltipPanel.rect.height * 0.2f, 0);
                    pos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, tooltipPos);
                }
                // Clamp right
                if (pos.x + tooltipSize.x > screenSize.x - padding)
                {
                    tooltipPos.x -= (pos.x + tooltipSize.x - (screenSize.x - padding));
                }
                // Clamp top
                if (pos.y + tooltipSize.y > screenSize.y - padding)
                {
                    tooltipPos.y -= (pos.y + tooltipSize.y - (screenSize.y - padding));
                }
                // Clamp bottom
                if (pos.y < padding)
                {
                    tooltipPos.y += (padding - pos.y);
                }
            }
            // Convert world position to local position in parent canvas
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, tooltipPos), canvas.worldCamera, out localPoint);
            tooltipPanel.anchoredPosition = localPoint;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
    }
    public void TryUpgrade(UpgradeNode node)
    {
        int coin = InventoryManager.Instance?.Coin ?? 0;
        if (UpgradeManager.Instance.CanUpgrade(node.id, coin))
        {
            if (UpgradeManager.Instance.UpgradeNode(node.id, ref coin))
            {
                InventoryManager.Instance.SetCoin(coin);
                RefreshUI();
            }
        }
    }

}

