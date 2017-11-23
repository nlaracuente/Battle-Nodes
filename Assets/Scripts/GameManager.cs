using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Controls the play session and handles player controls
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Variables
    /// <summary>
    /// A reference to the GameManager instance
    /// </summary>
    public static GameManager instance = null;

    /// <summary>
    /// A reference to the unique player Id for the local instance of the game
    /// </summary>
    [SerializeField]
    string localPlayerId;
    public string LocalPlayerId
    {
        get { return this.localPlayerId; }
    }

    Player localPlayer = null;
    public Player LocalPlayer {
        get { return localPlayer; }
    }

    /// <summary>
    /// The main canvas that contains the title screen
    /// as well as the message screen
    /// </summary>
    MainUICanvas mainCanvas;
    MainUICanvas MainCanvas
    {
        get {
            if(this.mainCanvas == null) {
                this.mainCanvas = FindObjectOfType<MainUICanvas>();
            }
            return this.mainCanvas;
        }
    }

    /// <summary>
    /// A reference to the UI camera which is used to toggle showing the player the different title screens
    /// </summary>
    [SerializeField]
    Camera uiCamera;

    /// <summary>
    /// Contains a list of active users associated by their name
    /// </summary>
    Dictionary<string, Player> activeUsers = new Dictionary<string, Player>();

    /// <summary>
    /// Contains a list of all the defeated users to know when to trigger a game over
    /// </summary>
    List<string> defeatedUsers = new List<string>();

    /// <summary>
    /// Instance of the player prefab
    /// </summary>
    [SerializeField]
    GameObject playerPrefab;

    /// <summary>
    /// Instance of the player camera prefab
    /// </summary>
    [SerializeField]
    GameObject localCameraPrefab;

    /// <summary>
    /// Instance of the camera to use when looking at non-local players
    /// </summary>
    [SerializeField]
    GameObject socketCameraPrefab;

    /// <summary>
    /// Containers for all possible spawn points
    /// </summary>
    [SerializeField]
    SpawnPointInfoStruct[] spawnPoints;

	/// <summary>
	/// The sound clip player prefab.
	/// </summary>
	[SerializeField]
	GameObject SoundClipPlayerPrefab;

    /// <summary>
    /// How long to wait before notifying a game over
    /// </summary>
    [SerializeField]
    float gameOverDelay = 3f;

    /// <summary>
    /// How long to wait before redirecting the user back to the lobby
    /// </summary>
    [SerializeField]
    float redirectToLobbyDelay = 3f;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Sets the instance of the GameManager and makes it persistent
    /// </summary>
    void Awake()
    {
        if(instance == null){
            instance = this;
        } else if (instance != this){
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Needs to always run in the background
        Application.runInBackground = true;
    }

    /// <summary>
    /// Used only when debugging to create a user when the server isn't running
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private void Start()
    {
        //this.CreateLocalPlayer("nelson_l", "bluehash39", 0);
        //this.RegisterPlayer("rodil", "monkeyKing", 1);
        //this.RegisterPlayer("shaun", "LobsterMan", 2);
        //this.RegisterPlayer("mikesk", "IHaveAnIdea", 3);
        //this.RegisterPlayer("shaun", "LobsterMan", 2);
        //this.RegisterPlayer("nelson_l", "bluehash39", 0);

        //this.OnAllPlayersReady();
        // StartCoroutine(this.NotifyGameOver());
    }
#endif
    #endregion

    /// <summary>
    /// Registers the given player as the local player so that all user driven
    /// events only occurs for this local user and notifies that this player is
    /// ready to play the game
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="playerName"></param>
    /// <param name="spawnIndex"></param>
    public void CreateLocalPlayer(string playerId, string playerName, int spawnIndex)
    {
        this.localPlayerId = playerId;
        this.RegisterPlayer(playerId, playerName, spawnIndex, true);
        this.NotifyPlayerReady(playerId);
    }

    /// <summary>
    /// Called when a player has connected for the first time to spawn them on the given
    /// spawnPoint and creates/associates the player camera
    /// </summary>
    /// <param name="playerId"></param>
    public void RegisterPlayer(string playerId, string playerName, int spawnIndex, bool isLocalPlayer = false)
    {
        // Index not recognized
        if (spawnIndex < 0 || spawnIndex >= this.spawnPoints.Length) {
            Debug.Log("Spawn Index: " + spawnIndex + " not recognized. Using a random one instead");
            spawnIndex = Random.Range(0, this.spawnPoints.Length);
        }

        // Position and Color for the tank
        SpawnPointInfoStruct spawnInfo = this.spawnPoints[spawnIndex];

        // Player already registered?
        // Then make sure that we still have the correct position and player name associtaed with them
        // Also, make sure the colors are correct
        if (this.activeUsers.ContainsKey(playerId)) {
            Player activeplayer = this.activeUsers[playerId];            
            activeplayer.GamerTag = playerName;
            activeplayer.transform.position = spawnInfo.transform.position;
            activeplayer.MeshRendererMaterial = spawnInfo.material;
            return;
        }

        // Set the spawn point
        Vector3 spawnPoint = spawnInfo.transform.position;
        
        // Container for all player objects
        GameObject mainContainerGO = GameObject.Find("_Players");

        // Not created yet
        if(mainContainerGO == null) {
            mainContainerGO = new GameObject("_Players");
        }

        // Container for this new player
        GameObject playerContainerGO = new GameObject(playerId);
        playerContainerGO.transform.SetParent(mainContainerGO.transform);

        GameObject camPrefab = this.socketCameraPrefab;
        if (isLocalPlayer) {
            camPrefab = this.localCameraPrefab;
        }

        GameObject playerGO = Instantiate(this.playerPrefab, spawnPoint, Quaternion.identity, playerContainerGO.transform);
        GameObject cameraGO = Instantiate(camPrefab, playerContainerGO.transform);

        playerGO.name = "Tank";
        cameraGO.name = "Camera";

        Player player = playerGO.GetComponent<Player>();
        IsometricCamera isoCamera = cameraGO.GetComponent<IsometricCamera>();

        player.GamerTag = playerName;
        player.PlayerId = playerId;
        player.IsoCamera = cameraGO.GetComponentInChildren<Camera>();
        isoCamera.Target = playerGO.transform;

        // Update the tank's color and gamer tag color
        player.MeshRendererMaterial = spawnInfo.material;
        player.GamerTagColor = spawnInfo.gamerTagColor;

        // Make the player face towards the center of the map on spawn
        Vector3 lookAt = Vector3.zero - playerGO.transform.position;
        Quaternion currentRot = playerGO.transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(lookAt);
        playerGO.transform.rotation = targetRot;

        // Finally, local players require the "local player" component
        if (isLocalPlayer) {
            playerGO.AddComponent<LocalPlayerController>();
            localPlayer = player;

			// Set the waypoint marker to use the gamer tag color
			GameObject.FindGameObjectWithTag("Waypoint").GetComponent<WaypointMarker>().WaypointColor = spawnInfo.gamerTagColor;
        }

        // Register player
        this.activeUsers.Add(playerId, player);
    }

    /// <summary>
    /// Removes the given player id from the active users list
    /// and removes the player from the game session if it exists
    /// </summary>
    /// <param name="playerId"></param>
    public void UnregisterPlayer(string playerId)
    {
        // Already removed?
        if (!this.activeUsers.ContainsKey(playerId)) {
            Debug.Log("Player " + playerId + " is already removed");
            return;
        }

        Player player = this.activeUsers[playerId];
        Debug.Log("Player " + player.gameObject + " has left the game");
        this.OnPlayerDefeated(playerId);
    }

	/// <summary>
	/// Plays the given sound at the given volume and position
	/// </summary>
	/// <param name="clip">Clip.</param>
	/// <param name="volume">Volume.</param>
	/// <param name="atPosition">Where to spawn the audioclip.</param>
	public void PlaySound(AudioClip clip, float volume, Vector3 atPosition)
	{
		GameObject clipGO = Instantiate(this.SoundClipPlayerPrefab, atPosition, Quaternion.identity);
		SoundClipPlayer soundPlayer = clipGO.GetComponent<SoundClipPlayer>();
		soundPlayer.Play(clip, volume);
	}

    /// <summary>
    /// Checks if the list of defeated users is -1 from the total activeUsers
    /// That means that there's only player left and therefore we have a victor
    /// </summary>
    public void CheckForGameOver()
    {
        if(this.defeatedUsers.Count >= this.activeUsers.Count - 1) {
            StartCoroutine(this.NotifyGameOver());
        }
    }

    #region Notifiers
    /// <summary>
    /// Notifies the game server that this player is ready
    /// </summary>
    /// <param name="playerId"></param>
    public void NotifyPlayerReady(string playerId)
    {
        SocketManager.instance.NotifyPlayerReady(playerId);
    }
    
    /// <summary>
    /// Notifies the socket manager to broadcast a player movement request
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="targetPosition"></param>
    public void NotifyPlayerMovement(string playerId, Vector3 targetPosition)
    {
        this.OnPlayerMove(playerId, targetPosition);
        SocketManager.instance.NotifyPlayerMove(playerId, targetPosition);
    }

    /// <summary>
    /// Notifies the socket Manager that update the rotation of a turret
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="targetLookAt"></param>
    public void NotifyPlayerTurretRotation(string playerId, Vector3 targetLookAt)
    {
        this.OnPlayerTurretRotate(playerId, targetLookAt);
        SocketManager.instance.NotifyPlayerTurretRotation(playerId, targetLookAt);
    }

    /// <summary>
    /// Notifies the socket manager that a player initated an attack
    /// </summary>
    /// <param name="playerId"></param>
    public void NotifyPlayerAttack(string playerId, float force, Vector3 turretRotation)
    {
        this.OnPlayerAttack(playerId, force, turretRotation);
        SocketManager.instance.NotifyPlayerAttack(playerId, force, turretRotation);
    }

    /// <summary>
    /// Notifies the socket manager that a player has been damaged
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="newHealth"></param>
    public void NotifyPlayerDamaged(string attackerId, string otherPlayerId, int newHealth)
    {
        this.OnPlayerDamaged(attackerId, otherPlayerId, newHealth);
        SocketManager.instance.NotifyPlayerDamaged(attackerId, otherPlayerId, newHealth);
    }

    /// <summary>
    /// Notifies the socket manager that a player has been damaged
    /// </summary>
    /// <param name="playerId"></param>
    public void NotifyPlayerDefeated(string playerId)
    {
        this.OnPlayerDefeated(playerId);
        SocketManager.instance.NotifyPlayerDefeated(playerId);
    }

    /// <summary>
    /// Delays the notification that a game session is over
    /// </summary>
    /// <returns></returns>
    IEnumerator NotifyGameOver()
    {
        yield return new WaitForSeconds(this.gameOverDelay);

        // Get the winner of this session
        // Failsafe, defaults to this player in the event we can't determine who won
        string winnerId = this.LocalPlayerId;

        foreach (string activeUserId in this.activeUsers.Keys) {
            // User is not defeated...must be the winner
            if (!this.defeatedUsers.Contains(activeUserId)) {
                winnerId = activeUserId;
            }
        }

        SocketManager.instance.NotifyGameOver(winnerId);
    }

    /// <summary>
    /// Delays a redirect back to lobby
    /// </summary>
    /// <returns></returns>
    IEnumerator RedirectToLobby()
    {
        yield return new WaitForSeconds(this.redirectToLobbyDelay);
        Application.ExternalEval("location = '/leaders'");
    }
    #endregion

    #region Action Handlers
    /// <summary>
    /// Disables the title screen
    /// </summary>
    public void OnAllPlayersReady()
    {
        this.MainCanvas.HideTitleScreen();

        // Activate the players
        // Disable all players and cameras to be safe
        foreach (KeyValuePair<string, Player> activePlayer in this.activeUsers) {
            Player player = activePlayer.Value;
            player.IsDisabled = false;
        }

    }

    /// <summary>
    /// Notifies the given player to move towards the target position
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="targetPosition"></param>
    public void OnPlayerMove(string playerId, Vector3 targetPosition)
    {
        // Unknown user
        if (!this.activeUsers.ContainsKey(playerId)) {
            Debug.Log("Player " + playerId + " is not a registered user");
            return;
        }

        Player player = this.activeUsers[playerId];
        player.Move(targetPosition);
    }

    /// <summary>
    /// Triggers an updated of where the turret should be facing
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="targetLookAt"></param>
    public void OnPlayerTurretRotate(string playerId, Vector3 targetLookAt)
    {
        // Unknown user
        if (!this.activeUsers.ContainsKey(playerId)) {
            Debug.Log("Player " + playerId + " is not a registered user");
            return;
        }

        Player player = this.activeUsers[playerId];
        player.TurretLookAt = targetLookAt;
    }

    /// <summary>
    /// Triggers the specify player to attack
    /// </summary>
    /// <param name="playerId"></param>
    public void OnPlayerAttack(string playerId, float force, Vector3 turretRotation)
    {
        // Unknown user
        if (!this.activeUsers.ContainsKey(playerId)) {
            Debug.Log("Player " + playerId + " is not a registered user");
            return;
        }

        Player player = this.activeUsers[playerId];
        player.Attack(force, turretRotation);
    }

    /// <summary>
    /// Triggers the specify player to take damage
    /// The "attackerId" is mainly for notifications that says "this player successfully attacked this other one"
    /// It is currently unsused
    /// </summary>
    /// <param name="attackerId"></param>
    /// <param name="otherPlayerId"></param>
    /// <param name="newHealth"></param>
    public void OnPlayerDamaged(string attackerId, string otherPlayerId, int newHealth)
    {
        // Unknown user
        if (!this.activeUsers.ContainsKey(otherPlayerId)) {
            Debug.Log("Player " + otherPlayerId + " is not a registered user");
            return;
        }

        Player player = this.activeUsers[otherPlayerId];
        player.Damaged(newHealth);
    }

    /// <summary>
    /// Triggers the specify player's defeated state
    /// </summary>
    /// <param name="playerId"></param>
    public void OnPlayerDefeated(string playerId)
    {
        // Unknown user
        if (!this.activeUsers.ContainsKey(playerId)) {
            Debug.Log("Player " + playerId + " is not a registered user");
            return;
        }

        // Register the defeated user
        this.defeatedUsers.Add(playerId);

        Player player = this.activeUsers[playerId];
        player.Defeated(playerId);
    }

    /// <summary>
    /// Display either the victor or defeated screen depending if the winnerId
    /// matches or not then delay a redirect back to the lobby
    /// </summary>
    public void OnGameOver(string winnerId)
    {
        // Disable all players and cameras to be safe
        foreach (KeyValuePair<string,Player> activePlayer in this.activeUsers) {
            Player player = activePlayer.Value;
            player.IsDisabled = true;
        }

        if (winnerId == this.LocalPlayerId) {
            MainCanvas.ShowVictoryScreen();
        } else {
            MainCanvas.ShowDefeatedScreen();
        }

        StartCoroutine(this.RedirectToLobby());
    }
    #endregion
}
