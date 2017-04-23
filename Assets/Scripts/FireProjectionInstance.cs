using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectionInstance : MonoBehaviour {
	public float projectionSpeed = 0.66f;
	public int fireRange = 20;
	public bool isShooting = false;

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
			transform.localPosition = Vector2.Lerp (transform.localPosition, 
				transform.right * 35f,
				Time.deltaTime * projectionSpeed);
		}
	}
}
