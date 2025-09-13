using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ensures there's always a Canvas available for UI creation
/// Auto-creates Canvas if none exists in the scene
/// </summary>
public class CanvasEnsurer : MonoBehaviour
{
    [Header("Canvas Settings")]
    public bool createCanvasOnAwake = true;
    public string canvasName = "Game Canvas";
    public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
    public int sortingOrder = 0;

    void Awake()
    {
        if (createCanvasOnAwake)
        {
            EnsureCanvas();
        }
    }

    [ContextMenu("Ensure Canvas Exists")]
    public Canvas EnsureCanvas()
    {
        // Check if canvas already exists
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log($"Canvas already exists: {existingCanvas.name}");
            return existingCanvas;
        }

        // Create new canvas
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = renderMode;
        canvas.sortingOrder = sortingOrder;

        // Add CanvasScaler for responsive UI
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Add GraphicRaycaster for UI interactions
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log($"Created new Canvas: {canvasName}");
        return canvas;
    }

    // Static method for easy access
    public static Canvas GetOrCreateCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        // Create canvas if none exists
        GameObject canvasObj = new GameObject("Auto Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("Auto-created Canvas for UI");
        return canvas;
    }
}