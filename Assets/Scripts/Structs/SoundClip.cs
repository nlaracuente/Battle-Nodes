using System;
using UnityEngine;

/// <summary>
/// Information about a sound clip such as the clip, volume, and pitch
/// </summary>
[Serializable]
public struct SoundClipStruct
{
	/// <summary>
	/// The sound clip
	/// </summary>
	public AudioClip clip;

	/// <summary>
	/// The volume
	/// </summary>
	public float volume;
}

