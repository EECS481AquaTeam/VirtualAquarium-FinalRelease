using UnityEngine;
using System.Collections;

public class Whale : MonoBehaviour {
	
	public void Swim()
	{
		GetComponent<Animation> ().Play("swim");
	}
	
	public void SwimFast()
	{
		GetComponent<Animation> ().Play("fastswim");
	}
	
	public void SwimFast2()
	{
		GetComponent<Animation> ().Play("fastswim2");
	}
	
	public void Dive()
	{
		GetComponent<Animation> ().Play("dive");
	}
	
	public void Die()
	{
		GetComponent<Animation> ().Play("death");
	}
}