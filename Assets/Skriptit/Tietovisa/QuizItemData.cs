using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz Item", menuName = "Quiz/Item")]
public class QuizItemData : ScriptableObject
{
    public string itemName;
    public string unlockID;
    public Texture image;

    [TextArea]
    public string info;

    [TextArea]
    public string hint;
}
