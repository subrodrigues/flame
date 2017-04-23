using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

	Player player;
	Vector2 directionalInput;
	float thumbStickAngle;

	void Start () {
		player = GetComponent<Player> ();
	}
	
	void Update () {
		directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetButton ("Fire1")) {
			player.GetFireStance().UpdateFireProjectionStance (true);

			if (directionalInput.x == 0 && directionalInput.y == 0) {
				player.GetFireStance().EnableFireArrowInstance (false);
			} else {
				thumbStickAngle = Mathf.Atan2(directionalInput.y, directionalInput.x) * Mathf.Rad2Deg;
				player.GetFireStance().UpdateFireArrowDirection (thumbStickAngle);
			}
		} else {
			player.GetFireStance().UpdateFireProjectionStance (false);
			player.SetDirectionalInput (directionalInput);

			if (Input.GetButtonDown ("Jump")) {
				player.OnJumpInputDown (true);
			} else {
				player.OnJumpInputDown (false);
			}
			if (Input.GetButtonUp ("Jump")) {
				player.OnJumpInputUp (true);
			} else {
				player.OnJumpInputUp (false);
			} 
		}
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 1f);

		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position, directionalInput.normalized);

		Gizmos.color = Color.green;
		Gizmos.DrawRay(transform.position, directionalInput);
	}
}
