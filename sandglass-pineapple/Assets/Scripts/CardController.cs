using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IPointerClickHandler
{
    public int CardID { get; private set; }

    [Header("Card Visuals")]
    [SerializeField] private Image cardSpriteRenderer;
    [SerializeField] private AudioClip cardTap;
    [SerializeField] private AudioClip matchSound;

    [HideInInspector] public Sprite frontSprite;
    [HideInInspector] public Sprite backSprite;

    private bool isFlipped = false;
    private bool isResolved = false;

    public bool IsResolved => isResolved;

    private MemoryMatchGameManager gameManager;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        MemoryMatchGameManager.SetColliderMessage += DisableColliderTemporarily;
    }

    private void OnDestroy()
    {
        MemoryMatchGameManager.SetColliderMessage -= DisableColliderTemporarily;
    }

    private void Start()
    {
        canvasGroup.blocksRaycasts = false;
        FlipCard();                    // Show front on start
        Invoke(nameof(FlipBack), 1f);  // Then flip back after delay
    }

    public void InitializeCard(int id, Sprite front, MemoryMatchGameManager manager, Sprite back)
    {
        CardID = id;
        frontSprite = front;
        backSprite = back;
        gameManager = manager;

        cardSpriteRenderer.sprite = backSprite; // Start hidden
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isFlipped || isResolved) return;

        gameManager.OnCardSelected(this);
        // Optional: AudioSource.PlayClipAtPoint(cardTap, Camera.main.transform.position);
    }

    public void FlipCard()
    {
        isFlipped = true;
        StartCoroutine(FlipToAngle(showFront: true, duration: 0.5f));
    }

    public void FlipBack()
    {
        isFlipped = false;
        StartCoroutine(FlipToAngle(showFront: false, duration: 0.5f, () =>
        {
            canvasGroup.blocksRaycasts = true;
        }));
    }

    public void ResolveCard()
    {
        isResolved = true;
        StartCoroutine(BounceThenShrink());
        // Optional: AudioSource.PlayClipAtPoint(matchSound, Camera.main.transform.position);
    }

    public void DisableColliderTemporarily()
    {
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(EnableColliderAfterDelay(0.5f));
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isResolved)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    private IEnumerator FlipToAngle(bool showFront, float duration, Action onComplete = null)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // Rotate to 90°
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float yRotation = Mathf.LerpAngle(0f, 90f, elapsed / halfDuration);
            transform.eulerAngles = new Vector3(0f, yRotation, 0f);
            yield return null;
        }

        // Switch sprite at halfway
        cardSpriteRenderer.sprite = showFront ? frontSprite : backSprite;

        elapsed = 0f;

        // Finish rotation to 180° or reset to 0°
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float yRotation = Mathf.LerpAngle(90f, 180f, elapsed / halfDuration);
            transform.eulerAngles = new Vector3(0f, yRotation, 0f);
            yield return null;
        }

        transform.eulerAngles = new Vector3(0f, showFront ? 180f : 0f, 0f);
        onComplete?.Invoke();
    }

    private IEnumerator BounceThenShrink()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bounceScale = originalScale * 1.1f;
        Vector3 zeroScale = Vector3.zero;

        float t = 0f;
        float bounceDuration = 0.3f;

        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, bounceScale, t / bounceDuration);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        float shrinkDuration = 0.2f;

        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(bounceScale, zeroScale, t / shrinkDuration);
            yield return null;
        }

        // Optional: Destroy(gameObject);
    }

    public string GetCardSpriteName()
    {
        return frontSprite != null ? frontSprite.name : "None";
    }

    public void SetCardState(bool flipped, bool matched)
    {
        if (matched)
        {
            ResolveCard();
        }
        else if (flipped)
        {
            FlipCard();
        }
    }
}
