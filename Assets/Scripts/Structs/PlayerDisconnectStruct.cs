using UnityEngine;
/// <summary>
/// Container for player's disconnect information
/// </summary>

[System.Serializable]
public struct PlayerDisconnectStruct
{
    /// <summary>
    /// ID of the player
    /// </summary>
    public string id;

    /// <summary>
    /// Server timestamp when user was disconnected
    public string timstamp;
}