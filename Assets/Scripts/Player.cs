using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Main player script
/// Controls player input
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// A reference to the NavMeshAgent component
    /// </summary>
    NavMeshAgent navMeshAgent;

	/// <summary>
	/// A reference to the waypoint marker
	/// </summary>
	WaypointMarker waypoint;

    /// <summary>
    /// Holds the direction the turret needs to face 
    /// </summary>
    Vector3 turretLookAt = Vector3.zero;
    public Vector3 TurretLookAt
    {
        set { this.turretLookAt = value; }
        get { return this.turretLookAt; }
    }

    /// <summary>
    /// Keeps track of how many frames have passed since the notification was sent
    /// </summary>
    int notifyRotationCount = 0;

    /// <summary>
    /// The name to display above the player and to reference the player by
    /// </summary>
    string gamerTag;
    public string GamerTag
    {
        set {
            this.gamerTag = value;
            // Updates the game tag ui
            GetComponentInChildren<Text>().text = value;
        }
        get { return this.gamerTag; }
    }

    /// <summary>
    /// Player's current health
    /// </summary>
    int health = 100;
    public int Health
    {
        // Prevent the health from going under/over the limits
        set {
            this.health = Mathf.Max(0, Mathf.Min(value, this.maxHealth));

            if(this.health == 0) {
                GameManager.instance.NotifyPlayerDefeated(this.playerId);
            }
        }
        get { return this.health; }
    }

    /// <summary>
    /// Maximum health the player can have
    /// </summary>
    [SerializeField]
    int maxHealth = 100;
    public int MaxHealth
    {
        get { return this.maxHealth; }
    }

    /// <summary>
    /// A unique identifier created by the server to associated with this player
    /// </summary>
    [SerializeField]
    string playerId;
    public string PlayerId
    {
        set { this.playerId = value; }
        get { return this.playerId; }
    }

    /// <summary>
    /// A reference to the tank's turret
    /// </summary>
    [SerializeField]
    GameObject turretGO;
    public GameObject TurretGO
    {
        get { return this.turretGO; }
    }

    /// <summary>
    /// A reference to the Isometric Camera
    /// </summary>
    [SerializeField]
    Camera isoCamera;
    public Camera IsoCamera
    {
        set { this.isoCamera = value; }
        get { return this.isoCamera; }
    }

    /// <summary>
    /// A reference to the shell prefab
    /// </summary>
    [SerializeField]
    GameObject shellPrefab;

    /// <summary>
    /// Where to spawn the tank shell when fired
    /// </summary>
    [SerializeField]
    GameObject shellSpawnPoint;

    /// <summary>
    /// A reference to the destroyed tank to display when this player is destroyed
    /// </summary>
    [SerializeField]
    GameObject destroyedTank;

    /// <summary>
    /// A reference to the tank explosion to play on player death
    /// </summary>
    [SerializeField]
    GameObject deathExplosion;

    /// <summary>
    /// The layer mask containing the ground
    /// </summary>
    [SerializeField]
    LayerMask groundMask;
    public LayerMask GroundMask
    {
        get { return this.groundMask; }
    }
    
    /// <summary>
    /// How fast to rotate the turret
    /// </summary>
    [SerializeField]
    float turretRotationDamp = 100f;

    /// <summary>
    /// How far to cast the ray from the mouse to the game world
    /// </summary>
    [SerializeField]
    float rayDistance = 100f;
    public float RayDistance
    {
        get { return this.rayDistance; }
    }

    /// <summary>
    /// The audio source for when the player is idle
    /// </summary>
    [SerializeField]
    AudioSource idleAudioSource;

    /// <summary>
    /// The audio source for when the player is moving
    /// </summary>
    [SerializeField]
    AudioSource movingAudioSource;

    /// <summary>
    /// How many frames to allow to pass before updating the rotation of the turret to the network
    /// </summary>
    [SerializeField]
    int notifyRotationDelay = 10;

    /// <summary>
    /// Contains all the mesh renderers that make up the tanks
    /// This is so that we can change the color
    /// </summary>
    [SerializeField]
    MeshRenderer[] tankRenderers;

    /// <summary>
    /// How load to play the idle soundclip
    /// </summary>
    [SerializeField]
    float idleVolume = .10f;

    /// <summary>
    /// How load to play the moving soundclip
    /// </summary>
    [SerializeField]
    float movingVolume = .5f;

    /// <summary>
    /// A reference to the attack being charged
    /// </summary>
    [SerializeField]
    SoundClipStruct chargeSound;
    public SoundClipStruct ChargeSound
    {
        get { return this.chargeSound; }
    }

    /// <summary>
    /// True prevents any actions from happening on the player
    /// Defaults to true until the "all player ready" call is made
    /// </summary>
    bool isDisabled = true;
    public bool IsDisabled
    {
        set {
            if (value) {
                this.navMeshAgent.isStopped = true;
            }
            this.isDisabled = value;
        }
        get { return this.isDisabled; }
    }

    /// <summary>
    /// Updates the materials assigned to each tank part
    /// This updates the destroyed tank too
    /// </summary>
    public Material MeshRendererMaterial
    {
        set {
            foreach(MeshRenderer renderer in this.tankRenderers) {
                renderer.material = value;
            }
            this.destroyedTank.GetComponent<MeshRenderer>().material = value;
        }
    }

    /// <summary>
    /// Sets the color of the gamer tag's text to the given value
    /// </summary>
    public Color GamerTagColor
    {
        set { GetComponentInChildren<Text>().color = value; }
    }

    /// <summary>
    /// Initialize
    /// </summary>
    void Start ()
    {
        this.health = this.maxHealth;
        this.navMeshAgent = GetComponent<NavMeshAgent>();
		this.waypoint = FindObjectOfType<WaypointMarker>();
	}

    /// <summary>
    /// Plays audio based on whether the player is moving or not
    /// </summary>
    private void FixedUpdate()
    {
        if (this.isDisabled) {
            return;
        }

        if (this.navMeshAgent.remainingDistance <= this.navMeshAgent.stoppingDistance) {
            // Stopped moving
            if (!this.navMeshAgent.hasPath || Mathf.Abs(this.navMeshAgent.velocity.sqrMagnitude) < float.Epsilon) {
                this.idleAudioSource.volume = this.idleVolume;
                this.movingAudioSource.volume = 0f;
            }
        } else {
            this.idleAudioSource.volume = 0f;
            this.movingAudioSource.volume = this.movingVolume;
        }
    }

    /// <summary>
    /// Continuously updates the direction in which the turret is facing
    /// Notifying the GameManager to update the others only at certain intervals
    /// </summary>
    private void LateUpdate()
    {
        if (this.isDisabled) {
            return;
        }

        // Avoids the warnings about zero vector rotations
        if (this.turretLookAt != Vector3.zero) {
            Quaternion currentRot = this.turretGO.transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(this.turretLookAt);
            this.turretGO.transform.rotation = Quaternion.Lerp(currentRot, targetRot, this.turretRotationDamp * Time.deltaTime);
        }

        // The following notification does not need to be sent if it is not the local user
        if (GameManager.instance.LocalPlayerId != this.playerId) {
            return;
        }

        this.notifyRotationCount++;
        if(this.notifyRotationCount >= this.notifyRotationDelay) {
            this.notifyRotationCount = 0;
            GameManager.instance.NotifyPlayerTurretRotation(this.playerId, this.turretLookAt);
        }
    }

    /// <summary>
    /// Tells the navigation mesh to move the player to the given position
    /// </summary>
    /// <param name="targetPosition"></param>
    public void Move(Vector3 targetPosition)
    {
        if (this.isDisabled) {
            return;
        }

        this.navMeshAgent.destination = targetPosition;

        if(GameManager.instance.LocalPlayerId == this.playerId) {
            this.waypoint.SetMarker(targetPosition);
        }
    }

    /// <summary>
    /// Spawns a tank shell and sets the association with this player
    /// </summary>
    public void Attack(float force, Vector3 turretRotation)
    {
        if (this.isDisabled) {
            return;
        }

        // Syncs up the rotations
        this.turretLookAt = turretRotation;

        GameObject shellGO = Instantiate(this.shellPrefab, this.shellSpawnPoint.transform.position, this.shellSpawnPoint.transform.rotation);
        TankShell tankShell = shellGO.GetComponent<TankShell>();
        tankShell.PlayerId = this.playerId;
        tankShell.FireForce = force;
    }

    /// <summary>
    /// Player was damage, updates the health to represent the damage inflcited
    /// </summary>
    /// <param name="newHealth"></param>
    public void Damaged(int newHealth)
    {
        if (this.isDisabled) {
            return;
        }

        this.Health = newHealth;
    }

    /// <summary>
    /// Disables this player
    /// Shows the explosions, makes the player tank not render
    /// Renders the broken tank
    /// If there is only one player left, triggers the "GameOver" sending over the "victor"
    /// </summary>
    public void Defeated(string playerId)
    {
        // Make sure they look dead
        this.health = 0;

        // Already did...ignore the request
        if (this.isDisabled) {
            return;
        }

        // Disable and stop moving
        this.IsDisabled = true;
        Destroy(Instantiate(this.deathExplosion, this.transform.position, Quaternion.identity), 3f);

        foreach (MeshRenderer meshRenderer in this.tankRenderers) {
            meshRenderer.enabled = false;
        }

        this.destroyedTank.SetActive(true);
     
        // Only the local player will notify a game over if one exists
        if(playerId == GameManager.instance.LocalPlayerId) {
            GameManager.instance.CheckForGameOver();
        }
    }
}
