using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectionInstance : MonoBehaviour {
	public float projectionSpeed = 0.01f;
	public bool isShooting = false;
	FireStance mFireStance;

	void Start () {
		mFireStance = transform.parent.GetComponent<FireStance> ();
	}

	public void ShootProjectile(){
		if (!isShooting) {
			isShooting = true;
		}
	}

	public void destroySelf(){
		if (!isShooting) {
			Destroy (this);
		}
	}

	void FixedUpdate () {
		if (isShooting) {
			mFireStance.fireProjectionUpdatedPosition (transform.right * projectionSpeed);
		}
	}
}
