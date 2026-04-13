using UnityEngine;

public class MuistipeliScoreScript : MonoBehaviour
{
    private int countGuesses;

    
   public void HandleScore()
   {
       countGuesses++;
       Debug.Log("Guesses: " + countGuesses);

   }

   public int GetGuesses()
   {
       return countGuesses;
   }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
