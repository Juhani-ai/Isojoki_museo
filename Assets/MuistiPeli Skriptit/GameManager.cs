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
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip pickCardClip;
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
    private Sprite[] namePuzzles; // from EsineidenNimet


    public GameObject UudestaanNappi1;

    [Header("End-of-round buttons")]
    [SerializeField] private GameObject SeuraavaNappiObj;
    [SerializeField] private GameObject PaavalikkoNappiObj;
    [Header("Info panel")]
    [SerializeField] private GameObject OhjeetNappiObj;
    [SerializeField] private MuistipeliScoreScript scoreScript;
    [Header("Scene Navigation")]
    [SerializeField] private string Päävalikko = "Päävalikko";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("Kuvat/Esineet");
        namePuzzles = Resources.LoadAll<Sprite>("Kuvat/EsineidenNimet");

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

        if (timeScript != null)
        {
            timeScript.TimerExpired -= HandleTimerExpired;
            timeScript.TimerExpired += HandleTimerExpired;
        }
        if (scoreScript == null)
        {
            scoreScript = FindAnyObjectByType<MuistipeliScoreScript>();
        }
        if (scoreScript == null)
        {
            Debug.LogError("GameManager: No MuistipeliScoreScript found. Score will not increase.");
        }
        infoById = objectInfos
            .Where(e => !string.IsNullOrWhiteSpace(e.id))
            .ToDictionary(
                e => e.id.Trim().ToLowerInvariant(),
                e => e.info ?? ""
            );

        WireEndButtons();
        SetEndButtonsVisible(false);

        WireOhjeetButton();
    }

    private void WireEndButtons()
    {
        var restartBtn = UudestaanNappi1 != null ? UudestaanNappi1.GetComponent<Button>() : null;
        if (restartBtn != null)
        {
            restartBtn.onClick.RemoveListener(UudestaanNappi);
            restartBtn.onClick.AddListener(UudestaanNappi);
        }

        var nextBtn = SeuraavaNappiObj != null ? SeuraavaNappiObj.GetComponent<Button>() : null;
        if (nextBtn != null)
        {
            nextBtn.onClick.RemoveListener(SeuraavaNappi);
            nextBtn.onClick.AddListener(SeuraavaNappi);
        }

        var menuBtn = PaavalikkoNappiObj != null ? PaavalikkoNappiObj.GetComponent<Button>() : null;
        if (menuBtn != null)
        {
            menuBtn.onClick.RemoveListener(PoistuNappi);
            menuBtn.onClick.AddListener(PoistuNappi);
        }
    }

    private void WireOhjeetButton()
    {
        var ohjeBtn = OhjeetNappiObj != null ? OhjeetNappiObj.GetComponent<Button>() : null;
        if (ohjeBtn != null)
        {
            ohjeBtn.onClick.RemoveListener(OhjeetNappi);
            ohjeBtn.onClick.AddListener(OhjeetNappi);
        }
    }

    private struct Card
    {
        public string id;
        public Sprite front;
    }
    private readonly List<Card> cards = new();

    private bool roundFinished;

    private void OnDestroy()
    {
        if (timeScript != null)
        {
            timeScript.TimerExpired -= HandleTimerExpired;
        }
    }

    private void SetEndButtonsVisible(bool visible)
    {
        if (UudestaanNappi1 != null) UudestaanNappi1.SetActive(visible);
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(visible);

        // "Seuraava" only makes sense if there is a next difficulty.
        bool hasNext = currentDifficulty == Difficulty.Easy || currentDifficulty == Difficulty.Medium;
        if (SeuraavaNappiObj != null) SeuraavaNappiObj.SetActive(visible && hasNext);
    }

    private void SetEndButtonsVisibleOnTimeout()
    {
        if (UudestaanNappi1 != null) UudestaanNappi1.SetActive(true);
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(true);
        if (SeuraavaNappiObj != null) SeuraavaNappiObj.SetActive(true);
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
        currentDifficulty = Difficulty.Easy;
        StartGame(2, 2);
        timeScript?.StartTimer(easySeconds);
    }

    public void StartMedium()
    {
        currentDifficulty = Difficulty.Medium;
        StartGame(4, 4);
        timeScript?.StartTimer(mediumSeconds);
    }

    public void StartHard()
    {
        currentDifficulty = Difficulty.Hard;
        StartGame(4, 4); // or whatever
        if (!TryAddGamePuzzlesHard()) { Debug.LogError("Not enough unique puzzle types for hard difficulty."); return; }
        AddListeners();
        timeScript?.StartTimer(hardSeconds);
    }

    private void StartGame(int r, int c)
    {
        rows = r;
        cols = c;

        // Reset state
        firstGuess = secondGuess = false;
        countCorrectGuesses = 0;
        roundFinished = false;
        SetEndButtonsVisible(false);

        // Starting a new difficulty should return the info panel to normal (pair list) mode.
        showOhjeet = false;
        ClearMatchedInfo();

        scoreScript?.ResetScore();

        // Rebuild board
        CreateBoard(rows, cols);

        // IMPORTANT: you must have enough puzzle pairs for this board size
        if (!TryAddGamePuzzles())
        {
            Debug.LogError("Not enough unique puzzle types for this difficulty.");
            return;
        }

        Shuffled(gamePuzzles);
        cards.Clear();
        for (int i = 0; i < gamePuzzles.Count; i++)
        {
            cards.Add(new Card
            {
                id = GetPuzzleId(gamePuzzles[i]),
                front = gamePuzzles[i]
            });
        }

        if (cards.Count != btns.Count)
            Debug.LogError($"Deck size mismatch: cards={cards.Count}, buttons={btns.Count}");

        AddListeners();

        gameGuesses = btns.Count / 2; // use button count, not gamePuzzles.Count
    }

    private void HandleTimerExpired()
    {
        // Timer hit zero: end the round even if not all pairs were found.
        if (roundFinished) return;

        roundFinished = true;
        ClearMatchedInfo();

        // Freeze the board so the player can't keep matching after time is up.
        if (firstRevealRoutine != null)
        {
            StopCoroutine(firstRevealRoutine);
            firstRevealRoutine = null;
        }
        firstGuess = secondGuess = false;
        for (int i = 0; i < btns.Count; i++)
        {
            if (btns[i] != null) btns[i].interactable = false;
        }

        // Show all three buttons as requested (Restart / Next / Main menu).
        SetEndButtonsVisibleOnTimeout();
    }

    private void ClearMatchedInfo()
    {
        matchedInfoText = "";
        RefreshInfoPanel();
    }

    private void RefreshInfoPanel()
    {
        if (infoPopupPanel == null || infoPopupText == null) return;

        if (showOhjeet)
        {
            infoPopupText.text = ohjeetTeksti ?? "";
            infoPopupPanel.SetActive(true);
            return;
        }

        if (string.IsNullOrWhiteSpace(matchedInfoText))
        {
            infoPopupText.text = "";
            infoPopupPanel.SetActive(false);
            return;
        }

        infoPopupText.text = matchedInfoText;
        infoPopupPanel.SetActive(true);
    }

    private void ShowInfoPopup(string id)
    {
        if (infoPopupPanel == null || infoPopupText == null) return;

        string key = (id ?? "").Trim().ToLowerInvariant();
        string text = infoById != null && infoById.TryGetValue(key, out var t) && !string.IsNullOrWhiteSpace(t)
            ? t
            : key; // fallback

        // Accumulate matched pair names/info. Only clear on timer expiry, round end, or difficulty change.
        if (string.IsNullOrWhiteSpace(matchedInfoText))
            matchedInfoText = text;
        else
            matchedInfoText += "\n" + text;

        // If instructions are currently shown, keep them on screen; the pair list continues accumulating.
        if (!showOhjeet)
            RefreshInfoPanel();
    }
    private enum Difficulty { None, Easy, Medium, Hard }
    private Difficulty currentDifficulty;

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
    private bool TryAddGamePuzzlesHard()
    {
        cards.Clear();
        int neededPairs = btns.Count / 2;

        // Photos grouped by id
        var photoGroups = puzzles
            .Where(p => p != null)
            .GroupBy(p => GetPuzzleId(p))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToList();

        // Names mapped by id (take first per id)
        var nameMap = namePuzzles
            .Where(p => p != null)
            .GroupBy(p => GetPuzzleId(p))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToDictionary(g => g.Key, g => g.First());

        // Only ids that exist in BOTH folders
        var available = photoGroups.Where(g => nameMap.ContainsKey(g.Key)).ToList();
        if (available.Count < neededPairs) return false;

        // shuffle available (same way you shuffle now)
        for (int i = 0; i < available.Count; i++)
        {
            int r = Random.Range(i, available.Count);
            (available[i], available[r]) = (available[r], available[i]);
        }

        for (int i = 0; i < neededPairs; i++)
        {
            string id = available[i].Key;
            Sprite photo = available[i].First();
            Sprite nameSprite = nameMap[id];

            cards.Add(new Card { id = id, front = photo });
            cards.Add(new Card { id = id, front = nameSprite });
        }

        // shuffle cards so photo/name positions are random
        for (int i = 0; i < cards.Count; i++)
        {
            int r = Random.Range(i, cards.Count);
            (cards[i], cards[r]) = (cards[r], cards[i]);
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
            btn.interactable = true;
            btn.image.sprite = bgImage;
            btn.image.color = Color.white;
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


    [System.Serializable]
    public class ObjectInfoEntry
    {
        public string id;                 // e.g. "sahtikulho" (same as GetPuzzleId output)
        [TextArea] public string info;    // the popup text
    }

    [SerializeField] private GameObject infoPopupPanel;
    [SerializeField] private TMP_Text infoPopupText;
    [SerializeField] private float infoPopupSeconds = 2f;
    [SerializeField] private float clearInfoOnWinDelaySeconds = 1.5f;
    [Header("Ohjeet")]
    [TextArea]
    [SerializeField] private string ohjeetTeksti = "Etsi kaikki parit ennen kuin aika loppuu.\n\nKäytä hiirtä pelaamiseen.\n\nHelpossa ja keskitasossa etsi kuvaparit. Vaikeassa etsi kuvan esineen nimi.";
    [SerializeField] private List<ObjectInfoEntry> objectInfos = new();

    private Dictionary<string, string> infoById;
    private Coroutine infoRoutine;
    private string matchedInfoText = "";
    private bool showOhjeet;

    // Can be wired directly from the Button's OnClick in the Inspector.
    public void OhjeetNappi()
    {
        showOhjeet = !showOhjeet;
        RefreshInfoPanel();
    }

    private void ScheduleClearInfoPopup(float delaySeconds)
    {
        if (infoRoutine != null)
        {
            StopCoroutine(infoRoutine);
            infoRoutine = null;
        }

        float delay = Mathf.Max(0f, delaySeconds);
        infoRoutine = StartCoroutine(ClearInfoPopupAfter(delay));
    }

    private IEnumerator ClearInfoPopupAfter(float delaySeconds)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        infoRoutine = null;
        ClearMatchedInfo();
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
            btns[firstGuessIndex].image.sprite = cards[index].front;

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
        btns[secondGuessIndex].image.sprite = cards[index].front;

        if (firstRevealRoutine != null)
        {
            StopCoroutine(firstRevealRoutine);
            firstRevealRoutine = null;
        }

        bool isMatch = cards[firstGuessIndex].id == cards[secondGuessIndex].id;
        Debug.Log($"First guess index: {firstGuessIndex}, Second guess index: {secondGuessIndex}, " +
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
        bool isMatch = cards[firstGuessIndex].id == cards[secondGuessIndex].id;

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

                ShowInfoPopup(cards[firstGuessIndex].id);
            
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
            roundFinished = true;
            SetEndButtonsVisible(true);

            // Let the player briefly see the full stacked list, then clear.
            ScheduleClearInfoPopup(clearInfoOnWinDelaySeconds);

            Debug.Log("Final score: " + scoreScript.GetScore() + ", pairs found: " + scoreScript.GetPairsFound());
            scoreScript.OnGameFinished();
            timeScript?.StopTimer();
            //}
        }
    }

    public void PoistuNappi()
    {
        SceneManager.LoadScene("1. Päävalikko");
    }

    public void SeuraavaNappi()
    {
        if (!roundFinished)
        {
            Debug.LogWarning("SeuraavaNappi pressed but round is not finished yet.");
            return;
        }

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                StartMedium();
                break;
            case Difficulty.Medium:
                StartHard();
                break;
            case Difficulty.Hard:
            case Difficulty.None:
            default:
                // No next difficulty; keep it simple and go back to main menu.
                PoistuNappi();
                break;
        }
    }

    // Hook this to "UudestaanNappi1" if you want to replay the same difficulty without reloading the whole scene.
    public void UudestaanNappi()
    {
        if (!roundFinished)
        {
            Debug.LogWarning("UudestaanNappi pressed but round is not finished yet.");
            return;
        }

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                StartEasy();
                break;
            case Difficulty.Medium:
                StartMedium();
                break;
            case Difficulty.Hard:
                StartHard();
                break;
            case Difficulty.None:
            default:
                RestartGame();
                break;
        }
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