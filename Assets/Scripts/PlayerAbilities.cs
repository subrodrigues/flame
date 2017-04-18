using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour {
	// Instantiates a Fire Projection every 0.5 seconds
	public float fireRate = 0.5f;
	private float nextFire = 0.0f;
	public GameObject mProjectile;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton ("Fire1") && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			FireProjection ();
		}
	}

	void FireProjection(){
		GameObject clone = (GameObject) Instantiate (mProjectile, transform.position, Quaternion.identity);
		Destroy (clone, 2);
	}
}
