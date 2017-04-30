using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (LandEnemy))]
public class LandEnemyAI : MonoBehaviour {

	LandEnemy enemy;
	Vector2 directionalAI;
	bool isToRest = false;
	float thumbStickAngle;

	void Start () {
		enemy = GetComponent<LandEnemy> ();

		directionalAI = new Vector2 (1, -1);
		enemy.SetDirectionalAI (directionalAI);
	}
	
	void FixedUpdate () {
		//int randomVal = Random.Range (0, 4);

		if(enemy.IsAtRest()){
			if (enemy.GetEnemyController2D ().collisions.facingDir == -1) {
				directionalAI = new Vector2 (1, 0);
			} else {
				directionalAI = new Vector2 (-1, 0);
			}
			enemy.GetEnemyController2D ().SetPathBlocked (false);
			enemy.SetAtRest (false);
			enemy.SetDirectionalAI (directionalAI);
		}
		/*
		if (randomVal == 0) {
			// TODO: Rest for a while (X time)
			isToRest = true;
		} else {
			isToRest = false;
		}

		enemy.updateRestState (isToRest);
		*/
	}
}
