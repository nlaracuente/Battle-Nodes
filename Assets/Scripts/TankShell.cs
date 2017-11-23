using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behavior of a tank shell when it is fired
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TankShell : MonoBehaviour
{
    /// <summary>
    /// How much force to apply to launch the shell
    /// </summary>
    [SerializeField]
    float fireForce;
    public float FireForce
    {
        set { this.fireForce = value; }
    }

    /// <summary>
    /// How much damage the tank shell will inflict when 
    /// it colliders with a player
    /// </summary>
    [SerializeField]
    int damagePower = 10;
    public int DamagePower
    {
        set { this.damagePower = Mathf.Abs(value); }
        get { return this.damagePower; }
    }

    /// <summary>
    /// How long before removing the explosion
    /// </summary>
    [SerializeField]
    float explosionDestroyDelay = 1f;

    /// <summary>
    /// A r
    /// </summary>
    [SerializeField]
    GameObject shellExplosionPrefab;

    /// <summary>
    /// The Id of the player who fired this shell
    /// </summary>
    string playerId;
    public string PlayerId
    {
        set { this.playerId = value; }
    }

	/// <summary>
	/// The sound clip for when the shell is fired
	/// </summary>
	[SerializeField]
	SoundClipStruct firedSoundClip;

	/// <summary>
	/// The sound clip for when the shell collides with an object
	/// </summary>
	[SerializeField]
	SoundClipStruct collisionSoundClip;

    /// <summary>
    /// Triggers the shell to "launch" forward and plays shooting sound
    /// </summary>
    private void Start()
    {
		GameManager.instance.PlaySound(this.firedSoundClip.clip, this.firedSoundClip.volume, this.transform.position);
        GetComponent<Rigidbody>().velocity = this.transform.forward * this.fireForce;
    }

    /// <summary>
    /// Changes the rotation of the bullet to match its current velocity
    /// </summary>
    private void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    /// <summary>
    /// Expected when collision happens with another player as they contain a trigger collider
    /// Spawns the explision
    /// Notifies the GameManager that a player has been damaged
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        Player otherPlayer = other.gameObject.GetComponent<Player>();

        // Avoid the owner
        if(otherPlayer != null){
            if (otherPlayer.PlayerId == this.playerId) {
                return;
            }
            GameManager.instance.NotifyPlayerDamaged(this.playerId, otherPlayer.PlayerId, otherPlayer.Health - this.damagePower);
        }

		this.ShellCollision();   
    }

    /// <summary>
    /// Expected when collision happens with the environment as they have a non-trigger collider
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();

        // Avoid the owner
        if (player != null && player.PlayerId == this.playerId) {
            return;
        }

        this.ShellCollision();
    }

    /// <summary>
    /// Plays the collision sound
	/// Spawns the explosion prefab with a delayed destroy
	/// Destroys this shell object
    /// </summary>
	void ShellCollision()
    {
		GameManager.instance.PlaySound (this.collisionSoundClip.clip, this.collisionSoundClip.volume, this.transform.position);
        GameObject explosionGO = Instantiate(this.shellExplosionPrefab, this.transform.position, Quaternion.identity);
        Destroy(explosionGO, this.explosionDestroyDelay);
		Destroy(this.gameObject);
    }
}
