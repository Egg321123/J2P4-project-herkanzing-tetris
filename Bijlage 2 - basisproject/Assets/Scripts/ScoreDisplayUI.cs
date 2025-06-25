using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayUI : MonoBehaviour
{
    public GameObject digitPrefab;       // Prefab with Image component
    public Sprite[] numberSprites;       // Sprites for digits 0-9

    private List<GameObject> digitObjects = new List<GameObject>();
    private int score = 0;
    public void Start()
    {
        DisplayScore();
    }
    public void AddScore(int amount)
    {
        score += amount;
        DisplayScore();
    }

    public void DisplayScore()
    {
        // Clear previous digits
        foreach (var digit in digitObjects)
        {
            Destroy(digit);
        }
            
        digitObjects.Clear();

        string scoreString = score.ToString();

        for (int i = 0; i < scoreString.Length; i++)
        {
            int digit = scoreString[i] - '0';

            GameObject digitGameObject = Instantiate(digitPrefab, transform);
            Image img = digitGameObject.GetComponent<Image>();
            if (img != null && digit >= 0 && digit < numberSprites.Length)
                img.sprite = numberSprites[digit];

            digitObjects.Add(digitGameObject);
        }
    }

    public void ResetScore()
    {
        score = 0;
        DisplayScore();
    }
}
