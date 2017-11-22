using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Forces the canvas to always rotates to face the camera
/// </summary>
public class PlayerCanvas : MonoBehaviour
{
    /// <summary>
    /// A reference to the parent player script
    /// </summary>
    Player player;
    Player ParentPlayer
    {
        get {
            if(this.player == null) {
                this.player = GetComponentInParent<Player>();
            }
            return this.player;
        }
    }

    /// <summary>
    /// Sets the rotation of this object to always face the player's camera
    /// </summary>
    private void LateUpdate()
    {
        Player localPlayer = GameManager.instance.LocalPlayer;
        if(localPlayer) {
            this.transform.rotation = Quaternion.LookRotation(localPlayer.IsoCamera.transform.forward);    
        }

    }
}
