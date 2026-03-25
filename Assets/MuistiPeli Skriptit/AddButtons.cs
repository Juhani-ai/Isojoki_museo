using UnityEngine;

public class AddButtons : MonoBehaviour
{
    [Serializefield]
    private transform puzzleField
    
    [Serializefield]
    private GameObject button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i <8; i++)
         {
           GameObject _button = Instantiate(button);
           _button.name = "" + i;
           _button.transform.SetParent(puzzleField, false);
         }
    }


    // Update is called once per frame
    void Update()
    {
        

    }
}
