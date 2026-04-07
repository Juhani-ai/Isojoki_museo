using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private Sprite bgImage;

    public Sprite[] puzzles;

    public List<Sprite> gamePuzzles = new List<Sprite>();

    public List<Button> btns = new List<Button>();

    private bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

private void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("Kuvat/Esineet");
    }

    void Start()
    {
        GetButtons();
        AddListeners();
        AddGamePuzzles();
    }

   void GetButtons()
    {
    GameObject[] objects = GameObject.FindGameObjectsWithTag("puzzle8tn");
    for (int i = 0; i < objects.Length; i++)
        {
        btns.Add(objects[i].GetComponent<Button>());
        btns[i].image.sprite = bgImage;
        }
    }

    void AddGamePuzzles()
    {
      int looper = btns.Count;
      int index = 0;
      for (int i = 0; i < looper; i++)
      {
          if(index == looper/2)
          {
              index = 0;
          }
          gamePuzzles.Add(puzzles[index]);
          index++;
      }
    }

    void AddListeners()
    {
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(() => PickPuzzle());
        }
    }

   public void PickPuzzle()
   {
       // string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
       if(!firstGuess)
       {
          firstGuess = true;
          firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
          btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
       }
       else if(!secondGuess)
       {
           secondGuess = true;
           secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
           btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];
       }
   }


}
