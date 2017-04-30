using UnityEngine;
using System.Collections;

[RequireComponent (typeof (LandEnemyController2D))]
public class LandEnemy : MonoBehaviour {

	const int DEFAULT_SPRITE_SCALE = 4;

	public float timeToJumpApex = 0.4f; // how long to reach highest point
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public Vector3 velocity;

	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;
	float moveSpeed = 6;

	float maxJumpVelocity;	
	float minJumpVelocity;	
	float gravity;
	float velocityXSmoothing;

	LandEnemyController2D controller;

	/** GameObject components */
	public Animator animator;
	public SpriteRenderer spriteRenderer;

	/** PlayerInput move logic variables */
	public Vector2 directionalAI;
	public bool isAtRest;

	void Start () {
		controller = GetComponent<LandEnemyController2D> ();
		animator = GetComponentInChildren<Animator> ();
		spriteRenderer = GetComponentInChildren<SpriteRenderer> ();

		gravity = - (2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJumpApex; 
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}
		
	public void SetDirectionalAI(Vector2 input){
		directionalAI = input;
	}

	void Update () {

		if (!isAtRest) {
			if (controller.IsPathBlocked ()) {
				ResetDirectionalAI ();
				isAtRest = true;
			}

			float targetVelocityX = directionalAI.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			int wallDirX = (controller.collisions.left) ? -1 : 1;

			AirLogic (directionalAI);
			velocity.y += gravity * Time.deltaTime;

			controller.Move (velocity * Time.deltaTime, directionalAI);
		}

		SetAnimatorState ();

		//OnDrawGizmos ();
	}

	void AirLogic (Vector2 input){
		if (controller.collisions.below) {
			velocity.y = 0;
		}
	}

	void Jump (){
		if (controller.collisions.below) {
			velocity.y = maxJumpVelocity;
		}
	}

	/**
	 * Set the proper Animator state in order to play the correct Animation
	 * */
	void SetAnimatorState () {
		if (directionalAI.x != 0 && !isAtRest) {
			animator.SetBool ("IsMoving", true);
			if (directionalAI.x > 0) {
				// Moving Right
				animator.transform.localScale = new Vector3 (DEFAULT_SPRITE_SCALE, DEFAULT_SPRITE_SCALE, DEFAULT_SPRITE_SCALE);
			}
			else {
				// Moving Left
				animator.transform.localScale = new Vector3 (-DEFAULT_SPRITE_SCALE, DEFAULT_SPRITE_SCALE, DEFAULT_SPRITE_SCALE);
			}
		}
		else {
			animator.SetBool ("IsMoving", false);
		}
	}
		
	public bool HasAGrip(){
		return controller.collisions.below;
	}

	public void ResetDirectionalAI(){
		directionalAI = new Vector2 (0, 0);
	}

	public void ShowRenderer(bool isToShow){
		if (isToShow && !spriteRenderer.enabled)
			spriteRenderer.enabled = true;
		
		if (!isToShow && spriteRenderer.enabled)
			spriteRenderer.enabled = false;	
	}

	public void UpdatePosition(Vector2 updatedPos){
		transform.position = updatedPos;
	}

	public void SetAtRest(bool isToRest){
		this.isAtRest = isToRest;
	}

	public bool IsAtRest(){
		return isAtRest;
	}

	public LandEnemyController2D GetEnemyController2D(){
		return controller;
	}
		
}
