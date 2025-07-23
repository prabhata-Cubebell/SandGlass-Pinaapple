using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveandLoad 
{  
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

