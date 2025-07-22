using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryMatchGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    private int easyLevelPairs = 4;
    private int mediumLevelPair = 5;
    private int hardLevelPairs = 6;

    [Header("UI Elements")]
    public Image timerBar;
    public GameObject winPanel, loosePanel, nextBtnPopup, clainBtn;
    public GameObject levelSelectionPanel;

    public TextMeshProUGUI gamePlayCurrentScore;
    public TextMeshProUGUI gamePlayBestScore;
    public TextMeshProUGUI totalCollectCoin;
    public TextMeshProUGUI winpanelTextScore;

    [Header("Game Objects")]
    public GameObject cardPrefab;
    //public Camera gameCame;

    [Header("Card Sprites")]
    public List<Sprite> cardSprites;
    public List<Sprite> cardCover;

    [Header("Spawn List")]
    public List<Transform> easyLevelPositions;
    public List<Transform> midLevelPosition;
    public List<Transform> hardLevelPositions;
    public static Action SetColliderMessage;

    public Transform winPanalCenterPos, winPanalDownPos;
    public GameObject particaleEffect;
    public List<Transform> popUpStarts;
    public GameObject lockImage;

    private List<CardController> activeCards = new List<CardController>();
    private CardController firstSelectedCard;
    private CardController secondSelectedCard;
    //private bool canSelect = true;
    [SerializeField] private float gameDuration = 60f;
    private float timer;
    private int selectedGame;
    private bool hasGameEnded = true, handTuterial;
    private int currentScore = 0;
    private int totalCoinInGame = 0;
    private int coinForThisLevel = 0;

    [SerializeField] private ExtraVeribals audioFile;

    private void Start()
    {
       
        levelSelectionPanel.SetActive(true);
        winPanel.SetActive(false);
        loosePanel.SetActive(false);
        timerBar.gameObject.SetActive(false);

        totalCollectCoin.text = "0"; // Replace with actual saved value if needed
    }

    private void Update()
    {
        UpdateTimer();
    }

    public void SetupAndStartGame(int numberOfPairs)
    {
        SetGameDuration(numberOfPairs);
        GameStartFunction(numberOfPairs);
        Invoke(nameof(DelayHandShow), 1.5f);
    }

    private void DelayHandShow()
    {
        if (!handTuterial)
        {
           
            handTuterial = true;
        }
    }

    public void GameStartFunction(int numberOfPairs)
    {
        selectedGame = numberOfPairs;
        hasGameEnded = false;
        levelSelectionPanel.SetActive(false);
        winPanel.SetActive(false);
        //clainBtn.SetActive(true);
        nextBtnPopup.SetActive(false);

        InitializeGame(selectedGame);
    }

    private void SetGameDuration(int numberOfPairs)
    {
        if (numberOfPairs == easyLevelPairs)
            gameDuration = 60;
        else if (numberOfPairs == mediumLevelPair)
            gameDuration = 90;
        else if (numberOfPairs == hardLevelPairs)
            gameDuration = 120f;
    }

    private void InitializeGame(int numberOfPairs)
    {
        SuffelSpriteList(cardCover);
        ClearBoard();
        timerBar.gameObject.SetActive(true);

        if (numberOfPairs > cardSprites.Count)
            return;

        List<CardController> cardList = GenerateCardPairs(numberOfPairs);
        ArrangeCardsByLevel(cardList, numberOfPairs);
        ShuffleCards(cardList);
        activeCards.AddRange(cardList);
        timer = gameDuration;
    }

    private List<CardController> GenerateCardPairs(int numberOfPairs)
    {
        List<CardController> cardList = new List<CardController>();

        for (int i = 0; i < numberOfPairs; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CardController card = Instantiate(cardPrefab).GetComponent<CardController>();
                card.InitializeCard(i, cardSprites[i], this, cardCover[0]);
                cardList.Add(card);
            }
        }

        return cardList;
    }

    private void ArrangeCardsByLevel(List<CardController> cardList, int numberOfPairs)
    {
        List<Transform> positions = numberOfPairs == easyLevelPairs ? easyLevelPositions :
                                     numberOfPairs == mediumLevelPair ? midLevelPosition :
                                     hardLevelPositions;

        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.position = positions[i].position;
            cardList[i].transform.SetParent(positions[i]);
        }
    }

    private void ShuffleCards(List<CardController> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, cards.Count);
            var temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    private void SuffelSpriteList(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            int rand = UnityEngine.Random.Range(0, sprites.Count);
            Sprite temp = sprites[i];
            sprites[i] = sprites[rand];
            sprites[rand] = temp;
        }
    }
    public void OnCardSelected(CardController card)
    {
        if (secondSelectedCard != null) return;
        card.FlipCard();

        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
        }
        else
        {
            secondSelectedCard = card;
            //canSelect = false;
            StartCoroutine(ValidateMatch(firstSelectedCard, secondSelectedCard));
        }
    }

    private IEnumerator ValidateMatch(CardController card1, CardController card2)
    {
        yield return new WaitForSeconds(0.5f);

        if (card1.CardID == card2.CardID)
        {
            card1.ResolveCard();
            card2.ResolveCard();
            currentScore += 1;
        }
        else
        {
            card1.FlipBack();
            card2.FlipBack();
        }

        StartCoroutine(CardsColliderController());
    }

    public IEnumerator CardsColliderController()
    {
        SetColliderMessage?.Invoke();
        yield return new WaitForSeconds(0.5f);

        firstSelectedCard = null;
        secondSelectedCard = null;
        //canSelect = true;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        foreach (var card in activeCards)
        {
            if (!card.IsResolved) return;
        }

        DisplayEndGameMessage(true);
    }

    private void DisplayEndGameMessage(bool isWin)
    {
        //canSelect = false;
        hasGameEnded = true;
        timerBar.gameObject.SetActive(false);
        timer = 0;

        if (isWin)
        {
            WinFunction();
        }
        else
        {
            loosePanel.SetActive(true);
        }

        EndGame();
    }

    private void EndGame()
    {
        SuffelSpriteList(cardSprites);
        SuffelSpriteList(cardCover);
        ClearBoard();
        //canSelect = false;
        ShowLevelSelectionMenu();
    }

    private void ClearBoard()
    {
        foreach (var card in activeCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        activeCards.Clear();
    }

    private void UpdateTimer()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            timerBar.fillAmount = timer / gameDuration;
        }
        else if (!hasGameEnded)
        {
            DisplayEndGameMessage(false);
        }
    }

    private void ShowLevelSelectionMenu()
    {
        winPanel.SetActive(true);
    }

    public void WinFunction()
    {
        particaleEffect.SetActive(true);
        coinForThisLevel = GetCoin(gameDuration, timer);
        winpanelTextScore.text = coinForThisLevel + " Coins";
        winPanel.SetActive(true);
        StartCoroutine(PlayPopSound());
    }

    public void LoseFunction()
    {
        coinForThisLevel = 0;
    }

    IEnumerator PlayPopSound()
    {
        yield return new WaitForSeconds(2);
    }

    public void NextBtnFunction()
    {
        GameStartFunction(selectedGame);
    }

    public void PressOnClaimBtn()
    {
        totalCoinInGame += coinForThisLevel;
        StartCoroutine(CoinAnimation());
    }

    IEnumerator CoinAnimation()
    {
        //clainBtn.SetActive(false);
        int coinToAnimate = coinForThisLevel;

        while (coinToAnimate > 0)
        {
            yield return new WaitForSeconds(0.1f);
            coinToAnimate--;
        }

        coinForThisLevel = 0;
        totalCoinInGame = 0;
        nextBtnPopup.SetActive(true);
    }

    int GetCoin(float maxValue, float currentValue)
    {
        if (currentValue <= 0) return 0;

        float percentage = (currentValue / maxValue) * 100f;

        if (percentage >= 75) return 15;
        else if (percentage >= 50) return 10;
        else if (percentage >= 25) return 5;
        else return 2;
    }
}

[System.Serializable]
public class ExtraVeribals
{
    public AudioClip bgmusic, bellAudio, winAudio, seleteLevel, unmatch, countDown, tap, coinCollect;
    public AudioClip pop1Sound, pop2Sound;
}
