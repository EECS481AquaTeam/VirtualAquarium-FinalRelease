using UnityEngine;
using System.Collections;

public class whaleClass {
	public GameObject whale;
	public Vector3 targetPos;
	public bool targetReached;
	public whaleClass(GameObject w, Vector3 targetPosition) {
		whale = w;
		targetPos = targetPosition;
		//targetReached = false;
	}
}

public class AquariumGame : MonoBehaviour {
	public static int count = 0;
	public static int numObjects = 3; 
	public static int lineCount = 0;
	
	public Vector3 targetPos;
	public Vector3 offscreenPos;
	public Vector3 onscreenPos;
	//public Vector3 startPos = new Vector3(12,3,1);
	
	public Vector3 clickedPos = new Vector3 (-100, -100, -100); //kevin
	//public bool kinectClickedOn = false;
	
//	private AquariumMusic music;  // how this module plays music in the application
	
	public bool end = false;
	
	public GameObject[] ws2;
	
	public whaleClass[] whaleList2 = new whaleClass[numObjects];
	
	void Start() {
		
		Debug.Log ("Start");

		
		for (int i = 0; i < numObjects; ++i) {
			ws2[i] = Instantiate(ws2[i]);
			ws2[i].GetComponent<ActionObject>().Initialize(Utility.GetRandomVector(0f,10f,5f,10f), Random.Range (0.5f, 1.5f));
			ws2[i].GetComponent<ActionObject>().UnGlow();
			whaleList2 [i] = new whaleClass (ws2[i], targetPos);
			
			Debug.Log ("Instantiate whale"); 
		}
		
		foreach (whaleClass w in whaleList2) {
			w.targetPos = Utility.GetRandomVector(0f,10f,5f,10f);
			w.whale.GetComponent<ActionObject>().MoveTowardsTarget(w.targetPos);
		}
	}
	// Update is called once per frame
	void Update () {
		foreach (whaleClass w in whaleList2) {
			if (w.whale.GetComponent<ActionObject>().ClickedOn()) {
				w.targetPos = Utility.GetRandomVector(0f,10f,5f,10f);
				w.whale.GetComponent<ActionObject>().MoveTowardsTarget(w.targetPos);
			}
			if (Utility.V3Equal(w.whale.GetComponent<ActionObject>().pos, w.targetPos)) {
				w.targetPos = Utility.GetRandomVector(0f,10f,5f,10f);
				w.whale.GetComponent<ActionObject>().MoveTowardsTarget(w.targetPos);
			}
		}
		
	}
}