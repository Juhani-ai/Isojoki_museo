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

    public bool firstGuess, secondGuess;

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
        firstGuess = false;
        secondGuess = false;

        GetButtons();
        AddListeners();
        AddGamePuzzles();
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
      Debug.Log("Game Puzzles Count: " + gamePuzzles.Count);
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

   public void PickPuzzle(int index)
   {
       // Ignore input if second card already picked (until you resolve match/mismatch)
       if (secondGuess) return;

       if (!firstGuess)
       {
           firstGuess = true;
           firstGuessIndex = index;
           btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
           return;
       }

       // Prevent selecting same card twice
       if (index == firstGuessIndex) return;

       if (!secondGuess)
       {
           secondGuess = true;
           secondGuessIndex = index;
           btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];
      
         if(firstGuessPuzzle == secondGuessPuzzle)
        {
            print("puzzles match");
        }
        else
        {
            print("puzzles don't match");
        }
       }
   }
 
   // POISTA MYÖHEMMIN!!!!!!!!!!!!!!!
   //IEnumerator CheckThePuzzleMatch()
  // {
   //    btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
    //   btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
  // }
}
