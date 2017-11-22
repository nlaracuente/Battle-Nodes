using UnityEngine;

[System.Serializable]
public struct DamageStruct
{
    public string attackerId;
    public string damagedPlayerId;
    public int damage;

    public DamageStruct(string attackerId, string damagedPlayerId, int damage)
    {
        this.attackerId = attackerId;
        this.damagedPlayerId = damagedPlayerId;
        this.damage = damage;
    }

}
