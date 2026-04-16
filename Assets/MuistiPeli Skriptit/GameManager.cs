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
    [SerializeField] private GameObject puzzleBtnPrefab; // PuzzleBtn
    [SerializeField] private Transform boardParent;      // GridLayoutGroup parent
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;
    [SerializeField] private TimeScript timeScript;

    [Header("Difficulty Timers (seconds)")]
    [SerializeField] private float easySeconds = 10f;
    [SerializeField] private float mediumSeconds = 60f;
    [SerializeField] private float hardSeconds = 120f;
    [SerializeField]
    private Sprite bgImage;

    [Header("Reveal Timing")]
    [SerializeField] private float firstCardRevealSeconds = 1.5f;

    public Sprite[] puzzles;

    public List<Sprite> gamePuzzles = new List<Sprite>();

    public List<Button> btns = new List<Button>();

    public bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    private Coroutine firstRevealRoutine;

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
        if (timeScript == null)
        {
            timeScript = FindAnyObjectByType<TimeScript>();
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
        /* firstGuess = false;
         secondGuess = false;

         CreateBoard(rows, cols);     // <-- uusi
         //GetButtons();
         AddListeners();
         AddGamePuzzles();
         Shuffled(gamePuzzles);
         gameGuesses = gamePuzzles.Count / 2;
         */
    }
    // Called by UI buttons
  public void StartEasy()
{
    StartGame(2, 2);
    timeScript?.StartTimer(easySeconds);
}

public void StartMedium()
{
    StartGame(4, 4);
    timeScript?.StartTimer(mediumSeconds);
}

public void StartHard()
{
    StartGame(4, 4);
    timeScript?.StartTimer(hardSeconds);
}

    private void StartGame(int r, int c)
    {
        rows = r;
        cols = c;

        // Reset state
        firstGuess = secondGuess = false;
        countCorrectGuesses = 0;
        UudestaanNappi1.SetActive(false);

        // Rebuild board
        CreateBoard(rows, cols);

        // IMPORTANT: you must have enough puzzle pairs for this board size
        if (!TryAddGamePuzzles())
        {
            Debug.LogError("Not enough unique puzzle types for this difficulty.");
            return;
        }

        Shuffled(gamePuzzles);
        AddListeners();

        gameGuesses = btns.Count / 2; // use button count, not gamePuzzles.Count
    }

    private bool TryAddGamePuzzles()
    {
        gamePuzzles.Clear();
        int neededPairs = btns.Count / 2;

        var uniqueGroups = puzzles
            .Where(p => p != null)
            .GroupBy(p => GetPuzzleId(p))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToList();

        if (uniqueGroups.Count < neededPairs)
            return false;

        // shuffle groups
        for (int i = 0; i < uniqueGroups.Count; i++)
        {
            int r = Random.Range(i, uniqueGroups.Count);
            (uniqueGroups[i], uniqueGroups[r]) = (uniqueGroups[r], uniqueGroups[i]);
        }

        for (int i = 0; i < neededPairs; i++)
        {
            var chosen = uniqueGroups[i].First();
            gamePuzzles.Add(chosen);
            gamePuzzles.Add(chosen);
        }
        return true;
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
    void CreateBoard(int r, int c)
    {
        for (int i = boardParent.childCount - 1; i >= 0; i--)
            Destroy(boardParent.GetChild(i).gameObject);
        btns.Clear();

        int total = r * c;
        if (total % 2 != 0)
        {
            Debug.LogError("Board size must be even (pairs).");
            return;
        }

        // (valinnainen) tyhjennä vanhat napit jos vaihdat kokoa lennosta
        // foreach (Transform child in boardParent) Destroy(child.gameObject);

        for (int i = 0; i < total; i++)
        {
            var go = Instantiate(puzzleBtnPrefab, boardParent);
            var btn = go.GetComponent<Button>();
            btn.image.sprite = bgImage;
            btns.Add(btn);
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

            if (firstRevealRoutine != null)
            {
                StopCoroutine(firstRevealRoutine);
            }
            firstRevealRoutine = StartCoroutine(AutoHideFirstGuess(firstGuessIndex));
            return;
        }

        if (index == firstGuessIndex) return;

        secondGuess = true;
        secondGuessIndex = index;
        btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];

        if (firstRevealRoutine != null)
        {
            StopCoroutine(firstRevealRoutine);
            firstRevealRoutine = null;
        }

        bool isMatch = GetPuzzleId(gamePuzzles[firstGuessIndex]) == GetPuzzleId(gamePuzzles[secondGuessIndex]);
        Debug.Log($"First guess: {gamePuzzles[firstGuessIndex].name}, Second guess: {gamePuzzles[secondGuessIndex].name}, " +
                   (isMatch ? "puzzles match" : "puzzles don't match"));

        StartCoroutine(CheckThePuzzleMatch());
    }

    private IEnumerator AutoHideFirstGuess(int index)
    {
        float wait = Mathf.Max(0f, firstCardRevealSeconds);
        yield return new WaitForSeconds(wait);

        // If player hasn't chosen the second card in time, hide the first one.
        if (firstGuess && !secondGuess && firstGuessIndex == index)
        {
            btns[index].image.sprite = bgImage;
            firstGuess = false;
        }

        firstRevealRoutine = null;
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
        if (countCorrectGuesses == gameGuesses)
        {
            Debug.Log("Game Finished");
            UudestaanNappi1.SetActive(true);

            //if (scoreScript != null)
            //{
        
            Debug.Log("Final score: " + scoreScript.GetScore() + ", pairs found: " + scoreScript.GetPairsFound());
            scoreScript.OnGameFinished();
            timeScript?.StopTimer();
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