using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveandLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class SaveData
{
    public int rows;
    public int columns;
    public float timeLeft;
    public int currentScore;
    public List<CardSaveData> cards;
}

[Serializable]
public class CardSaveData
{
    public int cardID;
    public string spriteName;
    public bool isFlipped;
    public bool isMatched;
}

