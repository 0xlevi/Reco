using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Timers;

public class GameLogic : MonoBehaviour 
{
	public Button NextLvlButton;
	public Button RestartButton;

	private GUIStyle taskStyle;
	private GUIStyle infoStyle;

	public Font pixelFont;

	public void Start()
	{
		NextLvlButton = NextLvlButton.GetComponent<Button> ();
		RestartButton = RestartButton.GetComponent<Button> ();
		NextLvlButton.gameObject.SetActive (false);
		RestartButton.gameObject.SetActive (false);

		taskStyle = new GUIStyle ();
		taskStyle.alignment = TextAnchor.MiddleCenter;
		taskStyle.fontSize = 28;
		taskStyle.wordWrap = false;
		taskStyle.normal.textColor = Color.white;
		taskStyle.font = pixelFont;

		infoStyle = new GUIStyle ();
		infoStyle.alignment = TextAnchor.MiddleLeft;
		infoStyle.fontSize = 18;
		infoStyle.wordWrap = false;
		infoStyle.normal.textColor = new Color (0.75f, 0.75f, 0.85f);
		infoStyle.font = pixelFont;
	}

	// Update is called once per frame
	public void Update () 
	{
		if (GameEnv.playing)
		{
			if (GameEnv.taskTimeRamaining > 0) 
			{
				GameEnv.taskTimeRamaining -= Time.deltaTime;
			}
			else	
			{
				GameEnv.levelTimeRamaining -= Time.deltaTime;
			}

		}

		if (GameEnv.levelTimeRamaining <= 0) {
			GameEnv.playing = false;			
		}
	}

	public void OnGUI()
	{
		if (GameEnv.playing) 
		{
			if (GameEnv.levelTimeRamaining > 0)
			{
				GUI.Label(new Rect(10 , 35, 100, 100), "Осталось " + GameEnv.levelTimeRamaining.ToString () + " сек", infoStyle);
				GUI.Label(new Rect(10 , 5, 100, 100), "Уровень " + GameEnv.level, infoStyle);
				//Timer.text = "Осталось " + GameEnv.levelTimeRamaining.ToString () + " сек";
			} 
			else 
			{
				GUI.Label(new Rect(10 , 120, 100, 100), "Время вышло", infoStyle);
				GameEnv.playing = false;
				GameEnv.drawing = false;
			}

			if (GameEnv.taskTimeRamaining > 0) 
			{
				GUI.Label(new Rect(Screen.width / 2 - 50 , Screen.height / 2 - 140, 100, 100), "Нарисуйте такое", taskStyle);
				GUI.DrawTexture(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 100), GameEnv.examplesT2D[GameEnv.taskNumber], ScaleMode.StretchToFill);
				GameEnv.drawing = false;
			}
			else 
			{
				GameEnv.drawing = true;
			}
		}
		else if (GameEnv.drawing == true)
		{
			GameEnv.drawing = false;
			if (GameEnv.taskTimeRamaining <= 0) 
			{
				if (GameEnv.hammingDistance >= 0 && GameEnv.hammingDistance <= GameEnv.accuracy)
				{
					NextLvlButton.gameObject.SetActive(true);
				}
				else if (GameEnv.hammingDistance > GameEnv.accuracy || GameEnv.hammingDistance == -1)
				{
					RestartButton.gameObject.SetActive(true);
					GUI.Label(new Rect(Screen.width / 2 - 50 , Screen.height / 2 - 200, 600, 300), "Пройдено уровней: " + GameEnv.level, taskStyle);
				}
			}
		}
	}
}
