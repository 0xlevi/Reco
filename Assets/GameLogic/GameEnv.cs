using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

public class GameEnv : MonoBehaviour 
{
	public static int level = 0;
	public static bool playing = false;
	public static bool drawing = false;
	public static int accuracy = 21; //the maximum allowable Hamming distance
	private static float timeStart = 50; //the value of which is counted the time for the levels
	public static float levelTimeRamaining;
	public static float taskTimeRamaining;
	public static List<long> hammingDistances; //list of Hamming distances for tasks
	public static int hammingDistance; //current Hamming distance
	public static List<Texture2D> examplesT2D = new List<Texture2D>(); //a list of images that are used to demonstrate a task (Texture2D for Unity)
	public static List<IplImage> tasksIpl = new List<IplImage>(); //a list of images that are used to determine whether a user have drawn (IplImage for OpenCV)
	private static int lastTaskNumber = -1000; //because of this value tasks are not repeated
	public static int taskNumber;


	public static void LoadTasks()
	{
		var exampleFiles = Directory.GetFiles (Application.dataPath + "/Tasks/Examples", "*.png");
		var taskFiles = Directory.GetFiles (Application.dataPath + "/Tasks/Tasks", "*.png");

		foreach (var item in taskFiles) 
		{
			examplesT2D.Add(LoadPNG(item));
		}
		foreach (var item in exampleFiles) 
		{
			tasksIpl.Add(Cv.LoadImage(item));
		}
	}

	public static Texture2D LoadPNG(string filePath)
	{		
		Texture2D tex = null;
		byte[] fileData;
		
		if (File.Exists(filePath))     
		{
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}

	public static void SetNewTime()
	{
		levelTimeRamaining = timeStart / (float)(level);
		taskTimeRamaining = 3;
	}

	public static void NextLevel()
	{
		if (hammingDistance == -1 || hammingDistance > accuracy)
			level = 0;

		level++;
		hammingDistance = -1;
		Random.seed = System.DateTime.Now.Minute + System.DateTime.Now.Second;

		do 
		{
			taskNumber = Random.Range (0, tasksIpl.Count);
		} while(taskNumber == lastTaskNumber);

		lastTaskNumber = taskNumber;
		SetNewTime ();
	}
}
