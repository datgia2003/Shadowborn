using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DieUIManager : MonoBehaviour
{
    private GameObject dieCanvas;
    public int coins = 0;
    public int stagesCleared = 0;

    public void ShowDieUI(int coinAmount, int stageCount)
    {
        coins = coinAmount;
        stagesCleared = stageCount;
        if (dieCanvas == null)
            CreateDieUI();
        UpdateDieUI();
        dieCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    void CreateDieUI()
    {
        // Canvas
        dieCanvas = new GameObject("DieCanvas");
        var canvas = dieCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dieCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        dieCanvas.AddComponent<GraphicRaycaster>();

        // Panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(dieCanvas.transform);
        var image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.85f);
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 500);
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Title
        GameObject title = CreateText("You Died", 36, TextAnchor.MiddleCenter, new Vector2(0, 180));
        title.transform.SetParent(panel.transform);

        // Coin text
        GameObject coinText = CreateText("Coins: " + coins, 24, TextAnchor.MiddleCenter, new Vector2(0, 100));
        coinText.name = "CoinText";
        coinText.transform.SetParent(panel.transform);

        // Stage text
        GameObject stageText = CreateText("Stages Cleared: " + stagesCleared, 24, TextAnchor.MiddleCenter, new Vector2(0, 40));
        stageText.name = "StageText";
        stageText.transform.SetParent(panel.transform);

        // Try Again Button
        GameObject tryAgainBtn = CreateButton("Try Again", TryAgain, new Vector2(0, -20));
        tryAgainBtn.transform.SetParent(panel.transform);

        // Main Menu Button
        GameObject mainMenuBtn = CreateButton("Main Menu", ReturnToMainMenu, new Vector2(0, -80));
        mainMenuBtn.transform.SetParent(panel.transform);
    }

    void UpdateDieUI()
    {
        var coinText = dieCanvas.transform.Find("Panel/CoinText").GetComponent<Text>();
        coinText.text = "Coins: " + coins;
        var stageText = dieCanvas.transform.Find("Panel/StageText").GetComponent<Text>();
        stageText.text = "Stages Cleared: " + stagesCleared;
    }

    GameObject CreateText(string text, int fontSize, TextAnchor anchor, Vector2 pos)
    {
        GameObject go = new GameObject(text);
        var txt = go.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = fontSize;
        txt.alignment = anchor;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380, 50);
        rect.anchoredPosition = pos;
        rect.localPosition = new Vector3(pos.x, pos.y, 0);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        return go;
    }

    GameObject CreateButton(string text, UnityEngine.Events.UnityAction action, Vector2 pos)
    {
        GameObject go = new GameObject(text + "Btn");
        var btn = go.AddComponent<Button>();
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.1f, 0.2f, 0.9f);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 50);
        rect.anchoredPosition = pos;
        rect.localPosition = new Vector3(pos.x, pos.y, 0);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        btn.onClick.AddListener(action);

        GameObject txtGo = CreateText(text, 24, TextAnchor.MiddleCenter, Vector2.zero);
        txtGo.transform.SetParent(go.transform);
        var txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.sizeDelta = new Vector2(320, 50);
        txtRect.localPosition = Vector3.zero;
        return go;
    }

    void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
