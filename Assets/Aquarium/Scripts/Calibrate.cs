using UnityEngine;
using System.Collections;

public class Calibrate : MonoBehaviour {

	public static Vector2 topLeft;
	public static Vector2 bottomRight;

	public static float minDepth;
	public static float maxDepth;
	private float z;
	private float x;
	private float y;

	private bool gettingZ;
	private float nextActionTime;
	private int points = 0;

	private bool gettingTopLeft = false;
	private bool waitingOnLeft = false;
	private bool gettingBottomRight = false;
	private bool waitingOnRight = false;

	private bool initialState = false;
	private bool finishedCalibration = false;
	
	// Use this for initialization
	void Start () {
		Debug.Log ("Start Calibration");
		getNonPushedZ ();
	}
	
	// Update is called once per frame
	void Update () {
		if (gettingZ) {
			if (Time.time > nextActionTime && points < 5) {
				if (!(DepthSourceViewAquarium.maxDepth == 450)) {
					points += 1;
					nextActionTime = nextActionTime + 1;
					z += DepthSourceViewAquarium.maxDepth;
				}
				Debug.Log (DepthSourceViewAquarium.maxDepth);
			} else if (points == 5) {
				//Debug.Log (z / 5);
				gettingZ = false;
				minDepth = z / 5;

				waitingOnLeft = true;
				points = 0;
							
				Debug.Log ("FINISHED Z: " + minDepth);
			}
		} else if (gettingTopLeft && waitingOnLeft) {
			if (Time.time > nextActionTime && points < 5) {
				points += 1;
				nextActionTime = nextActionTime + 1;
				z += DepthSourceViewAquarium.maxDepth;
				x += DepthSourceViewAquarium.maxDepthCoor.x;
				y += DepthSourceViewAquarium.maxDepthCoor.y;
				Debug.Log (DepthSourceViewAquarium.maxDepthCoor.x + " , " + DepthSourceViewAquarium.maxDepthCoor.y);
				Debug.Log (DepthSourceViewAquarium.maxDepth);

			} else if (points == 5) {
				//Debug.Log (z / 5);
				//	Debug.Log (x / 5);
				//	Debug.Log (y / 5);
				gettingTopLeft = false;
				waitingOnLeft = false;

				topLeft.x = x / 5;
				topLeft.y = y / 5;

				x = 0;
				y = 0;

				waitingOnRight = true;
				initialState = false;
				points = 0;
				getPushedTopLeftZ ();
				Debug.Log ("FINISHED TOP LEFT" + topLeft);
			}
		} else if (gettingBottomRight && waitingOnRight) {
			if (Time.time > nextActionTime && points < 5) {
				points += 1;
				nextActionTime = nextActionTime + 1;
				z += DepthSourceViewAquarium.maxDepth;
				x += DepthSourceViewAquarium.maxDepthCoor.x;
				y += DepthSourceViewAquarium.maxDepthCoor.y;
				Debug.Log (DepthSourceViewAquarium.maxDepthCoor.x + " , " + DepthSourceViewAquarium.maxDepthCoor.y);
				Debug.Log (DepthSourceViewAquarium.maxDepth);
				
			} else if (points == 5) {
				//Debug.Log (z / 5);
				//	Debug.Log (x / 5);
				//	Debug.Log (y / 5);
				gettingBottomRight = false;
				waitingOnRight = false;
				initialState = false;
				
				bottomRight.x = x / 5;
				bottomRight.y = y / 5;
				
				finishedCalibration = true;
				points = 0;
				//getPushedTopLeftZ ();
				Debug.Log ("FINISHED BOTTOM RIGHT" + bottomRight);
			}
		} else if (waitingOnLeft == true && initialState == true) {
			if (DepthSourceViewAquarium.maxDepth + 2.0f < minDepth) {
				getPushedTopLeftZ ();
			} else {
				Debug.Log ("PUSH TOP LEFT");
			}
		} else if (waitingOnRight == true && initialState == true) {
			if (DepthSourceViewAquarium.maxDepth + 2.0f < minDepth) {
				getPushedBottomRightZ ();
			} else {
				Debug.Log ("PUSH BOTTOM RIGHT");
			}
		} else if (finishedCalibration == true) {
			Debug.Log ("FINISHED CALIBRATION");
			DepthSourceViewAquarium.minDepthCalibration = minDepth;
			DepthSourceViewAquarium.topLeftCalibration = topLeft;
			DepthSourceViewAquarium.bottomRightCalibration = bottomRight;
			//STOP RUNNING SCRIPT HERE
			this.enabled = false;
			GetComponent<LineGame>().enabled = true;

			Debug.Log ("DID IT END?");
		} else if (initialState == false) {
			Debug.Log ("GO BACK TO INITIAL STATE");
			if (Mathf.Abs(DepthSourceViewAquarium.maxDepth - minDepth) < 0.5f) {
				initialState = true;
			}
		}

	}

	void getNonPushedZ()
	{
		gettingZ = true;
		float begin = Time.time;
		nextActionTime = begin + 1;
	}

	void getPushedTopLeftZ()
	{
		gettingTopLeft = true;
		float begin = Time.time;
		nextActionTime = begin + 1;
	}
	void getPushedBottomRightZ()
	{
		gettingBottomRight = true;
		float begin = Time.time;
		nextActionTime = begin + 1;
	}
}
