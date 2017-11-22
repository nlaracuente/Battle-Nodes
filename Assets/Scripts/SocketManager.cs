using UnityEngine;
using System.Collections;
using UnitySocketIO;
using UnitySocketIO.Events;

public class SocketManager : MonoBehaviour
{
    /// A reference to the SocketManager instance
    public static SocketManager instance = null;

    /// A reference to the socket.io 
    [SerializeField]
    SocketIOController io;

    private Vector3 lastTargetToLookAt;

    /// Sets the instance of the SocketManager and makes it persistent
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

#if UNITY_WEBGL
        string url = "battle-nodes.herokuapp.com";
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
        io.On("connect", handleConnect);
        io.On("identity", handleIdentity);
        io.On("allplayersready", handleAllPlayersReady);
        io.On("gameinfo", handleGameInfo);

        io.On("playerMove", handlePlayerMove);
        io.On("turretRotate", handleTurretRotate);
        io.On("attack", handleAttack);
        io.On("damage", handleDamage);

        io.On("playerDefeated", handlePlayerDefeated);
        io.On("gameOver", handleGameOver);
        io.On("gameover", handleGameOver);

        io.On("playerLeft", handlePlayerLeft);

        io.Connect();
    }

    void handleConnect (SocketIOEvent e) {
        Debug.Log("SocketIO connected");
        if (string.IsNullOrEmpty(GameManager.instance.LocalPlayerId))
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            io.Emit("getmockidentity");
            io.Emit("playerready");
            GameManager.instance.OnAllPlayersReady();
#else
            io.Emit("getidentity");
            io.Emit("playerready");
#endif
        }
    }

    void handleIdentity(SocketIOEvent e)
    {
        Debug.Log("handleIdentity");
        Debug.Log(e.data);
        PlayerInfoStruct playerInfo = JsonUtility.FromJson<PlayerInfoStruct>(e.data);
        GameManager.instance.CreateLocalPlayer(playerInfo.id, playerInfo.handle, playerInfo.spawnPoint);
        //io.Emit("getgameinfo");
    }

    void handleGameInfo(SocketIOEvent e)
    {
        Debug.Log("handleGameInfo");
        Debug.Log(e.data);
        GameInfoStruct gameInfo = JsonUtility.FromJson<GameInfoStruct>(e.data);
        Debug.Log(gameInfo.players);
        foreach(PlayerInfoStruct player in gameInfo.players)  {
            GameManager.instance.RegisterPlayer(player.id, player.handle, player.spawnPoint); 
        }
    }

    void handleAllPlayersReady(SocketIOEvent e)
    {
        Debug.Log("handleAllPlayersReady");
        GameManager.instance.OnAllPlayersReady();
    }

    void handlePlayerMove(SocketIOEvent e)
    {
        //Debug.Log("handlePlayerMove");
        //Debug.Log(e.data);
        PlayerMovementStruct playerMovement = JsonUtility.FromJson<PlayerMovementStruct>(e.data);
        GameManager.instance.OnPlayerMove(playerMovement.id, new Vector3(playerMovement.x, playerMovement.y, playerMovement.z));

    }

    void handleTurretRotate(SocketIOEvent e)
    {
        //Debug.Log("handleTurretRotate");
        //Debug.Log(e.data);
        PlayerMovementStruct turretRotationMessage = JsonUtility.FromJson<PlayerMovementStruct>(e.data);
        GameManager.instance.OnPlayerTurretRotate(turretRotationMessage.id, new Vector3(turretRotationMessage.x, turretRotationMessage.y, turretRotationMessage.z));
    }

    void handleAttack(SocketIOEvent e)
    {
        //Debug.Log("handleAttack");
        //Debug.Log(e.data);
        PlayerAttackStruct attackMessage = JsonUtility.FromJson<PlayerAttackStruct>(e.data);
        GameManager.instance.OnPlayerAttack(attackMessage.id, attackMessage.force);
    }

    void handleDamage(SocketIOEvent e)
    {
        Debug.Log("handleDamage");
        Debug.Log(e.data);
        DamageStruct damageMessage = JsonUtility.FromJson<DamageStruct>(e.data);
        GameManager.instance.OnPlayerDamaged(damageMessage.attackerId, damageMessage.damagedPlayerId, damageMessage.damage);
    }


    //handlePlayerDefeated
    //handleGameOver

    void handlePlayerDefeated(SocketIOEvent e) {
        Debug.Log("handlePlayerDefeated");
        Debug.Log(e.data);
        PlayerIdStruct message = JsonUtility.FromJson<PlayerIdStruct>(e.data);
        GameManager.instance.OnPlayerDefeated(message.id);
    }

    void handleGameOver(SocketIOEvent e)
    {
        Debug.Log("handleGameOver");
        Debug.Log(e.data);
        PlayerIdStruct message = JsonUtility.FromJson<PlayerIdStruct>(e.data);
        GameManager.instance.OnGameOver(message.id);
    }


    void handlePlayerLeft(SocketIOEvent e)
    {
        Debug.Log("handlePlayerLeft");
        Debug.Log(e.data);
        PlayerDisconnectStruct disconnectMessage = JsonUtility.FromJson<PlayerDisconnectStruct>(e.data);
        GameManager.instance.UnregisterPlayer(disconnectMessage.id);
    }

    public void notifyPlayerMove(string id, Vector3 destination) {
        PlayerMovementStruct moveMessage = new PlayerMovementStruct(id, destination.x, destination.y, destination.z);
        io.Emit("playerMove", JsonUtility.ToJson(moveMessage));
    }

    public void notifyPlayerTurretRotation(string playerId, Vector3 targetLookAt) {
        if(lastTargetToLookAt != targetLookAt) {
            PlayerMovementStruct turretRotationMessage = new PlayerMovementStruct(playerId, targetLookAt.x, targetLookAt.y, targetLookAt.z);
            io.Emit("turretRotate", JsonUtility.ToJson(turretRotationMessage));
            lastTargetToLookAt = targetLookAt;
        }
    }

    public void notifyPlayerAttack(string playerId, Vector3 direction, float force) {
        PlayerAttackStruct attackMessage = new PlayerAttackStruct(playerId, direction.x, direction.y, direction.z, force);
        io.Emit("attack", JsonUtility.ToJson(attackMessage));
    }

    public void notifyPlayerDamaged(string attackerId, string damagedPlayerId, int damage) {
        DamageStruct damageMessage = new DamageStruct(attackerId, damagedPlayerId, damage);
        io.Emit("damage", JsonUtility.ToJson(damageMessage));
    }

    public void notifyGameOver(string id)
    {
        PlayerIdStruct message = new PlayerIdStruct(id);
        io.Emit("gameOver", JsonUtility.ToJson(message));
        io.Emit("gameover", JsonUtility.ToJson(message));
    }

    public void notifyPlayerDefeated(string id)
    {
        PlayerIdStruct message = new PlayerIdStruct(id);
        io.Emit("playerDefeated", JsonUtility.ToJson(message));
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