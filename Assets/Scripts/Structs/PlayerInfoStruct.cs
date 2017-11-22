using UnityEngine;
/// <summary>
/// Container for player's basic information
/// </summary>

[System.Serializable]
public struct PlayerInfoStruct
{
    /// <summary>
    /// ID of the player
    /// </summary>
    public string id;

    /// <summary>
    /// Name of the player
    /// </summary>
    public string handle;

    /// <summary>
    /// Spawn point index
    /// </summary>
    public int spawnPoint;
}
