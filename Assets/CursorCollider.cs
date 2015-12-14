using UnityEngine;
using System.Collections;

public class CursorCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update(){
		if( Input.GetButtonDown( "Fire1" ) ){
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			// set the position 1 meter ahead of the camera
			Debug.Log ("InputMousePosition: " + Input.mousePosition);
			cube.transform.position = new Vector3(Input.mousePosition.x/100.0f, Input.mousePosition.y/100.0f, -10.0f);
			cube.transform.rotation = transform.rotation;
		}
	}
}
