using UnityEngine;
using System.Collections;

public class AttackerWithTarget {
	public GameObject attacker;
	public Vector3 target;
	public AttackerWithTarget(GameObject a, Vector3 targetPosition) {
		attacker = a;
		target = targetPosition;
	}
}

public class ProtectGame : MonoBehaviour {

	private Vector3 targetPos = new Vector3(15f, 7.5f, Utility.Z);
	public GameObject defender;
	private int health;
	private bool gameStarted = false;
	
	public GameObject[] attacks;
	AttackerWithTarget[] attackers = new AttackerWithTarget[10];

	private AquariumMusic music;  // how this module plays music in the application
	
	void Start() {
		defender = Instantiate (defender);
		Utility.InitializeFish (defender, targetPos, Utility.Z);

		defender.GetComponent<ActionObject> ().Glow (Color.white);

		music = GetComponent<AquariumMusic> ();
	}

	void Update () {
		ActionObject defenderScript = defender.GetComponent<ActionObject>();
		if (!gameStarted) {
			if (defenderScript.ClickedOn()) {
				gameStarted = true;
				RestartGame();
			}
			return;
		}

		for (int i = 0; i < attackers.Length; ++i) {
			if (!attackers [i].attacker)
				continue;
			
			GameObject attacker = attackers [i].attacker;
			
			ActionObject script = attacker.GetComponent<ActionObject> ();
			
			if (Utility.V3Equal (script.pos, attackers [i].target)) {
				if (Utility.V3Equal (script.pos, targetPos)) {
					defenderScript.Pulse(Color.red, 1);

					--health;
					Destroy (attacker);

					attackers [i].attacker = null;
				} else
					attackers [i].target = targetPos;
				
			} else {
				if (script.ClickedOn () && Utility.V3Equal (attackers [i].target, targetPos)) {
					attackers [i].target = GetNewTarget (script); 
					music.PlayPositiveFeedback();
				}
				script.MoveTowardsTarget (attackers [i].target);
			}
		}
		
		if (health == 0) {
			print ("Game over, attackers win!");

			music.PlayNegativeFeedback();

			foreach (AttackerWithTarget g in attackers) {
				if (g.attacker) {
					Destroy (g.attacker);
				}
			}
			defenderScript.Glow (Color.white);

			gameStarted = false;
		}
	}

	private void RestartGame()
	{
		defender.GetComponent<ActionObject>().UnGlow();
		ResetHealth ();
		ResetAttackers ();
	}
	
	private void ResetHealth()
	{
		health = attackers.Length;
	}
	
	private void ResetAttackers()
	{
		for (int i = 0; i < attackers.Length; ++i) {
			attackers [i] = new AttackerWithTarget (Instantiate (attacks [i]), targetPos);
			
			GameObject attacker = attackers [i].attacker;
			
			float x = Random.Range (-33, 65);
			float y = Mathf.Sqrt (65 * 65 - x * x) + Random.Range (0, 10);
			
			if (x > 0) {
				// if y is a value that if it was negative would not make the object more
				// than 30 degrees below the horizontal, then randomly flip the sign
				if (y < 60 * Mathf.Sin (0.523599f)) {
					y = (Random.value > 0.75) ? y : -y;
				}
			}
			else if (x == 0) {
				y = 50;
			}
			
			x += targetPos.x;
			y += targetPos.y;
			
			Utility.InitializeFish (attacker, new Vector3 (x, y, Utility.Z), Random.Range (2, 6));
			attacker.GetComponent<ActionObject> ().MakeUndestroyable ();
		}
	}
	
	private Vector3 GetNewTarget(ActionObject a)
	{
		if (a.pos.x == targetPos.x) // In a vertical line
			return new Vector3 (targetPos.x, 50+targetPos.y, Utility.Z);
		else if (a.pos.y == targetPos.y) // In a horizontal line
			return new Vector3 (50+targetPos.x, targetPos.y, Utility.Z);

		float slope = (targetPos.y - a.pos.y) / (targetPos.x - a.pos.x);
		float diff = 20f;

		if (a.pos.x < targetPos.x) // left
			return new Vector3 (targetPos.x - diff, targetPos.y - (slope * diff), Utility.Z);
		else // right
			return new Vector3 (targetPos.x + diff, targetPos.y + (slope * diff), Utility.Z);
	}
}