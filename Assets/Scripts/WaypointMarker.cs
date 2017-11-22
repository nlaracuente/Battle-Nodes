using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks where the player is moving towards
/// </summary>
public class WaypointMarker : MonoBehaviour 
{
	/// <summary>
	/// How fast to ratate the waymarker
	/// </summary>
	[SerializeField]
	float rotationSpeed = 50f;

	/// <summary>
	/// A reference to the sprite renderer
	/// </summary>
	new SpriteRenderer renderer;
	SpriteRenderer Renderer
	{
		get{
			if (this.renderer == null) {
				this.renderer = GetComponent<SpriteRenderer> ();
			}
			return this.renderer;
		}
	}

	/// <summary>
	/// Sets the color of the waypoint.
	/// </summary>
	/// <value>The color of the waypoint.</value>
	public Color WaypointColor
	{
		set{ this.Renderer.color = value;}
	}

	/// <summary>
	/// Rotates the marker
	/// </summary>
	void Update () 
	{
		this.transform.Rotate (new Vector3 (0f, 0f, this.rotationSpeed * Time.deltaTime));
	}

	/// <summary>
	/// Moves the marker to the given position and enables it
	/// </summary>
	/// <param name="position">Position.</param>
	public void SetMarker(Vector3 position)
	{
		// Ignore Y axis as this is predetermined
		this.transform.position = new Vector3(
			position.x, 
			this.transform.position.y, 
			position.z
		);
		this.Renderer.enabled = true;
	}

	/// <summary>
	/// Disables the marker.
	/// </summary>
	public void DisableMarker()
	{
		this.Renderer.enabled = false;
	}
}
