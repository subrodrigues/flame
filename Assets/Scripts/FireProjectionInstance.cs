using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectionInstance : MonoBehaviour {
	public float projectionSpeed = 0.66f;
	public int fireRange = 30;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		//transform.Translate(Vector3.right * 5 * Time.deltaTime); 
		transform.localPosition = Vector3.Lerp (transform.localPosition, 
			new Vector3 (transform.localPosition.x + fireRange, transform.localPosition.y, transform.localPosition.z),
			Time.deltaTime * projectionSpeed);
	}
}
