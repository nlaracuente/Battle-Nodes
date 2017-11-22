
/// <summary>
/// Container for player's movement
/// </summary>

[System.Serializable]
public struct PlayerAttackStruct
{
    /// <summary>
    /// id of the player
    /// </summary>
    public string id;

    /// <summary>
    /// Shot vector
    /// </summary>
    public float x, y, z;

    public float force;

    public PlayerAttackStruct(string _id, float _x, float _y, float _z, float _force)
    {
        this.id = _id;
        this.x = _x;
        this.y = _y;
        this.z = _z;
        this.force = _force;
    }

}
