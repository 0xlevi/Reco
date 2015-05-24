using UnityEngine; 
using System;
using System.Threading;
using System.IO;
using System.Collections; 
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using OpenCvSharp;

public class DrawLine : MonoBehaviour 
{
	private bool isMousePressed = false;
	const int HASH_SIZE = 11*11; //size of hash for thumbnailed images
	const int THUMBNAIL_SIZE = 11; //width and height for thumbnils
	private List<bool[]> taskHashes = new List<bool[]> ();  //hashes for tasks

	//vars for drawing the line
	public GameObject lineDrawPrefabs; // this is where we put the prefabs object
	private GameObject lineDrawPrefab;
	private LineRenderer lineRenderer;
	private List<Vector3> drawPoints = new List<Vector3>();
	
	void Start () 
	{
		GameEnv.LoadTasks ();	//load all the tasks

		//generate hashes for all tasks
		foreach (var image in GameEnv.tasksIpl) 
		{
			taskHashes.Add(computeHash(image));
		}
	}
	// Update is called once per frame
	void Update () 
	{
		if (GameEnv.drawing) 
		{
			if (Input.GetMouseButtonDown (0))  // left mouse down, make a new line renderer
			{
				ClearDrawed ();

				isMousePressed = true;
				lineDrawPrefab = GameObject.Instantiate (lineDrawPrefabs) as GameObject;
				lineDrawPrefab.tag = "LineDraw";
				lineRenderer = lineDrawPrefab.GetComponent<LineRenderer> ();
				lineRenderer.SetVertexCount (0);
			} 
			else if (Input.GetMouseButtonUp (0))  // left mouse up, stop drawing
			{
				isMousePressed = false;

				if (drawPoints.Count > 0) 
				{
					StartCoroutine (SaveScreenshot ());
				}

			}
		
			if (isMousePressed) 
			{
				// when the left mouse button pressed
				// continue to add vertex to line renderer
				Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				mousePos.z = 0;

				if (!drawPoints.Contains (mousePos)) {
					drawPoints.Add (mousePos);
					lineRenderer.SetVertexCount (drawPoints.Count);

				}
				lineRenderer.SetPosition (drawPoints.Count - 1, mousePos);
			}
		} 
		else if (drawPoints.Count > 0) ClearDrawed ();
	}

	void ClearDrawed ()
	{
		drawPoints = new List<Vector3> ();
		GameObject[] delete = GameObject.FindGameObjectsWithTag ("LineDraw");
		int deleteCount = delete.Length;
		for (int i = deleteCount - 1; i > 0; i--)
			Destroy (delete [i]);
	}

	private IEnumerator SaveScreenshot()
	{
		yield return new WaitForSeconds (0.1f);

		Debug.Log ("Saving screenshot...");

		//find borders of drawed image and cut it
		float[] borders = getBorders ();
		Vector2 worldFromPoint = new Vector2(borders[0], borders[1]);
		Vector2 worldToPoint = new Vector2(borders[2], borders[3]);

		Vector2 screenFromPoint = Camera.main.WorldToScreenPoint (worldFromPoint);
		Vector2 screenToPoint = Camera.main.WorldToScreenPoint (worldToPoint);

		int width = (int)(screenToPoint.x - screenFromPoint.x);
		int height = (int)(screenToPoint.y - screenFromPoint.y);

		if (width > 50 && height > 50) 
		{
			Debug.Log ("Making texture");
			Texture2D tex = new Texture2D (width, height, TextureFormat.RGB24, true);
			tex.ReadPixels (new Rect (screenFromPoint.x, screenFromPoint.y, width, height), 0, 0);
			tex.Apply ();
			IplImage drawed = Texture2DtoIplImage (tex);

			Debug.Log ("Detecting");
			compare (drawed);
		}
		yield return new WaitForEndOfFrame ();
	}

	private void compare(IplImage image0) //compare drawed image with task's hashes
	{
		bool[] hash0 = computeHash (image0);
		Debug.Log (">>> hash of drawed image = " + hash0 + "<<<");

		short idealHammingDistance = 100;
		int idealHammingDistaneIndex = 0;
		for (int i = 0; i < taskHashes.Count; i++) 
		{
			short hammingDistance = computeHammingDistance (hash0, taskHashes[i]);
			if (hammingDistance < idealHammingDistance)
			{
				idealHammingDistance = hammingDistance;
				idealHammingDistaneIndex = i;
			}
		}

		Debug.Log(string.Format("!Picture Hamming Distance = {0}", idealHammingDistance));
		Debug.Log(string.Format("!Example number = {0}", GameEnv.taskNumber));
		Debug.Log(string.Format("!Founded number = {0}", idealHammingDistaneIndex));

		//if Hamming distance is ok and user have drawn right image then evr ok
		if (idealHammingDistance < GameEnv.accuracy) 
		{
			if (idealHammingDistaneIndex == GameEnv.taskNumber) 
			{
				GameEnv.hammingDistance = idealHammingDistance;
				GameEnv.levelTimeRamaining = 0;
			}
		} 
		else 
		{
			GameEnv.hammingDistance = -1;
		}
	}

	private bool[] computeHash(IplImage image)
	{	
		if (image == null)
			return null;

		//creating thumbnail:
		// 1) resize image
		// 2) make it black&white
		// 3) binarize image (only black or white pixels)

		IplImage res = Cv.CreateImage (Cv.Size (THUMBNAIL_SIZE, THUMBNAIL_SIZE), image.Depth, image.NChannels);
		IplImage gray = Cv.CreateImage (Cv.Size (THUMBNAIL_SIZE, THUMBNAIL_SIZE), BitDepth.U8, 1);
		IplImage bin = Cv.CreateImage (Cv.Size (THUMBNAIL_SIZE, THUMBNAIL_SIZE), BitDepth.U8, 1);

		Cv.Resize (image, res);
		Cv.CvtColor (res, gray, ColorConversion.BgrToGray);
		CvScalar average = Cv.Avg (gray); //average gray shade
		Cv.Threshold (gray, bin, average [0], 255, ThresholdType.Binary);

		// show images (for debug; must be included OpenCvSharp.UserInterface)
		/*IplImage dst3 = Cv.CreateImage( Cv.Size(128, 128), BitDepth.U8, 3);
		IplImage dst1 = Cv.CreateImage( Cv.Size(128, 128), BitDepth.U8, 1);
		Cv.NamedWindow( "screenshot" + average);
		Cv.ShowImage( "screenshot" + average, image );
		Cv.Resize(res, dst3, Interpolation.NearestNeighbor);

		Cv.NamedWindow( "bin" + average);
		Cv.Resize(bin, dst1, Interpolation.NearestNeighbor);
		Cv.ShowImage( "bin" + average, dst1 );
		
		Cv.ReleaseImage(dst3);
		Cv.ReleaseImage(dst1);*/

		bool[] hash = new bool[HASH_SIZE];
		byte[] ptr = new byte[HASH_SIZE]; //Image data
		Marshal.Copy (bin.ImageData, ptr, 0, HASH_SIZE);
		int i = 0;

		for (int y = 0; y < bin.Height; y++)
		{
			for (int x = 0; x < bin.Width; x++) 
			{
				hash[i] = (ptr[(y+1)*x] < average);
				i++;
			}
		}

		Cv.ReleaseImage (image);
		Cv.ReleaseImage (res);
		Cv.ReleaseImage (gray);
		Cv.ReleaseImage (bin);

		return hash;
	}

	private short computeHammingDistance(bool[] hash0, bool[] hash1)
	{
		short dist = 0;

		for (int i = 0; i < hash0.Length; i++) {
			if (hash0[i] != hash1[i]) {
				dist++;
			}
		}

		Debug.Log("HAMMING DISTANCE = " + dist);
		return dist;
	}

	private IplImage Texture2DtoIplImage (Texture2D texture)
	{		
		int j = texture.height;
		IplImage output = new IplImage (Cv.Size (texture.width, texture.height), BitDepth.U8, 3);

		for (int v = 0; v < texture.height; ++v) {
			for (int u = 0; u < texture.width; ++u) {
				
				CvScalar col = new CvScalar ();
				col.Val0 = (double)texture.GetPixel (u, v).b * 255;
				col.Val1 = (double)texture.GetPixel (u, v).g * 255;
				col.Val2 = (double)texture.GetPixel (u, v).r * 255;
				
				j = texture.height - v - 1;
				
				output.Set2D (j, u, col);
			}
		}
		return output;
		//Cv.SaveImage ("C:\\Hasan.jpg", matrix);
	}


	private float[] getBorders()
	{
		//0 - min x
		//1 - min y
		//2 - max x
		//3 - max y
		float[] results = new float[4] {drawPoints [0].x,
										drawPoints [0].y,
										drawPoints [0].x,
										drawPoints [0].y};
			
		for (int i = 0; i < drawPoints.Count; i++) 
		{
			//min x
			if (drawPoints[i].x < results[0]) results[0] = drawPoints[i].x;
			//min y
			if (drawPoints[i].y < results[1]) results[1] = drawPoints[i].y;
			//max x
			if (drawPoints[i].x > results[2]) results[2] = drawPoints[i].x;
			//max y
			if (drawPoints[i].y > results[3]) results[3] = drawPoints[i].y;
		}
		return results;
	}
}