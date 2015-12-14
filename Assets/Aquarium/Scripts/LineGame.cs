using UnityEngine;
using System.Collections;

public enum objectState {NORMAL, MOVINGTO, DONE, SHOULD_DIVE, DIVING, RESTART};
public class whaleWithState {
	public GameObject whale;
	public objectState state;
	public Vector3 targetPos;
	public Vector3 diveTargetPos;
	public whaleWithState(GameObject w, objectState s, Vector3 targetPosition, Vector3 diveTargetPosition) {
		whale = w;
		state = s;
		targetPos = targetPosition;
		diveTargetPos = diveTargetPosition;
	}
}

public class LineGame : MonoBehaviour {
	public static int count = 0;
	public static int numObjects = 4; 
	public static int lineCount = 0;
	
	public Vector3 targetPos;
	public Vector3 diveTargetPos = new Vector3 (0, 0, 0);
	
	public Vector3 offscreenPos = new Vector3 (-14,0,0);
	
	public Vector3 onscreenPos;

	private AquariumMusic music;

	//public Color orange = new Color(255F,165F,0F);

	public Vector3 clickedPos = new Vector3 (-100, -100, -100); //kevin
	//public bool kinectClickedOn = false;
	
	public Vector3 whalePos;
	
	public GameObject[] ws;
	
	public whaleWithState[] whaleList = new whaleWithState[4];

	void Start() {
		Debug.Log ("Start");

		music = GetComponent<AquariumMusic> ();
	
		targetPos = new Vector3(7f,2.5f,0);
		for (int i = 0; i < 4; ++i) {
			whaleList [i] = new whaleWithState (Instantiate(ws[i]), objectState.NORMAL, targetPos, diveTargetPos);
			whaleList[i].whale.GetComponent<ActionObject>().MakeUndestroyable();
			whaleList[i].whale.GetComponent<ActionObject>().MakeUnrotateable();
			whaleList[i].whale.GetComponent<ActionObject>().Glow (Color.white);
			Debug.Log ("Instantiate whale");
			targetPos.x = targetPos.x + 2f;
		}
		
		foreach (whaleWithState w in whaleList) {
			w.state = objectState.NORMAL;
			onscreenPos = Utility.GetRandomVector(5f, 15f, 0f, 5f);
			w.whale.GetComponent<ActionObject>().MoveTowardsTarget(onscreenPos);
		}
	}

	
	void Update() {
		foreach (whaleWithState w in whaleList) {
			ActionObject script = w.whale.GetComponent<ActionObject>();
			switch (w.state) {
			case objectState.NORMAL:
				/*kinectClickedOn, clickedPos*/
				if (script.ClickedOn ()) {
					w.state = objectState.MOVINGTO;
					script.MoveTowardsTarget(w.targetPos);
					script.Glow (Color.green);
					break;
				}
				break;
			case objectState.MOVINGTO:
				//print ("MOVING TO");
				if (Utility.V3Equal(script.pos, w.targetPos)) {
					lineCount++;
					if (lineCount == numObjects) {
						lineCount = 0;
						music.PlayPositiveFeedback();
						foreach (whaleWithState item in whaleList) {
							item.state = objectState.SHOULD_DIVE;
						}
					} 
					else {
						w.state = objectState.DONE;
					}
				}
				break;
			case objectState.SHOULD_DIVE:
				w.diveTargetPos.x = w.targetPos.x - 0.5f;
				w.diveTargetPos.y = w.targetPos.y - 1;
				script.MoveTowardsTarget(w.diveTargetPos);
				//w.whale.GetComponent<Animation>()["dive"].wrapMode=WrapMode.Once;
				//w.whale.GetComponent<Whale>().Dive();
				//GetComponent<ActionObject>().shouldMove = false;
				w.state = objectState.DIVING;
				Debug.Log (lineCount);
				break;
			case objectState.DIVING:
				print ("IS DIVING");
				if (Utility.V3Equal(script.pos, w.diveTargetPos)) {
					//if (!w.whale.GetComponent<Animation>().IsPlaying("dive")) {
					Debug.Log ("Dive complete");
					lineCount++;
					Debug.Log (lineCount);
				}
				if (lineCount == numObjects) {
						//all objects must dive
					lineCount = 0;
					foreach (whaleWithState item in whaleList) {
						item.state = objectState.RESTART;
					}
				} 
				//}
				break;
			case objectState.DONE:
				break;
			case objectState.RESTART:
				Debug.Log ("RESTART");
				//GetComponent<ActionObject>().shouldMove = true;
				targetPos = new Vector3(7f,2.5f,0);
				foreach (whaleWithState item in whaleList) {
					lineCount = 0;
					onscreenPos = Utility.GetRandomVector(5f, 15f, 0f, 5f);
					item.whale.GetComponent<ActionObject>().MoveTowardsTarget(onscreenPos);
					item.targetPos = targetPos;
					targetPos.x = targetPos.x + 2f; //update x
					item.state = objectState.NORMAL;
					item.whale.GetComponent<ActionObject>().Glow(Color.white);
				}
				break;
			}
		}
		//kinectClickedOn = false;
	}
}