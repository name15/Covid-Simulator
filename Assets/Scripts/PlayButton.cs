using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
	public void PlayGame ()
	{
		SceneManager.LoadScene("Simulator");
	}
		
	public void QuitGame()
	{
		Debug.Log("Application.Quit");
		Application.Quit();
	}
}
