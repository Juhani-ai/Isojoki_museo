using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Kysymykset
{
    public Texture image;

    public string[] answers = new string[4];
    public int correctAnswerIndex;

    [TextArea]
    public string hint;

    [TextArea]
    public string infoText;
}
