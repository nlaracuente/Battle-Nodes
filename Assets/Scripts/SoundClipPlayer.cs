using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spanwed to play a single clip at a given volume
/// Destroys itself once the clip is finished playing
/// </summary>
public class SoundClipPlayer : MonoBehaviour
{
    /// <summary>
    /// True when the soundclip has been triggered
    /// </summary>
    bool audioPlayed = false;

    /// <summary>
    /// A reference to the audio source
    /// </summary>
    AudioSource audioSource;
	AudioSource Source
	{
		get{
			if (this.audioSource == null) {
				this.audioSource = GetComponent<AudioSource>();
			}
			return this.audioSource;
		}
	}

	/// <summary>
	/// Wait until the audio has been played and has finished playing
	/// </summary>
	void Update()
	{
		// Wait until the audio has been played
		if (!this.audioPlayed) {
			return;
		}

		// Done playing, destroy self
		if (!this.Source.isPlaying) {
			Destroy (this.gameObject);
		}
	}

	/// <summary>
	/// Play the specified clip and volume.
	/// </summary>
	/// <param name="clip">Clip.</param>
	/// <param name="volume">Volume.</param>
	public void Play(AudioClip clip, float volume)
	{
		this.Source.clip = clip;
		this.Source.volume = volume;
		this.Source.Play();
		this.audioPlayed = true;
	}

    /// <summary>
    /// Forces the sound being played to stop
    /// </summary>
    public void ForceStop()
    {
        this.Source.Stop();
    }
}
