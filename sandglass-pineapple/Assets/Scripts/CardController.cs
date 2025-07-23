using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardController : MonoBehaviour, IPointerClickHandler
{
    public int CardID { get; private set; }
    public Image cardSpriteRenderer;

    public Image cardCoverSprit;
    private MemoryMatchGameManager gameManager;
    private bool isFlipped = false;
    private bool isResolved = false;
    private CanvasGroup canvasGroup;

    [SerializeField] private AudioClip CardTap, matched;
    public bool IsResolved => isResolved;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
       // cardCoverSprit = GetComponent<Image>();
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
        FlipCard();
        Invoke(nameof(FlipBack), 1f);
    }

    public void InitializeCard(int id, Sprite sprite, MemoryMatchGameManager manager, Sprite ownSprit)
    {
        CardID = id;
        cardSpriteRenderer.sprite = sprite;
        gameManager = manager;
        cardCoverSprit.sprite = ownSprit;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFlipped && !isResolved)
        {
            gameManager.OnCardSelected(this);
        }
    }

    public void FlipCard()
    {
        isFlipped = true;
        StartCoroutine(FlipToAngle(180f, 0.5f));
    }

    public void FlipBack()
    {
        isFlipped = false;
        StartCoroutine(FlipToAngle(0f, 0.5f, () => canvasGroup.blocksRaycasts = true));
    }

    public void ResolveCard()
    {
        isResolved = true;
        StartCoroutine(BounceThenShrink());
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

    private IEnumerator FlipToAngle(float targetYAngle, float duration, Action onComplete = null)
    {
        float elapsed = 0f;
        Vector3 startRotation = transform.eulerAngles;
        Vector3 endRotation = new Vector3(startRotation.x, targetYAngle, startRotation.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float yRotation = Mathf.LerpAngle(startRotation.y, endRotation.y, t);
            transform.eulerAngles = new Vector3(startRotation.x, yRotation, startRotation.z);
            yield return null;
        }

        transform.eulerAngles = endRotation;
        onComplete?.Invoke();
    }

    private IEnumerator BounceThenShrink()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bounceScale = originalScale * 1.1f;

        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, bounceScale, t / duration);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        duration = 0.2f;
        Vector3 zeroScale = Vector3.zero;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(bounceScale, zeroScale, t / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    public string GetCardSpriteName()
    {
        return cardCoverSprit.name; // frontSprite = your visible card image
    }

    public void SetCardState(bool flipped, bool matched)
    {
        if (matched)
            ResolveCard();
        else if (flipped)
            FlipCard();
    }

}
