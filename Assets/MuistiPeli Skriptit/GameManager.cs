using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public List<Button> btns = new List<Button>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetButtons();
    }

   void GetButtons()
    {
        GameObject[] objects = GameObject.findGameObjectsWithTag("puzzle8tn");
        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<GetButtons>());
        }
    }
   
}
