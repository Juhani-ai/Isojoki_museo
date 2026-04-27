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

    [Header("Difficulty Scoring")]
    [SerializeField] private int easyPointsPerPair = 5;
    [SerializeField] private int mediumPointsPerPair = 10;
    [SerializeField] private int hardPointsPerPair = 20;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip pickCardClip;
    [SerializeField]
    private Sprite bgImage;

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

    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    private Coroutine firstRevealRoutine;
    private Sprite[] namePuzzles; // from EsineidenNimet


    public GameObject UudestaanNappi1;

    [Header("Difficulty buttons")]
    [SerializeField] private GameObject HelppoNappiObj;
    [SerializeField] private GameObject KeskiTasoNappiObj;
    [SerializeField] private GameObject VaikeaNappiObj;
    [SerializeField] private GameObject PoistuTasoihinNappiObj;

    [Header("Difficulty selection label")]
    [SerializeField] private GameObject TasonValintaTekstiObj;

    [Header("End-of-round buttons")]
    [SerializeField] private GameObject SeuraavaNappiObj;
    [SerializeField] private GameObject PaavalikkoNappiObj;

    [Header("Results")]
    [SerializeField] private GameObject TuloksetNappiObj;
    [Tooltip("How many different difficulties must be completed before TuloksetNappi becomes visible. Set to 0 to show after ANY 1 completed difficulty.")]
    [SerializeField] private int tuloksetShowWhenCompletedAtLeast = 0;
    [Header("Start menu")]
    [SerializeField] private GameObject AloitaPeliNappiObj;
    [Header("Info panel")]
    [SerializeField] private GameObject OhjeetNappiObj;
    [SerializeField] private MuistipeliScoreScript scoreScript;

    [Header("Rewards (Palkinnot)")]
    [Tooltip("Reward button shown after HARD is completed.")]
    [SerializeField] private GameObject PalkintoNappi1Obj;
    [Tooltip("Reward button shown after MEDIUM is completed.")]
    [SerializeField] private GameObject PalkintoNappi2Obj;
    [Tooltip("Reward button shown after EASY is completed.")]
    [SerializeField] private GameObject PalkintoNappi3Obj;

    [Tooltip("Sprite shown in the info panel when PalkintoNappi1 is pressed.")]
    [SerializeField] private Sprite palkinto1Sprite;
    [Tooltip("Sprite shown in the info panel when PalkintoNappi2 is pressed.")]
    [SerializeField] private Sprite palkinto2Sprite;
    [Tooltip("Sprite shown in the info panel when PalkintoNappi3 is pressed.")]
    [SerializeField] private Sprite palkinto3Sprite;
    [Tooltip("An Image component inside the info panel used to display the reward sprite.")]
    [SerializeField] private Image palkintoImageInInfoPanel;
    [Header("Scene Navigation")]
    [SerializeField] private string Päävalikko = "Päävalikko";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private readonly Dictionary<Difficulty, int> completedScoreByDifficulty = new();

    private bool showPalkinto;

    private void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("Kuvat/Esineet");
        namePuzzles = Resources.LoadAll<Sprite>("Kuvat/EsineidenNimet");

        ResolveOhjeetButtonRefIfNeeded();

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

        WireAloitaPeliButton();

        WireDifficultyButtons();

        WireTuloksetButton();
        UpdateTuloksetButtonVisibility();

        ResolvePalkintoRefsIfNeeded();
        WirePalkintoButtons();
        HideAllPalkintoButtons();
        ClearPalkintoFromInfoPanel();

        // Initial view: show start menu (Aloita + Ohjeet + Poistu), hide difficulty selection until Aloita.
        SetDifficultySelectionVisible(false);
        SetStartMenuVisible(true);
        SetMenuChromeVisible(true);

        // Optional convenience: if not set in Inspector, try to find a ParticleSystem under this GameObject.
        if (allPairsParticles == null)
        {
            allPairsParticles = GetComponentInChildren<ParticleSystem>(true);
        }
    }

    private void ResolvePalkintoRefsIfNeeded()
    {
        // Reward buttons: try by exact name first.
        if (PalkintoNappi1Obj == null) PalkintoNappi1Obj = GameObject.Find("PalkintoNappi1");
        if (PalkintoNappi2Obj == null) PalkintoNappi2Obj = GameObject.Find("PalkintoNappi2");
        if (PalkintoNappi3Obj == null) PalkintoNappi3Obj = GameObject.Find("PalkintoNappi3");

        // GameObject.Find only finds active objects; fallback to searching all Buttons in the scene.
        if (PalkintoNappi1Obj == null || PalkintoNappi2Obj == null || PalkintoNappi3Obj == null)
        {
            var buttons = Resources.FindObjectsOfTypeAll<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null) continue;
                if (!button.gameObject.scene.IsValid()) continue;

                if (PalkintoNappi1Obj == null && string.Equals(button.name, "PalkintoNappi1", System.StringComparison.OrdinalIgnoreCase))
                    PalkintoNappi1Obj = button.gameObject;
                else if (PalkintoNappi2Obj == null && string.Equals(button.name, "PalkintoNappi2", System.StringComparison.OrdinalIgnoreCase))
                    PalkintoNappi2Obj = button.gameObject;
                else if (PalkintoNappi3Obj == null && string.Equals(button.name, "PalkintoNappi3", System.StringComparison.OrdinalIgnoreCase))
                    PalkintoNappi3Obj = button.gameObject;

                if (PalkintoNappi1Obj != null && PalkintoNappi2Obj != null && PalkintoNappi3Obj != null)
                    break;
            }
        }

        // Reward image in info panel: optional auto-find by name.
        if (palkintoImageInInfoPanel == null)
        {
            var images = Resources.FindObjectsOfTypeAll<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i];
                if (img == null) continue;
                if (!img.gameObject.scene.IsValid()) continue;

                if (string.Equals(img.name, "PalkintoKuva", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(img.name, "PalkintoImage", System.StringComparison.OrdinalIgnoreCase))
                {
                    palkintoImageInInfoPanel = img;
                    break;
                }
            }
        }
    }

    private void WirePalkintoButtons()
    {
        ResolvePalkintoRefsIfNeeded();

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

    private void ShowPalkintoButtonForDifficulty(Difficulty difficulty)
    {
        // Only show one at a time.
        HideAllPalkintoButtons();

        switch (difficulty)
        {
            case Difficulty.Easy:
                if (PalkintoNappi3Obj != null) PalkintoNappi3Obj.SetActive(true);
                break;
            case Difficulty.Medium:
                if (PalkintoNappi2Obj != null) PalkintoNappi2Obj.SetActive(true);
                break;
            case Difficulty.Hard:
                if (PalkintoNappi1Obj != null) PalkintoNappi1Obj.SetActive(true);
                break;
            case Difficulty.None:
            default:
                break;
        }
    }

    private void ClearPalkintoFromInfoPanel()
    {
        showPalkinto = false;
        if (palkintoImageInInfoPanel != null)
        {
            palkintoImageInInfoPanel.sprite = null;
            palkintoImageInInfoPanel.gameObject.SetActive(false);
        }

        // Make sure the panel state is consistent immediately.
        RefreshInfoPanel();
    }

    private void ShowPalkintoInInfoPanel(Sprite sprite)
    {
        ResolvePalkintoRefsIfNeeded();
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

        // Make sure panel becomes visible.
        if (infoPopupPanel != null && !infoPopupPanel.activeInHierarchy)
            infoPopupPanel.SetActive(true);

        RefreshInfoPanel();
    }

    public void PalkintoNappi1()
    {
        HideAllPalkintoButtons();
        ShowPalkintoInInfoPanel(palkinto1Sprite);
    }

    public void PalkintoNappi2()
    {
        HideAllPalkintoButtons();
        ShowPalkintoInInfoPanel(palkinto2Sprite);
    }

    public void PalkintoNappi3()
    {
        HideAllPalkintoButtons();
        ShowPalkintoInInfoPanel(palkinto3Sprite);
    }

    private void ResolveTuloksetButtonRefIfNeeded()
    {
        if (TuloksetNappiObj != null) return;

        // NOTE: GameObject.Find() only finds ACTIVE objects, so we also have an inactive-capable fallback below.
        var byName = GameObject.Find("TuloksetNappi") ?? GameObject.Find("TulosteetNappi");
        if (byName != null)
        {
            TuloksetNappiObj = byName;
            return;
        }

        var buttons = Resources.FindObjectsOfTypeAll<Button>();

        // 1) Prefer exact-name matches (case-insensitive)
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null) continue;
            if (!button.gameObject.scene.IsValid()) continue;

            if (string.Equals(button.name, "TuloksetNappi", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(button.name, "TulosteetNappi", System.StringComparison.OrdinalIgnoreCase))
            {
                TuloksetNappiObj = button.gameObject;
                return;
            }
        }

        // 2) Then prefer more specific substrings
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null) continue;
            if (!button.gameObject.scene.IsValid()) continue;

            if (button.name.IndexOf("tulokset", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                button.name.IndexOf("tulosteet", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                TuloksetNappiObj = button.gameObject;
                return;
            }
        }

        // 3) Final fallback: any "tulos" substring
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null) continue;
            if (!button.gameObject.scene.IsValid()) continue;

            if (button.name.IndexOf("tulos", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                TuloksetNappiObj = button.gameObject;
                return;
            }
        }
    }

    private void WireTuloksetButton()
    {
        ResolveTuloksetButtonRefIfNeeded();
        if (TuloksetNappiObj == null) return;

        var btn = TuloksetNappiObj.GetComponent<Button>();
        if (btn == null) btn = TuloksetNappiObj.GetComponentInChildren<Button>(true);
        if (btn == null) return;

        TuloksetNappiObj = btn.gameObject;
        btn.onClick.RemoveListener(TuloksetNappi);
        btn.onClick.AddListener(TuloksetNappi);
    }

    private int GetTotalDifficultiesCount()
    {
        // Excluding None.
        return 3;
    }

    private int GetTuloksetRequiredCompletedCount()
    {
        int total = GetTotalDifficultiesCount();
        int required = tuloksetShowWhenCompletedAtLeast;
        if (required <= 0)
        {
            // Show after ANY 1 completed difficulty.
            required = 1;
        }

        return Mathf.Clamp(required, 1, total);
    }

    private bool ShouldShowTuloksetButton()
    {
        return completedScoreByDifficulty.Count >= GetTuloksetRequiredCompletedCount();
    }

    private void UpdateTuloksetButtonVisibility(bool forceHide = false)
    {
        ResolveTuloksetButtonRefIfNeeded();
        if (TuloksetNappiObj == null) return;

        bool show = !forceHide && ShouldShowTuloksetButton();
        TuloksetNappiObj.SetActive(show);

        // If the button is activeSelf but still not visible, it is likely under an inactive parent.
        if (show && !TuloksetNappiObj.activeInHierarchy)
        {
            Debug.LogWarning("GameManager: TuloksetNappi was enabled but is not visible (inactive parent in hierarchy). Move the button under an active Canvas/panel or assign TuloksetNappiObj to the correct object.");
        }
    }

    private void SaveCompletedScoreForCurrentDifficulty()
    {
        if (currentDifficulty == Difficulty.None) return;
        if (scoreScript == null) return;

        completedScoreByDifficulty[currentDifficulty] = Mathf.Max(0, scoreScript.GetScore());
        UpdateTuloksetButtonVisibility();
    }

    private void RemoveCompletedScoreForDifficulty(Difficulty difficulty)
    {
        if (difficulty == Difficulty.None) return;

        if (completedScoreByDifficulty.Remove(difficulty))
        {
            UpdateTuloksetButtonVisibility();
        }
    }

    private void ResolveOhjeetButtonRefIfNeeded()
    {
        if (OhjeetNappiObj != null) return;

        // Prefer an exact name match if possible.
        var byName = GameObject.Find("OhjeetNappi");
        if (byName != null)
        {
            OhjeetNappiObj = byName;
            return;
        }

        // Fallback: find a scene Button whose name contains "ohje" (works even if the object is inactive).
        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null) continue;
            if (!button.gameObject.scene.IsValid()) continue; // skip prefabs/assets

            if (button.name.IndexOf("ohje", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                OhjeetNappiObj = button.gameObject;
                return;
            }
        }

        Debug.LogWarning("GameManager: OhjeetNappiObj is not assigned and could not be auto-found. Assign the Ohjeet button GameObject in the Inspector so it can be hidden during gameplay.");
    }

    private void WireDifficultyButtons()
    {
        var easyBtn = HelppoNappiObj != null ? HelppoNappiObj.GetComponent<Button>() : null;
        if (easyBtn != null)
        {
            easyBtn.onClick.RemoveListener(StartEasy);
            easyBtn.onClick.AddListener(StartEasy);
        }

        var mediumBtn = KeskiTasoNappiObj != null ? KeskiTasoNappiObj.GetComponent<Button>() : null;
        if (mediumBtn != null)
        {
            mediumBtn.onClick.RemoveListener(StartMedium);
            mediumBtn.onClick.AddListener(StartMedium);
        }

        var hardBtn = VaikeaNappiObj != null ? VaikeaNappiObj.GetComponent<Button>() : null;
        if (hardBtn != null)
        {
            hardBtn.onClick.RemoveListener(StartHard);
            hardBtn.onClick.AddListener(StartHard);
        }

        var backBtn = PoistuTasoihinNappiObj != null ? PoistuTasoihinNappiObj.GetComponent<Button>() : null;
        if (backBtn != null)
        {
            backBtn.onClick.RemoveListener(PoistuTasoihinNappi);
            backBtn.onClick.AddListener(PoistuTasoihinNappi);
        }
    }

    private void SetDifficultySelectionVisible(bool visible)
    {
        if (HelppoNappiObj != null) HelppoNappiObj.SetActive(visible);
        if (KeskiTasoNappiObj != null) KeskiTasoNappiObj.SetActive(visible);
        if (VaikeaNappiObj != null) VaikeaNappiObj.SetActive(visible);

        if (TasonValintaTekstiObj != null) TasonValintaTekstiObj.SetActive(visible);

        // This button is shown at end-of-round (win/timeout), not while playing.
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);

        // Show results button only after enough difficulties are completed.
        if (TuloksetNappiObj != null)
        {
            TuloksetNappiObj.SetActive(visible && ShouldShowTuloksetButton());
        }
    }

    private void SetStartMenuVisible(bool visible)
    {
        if (AloitaPeliNappiObj != null) AloitaPeliNappiObj.SetActive(visible);
    }

    private void SetMenuChromeVisible(bool visible)
    {
        ResolveOhjeetButtonRefIfNeeded();
        if (OhjeetNappiObj != null) OhjeetNappiObj.SetActive(visible);

        // "PoistuNappi" in UI is the same as the main-menu button we use at end-of-round.
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(visible);
    }

    public void PoistuTasoihinNappi()
    {
        // Stop the current run and let the player choose a new difficulty.
        timeScript?.StopTimer();
        showOhjeet = false;
        ClearMatchedInfo();
        HideAllPalkintoButtons();
        ClearPalkintoFromInfoPanel();
        SetEndButtonsVisible(false);
        roundFinished = false;
        currentDifficulty = Difficulty.None;

        // Freeze the board while choosing difficulty again.
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

        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(true);
        SetMenuChromeVisible(true);

        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);
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

    private void WireAloitaPeliButton()
    {
        var startBtn = AloitaPeliNappiObj != null ? AloitaPeliNappiObj.GetComponent<Button>() : null;
        if (startBtn == null && AloitaPeliNappiObj != null)
            startBtn = AloitaPeliNappiObj.GetComponentInChildren<Button>(true);

        if (startBtn == null) return;

        int persistentCount = startBtn.onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentCount; i++)
        {
            var target = startBtn.onClick.GetPersistentTarget(i);
            var method = startBtn.onClick.GetPersistentMethodName(i);
            if (target == (UnityEngine.Object)this && method == nameof(AloitaPeliNappi))
            {
                return;
            }
        }

        startBtn.onClick.RemoveListener(AloitaPeliNappi);
        startBtn.onClick.AddListener(AloitaPeliNappi);
    }

    public void AloitaPeliNappi()
    {
        // Show difficulty selection after player chooses to start.
        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(true);
        SetMenuChromeVisible(true);
        showOhjeet = false;
        RefreshInfoPanel();
    }

    private void WireOhjeetButton()
    {
        ResolveOhjeetButtonRefIfNeeded();
        if (OhjeetNappiObj == null) return;

        // The serialized reference may point to a parent container OR a child (e.g., the label Text).
        // Try self, children, then parents.
        var ohjeBtn = OhjeetNappiObj.GetComponent<Button>();
        if (ohjeBtn == null) ohjeBtn = OhjeetNappiObj.GetComponentInChildren<Button>(true);
        if (ohjeBtn == null) ohjeBtn = OhjeetNappiObj.GetComponentInParent<Button>(true);

        if (ohjeBtn == null)
        {
            Debug.LogWarning("GameManager: OhjeetNappiObj does not resolve to a UI Button (self/children/parents). Assign the actual Button GameObject in the Inspector.");
            return;
        }

        // Normalize so future SetActive() hits the actual clickable button.
        OhjeetNappiObj = ohjeBtn.gameObject;

        // If the button is already wired in the Inspector (persistent event), don't add a duplicate
        // runtime listener. Duplicate listeners would toggle showOhjeet twice and effectively do nothing.
        int persistentCount = ohjeBtn.onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentCount; i++)
        {
            var target = ohjeBtn.onClick.GetPersistentTarget(i);
            var method = ohjeBtn.onClick.GetPersistentMethodName(i);
            if (target == (UnityEngine.Object)this && method == nameof(OhjeetNappi))
            {
                return;
            }
        }

        ohjeBtn.onClick.RemoveListener(OhjeetNappi);
        ohjeBtn.onClick.AddListener(OhjeetNappi);
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

        if (TuloksetNappiObj != null)
        {
            TuloksetNappiObj.SetActive(visible && ShouldShowTuloksetButton());
        }
    }

    private void SetEndButtonsVisibleOnTimeout()
    {
        if (UudestaanNappi1 != null) UudestaanNappi1.SetActive(true);
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(true);

        // "Seuraava" only makes sense if there is a next difficulty.
        bool hasNext = currentDifficulty == Difficulty.Easy || currentDifficulty == Difficulty.Medium;
        if (SeuraavaNappiObj != null) SeuraavaNappiObj.SetActive(hasNext);

        // Timeout is not “completed”, so keep results button hidden.
        if (TuloksetNappiObj != null) TuloksetNappiObj.SetActive(false);
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
        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(false);
        SetMenuChromeVisible(false);
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);
        currentDifficulty = Difficulty.Easy;
        StartGame(2, 2);
        timeScript?.StartTimer(easySeconds);
    }

    public void StartMedium()
    {
        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(false);
        SetMenuChromeVisible(false);
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);
        currentDifficulty = Difficulty.Medium;
        StartGame(4, 4);
        timeScript?.StartTimer(mediumSeconds);
    }

    public void StartHard()
    {
        SetStartMenuVisible(false);
        SetDifficultySelectionVisible(false);
        SetMenuChromeVisible(false);
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(false);
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

        // Gameplay active: hide instructions button. It will reappear only when returning to difficulties.
        ResolveOhjeetButtonRefIfNeeded();
        if (OhjeetNappiObj != null) OhjeetNappiObj.SetActive(false);

        // Hide results while actively playing.
        UpdateTuloksetButtonVisibility(forceHide: true);

        // Also hide the start/menu chrome during active gameplay.
        if (AloitaPeliNappiObj != null) AloitaPeliNappiObj.SetActive(false);
        if (PaavalikkoNappiObj != null) PaavalikkoNappiObj.SetActive(false);

        // Reset state
        firstGuess = secondGuess = false;
        countCorrectGuesses = 0;
        roundFinished = false;
        SetEndButtonsVisible(false);

        // Starting a new difficulty should return the info panel to normal (pair list) mode.
        showOhjeet = false;
        ClearMatchedInfo();
        HideAllPalkintoButtons();
        ClearPalkintoFromInfoPanel();

        // Configure scoring per difficulty (easy/medium vs hard).
        if (scoreScript != null)
        {
            int ppp = currentDifficulty switch
            {
                Difficulty.Easy => easyPointsPerPair,
                Difficulty.Medium => mediumPointsPerPair,
                Difficulty.Hard => hardPointsPerPair,
                _ => mediumPointsPerPair
            };
            scoreScript.SetPointsPerPair(ppp);
        }

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

        // No rewards on timeout.
        HideAllPalkintoButtons();
        ClearPalkintoFromInfoPanel();

        // Also show "back to difficulties".
        if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(true);
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

        if (Debug.isDebugBuild)
        {
            Debug.Log($"GameManager: OhjeetNappi pressed -> showOhjeet={showOhjeet}, " +
                      $"infoPopupPanel={(infoPopupPanel != null ? infoPopupPanel.name : "<null>")}, " +
                      $"infoPopupText={(infoPopupText != null ? infoPopupText.name : "<null>")}");
        }

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

            // Store this difficulty's score for the combined total.
            SaveCompletedScoreForCurrentDifficulty();

            if (allPairsParticles != null)
            {
                if (allPairsParticlesAnchor != null)
                    allPairsParticles.transform.position = allPairsParticlesAnchor.position;

                if (!allPairsParticles.gameObject.activeInHierarchy)
                    allPairsParticles.gameObject.SetActive(true);

                if (stopParticlesBeforePlay) allPairsParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                allPairsParticles.Play(true);
            }
            else
            {
                Debug.LogWarning("GameManager: allPairsParticles is not assigned/found, win effect won't play.");
            }

            SetEndButtonsVisible(true);

            // Show reward button for this completed difficulty.
            ShowPalkintoButtonForDifficulty(currentDifficulty);

            if (PoistuTasoihinNappiObj != null) PoistuTasoihinNappiObj.SetActive(true);

            // Let the player briefly see the full stacked list, then clear.
            ScheduleClearInfoPopup(clearInfoOnWinDelaySeconds);

            Debug.Log("Final score: " + scoreScript.GetScore() + ", pairs found: " + scoreScript.GetPairsFound());
            scoreScript.OnGameFinished();
            timeScript?.StopTimer();
            //}
        }
    }

    // Hook this to TuloksetNappi (or we auto-wire by name).
    public void TuloksetNappi()
    {
        int total = 0;
        foreach (var kvp in completedScoreByDifficulty)
        {
            total += Mathf.Max(0, kvp.Value);
        }

        if (scoreScript != null)
        {
            scoreScript.ShowCombinedTotal(total);
            return;
        }

        // Fallback: show in the info popup.
        matchedInfoText = $"Kokonaispisteet (kaikki tasot): {total}";
        showOhjeet = false;
        RefreshInfoPanel();
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

        // Replaying a difficulty should remove its previous completed score from the combined results.
        RemoveCompletedScoreForDifficulty(currentDifficulty);

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