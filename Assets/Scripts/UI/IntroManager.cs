using UnityEngine;
using Cinemachine;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Voice")]
    public AudioSource audioSource;
    public AudioClip[] voiceClips;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera gameplayCamera;
    public CinemachineVirtualCamera introCamera;

    [Header("Player")]
    public GameObject player;
    public PlayerController playerController;
    public PlayerCombat playerCombat;

    public SummonSkill summonSkill; // Nếu có summon skill, để khóa skill trong intro

    public SliceUpSkill sliceUpSkill; // Nếu có slice up skill, để khóa skill trong intro

    public TruthMultilateUltimate truthUltimate; // Nếu có truth ultimate, để khóa skill trong intro
    public DodgeSkill dodgeSkill; // Nếu có dodge skill, để khóa skill trong intro

    [Header("UI")]
    public GameObject hudCanvas;
    public GameObject speechBubble;
    public TMPro.TMP_Text speechText;

    [Header("Intro Settings")]
    public float walkDuration = 1.5f;
    public string[] introDialogues = {
        "Mình đã đến dungeon này...",
        "Phải cẩn thận từng bước!"
    };

    [Header("Camera Zoom")]
    public float introZoomSize = 2.5f; // Giá trị zoom khi intro
    public float zoomDuration = 0.8f;  // Thời gian chuyển zoom
    private float originalZoomSize;

    [Header("Intro UI Effect")]
    public GameObject startTextObj; // UI Text object for 'Bắt đầu'
    public TMPro.TMP_Text startText;
    public float startTextFadeDuration = 0.6f;
    public float startTextDisplayTime = 1.2f;

    private void Start()
    {
        PlacePlayerAtEntry();
        // Tự động lấy PlayerCombat nếu chưa gán
        if (playerCombat == null && player != null)
            playerCombat = player.GetComponent<PlayerCombat>();
        // Lưu lại size gốc của camera intro
        if (introCamera != null)
            originalZoomSize = introCamera.m_Lens.OrthographicSize;
        StartCoroutine(IntroSequence());
    }

    private void PlacePlayerAtEntry()
    {
        var roomManager = RoomManager.Instance;
        if (roomManager == null || player == null) return;
        GameObject currentRoom = roomManager.GetCurrentRoom();
        if (currentRoom == null) return;
        Transform entry = currentRoom.transform.Find("Entry");
        if (entry == null)
        {
            for (int i = 0; i < currentRoom.transform.childCount; i++)
            {
                var child = currentRoom.transform.GetChild(i);
                if (child.name.ToLower().Contains("entry"))
                {
                    entry = child;
                    break;
                }
            }
        }
        if (entry != null)
            player.transform.position = entry.position;
    }

    IEnumerator IntroSequence()
    {
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (introCamera != null) introCamera.Priority = 20;
        if (gameplayCamera != null) gameplayCamera.Priority = 10;
        // Zoom in mượt
        if (introCamera != null)
            yield return StartCoroutine(SmoothZoom(introCamera, introZoomSize, zoomDuration));

        // Lock input & combat hoàn toàn khi intro bắt đầu
        if (playerController != null) playerController.isIntroLock = true;
        if (playerCombat != null) playerCombat.isIntroLock = true;
        if (summonSkill != null) summonSkill.isIntroLock = true;
        if (sliceUpSkill != null) sliceUpSkill.isIntroLock = true;
        if (truthUltimate != null) truthUltimate.isIntroLock = true;
        if (dodgeSkill != null) dodgeSkill.isIntroLock = true;
        if (playerController != null) playerController.SetMoveInput(Vector2.right);
        yield return new WaitForSeconds(walkDuration);
        if (playerController != null) playerController.SetMoveInput(Vector2.zero);

        // Sau khi intro kết thúc, mở lại input & combat
        if (playerController != null) playerController.isIntroLock = false;
        if (playerCombat != null) playerCombat.isIntroLock = false;
        if (summonSkill != null) summonSkill.isIntroLock = false;
        if (sliceUpSkill != null) sliceUpSkill.isIntroLock = false;
        if (truthUltimate != null) truthUltimate.isIntroLock = false;
        if (dodgeSkill != null) dodgeSkill.isIntroLock = false;

        // Play random voice ONCE at intro
        PlayVoice(0);

        if (speechBubble != null) speechBubble.SetActive(true);
        for (int i = 0; i < introDialogues.Length; i++)
        {
            if (speechBubble != null && !speechBubble.activeSelf) speechBubble.SetActive(true);
            if (speechText != null) speechText.text = introDialogues[i];
            yield return new WaitForSeconds(audioSource != null && audioSource.clip != null ? audioSource.clip.length : 1.5f);
        }
        if (speechBubble != null) speechBubble.SetActive(false);

        // Zoom out mượt về size gốc
        if (introCamera != null)
            yield return StartCoroutine(SmoothZoom(introCamera, originalZoomSize, zoomDuration));

        if (introCamera != null) introCamera.Priority = 10;
        if (gameplayCamera != null) gameplayCamera.Priority = 20;
        if (hudCanvas != null) hudCanvas.SetActive(true);
        if (playerController != null) playerController.SetInputEnabled(true);
        // Hiện 'Bắt đầu' với hiệu ứng fade in/out
        if (startTextObj != null && startText != null)
            yield return StartCoroutine(ShowStartTextEffect());
    }

    private void PlayVoice(int idx)
    {
        // Chọn ngẫu nhiên 1 voice từ danh sách voiceClips
        if (audioSource != null && voiceClips != null && voiceClips.Length > 0)
        {
            int randomIdx = Random.Range(0, voiceClips.Length);
            if (voiceClips[randomIdx] != null)
            {
                audioSource.clip = voiceClips[randomIdx];
                audioSource.Play();
            }
        }
    }

    // Coroutine zoom mượt cho camera 2D
    IEnumerator SmoothZoom(CinemachineVirtualCamera cam, float targetSize, float duration)
    {
        float startSize = cam.m_Lens.OrthographicSize;
        float time = 0f;
        while (time < duration)
        {
            cam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cam.m_Lens.OrthographicSize = targetSize;
    }

    IEnumerator ShowStartTextEffect()
    {
        startTextObj.SetActive(true);
        startText.text = "Bắt đầu";
        // Bắt đầu ở ngoài màn hình bên phải
        RectTransform rect = startTextObj.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(Screen.width + rect.rect.width, rect.anchoredPosition.y);
        Vector2 centerPos = new Vector2(Screen.width / 2f, rect.anchoredPosition.y);
        Vector2 endPos = new Vector2(-rect.rect.width, rect.anchoredPosition.y);
        rect.anchoredPosition = startPos;
        startText.alpha = 0f;
        // Fade in + di chuyển vào giữa
        float t = 0f;
        while (t < startTextFadeDuration)
        {
            rect.anchoredPosition = Vector2.Lerp(startPos, centerPos, t / startTextFadeDuration);
            startText.alpha = Mathf.Lerp(0f, 1f, t / startTextFadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = centerPos;
        startText.alpha = 1f;
        // Giữ ở giữa một lúc
        yield return new WaitForSeconds(startTextDisplayTime);
        // Fade out + di chuyển sang trái
        t = 0f;
        while (t < startTextFadeDuration)
        {
            rect.anchoredPosition = Vector2.Lerp(centerPos, endPos, t / startTextFadeDuration);
            startText.alpha = Mathf.Lerp(1f, 0f, t / startTextFadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = endPos;
        startText.alpha = 0f;
        startTextObj.SetActive(false);
    }
}