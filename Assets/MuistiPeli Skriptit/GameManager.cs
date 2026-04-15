using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour

{
    [SerializeField] 
    private Sprite bgImage;

    public Sprite[] puzzles;

    public List<Sprite> gamePuzzles = new List<Sprite>();

    public List<Button> btns = new List<Button>();

    public bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    public GameObject UudestaanNappi1;
    [SerializeField] private MuistipeliScoreScript scoreScript;
    [Header("Scene Navigation")]
    [SerializeField] private string Päävalikko = "MainMenu";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

private void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("Kuvat/Esineet");

        if (scoreScript == null)
        {
            scoreScript = GetComponent<MuistipeliScoreScript>();
        }

        if (scoreScript == null)
        {
            scoreScript = FindAnyObjectByType<MuistipeliScoreScript>();
        }

        if (scoreScript == null)
        {
            Debug.LogError("GameManager: No MuistipeliScoreScript found. Score will not increase.");
        }
    }

    public void RestartGame()
    {
        // If you ever pause the game with Time.timeScale, this makes sure it resumes.
        Time.timeScale = 1f;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    void Start()
    {
        firstGuess = false;
        secondGuess = false;

        GetButtons();
        AddListeners();
        AddGamePuzzles();
        Shuffled(gamePuzzles);
        gameGuesses = gamePuzzles.Count / 2;
    }

   void GetButtons()
    {
    GameObject[] objects = GameObject.FindGameObjectsWithTag("puzzlebtn");
    for (int i = 0; i < objects.Length; i++)
        {
        btns.Add(objects[i].GetComponent<Button>());
        btns[i].image.sprite = bgImage;
        }
    }

    void AddGamePuzzles()
    {
        gamePuzzles.Clear();

        int neededPairs = btns.Count / 2;

        // Group by normalized ID so "Sahtikulho 3" and "Sahtikulho 4" count as one puzzle type
        var uniqueGroups = puzzles
            .Where(p => p != null)
            .GroupBy(p => GetPuzzleId(p))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToList();

        if (uniqueGroups.Count < neededPairs)
        {
            Debug.LogWarning($"Not enough unique puzzle types. Needed {neededPairs}, found {uniqueGroups.Count}.");
            neededPairs = uniqueGroups.Count;
        }

        // Shuffle groups, then take only needed unique pairs
        for (int i = 0; i < uniqueGroups.Count; i++)
        {
            int r = Random.Range(i, uniqueGroups.Count);
            var temp = uniqueGroups[i];
            uniqueGroups[i] = uniqueGroups[r];
            uniqueGroups[r] = temp;
        }

        for (int i = 0; i < neededPairs; i++)
        {
            Sprite chosen = uniqueGroups[i].First(); // one representative sprite per unique ID
            gamePuzzles.Add(chosen);
            gamePuzzles.Add(chosen);
        }
    }

    void AddListeners()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            int index = i; // capture correct index
            btns[i].onClick.RemoveAllListeners(); // prevent duplicates
            btns[i].onClick.AddListener(() => PickPuzzle(index));
        }
    }

    private string GetPuzzleId(Sprite s)
    {
        if (s == null) return string.Empty;
        // "Sahtikulho 4" -> "Sahtikulho"
        return Regex.Replace(s.name, @"\s+\d+$", "").Trim().ToLowerInvariant();
    }

   public void PickPuzzle(int index)
   {
       if (secondGuess) return;

       if (!firstGuess)
       {
           firstGuess = true;
           firstGuessIndex = index;
           btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
           return;
       }

       if (index == firstGuessIndex) return;

       secondGuess = true;
       secondGuessIndex = index;
       btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];

       bool isMatch = GetPuzzleId(gamePuzzles[firstGuessIndex]) == GetPuzzleId(gamePuzzles[secondGuessIndex]);
       Debug.Log($"First guess: {gamePuzzles[firstGuessIndex].name}, Second guess: {gamePuzzles[secondGuessIndex].name}, " +
                  (isMatch ? "puzzles match" : "puzzles don't match"));

       StartCoroutine(CheckThePuzzleMatch());
   }
   IEnumerator CheckThePuzzleMatch()
   {
      yield return new WaitForSeconds(0.5f);   
      bool isMatch = GetPuzzleId(gamePuzzles[firstGuessIndex]) == GetPuzzleId(gamePuzzles[secondGuessIndex]);

      if (isMatch)
      {

        btns[firstGuessIndex].interactable = false;
        btns[secondGuessIndex].interactable = false;
        btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
        btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
                if (scoreScript != null)
                {
                        scoreScript.OnPairMatched();
                }
        CheckTheGameFinished();
      }
      else
        {
            btns[firstGuessIndex].image.sprite = bgImage;
            btns[secondGuessIndex].image.sprite = bgImage;
        }
        yield return new WaitForSeconds(0.5f);

        firstGuess = secondGuess = false;   
   }

   void CheckTheGameFinished()
   {
      countCorrectGuesses++;
        if(countCorrectGuesses == gameGuesses)
        {
            Debug.Log("Game Finished");
            UudestaanNappi1.SetActive(true);

            //if (scoreScript != null)
            //{
                Debug.Log("Final score: " + scoreScript.GetScore() + ", pairs found: " + scoreScript.GetPairsFound());
                scoreScript.OnGameFinished();
            //}
        }
   }


   public void PoistuNappi()
    {
        SceneManager.LoadScene(Päävalikko);
    }

public void SeuraavaNappi()
    {
        Debug.Log("Seuraava ");
    }

   void Shuffled(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
           Sprite temp = list[i];
           int randomIndex = Random.Range(i, list.Count); 
              list[i] = list[randomIndex];
              list[randomIndex] = temp;
        }
    }
}