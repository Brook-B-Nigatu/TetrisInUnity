using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    private int score;
    private TextMeshProUGUI textComponent;
    public int Score
    {
        get { return score; }
        set { score = value; }
    }

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();    
    }


    void Start()
    {
        score = 0;
    }

    public void addScore(int toAdd)
    {
        score += toAdd;
        textComponent.text = "Score : " + score.ToString();
    }

}
