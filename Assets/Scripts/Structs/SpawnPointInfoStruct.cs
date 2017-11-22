using UnityEngine;

/// <summary>
/// Contains spawn point info such as position and the color
/// to use for the player's tank
/// </summary>
[System.Serializable]
public struct SpawnPointInfoStruct
{
    /// <summary>
    /// The transform object that represents the tank's spawn point
    /// </summary>
    public Transform transform;

    /// <summary>
    /// The material to apply to the tank's renderer
    /// </summary>
    public Material material;

    /// <summary>
    /// The color to use for the gamer tag
    /// </summary>
    public Color gamerTagColor;
}
