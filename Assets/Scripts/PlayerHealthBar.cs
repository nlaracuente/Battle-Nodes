using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the health bar ui display for the player
/// Shrinks and grows as the player takes/gets health
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    /// <summary>
    /// A reference to the parent player script
    /// </summary>
    Player player;
    Player ParentPlayer
    {
        get {
            if (this.player == null) {
                this.player = GetComponentInParent<Player>();
            }
            return this.player;
        }
    }

    /// <summary>
    /// Displays the current health
    /// </summary>
    void LateUpdate()
    {
        Vector3 localScale = this.transform.localScale;
        float xscale = (float)this.ParentPlayer.Health / (float)this.ParentPlayer.MaxHealth;
        this.transform.localScale = new Vector3(xscale, localScale.y, localScale.z);
    }
}
