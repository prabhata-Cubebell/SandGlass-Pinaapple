using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] GameObject levelSelect, gameplay, winPanel, losePanel, nextBtnPopup;
    public GameObject savePopup;

    [Header("Score UI")]
    [SerializeField] TextMeshProUGUI txtScore, txtBest, txtWinScore, txtWinBest;

    [Header("Timer")]
    [SerializeField] Image timerBar;

    [Header("Audio")]
    [SerializeField] ExtraVeribals clips;
    AudioSource source;

    void Awake()
    {
        if (Instance == null) Instance = this;
        source = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        MemoryMatchGameManager.OnScoreChanged += UpdateScore;
        MemoryMatchGameManager.OnTimerChanged += UpdateTimer;
        MemoryMatchGameManager.OnGameEnded += EndGameUI;
    }

    void OnDisable()
    {
        MemoryMatchGameManager.OnScoreChanged -= UpdateScore;
        MemoryMatchGameManager.OnTimerChanged -= UpdateTimer;
        MemoryMatchGameManager.OnGameEnded -= EndGameUI;
    }

    /* ---------- public buttons ---------- */

    public void Btn_StartLevel()
    {
                                  // square boards for now
        gameplay.SetActive(true);
        levelSelect.SetActive(false);
        MemoryMatchGameManager.Instance.StartLevel(MemoryMatchGameManager.Instance.rows, MemoryMatchGameManager.Instance.columns);
        Play(clips.seleteLevel);
    }

    public void Btn_SaveGame() => MemoryMatchGameManager.Instance.SaveGame();
    public void Btn_LoadGame() 
    {
        gameplay.SetActive(true); 
        levelSelect.SetActive(false);
        savePopup.SetActive(false);
        MemoryMatchGameManager.Instance.LoadGame();
    }

    public void Btn_NextRound() 
    {
        gameplay.SetActive(true);
        winPanel.SetActive(false);
        MemoryMatchGameManager.Instance.StartLevel(MemoryMatchGameManager.Instance.rows, MemoryMatchGameManager.Instance.columns);
    }

    public void Btn_ExitGame()
    {
        Application.Quit();
        MemoryMatchGameManager.Instance.SaveGame();
    }

    public void OnApplicationQuit()
    {
        MemoryMatchGameManager.Instance.SaveGame();
    }

    public void Btn_DeleteSaveData()
    {
        savePopup.SetActive(false);
        PlayerPrefs.DeleteKey("SavedGame");
        PlayerPrefs.Save();
    }

    public void Btn_BackToMenu()
    {
        levelSelect.SetActive(true);
        gameplay.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        MemoryMatchGameManager.Instance.SaveGame(); // Save current state
    }

    /* ---------- event receivers ---------- */

    void UpdateScore(int score)
    {
        txtScore.text = score.ToString();
        int best = Mathf.Max(score, PlayerPrefs.GetInt("BestScore", 0));
        PlayerPrefs.SetInt("BestScore", best);
        txtBest.text = best.ToString();
        txtWinBest.text = $"Best: {best}";
        txtWinScore.text = $"Score: {score}";
    }

    void UpdateTimer(float fill) => timerBar.fillAmount = fill;

    void EndGameUI(bool win)
    {
        gameplay.SetActive(false);
        if (win)
        {
            winPanel.SetActive(true);
            Play(clips.winAudio);
            nextBtnPopup.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
            Play(clips.bellAudio);
        }
    }

    /* ---------- utils ---------- */
    void Play(AudioClip c) { if (c) source.PlayOneShot(c); }
}


[System.Serializable]
public class ExtraVeribals
{
    public AudioClip bgmusic;
    public AudioClip bellAudio;
    public AudioClip winAudio;
    public AudioClip seleteLevel;
    public AudioClip unmatch;
    public AudioClip countDown;
    public AudioClip tap;
    public AudioClip pop1Sound;
    public AudioClip pop2Sound;
}
