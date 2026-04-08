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
        foreach (Button btn in btns)
        {
           Debug.Log("Add Listener Button: " + btn.name);
           btn.onClick.AddListener(() => PickPuzzle());
        }
    }

   public void PickPuzzle()
   {
       //string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.ToString();
       if(!firstGuess)
       {
           Debug.Log("First Guess: " + UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.ToString());
           firstGuess = true;
           firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.ToString());
           btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
       }
       else if(!secondGuess)
       {
           Debug.Log("Second Guess: " + UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.ToString());
           secondGuess = true;
           secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.ToString());
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


}
