using UnityEngine;
using System.Collections;
using UnitySocketIO;
using UnitySocketIO.Events;

/// <summary>
/// Manager for all network communications with socket io
/// </summary>
public class SocketManager : MonoBehaviour
{
    /// A reference to the SocketManager instance
    public static SocketManager instance = null;
    private Vector3 lastTargetToLookAt;

    /// A reference to the socket.io 
    [SerializeField]
    SocketIOController io;

    /// <summary>
    /// Sets the instance of the SocketManager and makes it persistent
    /// </summary>
    void Awake()
    {
        if (instance == null){
            instance = this;
        } else if (instance != this){
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

    #if UNITY_WEBGL
        string url = "battle-nodez.herokuapp.com";
        int port = 80;


        //string absoluteURL = Application.absoluteURL;
        //if(absoluteURL.Length > 0 && absoluteURL.IndexOf("herokuapp") == -1) {
        //    url = "battlenodes.net";
        //    port = 8080;
        //}

        Debug.Log("URL" + url);
        Debug.Log("Port" + port);

        io.settings.url = url;
        io.settings.port = port;
    #endif

        Debug.Log("SocketIO connected");
        io.On("connect", this.HandleConnect);
        io.On("identity", this.HandleIdentity);
        io.On("allplayersready", this.HandleAllPlayersReady);
        io.On("gameinfo", this.HandleGameInfo);

        io.On("playerMove", this.HandlePlayerMove);
        io.On("turretRotate", this.HandleTurretRotate);
        io.On("attack", this.HandleAttack);
        io.On("damage", this.HandleDamage);

        io.On("playerDefeated", this.HandlePlayerDefeated);
        io.On("gameOver", this.HandleGameOver);
        // io.On("gameover", this.HandleGameOver);

        io.On("playerLeft", this.HandlePlayerLeft);

        io.Connect();
    }

    void HandleConnect (SocketIOEvent e) {
        Debug.Log("SocketIO connected");
        if (string.IsNullOrEmpty(GameManager.instance.LocalPlayerId)) {
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            io.Emit("getmockidentity");
            GameManager.instance.OnAllPlayersReady();
        #endif

        #if UNITY_WEBGL
            io.Emit("getidentity");
        #endif
        }
    }

    void HandleIdentity(SocketIOEvent e)
    {
        Debug.Log("handleIdentity");
        Debug.Log(e.data);
        PlayerInfoStruct playerInfo = JsonUtility.FromJson<PlayerInfoStruct>(e.data);
        GameManager.instance.CreateLocalPlayer(playerInfo.id, playerInfo.handle, playerInfo.spawnPoint);
        //io.Emit("getgameinfo");
    }

    void HandleGameInfo(SocketIOEvent e)
    {
        Debug.Log("handleGameInfo");
        Debug.Log(e.data);
        GameInfoStruct gameInfo = JsonUtility.FromJson<GameInfoStruct>(e.data);
        Debug.Log(gameInfo.players);
        foreach(PlayerInfoStruct player in gameInfo.players)  {
            GameManager.instance.RegisterPlayer(player.id, player.handle, player.spawnPoint); 
        }
    }

    void HandleAllPlayersReady(SocketIOEvent e)
    {
        Debug.Log("********* allplayersready event *********");
        Debug.Log("handleAllPlayersReady");
        GameManager.instance.OnAllPlayersReady();
    }

    void HandlePlayerMove(SocketIOEvent e)
    {
        //Debug.Log("handlePlayerMove");
        //Debug.Log(e.data);
        PlayerMovementStruct playerMovement = JsonUtility.FromJson<PlayerMovementStruct>(e.data);
        GameManager.instance.OnPlayerMove(playerMovement.id, new Vector3(playerMovement.x, playerMovement.y, playerMovement.z));

    }

    void HandleTurretRotate(SocketIOEvent e)
    {
        //Debug.Log("handleTurretRotate");
        //Debug.Log(e.data);
        PlayerMovementStruct turretRotationMessage = JsonUtility.FromJson<PlayerMovementStruct>(e.data);
        GameManager.instance.OnPlayerTurretRotate(turretRotationMessage.id, new Vector3(turretRotationMessage.x, turretRotationMessage.y, turretRotationMessage.z));
    }

    void HandleAttack(SocketIOEvent e)
    {
        //Debug.Log("handleAttack");
        //Debug.Log(e.data);
        PlayerAttackStruct attackMessage = JsonUtility.FromJson<PlayerAttackStruct>(e.data);
        Vector3 turretRotation = new Vector3(attackMessage.x, attackMessage.y, attackMessage.z);
        GameManager.instance.OnPlayerAttack(attackMessage.id, attackMessage.force, turretRotation);
    }

    void HandleDamage(SocketIOEvent e)
    {
        Debug.Log("handleDamage");
        Debug.Log(e.data);
        DamageStruct damageMessage = JsonUtility.FromJson<DamageStruct>(e.data);
        GameManager.instance.OnPlayerDamaged(damageMessage.attackerId, damageMessage.damagedPlayerId, damageMessage.damage);
    }

    void HandlePlayerDefeated(SocketIOEvent e) {
        Debug.Log("handlePlayerDefeated");
        Debug.Log(e.data);
        PlayerIdStruct message = JsonUtility.FromJson<PlayerIdStruct>(e.data);
        GameManager.instance.OnPlayerDefeated(message.id);
    }

    void HandleGameOver(SocketIOEvent e)
    {
        Debug.Log("handleGameOver");
        Debug.Log(e.data);
        PlayerIdStruct message = JsonUtility.FromJson<PlayerIdStruct>(e.data);
        GameManager.instance.OnGameOver(message.id);
    }

    void HandlePlayerLeft(SocketIOEvent e)
    {
        Debug.Log("handlePlayerLeft");
        Debug.Log(e.data);
        PlayerDisconnectStruct disconnectMessage = JsonUtility.FromJson<PlayerDisconnectStruct>(e.data);
        GameManager.instance.UnregisterPlayer(disconnectMessage.id);
    }

    public void NotifyPlayerReady(string id)
    {
        PlayerIdStruct message = new PlayerIdStruct(id);
        io.Emit("playerready", JsonUtility.ToJson(message));
    }

    public void NotifyPlayerMove(string id, Vector3 destination) {
        PlayerMovementStruct moveMessage = new PlayerMovementStruct(id, destination.x, destination.y, destination.z);
        io.Emit("playerMove", JsonUtility.ToJson(moveMessage));
    }

    public void NotifyPlayerTurretRotation(string playerId, Vector3 targetLookAt) {
        if(lastTargetToLookAt != targetLookAt) {
            PlayerMovementStruct turretRotationMessage = new PlayerMovementStruct(playerId, targetLookAt.x, targetLookAt.y, targetLookAt.z);
            io.Emit("turretRotate", JsonUtility.ToJson(turretRotationMessage));
            lastTargetToLookAt = targetLookAt;
        }
    }

    public void NotifyPlayerAttack(string playerId, float force, Vector3 turretRotation) {
        PlayerAttackStruct attackMessage = new PlayerAttackStruct(playerId, turretRotation.x, turretRotation.y, turretRotation.z, force);
        io.Emit("attack", JsonUtility.ToJson(attackMessage));
    }

    public void NotifyPlayerDamaged(string attackerId, string damagedPlayerId, int damage) {
        DamageStruct damageMessage = new DamageStruct(attackerId, damagedPlayerId, damage);
        io.Emit("damage", JsonUtility.ToJson(damageMessage));
    }

    public void NotifyPlayerDefeated(string id)
    {
        PlayerIdStruct message = new PlayerIdStruct(id);
        io.Emit("playerDefeated", JsonUtility.ToJson(message));
    }

    public void NotifyGameOver(string id)
    {
        PlayerIdStruct message = new PlayerIdStruct(id);
        io.Emit("gameOver", JsonUtility.ToJson(message));
        GameManager.instance.OnGameOver(id);
    }

}
[System.Serializable]
public struct PlayerIdStruct
{
    /// <summary>
    /// ID of the player
    /// </summary>
    public string id;

    public PlayerIdStruct(string id)
    {
        this.id = id;
    }
}