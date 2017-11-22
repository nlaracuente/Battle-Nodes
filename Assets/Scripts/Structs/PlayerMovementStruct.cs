
/// <summary>
/// Container for player's movement
/// </summary>

[System.Serializable]
public struct PlayerMovementStruct
{
    /// <summary>
    /// id of the player
    /// </summary>
    public string id;

    /// <summary>
    /// Player's position
    /// </summary>
    public float x, y, z;

    public PlayerMovementStruct(string _id, float _x, float _y, float _z)
    {
        this.id = _id;
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }

}
