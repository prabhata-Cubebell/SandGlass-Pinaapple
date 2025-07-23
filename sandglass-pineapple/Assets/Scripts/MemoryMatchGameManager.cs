using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryMatchGameManager : MonoBehaviour
{
    public static MemoryMatchGameManager Instance;

    [Header("Game Settings")]
    public int rows = 2;
    public int columns = 2;

    [Header("UI Elements")]
    public Image timerBar;
    public GameObject winPanel, loosePanel, nextBtnPopup;
    public GameObject levelSelectionPanel, gameplayPanal;
    public TextMeshProUGUI gamePlayCurrentScore;
    public TextMeshProUGUI gamePlayBestScore;
    public TextMeshProUGUI winpanalBestScore;
    public TextMeshProUGUI winPanelCurrentScore;

    [Header("Game Objects")]
    public GameObject cardPrefab;
    public Transform gridParent;
    public GridLayoutGroup gridLayoutGroup;

    [Header("Card Sprites")]
    public List<Sprite> cardSprites;
    public List<Sprite> cardCover;

    public static Action SetColliderMessage;

    public Transform winPanalCenterPos, winPanalDownPos;
    public GameObject particaleEffect;
    public List<Transform> popUpStarts;

    private List<CardController> activeCards = new List<CardController>();
    private CardController firstSelectedCard;
    private CardController secondSelectedCard;

    [SerializeField] private float gameDuration = 60f;
    private float timer;
    private bool hasGameEnded = true, handTuterial;
    private int currentScore = 0;

    [SerializeField] private ExtraVeribals audioFile;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        levelSelectionPanel.SetActive(true);
        gameplayPanal.SetActive(false);
        winPanel.SetActive(false);
        loosePanel.SetActive(false);
        timerBar.gameObject.SetActive(false);

        gamePlayBestScore.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
        winpanalBestScore.text = gamePlayBestScore.text;

        if (audioFile.bgmusic)
        {
            audioSource.clip = audioFile.bgmusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        UpdateTimer();
    }

    public void SetupAndStartGame(/*int _rows, int _columns*/)
    {
        currentScore = 0;
        PlayAudio(audioFile.seleteLevel);
        /*rows = _rows;
        columns = _columns;*/
        SetGameDuration(rows * columns / 2);
        GameStartFunction();
    }

    private void SetGameDuration(int numberOfPairs)
    {
        gameDuration = numberOfPairs * 15f;
    }

    public void GameStartFunction()
    {
        hasGameEnded = false;
        gamePlayCurrentScore.text = currentScore.ToString();
        levelSelectionPanel.SetActive(false);
        gameplayPanal.SetActive(true);
        winPanel.SetActive(false);
        loosePanel.SetActive(false);
        nextBtnPopup.SetActive(false);

        InitializeGame();
    }

    private void InitializeGame()
    {
        SuffelSpriteList(cardCover);
        ClearBoard();
        timerBar.gameObject.SetActive(true);

        int numberOfPairs = (rows * columns) / 2;
        if (numberOfPairs > cardSprites.Count) return;

        SetupBoardLayout();
        List<CardController> cardList = GenerateCardPairs(numberOfPairs);
        ShuffleCards(cardList);
        activeCards.AddRange(cardList);
        timer = gameDuration;
    }

    /// <summary>
    /// Calculates a pixel‑perfect, square cell size that honours the GridLayoutGroup’s
    /// padding & spacing, then applies it – without changing the parent RectTransform.
    /// </summary>
    private void SetupBoardLayout()
    {
        if (rows <= 0 || columns <= 0) return;                       // safety

        // ----- 1.  Cache references & shorthand -----
        RectTransform gridRT = gridParent.GetComponent<RectTransform>();
        GridLayoutGroup glg = gridLayoutGroup;
        float spacing = glg.spacing.x;                      // assumed uniform
        Rect rect = gridRT.rect;

        // ----- 2.  Calculate the “usable” width & height inside the padding -----
        float usableWidth = rect.width - glg.padding.left - glg.padding.right;
        float usableHeight = rect.height - glg.padding.top - glg.padding.bottom;

        // ----- 3.  Subtract total spacing that will sit BETWEEN cells -----
        usableWidth -= spacing * (columns - 1);
        usableHeight -= spacing * (rows - 1);

        // ----- 4.  Derive the maximum square cell size that fits in both axes -----
        float maxCellW = usableWidth / columns;
        float maxCellH = usableHeight / rows;
        int cellSize = Mathf.FloorToInt(Mathf.Min(maxCellW, maxCellH)); // round for crisp pixels

        // ----- 5.  Push values into GridLayoutGroup -----
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = columns;
        glg.cellSize = new Vector2(cellSize, cellSize);

        // OPTIONAL polish: centre leftover slack evenly (nice when cellSize < maxCell on one axis)
        Vector2 newPadding = new Vector2(
            rect.width - (cellSize * columns + spacing * (columns - 1)),
            rect.height - (cellSize * rows + spacing * (rows - 1))
        );

        // keep original padding ratios while centering
        float padLeft = glg.padding.left + newPadding.x * 0.5f;
        float padRight = glg.padding.right + newPadding.x * 0.5f;
        float padTop = glg.padding.top + newPadding.y * 0.5f;
        float padBottom = glg.padding.bottom + newPadding.y * 0.5f;
        glg.padding = new RectOffset(
            Mathf.RoundToInt(padLeft),
            Mathf.RoundToInt(padRight),
            Mathf.RoundToInt(padTop),
            Mathf.RoundToInt(padBottom)
        );
    }



    private List<CardController> GenerateCardPairs(int numberOfPairs)
    {
        List<CardController> cardList = new List<CardController>();
        for (int i = 0; i < numberOfPairs; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CardController card = Instantiate(cardPrefab, gridParent).GetComponent<CardController>();
                card.InitializeCard(i, cardSprites[i], this, cardCover[0]);
                cardList.Add(card);
            }
        }
        return cardList;
    }

    private void ShuffleCards(List<CardController> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int r = UnityEngine.Random.Range(0, cards.Count);
            CardController tmp = cards[i];
            cards[i] = cards[r];
            cards[r] = tmp;
        }
    }

    private void SuffelSpriteList(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            int r = UnityEngine.Random.Range(0, sprites.Count);
            Sprite tmp = sprites[i];
            sprites[i] = sprites[r];
            sprites[r] = tmp;
        }
    }

    public void OnCardSelected(CardController card)
    {
        if (secondSelectedCard != null) return;
        card.FlipCard();
        PlayAudio(audioFile.tap);
        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
        }
        else
        {
            secondSelectedCard = card;
            StartCoroutine(ValidateMatch(firstSelectedCard, secondSelectedCard));
        }
    }

    private IEnumerator ValidateMatch(CardController card1, CardController card2)
    {
        yield return new WaitForSeconds(0.4f);
        if (card1.CardID == card2.CardID)
        {
            card1.ResolveCard();
            card2.ResolveCard();
            PlayAudio(audioFile.pop1Sound);
            currentScore += 1;
            gamePlayCurrentScore.text = currentScore.ToString();
            if (currentScore > PlayerPrefs.GetInt("BestScore", 0))
            {
                PlayerPrefs.SetInt("BestScore", currentScore);
                gamePlayBestScore.text = currentScore.ToString();
            }
        }
        else
        {
            PlayAudio(audioFile.unmatch);
            card1.FlipBack();
            card2.FlipBack();
        }
        StartCoroutine(CardsColliderController());
    }

    public IEnumerator CardsColliderController()
    {
        SetColliderMessage?.Invoke();
        yield return new WaitForSeconds(0.4f);
        firstSelectedCard = null;
        secondSelectedCard = null;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        foreach (CardController c in activeCards)
        {
            if (!c.IsResolved) return;
        }
        DisplayEndGameMessage(true);
    }

    private void DisplayEndGameMessage(bool isWin)
    {
        hasGameEnded = true;
        timerBar.gameObject.SetActive(false);
        timer = 0f;
        if (isWin)
        {
            WinFunction();
        }
        else
        {
            loosePanel.SetActive(true);
            PlayAudio(audioFile.bellAudio);
        }
        EndGame();
    }

    private void EndGame()
    {
        SuffelSpriteList(cardSprites);
        SuffelSpriteList(cardCover);
        ClearBoard();
    }

    private void ClearBoard()
    {
        foreach (CardController c in activeCards)
        {
            if (c != null) Destroy(c.gameObject);
        }
        activeCards.Clear();
    }

    private void UpdateTimer()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerBar.fillAmount = timer / gameDuration;
           // if (timer <= 10f && !audioSource.isPlaying) PlayAudio(audioFile.countDown);
        }
        else if (!hasGameEnded)
        {
            DisplayEndGameMessage(false);
        }
    }

    public void WinFunction()
    {
        StartCoroutine(MoveWinPanel());
        particaleEffect.SetActive(true);
        PlayAudio(audioFile.winAudio);
        winpanalBestScore.text = "Best Score : " + gamePlayBestScore.text;
        winPanelCurrentScore.text = "Current Score : " + currentScore;
        winPanel.SetActive(true);
        gameplayPanal.SetActive(false);
        foreach (Transform t in popUpStarts) Instantiate(particaleEffect, t.position, Quaternion.identity);
    }

    private IEnumerator MoveWinPanel()
    {
        winPanel.transform.position = winPanalDownPos.position;
        float t = 0f;
        Vector3 start = winPanalDownPos.position;
        Vector3 end = winPanalCenterPos.position;
        while (t < 1f)
        {
            t += Time.deltaTime;
            winPanel.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        nextBtnPopup.SetActive(true);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void NextBtnFunction()
    {
        PlayAudio(audioFile.pop2Sound);
        GameStartFunction();
    }

    public void WinPanalToEntryLevel()
    {
        winPanel.SetActive(false);
        gameplayPanal.SetActive(false);
        levelSelectionPanel.SetActive(true);
    }

    public void GamePlayToLevelSelection()
    {
        gameplayPanal.SetActive(false);
        levelSelectionPanel.SetActive(true);
        ClearBoard();
        hasGameEnded = true;
        timer = 0f;
    }
}

[System.Serializable]
public class ExtraVeribals
{
    public AudioClip bgmusic, bellAudio, winAudio, seleteLevel, unmatch, countDown, tap;
    public AudioClip pop1Sound, pop2Sound;
}
