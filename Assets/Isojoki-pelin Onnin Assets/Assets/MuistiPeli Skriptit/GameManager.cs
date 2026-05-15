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
    [SerializeField] private GameObject puzzleBtnPrefab; 
    [SerializeField] private Transform boardParent;      
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;
    [SerializeField] private TimeScript timeScript;

    [Header("Difficulty Timers (seconds)")]
    [SerializeField] private float easySeconds = 10f;
    [SerializeField] private float mediumSeconds = 60f;
    [SerializeField] private float hardSeconds = 120f;

    [Header("Difficulty Scoring")]
    [SerializeField] private int easyPointsPerPair = 5;
    [SerializeField] private int mediumPointsPerPair = 10;
    [SerializeField] private int hardPointsPerPair = 20;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip pickCardClip;
    [SerializeField] private Sprite bgImage;

    [Header("Reveal Timing")]
    [SerializeField] private float firstCardRevealSeconds = 1.5f;

    [Header("Win Effect")]
    [SerializeField] private ParticleSystem allPairsParticles;
    [SerializeField] private Transform allPairsParticlesAnchor;
    [SerializeField] private bool stopParticlesBeforePlay = true;

    public Sprite[] puzzles;
    public List<Sprite> gamePuzzles = new List<Sprite>();
    public List<Button> btns = new List<Button>();
    public bool firstGuess, secondGuess;

    private int countCorrectGuesses;
    private int gameGuesses;
    private int firstGuessIndex, secondGuessIndex;
    private Coroutine firstRevealRoutine;
    private Sprite[] namePuzzles;

    public GameObject UudestaanNappi1;

    [Header("Difficulty buttons")]
    [SerializeField] private GameObject HelppoNappiObj;
    [SerializeField] private GameObject KeskiTasoNappiObj;
    [SerializeField] private GameObject VaikeaNappiObj;
    [SerializeField] private GameObject PoistuTasoihinNappiObj;

    [Header("Difficulty selection label")]
    [SerializeField] private GameObject TasonValintaTekstiObj;

    [Header("Secret (Easter egg)")]
    [SerializeField] private GameObject SalainenNappiObj;
    [SerializeField] [TextArea(2, 6)] private string salainenEasterEggTeksti = "";

    [Header("End-of-round buttons")]
    [SerializeField] private GameObject SeuraavaNappiObj;
    [SerializeField] private GameObject PaavalikkoNappiObj;

    [Header("Results")]
    [SerializeField] private GameObject TuloksetNappiObj;

    [Header("Start menu")]
    [SerializeField] private GameObject AloitaPeliNappiObj;

    [Header("Info panel")]
    [SerializeField] private GameObject OhjeetNappiObj;
    [SerializeField] private MuistipeliScoreScript scoreScript;

    [Header("Rewards (Palkinnot)")]
    [SerializeField] private GameObject PalkintoNappi1Obj;
    [SerializeField] private GameObject PalkintoNappi2Obj;
    [SerializeField] private GameObject PalkintoNappi3Obj;
    [SerializeField] private Sprite palkinto1Sprite;
    [SerializeField] private Sprite palkinto2Sprite;
    [SerializeField] private Sprite palkinto3Sprite;
    [SerializeField] private Image palkintoImageInInfoPanel;
    [SerializeField] private float palkintoFitPadding = 0f;
    [SerializeField] private Vector2 palkintoFallbackSize = new Vector2(256f, 256f);

    [SerializeField] private GameObject saitPalkinnonTekstiObj;
    [SerializeField] private GameObject saitKaikkiPalkinnotTekstiObj;
    [SerializeField] private GameObject voititTekstiObj;

    [Header("Scene Navigation")]
    [SerializeField] private string Päävalikko = "Päävalikko";

    private readonly Dictionary<Difficulty, int> completedScoreByDifficulty = new();
    private readonly HashSet<Difficulty> unlockedRewards = new();
    private bool resetRewardsPending;
    private bool showPalkinto;

    private int salainenCompletionIndex;
    private bool salainenUnlocked;
    private bool salainenEasterEggAppended;
    private bool isDifficultySelectionVisible;
    private bool areEndButtonsVisible;

    private struct Card {
        public string id;
        public Sprite front;
    }
    private readonly List<Card> cards = new();
    private bool roundFinished;

    [System.Serializable]
    public class ObjectInfoEntry {
        public string id;
        [TextArea] public string info;
    }

    [SerializeField] private GameObject infoPopupPanel;
    [SerializeField] private TMP_Text infoPopupText;
    [SerializeField] private float clearInfoOnWinDelaySeconds = 1.5f;
    [Header("Ohjeet")]
    [TextArea] [SerializeField] private string ohjeetTeksti = "Etsi kaikki parit...";
    [SerializeField] private List<ObjectInfoEntry> objectInfos = new();

    private Dictionary<string, string> infoById;
    private Coroutine infoRoutine;
    private string matchedInfoText = "";
    private bool showOhjeet;

    private enum Difficulty { None, Easy, Medium, Hard }
    private Difficulty currentDifficulty;

    private void Awake() {
        puzzles = Resources.LoadAll<Sprite>("Kuvat/Esineet");
        namePuzzles = Resources.LoadAll<Sprite>("Kuvat/EsineidenNimet");
        ResolveOhjeetButtonRefIfNeeded();
        ResolveInfoPanelRefsIfNeeded();
        ResolvePoistuTasoihinButtonRefIfNeeded();
        ResolveTuloksetButtonRefIfNeeded();
        if (scoreScript == null) scoreScript = FindAnyObjectByType<MuistipeliScoreScript>();
        if (timeScript == null) timeScript = FindAnyObjectByType<TimeScript>();
        if (timeScript != null) {
            timeScript.TimerExpired -= HandleTimerExpired;
            timeScript.TimerExpired += HandleTimerExpired;
        }
        infoById = objectInfos.Where(e => !string.IsNullOrWhiteSpace(e.id)).ToDictionary(e => e.id.Trim().ToLowerInvariant(), e => e.info ?? "");
        WireEndButtons();
        SetEndButtonsVisible(false);
        WireOhjeetButton();
        WireAloitaPeliButton();
        WireDifficultyButtons();
        WirePoistuTasoihinButton();
        WireTuloksetButton();
        UpdateTuloksetButtonVisibility();
        WirePalkintoButtons();
        HideAllPalkintoButtons();
        SetSaitPalkinnonVisible(false);
        SetSaitKaikkiPalkinnotVisible(false);
        SetVoititVisible(false);
        ClearPalkintoFromInfoPanel();
        SetDifficultySelectionVisible(false);
        SetStartMenuVisible(true);
        SetMenuChromeVisible(true);
        SetPoistuTasoihinVisible(false);
        ResolveSalainenNappiRefIfNeeded();
        WireSalainenNappiButton();
        SetSalainenNappiVisible(false);
        if (allPairsParticles == null) allPairsParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    private string GetCorrectID(Sprite s) {
        if (s == null) return string.Empty;
        string rawName = Regex.Replace(s.name, @"\s+\d+$", "").Trim();
        if (string.IsNullOrEmpty(rawName)) return string.Empty;
        return char.ToUpper(rawName[0]) + rawName.Substring(1).ToLower();
    }

    public void StartEasy() { ApplyPendingRewardResetIfNeeded(); ResetMenuUI(); currentDifficulty = Difficulty.Easy; StartGame(2, 2); timeScript?.StartTimer(easySeconds); }
    public void StartMedium() { ApplyPendingRewardResetIfNeeded(); ResetMenuUI(); currentDifficulty = Difficulty.Medium; StartGame(4, 4); timeScript?.StartTimer(mediumSeconds); }
    public void StartHard() {
        ApplyPendingRewardResetIfNeeded(); ResetMenuUI(); currentDifficulty = Difficulty.Hard;
        rows = 4; cols = 4;
        PrepareGameBaseUI();
        if (!TryAddGamePuzzlesHard()) { Debug.LogError("Not enough unique items for Hard."); return; }
        AddListeners();
        timeScript?.StartTimer(hardSeconds);
    }

    private void PrepareGameBaseUI() {
        if (OhjeetNappiObj != null) OhjeetNappiObj.SetActive(false);
        UpdateTuloksetButtonVisibility(true);
        if (AloitaPeliNappiObj != null) AloitaPeliNappiObj.SetActive(false);
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(false);
        firstGuess = secondGuess = false; countCorrectGuesses = 0; roundFinished = false;
        SetEndButtonsVisible(false);
        SetPoistuTasoihinVisible(false);
        HideAllPalkintoButtons();
        SetSaitPalkinnonVisible(false);
        SetSaitKaikkiPalkinnotVisible(false);
        SetVoititVisible(false);
        showOhjeet = false;
        ClearMatchedInfo();
        ClearPalkintoFromInfoPanel();
        if (scoreScript != null) {
            int ppp = currentDifficulty switch { Difficulty.Easy => easyPointsPerPair, Difficulty.Medium => mediumPointsPerPair, Difficulty.Hard => hardPointsPerPair, _ => 10 };
            scoreScript.SetPointsPerPair(ppp); scoreScript.ResetScore();
        }
        CreateBoard(rows, cols);
        gameGuesses = (rows * cols) / 2;
    }

    private void StartGame(int r, int c) {
        rows = r; cols = c;
        PrepareGameBaseUI();
        if (!TryAddGamePuzzles()) return;
        Shuffled(gamePuzzles);
        cards.Clear();
        for (int i = 0; i < gamePuzzles.Count; i++) {
            cards.Add(new Card { id = GetCorrectID(gamePuzzles[i]), front = gamePuzzles[i] });
        }
        AddListeners();
    }

    /* private bool TryAddGamePuzzles() {
        gamePuzzles.Clear(); int neededPairs = (rows * cols) / 2;
        List<string> avatut = EsineRekisteri.HaeAvatutEsineet();
        var uniqueGroups = puzzles.Where(p => p != null).GroupBy(p => GetCorrectID(p)).Where(g => !string.IsNullOrEmpty(g.Key) && avatut.Contains(g.Key)).ToList();
        if (uniqueGroups.Count < neededPairs) { Debug.LogError($"Not enough unlocked items. Needed {neededPairs}, found {uniqueGroups.Count}."); return false; }
        for (int i = 0; i < uniqueGroups.Count; i++) { int r = Random.Range(i, uniqueGroups.Count); (uniqueGroups[i], uniqueGroups[r]) = (uniqueGroups[r], uniqueGroups[i]); }
        for (int i = 0; i < neededPairs; i++) { Sprite chosen = uniqueGroups[i].First(); gamePuzzles.Add(chosen); gamePuzzles.Add(chosen); }
        return true;
    }

    private bool TryAddGamePuzzlesHard() {
        cards.Clear(); int neededPairs = (rows * cols) / 2;
        List<string> avatut = EsineRekisteri.HaeAvatutEsineet();
        var photoGroups = puzzles.Where(p => p != null).GroupBy(p => GetCorrectID(p)).Where(g => !string.IsNullOrEmpty(g.Key) && avatut.Contains(g.Key)).ToList();
        var nameMap = namePuzzles.Where(p => p != null).GroupBy(p => GetCorrectID(p)).ToDictionary(g => g.Key, g => g.First());
        var available = photoGroups.Where(g => nameMap.ContainsKey(g.Key)).ToList();
        if (available.Count < neededPairs) return false;
        for (int i = 0; i < available.Count; i++) { int r = Random.Range(i, available.Count); (available[i], available[r]) = (available[r], available[i]); }
        for (int i = 0; i < neededPairs; i++) { string id = available[i].Key; cards.Add(new Card { id = id, front = available[i].First() }); cards.Add(new Card { id = id, front = nameMap[id] }); }
        for (int i = 0; i < cards.Count; i++) { int r = Random.Range(i, cards.Count); (cards[i], cards[r]) = (cards[r], cards[i]); }
        return true;
    }*/
    private string GetPuzzleId(Sprite s)
    {
        if (s == null) return string.Empty;
        // "Sahtikulho 4" -> "Sahtikulho"
        return Regex.Replace(s.name, @"\s+\d+$", "").Trim().ToLowerInvariant();

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

    public void PickPuzzle(int index) {
        if (secondGuess || roundFinished) return;
        if (!firstGuess) {
            firstGuess = true; firstGuessIndex = index; btns[firstGuessIndex].image.sprite = cards[index].front;
            if (firstRevealRoutine != null) StopCoroutine(firstRevealRoutine);
            firstRevealRoutine = StartCoroutine(AutoHideFirstGuess(firstGuessIndex));
            return;
        }
        if (index == firstGuessIndex) return;
        secondGuess = true; secondGuessIndex = index; btns[secondGuessIndex].image.sprite = cards[index].front;
        if (firstRevealRoutine != null) { StopCoroutine(firstRevealRoutine); firstRevealRoutine = null; }
        StartCoroutine(CheckThePuzzleMatch());
    }

    private IEnumerator CheckThePuzzleMatch() {
        yield return new WaitForSeconds(0.5f);
        bool isMatch = cards[firstGuessIndex].id == cards[secondGuessIndex].id;
        if (isMatch) {
            btns[firstGuessIndex].interactable = false; btns[secondGuessIndex].interactable = false;
            btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0); btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            int pts = currentDifficulty switch { Difficulty.Easy => easyPointsPerPair, Difficulty.Medium => mediumPointsPerPair, Difficulty.Hard => hardPointsPerPair, _ => 10 };
            Pistemanageri.LisaaPisteita(pts); scoreScript?.OnPairMatched();
            CheckTheGameFinished(); ShowInfoPopup(cards[firstGuessIndex].id);
        } else {
            btns[firstGuessIndex].image.sprite = bgImage; btns[secondGuessIndex].image.sprite = bgImage;
        }
        yield return new WaitForSeconds(0.5f);
        firstGuess = secondGuess = false;
    }

    private void CheckTheGameFinished() {
        countCorrectGuesses++;
        if (countCorrectGuesses == gameGuesses) {
            roundFinished = true; Pistemanageri.TallennaPisteet(); NoteDifficultyCompletedWin(currentDifficulty);
            SetVoititVisible(true); SaveCompletedScoreForCurrentDifficulty();
            if (allPairsParticles != null) {
                if (allPairsParticlesAnchor != null) allPairsParticles.transform.position = allPairsParticlesAnchor.position;
                allPairsParticles.gameObject.SetActive(true); if (stopParticlesBeforePlay) allPairsParticles.Stop(); allPairsParticles.Play();
            }
            SetEndButtonsVisible(true); UnlockRewardForDifficulty(currentDifficulty); UpdatePalkintoButtonsVisibility(true);
            bool all = AreAllRewardsUnlocked(); SetSaitKaikkiPalkinnotVisible(all); SetSaitPalkinnonVisible(!all);
            SetPoistuTasoihinVisible(true);
            ScheduleClearInfoPopup(clearInfoOnWinDelaySeconds);
            scoreScript?.OnGameFinished(); timeScript?.StopTimer();
        }
    }

    private void CreateBoard(int r, int c) {
        for (int i = boardParent.childCount - 1; i >= 0; i--) Destroy(boardParent.GetChild(i).gameObject);
        btns.Clear(); int total = r * c;
        for (int i = 0; i < total; i++) {
            var go = Instantiate(puzzleBtnPrefab, boardParent); var btn = go.GetComponent<Button>();
            btn.interactable = true; btn.image.sprite = bgImage; btns.Add(btn);
        }
    }

    private void AddListeners() {
        for (int i = 0; i < btns.Count; i++) {
            int idx = i; btns[i].onClick.RemoveAllListeners(); btns[i].onClick.AddListener(() => PickPuzzle(idx));
        }
    }

    private void Shuffled(List<Sprite> list) {
        for (int i = 0; i < list.Count; i++) { Sprite tmp = list[i]; int r = Random.Range(i, list.Count); list[i] = list[r]; list[r] = tmp; }
    }

    private IEnumerator AutoHideFirstGuess(int index) {
        yield return new WaitForSeconds(firstCardRevealSeconds);
        if (firstGuess && !secondGuess && firstGuessIndex == index) { btns[index].image.sprite = bgImage; firstGuess = false; }
        firstRevealRoutine = null;
    }

    private void ShowInfoPopup(string id) {
        string key = (id ?? "").Trim().ToLowerInvariant();
        string text = infoById != null && infoById.TryGetValue(key, out var t) ? t : key;
        matchedInfoText = string.IsNullOrWhiteSpace(matchedInfoText) ? text : matchedInfoText + "\n" + text;
        if (!showOhjeet) RefreshInfoPanel();
    }

    private void ClearMatchedInfo() { matchedInfoText = ""; RefreshInfoPanel(); }

    /* private void RefreshInfoPanel() {
        ResolveInfoPanelRefsIfNeeded();
        if (infoPopupPanel == null || infoPopupText == null) return;

        if (showOhjeet) {
            if (palkintoImageInInfoPanel != null) palkintoImageInInfoPanel.gameObject.SetActive(false);
            infoPopupText.enabled = true;
            if (!infoPopupText.gameObject.activeSelf) infoPopupText.gameObject.SetActive(true);
            infoPopupText.text = ohjeetTeksti ?? "";
            infoPopupPanel.SetActive(true);
            infoPopupPanel.transform.SetAsLastSibling();
        }
        else if (showPalkinto) {
            infoPopupText.enabled = true;
            if (!infoPopupText.gameObject.activeSelf) infoPopupText.gameObject.SetActive(true);
            infoPopupText.text = matchedInfoText;
            if (palkintoImageInInfoPanel != null) palkintoImageInInfoPanel.gameObject.SetActive(true);
            infoPopupPanel.SetActive(true);
            infoPopupPanel.transform.SetAsLastSibling();
        }
        else {
            if (palkintoImageInInfoPanel != null) palkintoImageInInfoPanel.gameObject.SetActive(false);
            if (string.IsNullOrWhiteSpace(matchedInfoText)) infoPopupPanel.SetActive(false);
            else {
                infoPopupText.enabled = true;
                if (!infoPopupText.gameObject.activeSelf) infoPopupText.gameObject.SetActive(true);
                infoPopupText.text = matchedInfoText;
                infoPopupPanel.SetActive(true);
                infoPopupPanel.transform.SetAsLastSibling();
            }
        }
    }*/
       private void RefreshInfoPanel()
    {
        if (infoPopupPanel == null || infoPopupText == null) return;

        if (showOhjeet)
        {
            if (palkintoImageInInfoPanel != null) palkintoImageInInfoPanel.gameObject.SetActive(false);
            infoPopupText.text = ohjeetTeksti ?? "";
            infoPopupPanel.SetActive(true);
            return;
        }

        if (showPalkinto)
        {
            // Reward mode: keep panel visible even if there is no text.
            infoPopupText.text = string.IsNullOrWhiteSpace(matchedInfoText) ? "" : matchedInfoText;
            if (palkintoImageInInfoPanel != null && palkintoImageInInfoPanel.sprite != null)
                palkintoImageInInfoPanel.gameObject.SetActive(true);

            infoPopupPanel.SetActive(true);
            return;
        }

        if (palkintoImageInInfoPanel != null) palkintoImageInInfoPanel.gameObject.SetActive(false);

        if (string.IsNullOrWhiteSpace(matchedInfoText))
        {
            infoPopupText.text = "";
            infoPopupPanel.SetActive(false);
            return;
        }

        infoPopupText.text = matchedInfoText;
        infoPopupPanel.SetActive(true);
    }

    public void ScheduleClearInfoPopup(float delaySeconds) {
        if (infoRoutine != null) StopCoroutine(infoRoutine);
        infoRoutine = StartCoroutine(ClearInfoPopupAfter(delaySeconds));
    }

    private IEnumerator ClearInfoPopupAfter(float delaySeconds) { yield return new WaitForSeconds(delaySeconds); ClearMatchedInfo(); infoRoutine = null; }

    private void HandleTimerExpired() {
        if (roundFinished) return;
        roundFinished = true; ClearMatchedInfo();
        for (int i = 0; i < btns.Count; i++) if (btns[i] != null) btns[i].interactable = false;
        SetEndButtonsVisible(true);
        // Requirement: PoistuTasoihinNappi is visible only after a difficulty is finished (win).
        SetPoistuTasoihinVisible(false);
    }

    public void UpdateTuloksetButtonVisibility(bool forceHide = false) {
        ResolveTuloksetButtonRefIfNeeded();
        if (TuloksetNappiObj == null) return;
        TuloksetNappiObj.SetActive(!forceHide && completedScoreByDifficulty.Count > 0);
    }

    private void ResetMenuUI() { SetStartMenuVisible(false); SetDifficultySelectionVisible(false); SetMenuChromeVisible(false); SetPoistuTasoihinVisible(false); }

    private void SetPoistuTasoihinVisible(bool v) {
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(v);
    }
    private void WireEndButtons() { if (UudestaanNappi1) UudestaanNappi1.GetComponent<Button>().onClick.AddListener(UudestaanNappi); if (SeuraavaNappiObj) SeuraavaNappiObj.GetComponent<Button>().onClick.AddListener(SeuraavaNappi); if (PaavalikkoNappiObj) PaavalikkoNappiObj.GetComponent<Button>().onClick.AddListener(PoistuNappi); }
    private void WireDifficultyButtons() { if (HelppoNappiObj) HelppoNappiObj.GetComponent<Button>().onClick.AddListener(StartEasy); if (KeskiTasoNappiObj) KeskiTasoNappiObj.GetComponent<Button>().onClick.AddListener(StartMedium); if (VaikeaNappiObj) VaikeaNappiObj.GetComponent<Button>().onClick.AddListener(StartHard); }

    private void WirePoistuTasoihinButton() {
        if (!PoistuTasoihinNappiObj) return;
        var btn = PoistuTasoihinNappiObj.GetComponent<Button>();
        if (btn == null) {
            Debug.LogWarning("GameManager: PoistuTasoihinNappiObj has no Button component.");
            return;
        }

        btn.onClick.RemoveListener(PoistuTasoihinNappi);
        btn.onClick.AddListener(PoistuTasoihinNappi);
    }
    private void WireOhjeetButton() {
        if (!OhjeetNappiObj) return;
        var btn = OhjeetNappiObj.GetComponent<Button>();
        if (btn == null) {
            Debug.LogWarning("GameManager: OhjeetNappiObj has no Button component.");
            return;
        }

        // Make sure a broken prefab/persistent onClick entry can't prevent our handler from running.
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OhjeetNappi);
    }
    private void WireAloitaPeliButton() { if (AloitaPeliNappiObj) AloitaPeliNappiObj.GetComponent<Button>().onClick.AddListener(AloitaPeliNappi); }
    private void WireTuloksetButton() {
        if (!TuloksetNappiObj) return;
        var btn = TuloksetNappiObj.GetComponent<Button>();
        if (btn == null) {
            Debug.LogWarning("GameManager: TuloksetNappiObj has no Button component.");
            return;
        }

        btn.onClick.RemoveListener(TuloksetNappi);
        btn.onClick.AddListener(TuloksetNappi);
    }

    private void WireSalainenNappiButton() {
        if (!SalainenNappiObj) return;
        var btn = SalainenNappiObj.GetComponent<Button>();
        if (btn == null) {
            Debug.LogWarning("GameManager: SalainenNappiObj has no Button component.");
            return;
        }

        btn.onClick.RemoveListener(SalainenNappi);
        btn.onClick.AddListener(SalainenNappi);
    }
    //private void WirePalkintoButtons() { if (PalkintoNappi1Obj) PalkintoNappi1Obj.GetComponent<Button>().onClick.AddListener(PalkintoNappi1); if (PalkintoNappi2Obj) PalkintoNappi2Obj.GetComponent<Button>().onClick.AddListener(PalkintoNappi2); if (PalkintoNappi3Obj) PalkintoNappi3Obj.GetComponent<Button>().onClick.AddListener(PalkintoNappi3); }
    private void WirePalkintoButtons()
    {
        //ResolvePalkintoRefsIfNeeded();

        var b1 = PalkintoNappi1Obj != null ? PalkintoNappi1Obj.GetComponent<Button>() : null;
        if (b1 == null && PalkintoNappi1Obj != null) b1 = PalkintoNappi1Obj.GetComponentInChildren<Button>(true);
        if (b1 != null)
        {
            PalkintoNappi1Obj = b1.gameObject;
            b1.onClick.RemoveListener(PalkintoNappi1);
            b1.onClick.AddListener(PalkintoNappi1);
        }

        var b2 = PalkintoNappi2Obj != null ? PalkintoNappi2Obj.GetComponent<Button>() : null;
        if (b2 == null && PalkintoNappi2Obj != null) b2 = PalkintoNappi2Obj.GetComponentInChildren<Button>(true);
        if (b2 != null)
        {
            PalkintoNappi2Obj = b2.gameObject;
            b2.onClick.RemoveListener(PalkintoNappi2);
            b2.onClick.AddListener(PalkintoNappi2);
        }

        var b3 = PalkintoNappi3Obj != null ? PalkintoNappi3Obj.GetComponent<Button>() : null;
        if (b3 == null && PalkintoNappi3Obj != null) b3 = PalkintoNappi3Obj.GetComponentInChildren<Button>(true);
        if (b3 != null)
        {
            PalkintoNappi3Obj = b3.gameObject;
            b3.onClick.RemoveListener(PalkintoNappi3);
            b3.onClick.AddListener(PalkintoNappi3);
        }
    }

    private void HideAllPalkintoButtons()
    {
        if (PalkintoNappi1Obj != null) PalkintoNappi1Obj.SetActive(false);
        if (PalkintoNappi2Obj != null) PalkintoNappi2Obj.SetActive(false);
        if (PalkintoNappi3Obj != null) PalkintoNappi3Obj.SetActive(false);
    }

    private void UpdatePalkintoButtonsVisibility(bool visible)
    {
        if (!visible)
        {
            HideAllPalkintoButtons();
            return;
        }

        // Show all unlocked rewards (one per completed difficulty).
        if (PalkintoNappi1Obj != null) PalkintoNappi1Obj.SetActive(unlockedRewards.Contains(Difficulty.Hard));
        if (PalkintoNappi2Obj != null) PalkintoNappi2Obj.SetActive(unlockedRewards.Contains(Difficulty.Medium));
        if (PalkintoNappi3Obj != null) PalkintoNappi3Obj.SetActive(unlockedRewards.Contains(Difficulty.Easy));
    }
    public void PoistuNappi() { SceneManager.LoadScene(Päävalikko); }
    public void UudestaanNappi() { completedScoreByDifficulty.Remove(currentDifficulty); UpdateTuloksetButtonVisibility(); if (currentDifficulty == Difficulty.Easy) StartEasy(); else if (currentDifficulty == Difficulty.Medium) StartMedium(); else StartHard(); }
    public void SeuraavaNappi() { if (currentDifficulty == Difficulty.Easy) StartMedium(); else if (currentDifficulty == Difficulty.Medium) StartHard(); else PoistuNappi(); }
    public void OhjeetNappi() {
        ResolveInfoPanelRefsIfNeeded();
        showOhjeet = true;
        RefreshInfoPanel();
    }

    public void PoistuTasoihinNappi() {
        // Return to difficulty selection inside the same scene.
        roundFinished = true;
        showOhjeet = false;
        ClearMatchedInfo();
        ClearPalkintoFromInfoPanel();
        HideAllPalkintoButtons();
        SetSaitPalkinnonVisible(false);
        SetSaitKaikkiPalkinnotVisible(false);
        SetVoititVisible(false);

        timeScript?.StopTimer();

        if (allPairsParticles != null) {
            allPairsParticles.Stop();
            allPairsParticles.gameObject.SetActive(false);
        }

        // Clear the board so you don't see the old round.
        if (boardParent != null) {
            for (int i = boardParent.childCount - 1; i >= 0; i--) {
                Destroy(boardParent.GetChild(i).gameObject);
            }
        }
        btns.Clear();
        cards.Clear();

        SetEndButtonsVisible(false);
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);
        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(true);
        SetMenuChromeVisible(true);
    }
    public void AloitaPeliNappi() { SetStartMenuVisible(false); SetDifficultySelectionVisible(true); }
    public void TuloksetNappi() { int t = completedScoreByDifficulty.Values.Sum(); scoreScript?.ShowCombinedTotal(t); }
    public void SalainenNappi() { if (!salainenEasterEggAppended) { salainenEasterEggAppended = true; matchedInfoText += "\n" + salainenEasterEggTeksti; RefreshInfoPanel(); } }
    public void PalkintoNappi1() { ShowPalkintoInInfoPanel(palkinto1Sprite); }
    public void PalkintoNappi2() { ShowPalkintoInInfoPanel(palkinto2Sprite); }
    public void PalkintoNappi3() { ShowPalkintoInInfoPanel(palkinto3Sprite); }

    private void SetEndButtonsVisible(bool v) { if (UudestaanNappi1) UudestaanNappi1.SetActive(v); if (PaavalikkoNappiObj) PaavalikkoNappiObj.SetActive(v); if (SeuraavaNappiObj) SeuraavaNappiObj.SetActive(v && currentDifficulty != Difficulty.Hard); areEndButtonsVisible = v; }
    private void SetDifficultySelectionVisible(bool v) { if (HelppoNappiObj) HelppoNappiObj.SetActive(v); if (KeskiTasoNappiObj) KeskiTasoNappiObj.SetActive(v); if (VaikeaNappiObj) VaikeaNappiObj.SetActive(v); if (TasonValintaTekstiObj) TasonValintaTekstiObj.SetActive(v); isDifficultySelectionVisible = v; }
    private void SetStartMenuVisible(bool v) { if (AloitaPeliNappiObj) AloitaPeliNappiObj.SetActive(v); }
    private void SetMenuChromeVisible(bool v) { if (OhjeetNappiObj) OhjeetNappiObj.SetActive(v); if (PaavalikkoNappiObj) PaavalikkoNappiObj.SetActive(v); }

    private void ResolveOhjeetButtonRefIfNeeded() {
        if (OhjeetNappiObj != null) return;

        // GameObject.Find only finds active objects.
        var byName = GameObject.Find("OhjeetNappi");
        if (byName != null) {
            OhjeetNappiObj = byName;
            return;
        }

        // Fallback: find an (even inactive) Button in a loaded scene whose name matches/contains "Ohje".
        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++) {
            var b = buttons[i];
            if (b == null) continue;
            var go = b.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;
            if (b.name.Equals("OhjeetNappi", System.StringComparison.OrdinalIgnoreCase) ||
                b.name.IndexOf("ohje", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                OhjeetNappiObj = go;
                return;
            }
        }
    }

    private void ResolvePoistuTasoihinButtonRefIfNeeded() {
        if (PoistuTasoihinNappiObj != null) return;

        var byName = GameObject.Find("PoistuTasoihinNappi");
        if (byName != null) {
            PoistuTasoihinNappiObj = byName;
            return;
        }

        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++) {
            var b = buttons[i];
            if (b == null) continue;
            var go = b.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

            if (b.name.Equals("PoistuTasoihinNappi", System.StringComparison.OrdinalIgnoreCase) ||
                b.name.IndexOf("tasoihin", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                PoistuTasoihinNappiObj = go;
                return;
            }
        }
    }

    private void ResolveInfoPanelRefsIfNeeded() {
        if (infoPopupPanel == null) {
            // GameObject.Find only finds active objects, so scan loaded scene objects instead.
            var transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++) {
                var t = transforms[i];
                if (t == null) continue;
                var go = t.gameObject;
                if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

                // Accept a few common names to avoid guessing too broadly.
                if (t.name.Equals("InfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    t.name.Equals("Infopanel", System.StringComparison.OrdinalIgnoreCase) ||
                    t.name.Equals("InfoPopupPanel", System.StringComparison.OrdinalIgnoreCase)) {
                    infoPopupPanel = go;
                    break;
                }
            }
        }

        if (infoPopupText == null) {
            if (infoPopupPanel != null) {
                infoPopupText = infoPopupPanel.GetComponentInChildren<TMP_Text>(true);
            }

            // Fallback: locate the TMP_Text directly by common names.
            if (infoPopupText == null) {
                var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
                for (int i = 0; i < texts.Length; i++) {
                    var t = texts[i];
                    if (t == null) continue;
                    var go = t.gameObject;
                    if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

                    if (t.name.Equals("InfoPopupText", System.StringComparison.OrdinalIgnoreCase) ||
                        t.name.Equals("InfoText", System.StringComparison.OrdinalIgnoreCase) ||
                        t.name.IndexOf("infopopup", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.name.IndexOf("infopanel", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                        infoPopupText = t;
                        break;
                    }
                }
            }
        }

        // If we have the text but no panel, use the text's parent as the panel.
        if (infoPopupPanel == null && infoPopupText != null) {
            var parent = infoPopupText.transform.parent;
            infoPopupPanel = parent != null ? parent.gameObject : infoPopupText.gameObject;
        }

        if (infoPopupPanel == null || infoPopupText == null) {
            Debug.LogWarning("GameManager: Info panel references missing. Assign InfoPopupPanel + InfoPopupText in Inspector, or name them InfoPanel/InfoPopupPanel and InfoPopupText.");
        }
    }
    private void ResolveSalainenNappiRefIfNeeded() {
        if (SalainenNappiObj != null) return;

        // GameObject.Find only finds active objects.
        var byName = GameObject.Find("SalainenNappi");
        if (byName != null) {
            SalainenNappiObj = byName;
            WireSalainenNappiButton();
            return;
        }

        // Fallback: find an (even inactive) Button in a loaded scene.
        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++) {
            var b = buttons[i];
            if (b == null) continue;
            var go = b.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

            if (b.name.Equals("SalainenNappi", System.StringComparison.OrdinalIgnoreCase) ||
                b.name.IndexOf("salainen", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                SalainenNappiObj = go;
                WireSalainenNappiButton();
                return;
            }
        }
    }

    private void ResolveTuloksetButtonRefIfNeeded() {
        if (TuloksetNappiObj != null) return;

        // GameObject.Find only finds active objects.
        var byName = GameObject.Find("TuloksetNappi");
        if (byName != null) {
            TuloksetNappiObj = byName;
            WireTuloksetButton();
            return;
        }

        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++) {
            var b = buttons[i];
            if (b == null) continue;
            var go = b.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

            if (b.name.Equals("TuloksetNappi", System.StringComparison.OrdinalIgnoreCase) ||
                b.name.IndexOf("tulokset", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                TuloksetNappiObj = go;
                WireTuloksetButton();
                return;
            }
        }
    }
    private void SetSalainenNappiVisible(bool v) { if (SalainenNappiObj) SalainenNappiObj.SetActive(v); }
    private void NoteDifficultyCompletedWin(Difficulty d) { if (d == Difficulty.Hard && salainenCompletionIndex == 0) salainenCompletionIndex = 1; else if (d == Difficulty.Easy && salainenCompletionIndex == 1) salainenCompletionIndex = 2; else if (d == Difficulty.Medium && salainenCompletionIndex == 2) { salainenCompletionIndex = 3; salainenUnlocked = true; SetSalainenNappiVisible(true); } else salainenCompletionIndex = 0; }
    //private void HideAllPalkintoButtons() { if (PalkintoNappi1Obj) PalkintoNappi1Obj.SetActive(false); if (PalkintoNappi2Obj) PalkintoNappi2Obj.SetActive(false); if (PalkintoNappi3Obj) PalkintoNappi3Obj.SetActive(false); }
    //private void UpdatePalkintoButtonsVisibility(bool v) { if (v) { if (PalkintoNappi1Obj) PalkintoNappi1Obj.SetActive(unlockedRewards.Contains(Difficulty.Hard)); if (PalkintoNappi2Obj) PalkintoNappi2Obj.SetActive(unlockedRewards.Contains(Difficulty.Medium)); if (PalkintoNappi3Obj) PalkintoNappi3Obj.SetActive(unlockedRewards.Contains(Difficulty.Easy)); } else HideAllPalkintoButtons(); }
    private void UnlockRewardForDifficulty(Difficulty d) { unlockedRewards.Add(d); }
    private bool AreAllRewardsUnlocked() { return unlockedRewards.Count >= 3; }
    private void ApplyPendingRewardResetIfNeeded() { if (resetRewardsPending) { resetRewardsPending = false; unlockedRewards.Clear(); HideAllPalkintoButtons(); } }
    private void SaveCompletedScoreForCurrentDifficulty() { if (currentDifficulty != Difficulty.None && scoreScript != null) completedScoreByDifficulty[currentDifficulty] = scoreScript.GetScore(); UpdateTuloksetButtonVisibility(); }
    private void SetSaitPalkinnonVisible(bool v) { if (saitPalkinnonTekstiObj) saitPalkinnonTekstiObj.SetActive(v); }
    private void SetSaitKaikkiPalkinnotVisible(bool v) { if (saitKaikkiPalkinnotTekstiObj) saitKaikkiPalkinnotTekstiObj.SetActive(v); }
    private void SetVoititVisible(bool v) { if (voititTekstiObj) voititTekstiObj.SetActive(v); }
    private void ClearPalkintoFromInfoPanel() { showPalkinto = false; if (palkintoImageInInfoPanel) palkintoImageInInfoPanel.gameObject.SetActive(false); }
    //private void ShowPalkintoInInfoPanel(Sprite s) { if (s) { showPalkinto = true; if (palkintoImageInInfoPanel) { palkintoImageInInfoPanel.sprite = s; palkintoImageInInfoPanel.gameObject.SetActive(true); } RefreshInfoPanel(); } }
    
    private void ShowPalkintoInInfoPanel(Sprite sprite)
    {
        //ResolvePalkintoRefsIfNeeded();
        EnsurePalkintoImageInInfoPanel();
        if (palkintoImageInInfoPanel == null)
        {
            Debug.LogWarning("GameManager: palkintoImageInInfoPanel is not assigned/found. Assign an Image inside the info panel to display rewards.");
            return;
        }

        if (sprite == null)
        {
            Debug.LogWarning("GameManager: Reward sprite is not assigned. Assign palkintoXSprite in the Inspector.");
            return;
        }

        // Reward view should take over the info panel.
        showOhjeet = false;
        matchedInfoText = "";
        showPalkinto = true;
        palkintoImageInInfoPanel.sprite = sprite;
        palkintoImageInInfoPanel.preserveAspect = true;
        palkintoImageInInfoPanel.gameObject.SetActive(true);

        ConfigurePalkintoImageLayout();

        // Make sure panel becomes visible.
        if (infoPopupPanel != null && !infoPopupPanel.activeInHierarchy)
            infoPopupPanel.SetActive(true);

        RefreshInfoPanel();
    }
    private void ConfigurePalkintoImageLayout()
    {
        if (palkintoImageInInfoPanel == null) return;
        if (infoPopupPanel == null) return;

        var panelRt = infoPopupPanel.transform as RectTransform;
        var imgRt = palkintoImageInInfoPanel.transform as RectTransform;
        if (panelRt == null || imgRt == null) return;

        // Center the image; do not stretch to the panel.
        imgRt.anchorMin = new Vector2(0.5f, 0.5f);
        imgRt.anchorMax = new Vector2(0.5f, 0.5f);
        imgRt.pivot = new Vector2(0.5f, 0.5f);
        imgRt.anchoredPosition = Vector2.zero;

        //if (palkintoUseNativeSize)
        //{
        //    // Sets sizeDelta based on the sprite pixel size.
            palkintoImageInInfoPanel.SetNativeSize();
        //}
        //else
        //{
        //    imgRt.sizeDelta = palkintoFallbackSize;
        //}

        // Fit inside the panel with padding; never scale up beyond native/fallback size.
        Vector2 size = imgRt.sizeDelta;
        //if (size.x <= 0f || size.y <= 0f)
        //    size = palkintoFallbackSize;

        float padding = Mathf.Max(0f, palkintoFitPadding);
        float maxW = Mathf.Max(1f, panelRt.rect.width - 2f * padding);
        float maxH = Mathf.Max(1f, panelRt.rect.height - 2f * padding);
        float scale = Mathf.Min(1f, maxW / size.x, maxH / size.y);
        imgRt.sizeDelta = size * scale;
    }
    private void EnsurePalkintoImageInInfoPanel()
    {
        if (palkintoImageInInfoPanel != null) return;
        if (infoPopupPanel == null) return;

        // Prefer an existing child image named appropriately.
        var childImages = infoPopupPanel.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < childImages.Length; i++)
        {
            var img = childImages[i];
            if (img == null) continue;

            if (string.Equals(img.name, "PalkintoKuva", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(img.name, "PalkintoImage", System.StringComparison.OrdinalIgnoreCase))
            {
                palkintoImageInInfoPanel = img;
                palkintoImageInInfoPanel.raycastTarget = false;
                return;
            }
        }

        // Otherwise create one.
        var go = new GameObject("PalkintoKuva", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(infoPopupPanel.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = palkintoFallbackSize;

        var imgComp = go.GetComponent<Image>();
        imgComp.raycastTarget = false;
        imgComp.preserveAspect = true;

        // Put it behind existing children (e.g., text), so reward text can still be shown if desired.
        go.transform.SetSiblingIndex(0);
        go.SetActive(false);

        palkintoImageInInfoPanel = imgComp;
    }
}

