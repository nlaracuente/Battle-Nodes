using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Designed to be mounted on an isometric camera rig
/// Follows the given target while keeping an isometric perpective
/// </summary>
public class IsometricCamera : MonoBehaviour
{
    /// <summary>
    /// The target to follow
    /// </summary>
    [SerializeField]
    Transform target;
    public Transform Target
    {
        set { this.target = value; }
    }

    /// <summary>
    /// How fast to track
    /// </summary>
    [SerializeField]
    float followSpeed = 5f;

    /// <summary>
    /// LateUpdate is preferred to give the target time to move first
    /// and allowing the camera to "trail" behind
    /// </summary>
    void LateUpdate()
    {
        if(this.target == null){
            return;
        }

        Vector3 targetPosition = this.target.position;
        this.transform.position = Vector3.Lerp(this.transform.position,
                                               targetPosition,
                                               this.followSpeed * Time.deltaTime);
    }
}
