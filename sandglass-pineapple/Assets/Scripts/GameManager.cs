using UnityEngine;
using System;                           
using UnityEngine.SceneManagement;      

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  
        LoadProgress();
    }

   
    [Header("Board Settings")]
    public int rows = 2;
    public int columns = 2;

    [Header("Score")]
    public int score = 0;

   
    public Action<int> OnScoreChanged;        
    public Action OnGameOver;

   
    public void AddScore(int value)
    {
        score += value;
        OnScoreChanged?.Invoke(score);
        SaveProgress();
    }

    public void EndGame()
    {
        OnGameOver?.Invoke();
        SaveProgress();                     
    }

    public void ResetGame()
    {
        score = 0;
        SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

   
    const string ScoreKey = "GM_SCORE";
    const string RowsKey = "GM_ROWS";
    const string ColsKey = "GM_COLS";

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(ScoreKey, score);
        PlayerPrefs.SetInt(RowsKey, rows);
        PlayerPrefs.SetInt(ColsKey, columns);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(ScoreKey)) return;  

        score = PlayerPrefs.GetInt(ScoreKey);
        rows = PlayerPrefs.GetInt(RowsKey);
        columns = PlayerPrefs.GetInt(ColsKey);
    }
}
