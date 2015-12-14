using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {

	public GameObject cursor_obj;
	private GameObject cursor_obj_instance;

	public static bool pressedDown = false;

	//GameObject redSphere;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if( Input.GetMouseButtonDown(0) ){
			//Camera camera = GetComponent<Camera>();
			Vector3 temp = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
			//Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(0,0,5));
			//Debug.Log ("WORLD POINT: " + worldPoint);

			cursor_obj_instance = (GameObject)Instantiate(cursor_obj, Camera.main.ViewportToWorldPoint(temp), Quaternion.identity);
			// set the position 1 meter ahead of the camera
			Debug.Log ("InputMousePosition: " + cursor_obj_instance.transform.position);
			pressedDown = true;
			//cube.transform.position = new Vector3(Input.mousePosition.x/100.0f, Input.mousePosition.y/100.0f, -10.0f);
			//cube.transform.rotation = transform.rotation;
		}
		if (pressedDown && cursor_obj) {
			Debug.Log ("Dragging: " + Input.mousePosition);
			Vector3 temp = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
			cursor_obj_instance.transform.position = temp;

		}

		if (Input.GetMouseButtonUp (0) && cursor_obj) {
			Debug.Log ("Delete Object: " + Input.mousePosition);

			Destroy(cursor_obj_instance);
			pressedDown = false;
		}

		/*if(Input.GetMouseButtonDown(0)){
			myCurrentObject = Instantiate(objectToInstantiate, camera.ScreenToWorldPoint(Input.mousePosition),Quaternion.identity);
		}
		if(Input.GetMouseButton(0) && myCurrentObject){
			myCurrentObject.transform.position =  camera.ScreenToWorldPoint(Input.mousePosition);
		}
		if(Input.GetMouseButtonUp(0) && myCurrentObject){
			myCurrentObject = null;
		}*/
	}
}
