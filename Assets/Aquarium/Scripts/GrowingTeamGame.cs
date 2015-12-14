using UnityEngine;
using System.Collections;
using System;

public enum turn {LEFT, RIGHT, START, END};

public class GrowingTeamGame : MonoBehaviour
{
	public static float WINNER_SCALE = 1.5f;
	private static float Y_VAL = 2.5f; // the horizonal location of all objects in the game
	
	turn turnState = turn.START; // which fish should grow next? 

	public GameObject left;		// the left fish in the game
	public GameObject right;	// the right fish in the game

	private DateTime last;		// the last time that a fish has grown or shrunk
	
	// locations for the fish on and off screen
	private Vector3 offscreenLeft  = new Vector3 (-20f,   Y_VAL, Utility.Z);
	private Vector3 offscreenRight = new Vector3 (40f,   Y_VAL, Utility.Z);
	private Vector3 onscreenLeft   = new Vector3 (2.5f, Y_VAL, Utility.Z);
	private Vector3 onscreenRight  = new Vector3 (17.5f, Y_VAL, Utility.Z);
	
	// Runs on the beginning of instantiation of this class
	void Start() {
		
		// Initialize each of the fish in the game
		left = Instantiate (left);
		right = Instantiate (right);
		Utility.InitializeFish (left, offscreenLeft);
		Utility.InitializeFish (right, offscreenRight);

		left.GetComponent<ActionObject> ().MakeUndestroyable ();
		right.GetComponent<ActionObject> ().MakeUndestroyable ();

		MoveOnScreen ();
		
		UpdateTime ();			 // set the initial time of the game
	}
	
	void OnEnable()
	{	
		MoveOnScreen ();
		turnState = turn.START;
		
		RescaleFish ();
	}
	
	// Runs once per frame
	void Update()
	{
		if (Utility.V3Equal (left.GetComponent<ActionObject>  ().pos, offscreenLeft) &&
			Utility.V3Equal (right.GetComponent<ActionObject> ().pos, offscreenRight))
			MoveOnScreen ();
		
		// If it hasn't been half a second since the last action, don't do anything
		if ((DateTime.Now - last).TotalSeconds < 0.5f)
			return;
		
		// If a fish has already been clicked, 
		if (turnState == turn.LEFT || turnState == turn.RIGHT) {
			
			// set grower to be the fish whose turn it is to grow
			ActionObject grower = (turnState == turn.LEFT) ? left.GetComponent<ActionObject> () : right.GetComponent<ActionObject> ();
			// set shrinker to be the fish whose turn it isn't to grow (it will shrink if clicked)
			ActionObject shrinker = (turnState == turn.RIGHT) ? left.GetComponent<ActionObject> () : right.GetComponent<ActionObject> ();
			
			// If the proper fish is clicked on, grow it
			if (grower.ClickedOn ()) {
				Grow (grower, (turnState == turn.LEFT) ? turn.RIGHT : turn.LEFT);
				grower.UnGlow();
				shrinker.Glow(Color.white);
			}
			
			// If the improper whale is clicked on, shrink both whales
			else if (shrinker.ClickedOn ()) {
				Shrink (grower, shrinker);
				shrinker.Pulse(Color.red, 2);
			}
			
			// If both of the fish are of the winning scale, the user wins!
			if (WinningScale (grower) && WinningScale (shrinker)) {
				turnState = turn.END;
				MoveOffScreen();
			}
		}
		
		// No fish has been clicked yet
		else if (turnState == turn.START) {
			
			// Get the ActionObject components of each of the fish
			ActionObject left_ob = left.GetComponent<ActionObject> ();
			ActionObject right_ob = right.GetComponent<ActionObject> ();
			
			// Grow a fish if it is clicked on
			if (left_ob.ClickedOn ()) {
				Grow (left_ob, turn.RIGHT);
				left_ob.UnGlow();
			}
			else if (right_ob.ClickedOn ()) {
				Grow (right_ob, turn.LEFT);
				right_ob.UnGlow();
			}
		}
		else if (turnState == turn.END) {}
	}
	
	void RescaleFish()
	{
		Vector3 s = left.GetComponent<ActionObject> ().scale;
		left.GetComponent<ActionObject> ().scale = new Vector3 (s.x / WINNER_SCALE, s.y / WINNER_SCALE, s.z / WINNER_SCALE);
		s = right.GetComponent<ActionObject> ().scale;
		right.GetComponent<ActionObject> ().scale = new Vector3 (s.x / WINNER_SCALE, s.y / WINNER_SCALE, s.z / WINNER_SCALE);
	}
	
	void MoveOnScreen()
	{
		turnState = turn.START;

		ActionObject leftA  = left.GetComponent<ActionObject> ();
		ActionObject rightA = right.GetComponent<ActionObject> ();

		leftA.Glow (Color.white);
		rightA.Glow (Color.white);

		leftA.SetScale ();
		rightA.SetScale ();

		Utility.MoveHelper (left, onscreenLeft, right, onscreenRight);
	}
	
	void MoveOffScreen()
	{
		left.GetComponent<ActionObject>  ().UnGlow ();
		right.GetComponent<ActionObject> ().UnGlow ();
		Utility.MoveHelper (left, offscreenLeft, right, offscreenRight);
	}
	
	void Grow(ActionObject item, turn new_state)
	{	
		item.Grow (1.05f);
		
		turnState = new_state;
		
		UpdateTime ();
	}
	
	void Shrink(ActionObject item1, ActionObject item2)
	{	
		ShrinkHelper (item1);
		ShrinkHelper (item2);
		
		UpdateTime ();
	}
	
	void ShrinkHelper(ActionObject item)
	{
		if (item.scale [0] > 0.5)
			item.Grow (0.95f);
	}
	
	bool WinningScale(ActionObject item)
	{
		return item.scale [0] >= WINNER_SCALE;
	}
	
	void UpdateTime()
	{
		last = DateTime.Now;
	}
}