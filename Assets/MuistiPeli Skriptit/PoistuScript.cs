using UnityEngine;

public class PoistuScript : MonoBehaviour
{
	public void QuitGame()
	{
		// In a built player, this closes the application.
		Application.Quit();

		// In the Unity Editor, Application.Quit() does nothing, so stop Play Mode.
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}