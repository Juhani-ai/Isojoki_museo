using UnityEngine;

public class AddButtons : MonoBehaviour
{
    [SerializeField]
    private Transform puzzleField;
    
    [SerializeField]
    private GameObject button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void awake()
    {
        for(int i = 0; i <8; i++)
         {
           GameObject _button = Instantiate(button);
           _button.name = "" + i;
           _button.transform.SetParent(puzzleField, false);
         }
    }
       
       void Start()
       {

       }


    // Update is called once per frame
    void Update()
    {
        

    }
}
