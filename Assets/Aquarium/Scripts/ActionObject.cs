using UnityEngine;
using System.Collections;
using System;

public class ActionObject : MonoBehaviour {
	
	private const float INCREASE_FACTOR = 1.01f;	// The rate at which the object grows
	private const float DECREASE_FACTOR = 0.99f;	// The rate at which the object shrinks
	private const float NORMAL_SIZE = 1.0f;		// The normal scale of an object
	private const int OBJECT_RADIUS = 11;			// How far around an object is considered touching the object
	
	private Vector3 targetLocation;		   // Where this object is aiming to go
	private Quaternion targetRotation; 	   // Which direction this object is aiming to turn
	private float speed;				   // How fast the object should move in a direction
	private bool moving;				   // If the object is moving or not
	private bool rotatedInPlace = false;   // If the object has been rotated into a reasonable direction while standing still
	private Vector3 directionOfMotion;	   // The direction that the object should be facing
	
	private bool destroyable = true;	   // If the object should be destroyed when it is outside the screen
	private bool rotatable = true;		   // If the object should could be rotated

	private Light lt;

	private bool hadColor = false;
	private Color lastColor;

	private bool pulsing = false;
	private DateTime lastPulse;
	private Color pulseColor;
	private int pulseTimes;

	public virtual void Awake()
	{
		lt = GetComponent<Light> ();

		targetLocation = pos = Utility.GetRandomVector (0,20,0,10);
		speed = UnityEngine.Random.Range (5,8);

		SetRotation ();
		SetScale ();
		UnGlow ();
	}
	
	// to initialize the location of an object, call Insantiate(x); followed by x.Initialize(position, speed);
	public virtual void Initialize (Vector3 pos_, float speed_)
	{
		targetLocation = pos = pos_;
		speed = speed_;
	}
	
	// Move the object towards the 
	public void Update()
	{
		// If the object isn't at its target location, move towards it
		if (!Utility.V3Equal (pos, targetLocation)) {
			moving = true;
			MoveTowardsTarget (targetLocation);
			rotatedInPlace = false;
			directionOfMotion = targetLocation - pos;
			if (tag == "whale")
				directionOfMotion = -directionOfMotion;
			else if (tag == "fish")
				directionOfMotion = Quaternion.Euler (0,90,0) * directionOfMotion;
		} else {
			if (!rotatedInPlace) {
				SetRotation ();
			}
			moving = false;
		}

		Pulse ();

		// If the object isn't at its target rotation, rotate towards it
		float rotationAngle = Quaternion.Angle (rotation, targetRotation);
		if (!moving) {
			if (rotationAngle > 2.0f && rotationAngle < 178.0f)
				RotateTowardsTarget ();
		} else {
			RotateTowardsTarget ();
		}

		// If the object is outside of the frame of the camera
		if (OutsideCamera () && destroyable) {
			Destroy (gameObject);
			Debug.Log ("Object destroyed!");
		}
	}

	public void Pulse(Color c, int times, float _intensity=0.5f)
	{
		pulseColor = c;
		lt.color = pulseColor;
		pulseTimes = times;
		lastPulse = DateTime.Now;

		if (pulsing && lt.enabled) {
			lastPulse = DateTime.Now;
			return;
		}

		pulsing = true;

		// save the previous color
		if (lt.enabled) {
			hadColor = true;
			lastColor = lt.color;
		} else if (!lt.enabled) {
			lt.enabled = true;
			hadColor = false;
		}
	}

	private void Pulse()
	{
		if (!pulsing)
			return;

		double secondsPassed = (DateTime.Now - lastPulse).TotalSeconds;

		if (secondsPassed < 0.5f) {
			return;
		} else if (secondsPassed < 1.0f) {
			lt.enabled = false;
			return;
		} else {
			lastPulse = DateTime.Now;

			if (pulseTimes == 1) {
				pulsing = false;
				if (hadColor)
					Glow (lastColor);
				hadColor = false;
				return;
			}
			if (pulseTimes > 1) {
				--pulseTimes;
			}
			lt.enabled = true;
		}
	}

	public void Glow(Color c, float _intensity=0.5f)
	{
		lt.enabled = true;
		lt.color = c;
		lt.intensity = _intensity;

		pulsing = false;
	}

	public void UnGlow()
	{
		lt.enabled = false;
	}

	// Sets the value of destroyable to false so that the object will not be destroyed when it is outside the camera
	public void MakeUndestroyable()
	{
		destroyable = false;
	}

	// Sets the value of rotatable to false so that the object will never be rotated
	public void MakeUnrotateable()
	{
		rotatable = false;
	}
	
	public bool IsMoving()
	{
		return moving;
	}

	// Scales the object by increase 
	public void Grow (float increase=INCREASE_FACTOR)
	{
		scale = scale * increase;
	}

	// Scales the object by decrease
	public void Shrink (float decrease=DECREASE_FACTOR)
	{
		Grow (decrease);
	}

	// Resets the scale to size
	public void ResetScale(float size=NORMAL_SIZE)
	{
		scale = new Vector3 (size, size, size);
	}

	// Moves towards the target, and sets the object to move towards the targetPosition
	public void MoveTowardsTarget(Vector3 targetPosition) {
		pos = Vector3.MoveTowards (pos, targetPosition, speed*Time.deltaTime);

		moving = true;
		targetLocation = targetPosition;
	}
	
	public Vector3 PositionOnScreen()
	{
		return Camera.main.WorldToScreenPoint (pos);
	}

	// Returns true if the object is clicked on by the mouse or by the Kinect
	public bool ClickedOn()
	{
		// Kinect is enabled
		if (Utility.kinectClickedOn) {
			if (Utility.pushingOnKinectOn) {

				// Scale the position down to 1 by 1
//				Vector3 scaledPos = PositionOnScreen ();
//				scaledPos.x /= 10f;
//				scaledPos.y /= 10f;
				Vector3 scaledPos = new Vector3();
				scaledPos.x = Utility.locationOfPush.x;
				scaledPos.y = Utility.locationOfPush.y;
				scaledPos.z = 10f;

				// If that position is pressed close to the object's position, return true
				if (Vector3.Distance (scaledPos, pos) < OBJECT_RADIUS) {
					print ("Action object clicked on through Kinect");
					print ("clickedLocation: " + pos);
					return true;
				}
			}
		}

		// Mouse is not being clicked
		if (!Input.GetMouseButton (0))
			return false;

		// Return if the mouse is clicked close to the object's position
		return (Vector3.Distance (PositionOnScreen (), Input.mousePosition) < OBJECT_RADIUS);
	}

	private bool OutsideCamera(float shift = 10)
	{
//		float maxx = 20 + shift;
//		float minx = 0 - shift;
		return (pos.x > 20 + shift ||
		        pos.x < -shift ||
		        pos.y > 10 + shift ||
		        pos.y < -shift);
	}

	// Sets the scale for each object based on the specfic prefab and how big it should be
	public void SetScale()
	{
		switch (tag) {
		case "whale":
			scale = new Vector3 (1f, 1f, 1f);
			break;
		case "cruscarp":
			scale = new Vector3 (10f, 10f, 10f);
			break;
		case "fish":
			scale = new Vector3 (10f, 10f, 10f);
			break;
		default:
			break;
		}
	}

	// Sets the rotation of the object either to a resting rotation or towards the target location
	private void RotateTowardsTarget()
	{
		if (!rotatable)
			return;
		targetRotation = Quaternion.LookRotation(directionOfMotion);
		Vector3 temp = Vector3.RotateTowards(transform.forward, directionOfMotion, 100*Mathf.Deg2Rad*Time.deltaTime, 0.0f);
		rotation = Quaternion.LookRotation (temp);
	}
	
	private void SetRotation() 
	{
		if (!rotatable)
			return;
		
		// A rotation has already been set in place
		if (rotatedInPlace)
			return;
		
		rotatedInPlace = true;
		
		float offset = UnityEngine.Random.value > 0.5f ? -100f : 100f;
		Vector3 temp = new Vector3(pos.x + offset, pos.y, pos.z) - pos;
		directionOfMotion = temp - pos;
		if (tag == "whale")
			directionOfMotion = -directionOfMotion;
		else if (tag == "fish")
			directionOfMotion = Quaternion.Euler (0,90,0) * directionOfMotion;

		
		targetRotation = Quaternion.LookRotation(directionOfMotion);
	}

	/* Getters & Setters */
	public Vector3 scale
	{
		get {
			return (transform.localScale);
		} set {
			transform.localScale = value;
		}
	}
	public Vector3 pos {
		get {
			return (transform.position);
		}
		set {
			transform.position = value;
		}
	}
	public Quaternion rotation {
		get {
			return (transform.rotation);
		}
		set {
			transform.rotation = value;
		}
	}
}