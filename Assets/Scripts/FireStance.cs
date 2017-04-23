using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStance : MonoBehaviour {
	
	Player player;
	public GameObject fireProjectionArrowPrefab;
	public const float FIRE_PROJECTILE_TIME_TO_LIVE = 0.3f; // In seconds

	private bool isInShootingProjectileMode = false;
	GameObject fireProjectionArrowInstance = null;

	void Start () {
		player = transform.parent.GetComponent<Player> ();
	}

	public bool EnableFireArrowInstance(bool isToShow){
		if (IsInShootingProjectileMode () || !player.HasAGrip())
			return false;
		
		if (isToShow) {
			fireProjectionArrowInstance = (GameObject) Instantiate (fireProjectionArrowPrefab, transform.position, Quaternion.identity);
			fireProjectionArrowInstance.transform.parent = transform;

			GetComponent<Renderer> ().enabled = false;
		} else {
			Destroy (fireProjectionArrowInstance);
			GetComponent<Renderer> ().enabled = true;
		}

		return true;
	}

	public void UpdateFireArrowDirection(float angle){
		if (fireProjectionArrowInstance == null) {
			if (!EnableFireArrowInstance (true))
				return;
		}

		fireProjectionArrowInstance.transform.rotation = Quaternion.Euler (0, 0, angle);
	}

	public void UpdateFireProjectionStance(bool isToEnter){
		if (IsInShootingProjectileMode () || !player.HasAGrip())
			return;
		
		if (isToEnter && !GetComponent<Renderer> ().enabled && fireProjectionArrowInstance == null ) {
			player.resetDirectionalInput ();
			player.showPlayerRenderer(false);
			GetComponent<Renderer> ().enabled = true;
		} else if (!isToEnter) {
			if (fireProjectionArrowInstance != null) {
				isInShootingProjectileMode = true;
				fireProjectionArrowInstance.GetComponent<FireProjectionInstance> ().ShootProjectile ();

				Invoke ("ExitFireProjectionStance", FIRE_PROJECTILE_TIME_TO_LIVE);
			} else {
				ExitFireProjectionStance ();
			}
		}
	//	print ("MEH Fire Stance");

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

	public void updatePosition(Vector2 updatedPos){
		player.updatePosition (updatedPos);
	}
}
