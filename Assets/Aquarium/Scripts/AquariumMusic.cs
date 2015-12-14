using UnityEngine;
using System.Collections;

public class AquariumMusic : MonoBehaviour
{
	AudioSource background;
	AudioSource positive;
	AudioSource negative;
	
	// Use this for initialization
	void Start ()
	{
		AudioSource[] audios = GetComponents<AudioSource>();
		background = audios[0];
		positive =   audios[1];
		negative =   audios[2];

		background.loop = true;
		positive.Stop ();
		negative.Stop ();
	}

	public void PlayPositiveFeedback()
	{
		PlayFeedback (positive);
	}

	public void PlayNegativeFeedback()
	{
		PlayFeedback (negative);
	}

	private void PlayFeedback(AudioSource source)
	{
		if (!source.isPlaying) {
			source.Play ();
		}
	}
}