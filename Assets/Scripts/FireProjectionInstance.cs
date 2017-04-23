using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectionInstance : MonoBehaviour {
	public float projectionSpeed = 0.66f;
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

	void Update () {
		if (isShooting) {
			Vector2 newPosition = Vector2.Lerp (transform.localPosition, 
				transform.right * 5f,
				Time.deltaTime * projectionSpeed);
			
			transform.localPosition = newPosition;
			mFireStance.updatePosition (transform.position);

		}
	}
}
