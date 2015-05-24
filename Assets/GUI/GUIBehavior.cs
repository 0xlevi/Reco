using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIBehavior : MonoBehaviour 
{
	public Button StartButton;
	public Button NextLvlButton;
	public Button RestartButton;
	// Use this for initialization
	public void Start () 
	{
		StartButton = StartButton.GetComponent<Button> ();
		NextLvlButton = NextLvlButton.GetComponent<Button> ();
		RestartButton = RestartButton.GetComponent<Button> ();

		NextLvlButton.gameObject.SetActive (false);
		RestartButton.gameObject.SetActive (false);
	}

	public void StartLevel()
	{
		StartButton.gameObject.SetActive (false);
		GameEnv.SetNewTime ();
		GameEnv.NextLevel ();
		GameEnv.playing = true;
	}

	public void NextLvl()
	{
		NextLvlButton.gameObject.SetActive (false);
		RestartButton.gameObject.SetActive (false);
		GameEnv.playing = true;
		GameEnv.NextLevel();
	}
}
