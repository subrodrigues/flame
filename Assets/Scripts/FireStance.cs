using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStance : MonoBehaviour {
	
	Player player;
	public GameObject fireProjectionArrowPrefab;

	private bool isInShootingProjectileMode = false;
	GameObject fireProjectionArrowInstance = null;

	void Start () {
		player = transform.parent.GetComponent<Player> ();
	}

	public void EnableFireArrowInstance(bool isToShow){
		if (isToShow) {
			fireProjectionArrowInstance = (GameObject) Instantiate (fireProjectionArrowPrefab, transform.position, Quaternion.identity);
			fireProjectionArrowInstance.transform.parent = transform;

			GetComponent<Renderer> ().enabled = false;
		} else {
			Destroy (fireProjectionArrowInstance);
			GetComponent<Renderer> ().enabled = true;
		}
	}

	public void UpdateFireArrowDirection(float angle){
		if (fireProjectionArrowInstance == null)
			EnableFireArrowInstance (true);

		fireProjectionArrowInstance.transform.rotation = Quaternion.Euler (0, 0, angle);
	}

	public void UpdateFireProjectionStance(bool isToEnter){
		if (isToEnter && 
			(!GetComponent<Renderer> ().enabled && fireProjectionArrowInstance == null)) {

			player.resetDirectionalInput ();
			player.showPlayerRenderer(false);
			GetComponent<Renderer> ().enabled = true;
		} else if (!isToEnter) {
			if (fireProjectionArrowInstance != null) {
				isInShootingProjectileMode = true;
				fireProjectionArrowInstance.GetComponent<FireProjectionInstance> ().ShootProjectile ();

				Invoke ("ExitFireProjectionStance", 0.4f);
			} else {
				ExitFireProjectionStance ();
			}
		}
	}

	void ExitFireProjectionStance (){
		if (fireProjectionArrowInstance != null) {
			isInShootingProjectileMode = false;
			Destroy (fireProjectionArrowInstance);
		}
		player.showPlayerRenderer(true);

		if (GetComponent<Renderer> ().enabled)
			GetComponent<Renderer> ().enabled = false;
	}

	public bool IsInShootingProjectileMode(){
		return isInShootingProjectileMode;
	}
}
