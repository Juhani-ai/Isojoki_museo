using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public GameObject UudestaanNappi;
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
              firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;
           btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
           return;
       }

       // Prevent selecting same card twice
       if (index == firstGuessIndex) return;

       if (!secondGuess)
       {
           secondGuess = true;
           secondGuessIndex = index;
           secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;
           btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];
        
        Debug.Log("First guess: " + firstGuessPuzzle + ", Second guess: " + secondGuessPuzzle);
        if(firstGuessPuzzle == secondGuessPuzzle)
        {
            print("puzzles match");
        }
        else
        {
            print("puzzles don't match");
        }

        StartCoroutine(CheckThePuzzleMatch());  
       }
   }
   IEnumerator CheckThePuzzleMatch()
   {
      yield return new WaitForSeconds(0.5f);   
      if(firstGuessPuzzle == secondGuessPuzzle)
      {
        btns[firstGuessIndex].interactable = false;
        btns[secondGuessIndex].interactable = false;
      
      btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
      btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);

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
            print("Game Finished");
            UudestaanNappi.SetActive(true);

            print("it took you " + countGuesses + " ");
        }
   }

public void SeuraavaNappi()
    {
        print("Seuraava ");
    }
    
  

public void UudestaanNappi()
{
  print("Uudestaan ");
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

