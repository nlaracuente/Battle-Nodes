using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all player input 
/// This script is assigned only to the local player object
/// </summary>
[RequireComponent(typeof(Player))]
public class LocalPlayerController : MonoBehaviour
{
    /// <summary>
    /// Controls the limits for the player's attack
    /// </summary>
    const float MIN_FORCE = 20;
    const float MAX_FORCE = 25;
    const float FORCE_TIME_MULTIPLIER = 30;

    /// <summary>
    /// A reference to the
    /// </summary>
    Player player;

    /// <summary>
    /// The layer mask containing the ground
    /// </summary>
    [SerializeField]
    LayerMask groundMask;

    /// <summary>
    /// How far to cast the ray from the mouse to the game world
    /// </summary>
    [SerializeField]
    float rayDistance = 100f;

    /// <summary>
    /// True while the player is holding down the attack power
    /// </summary>
    bool isCharging = false;

    /// <summary>
    /// How much force to apply when shooting
    /// </summary>
    [SerializeField]
    float fireForce = 20;

    /// <summary>
    /// How long the player must wait before they can shoot again
    /// </summary>
    [SerializeField]
    float attackCoolDownDelay = .30f;

    /// <summary>
    /// True when the player can shoot again
    /// </summary>
    [SerializeField]
    bool canShoot = true;

    /// <summary>
    /// A reference to the attack being charged
    /// </summary>
    SoundClipStruct chargeSound;

    /// <summary>
    /// Initialize
    /// </summary>
    private void Start()
    {
        this.player = GetComponent<Player>();
        this.groundMask = this.player.GroundMask;
        this.rayDistance = this.player.RayDistance;
        this.chargeSound = this.player.ChargeSound;

        // Only the local player has an audio listener
        this.gameObject.AddComponent<AudioListener>();
    }

    /// <summary>
    /// Handles player inputs
    /// </summary>
	void Update()
    {
        if (this.player.IsDisabled) {
            return;
        }

        // Creates a ray that starts for the mouse' current position
        // setting the direciton towards the ground
        Ray ray = this.player.IsoCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool collidedWithGround = Physics.Raycast(ray, out hit, this.rayDistance, this.groundMask);

        // Move
        if (collidedWithGround && Input.GetButtonDown("Fire2")) {
            Debug.DrawRay(ray.origin, ray.direction * this.rayDistance, Color.red, .25f);
            GameManager.instance.NotifyPlayerMovement(GameManager.instance.LocalPlayerId, hit.point);
        }

        // Store Turret Rotation
        if (collidedWithGround) {
            Vector3 lookAtPos = hit.point - this.player.TurretGO.transform.position;
            lookAtPos.y = 0f;
            this.player.TurretLookAt = lookAtPos;
        }

        //// Starting charging up
        //if (collidedWithGround && Input.GetButtonDown("Fire1")) {
        //    isCharging = true;
        //    fireForce = MIN_FORCE;
        //    GameManager.instance.PlaySound(this.chargeSound.clip, this.chargeSound.volume, this.transform.position);
        //}

        // Shoot!
        if (this.canShoot && collidedWithGround && Input.GetButtonUp("Fire1")) {
            this.canShoot = false;
            StartCoroutine(this.AttackCoolDown());
            GameManager.instance.NotifyPlayerAttack(GameManager.instance.LocalPlayerId, this.fireForce, this.player.TurretLookAt);
            // isCharging = false;
        }

        //if(!collidedWithGround) {
        //    isCharging = false;
        //}

        //// Charging up in progress
        //if(isCharging) {
        //    fireForce += Time.deltaTime * FORCE_TIME_MULTIPLIER;
        //    if (fireForce > MAX_FORCE) {
        //        fireForce = MAX_FORCE;
        //    }
        //}
    }

    /// <summary>
    /// Waits the predermine cool down time before allowing the player to shoot again
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(this.attackCoolDownDelay);
        this.canShoot = true;
    }
}
