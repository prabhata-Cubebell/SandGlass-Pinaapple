using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryMatchGameManager : MonoBehaviour
{
    public static MemoryMatchGameManager Instance { get; private set; }

    [Header("Grid Stuff")]
    public int rows = 2, columns = 2;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform gridParent;
    [SerializeField] GridLayoutGroup gridLayout;

    [Header("Sprites")]
    [SerializeField] List<Sprite> cardFronts;
    [SerializeField] List<Sprite> cardCovers;

    [Header("Timing")]
    [SerializeField] float secondsPerPair = 15f;
    float gameDuration;          // total time for this round
    float timer;                 // countdown
    bool hasGameEnded = true;

    /* ---------- public events for the UI layer ---------- */
    public static event Action<int> OnScoreChanged;
    public static event Action<float> OnTimerChanged;     // passes fillAmount 0‑1
    public static event Action<bool> OnGameEnded;        // true = win, false = lose
    public static Action SetColliderMessage;       // for card controller to disable collider temporarily

    /* ---------- runtime vars ---------- */
    readonly List<CardController> cards = new();
    CardController firstCard, secondCard;
    int currentScore = 0;

    /* ---------- life‑cycle ---------- */
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        Debug.unityLogger.logEnabled = false;
    }

    void Update()
    {
        if (hasGameEnded) return;
        timer -= Time.deltaTime;
        OnTimerChanged?.Invoke(timer / gameDuration);
        if (timer <= 0f) EndRound(false);
    }

    /* ===================  PUBLIC API  =================== */

    public void StartLevel(int r, int c)
    {
        rows = r; columns = c;
        currentScore = 0;
        gameDuration = (rows * columns / 2f) * secondsPerPair;
        BuildBoard();
        hasGameEnded = false;
        timer = gameDuration;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void SaveGame()
    {
        SaveData data = new()
        {
            rows = rows,
            columns = columns,
            currentScore = currentScore,
            timeLeft = timer,
            cards = new List<CardSaveData>()
        };
        foreach (var cc in cards)
        {
            data.cards.Add(new CardSaveData
            {
                cardID = cc.CardID,
                spriteName = cc.frontSprite.name,
                isMatched = cc.IsResolved
            });
        }
        PlayerPrefs.SetString("SavedGame", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SavedGame")) return;
        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("SavedGame"));
        rows = data.rows;
        columns = data.columns;
        currentScore = data.currentScore;
        gameDuration = (rows * columns / 2f) * secondsPerPair;
        timer = data.timeLeft;
        BuildBoard(data);                    // restore board state
        hasGameEnded = false;
        OnScoreChanged?.Invoke(currentScore);
    }

    /* ===================  GAMEPLAY  =================== */

    public void OnCardSelected(CardController card)
    {
        if (hasGameEnded || secondCard != null) return;

        card.FlipCard();

        if (firstCard == null) firstCard = card;
        else
        {
            secondCard = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.4f);

        if (firstCard.CardID == secondCard.CardID)
        {
            firstCard.ResolveCard();
            secondCard.ResolveCard();
            currentScore++;
            OnScoreChanged?.Invoke(currentScore);
            if (AllCardsMatched()) EndRound(true);
        }
        else
        {
            firstCard.FlipBack();
            secondCard.FlipBack();
        }

        firstCard = secondCard = null;
    }

    /* ===================  HELPERS  =================== */

    void BuildBoard(SaveData restoreData = null)
    {
        ClearBoard();
        GridUtility.FitSquareGrid(gridLayout, rows, columns);

        List<CardController> spawnedCards = new();

        if (restoreData == null)
        {
            spawnedCards = GenerateNewCards();
        }
        else
        {
            spawnedCards = RestoreSavedCards(restoreData);
        }

        OrderCardsInGrid(spawnedCards);
        cards.AddRange(spawnedCards);
    }

    void ClearBoard()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        cards.Clear();
    }

    List<CardController> GenerateNewCards()
    {
        int pairs = (rows * columns) / 2;
        List<CardController> cardList = new();

        for (int i = 0; i < pairs; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var card = SpawnCard(i, cardFronts[i]);
                cardList.Add(card);
            }
        }

        Shuffle(cardList);
        return cardList;
    }

    List<CardController> RestoreSavedCards(SaveData data)
    {
        List<CardController> restored = new();

        foreach (var c in data.cards)
        {
            Sprite front = cardFronts.Find(s => s.name == c.spriteName);
            var card = SpawnCard(c.cardID, front);
            card.SetCardState(false, c.isMatched);
            restored.Add(card);
        }

        return restored;
    }

    void OrderCardsInGrid(List<CardController> cardList)
    {
        for (int i = 0; i < cardList.Count; i++)
            cardList[i].transform.SetSiblingIndex(i);
    }

    CardController SpawnCard(int id, Sprite front)
    {
        var go = Instantiate(cardPrefab, gridParent);
        var card = go.GetComponent<CardController>();
        card.InitializeCard(id, front, this,cardCovers[0]);
        return card;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }


    bool AllCardsMatched()
    {
        foreach (var c in cards) if (!c.IsResolved) return false;
        return true;
    }

    void EndRound(bool win)
    {
        hasGameEnded = true;
        OnGameEnded?.Invoke(win);
        PlayerPrefs.DeleteKey("SavedGame");
    }

    /* ++ utility helpers ++ */
    static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
