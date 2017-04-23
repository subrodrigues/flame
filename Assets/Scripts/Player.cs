using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public Vector2 wallJumpClimb, wallJumpOff, wallLeap;

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;

	public float timeToJumpApex = 0.4f; // how long to reach highest point

	public float wallSlidingSpeedMax = 3;
	public float wallStickTime = 0.025f; // seconds
	float timeToWallUnstick; 

	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;
	float moveSpeed = 6;

	float gravity;
	float maxJumpVelocity;	
	float minJumpVelocity;	
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	/** Caleo GameObject components */
	public Animator playerAnimator;
	public SpriteRenderer playerRenderer;
	public FireStance fireStance;

	/** PlayerInput move logic variables */
	public Vector2 directionalInput;
	public bool isJumpInputDownPressed;
	public bool isJumpInputUpPressed;

	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = - (2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2); // deltaMovement = initialVelocity * time + ((acceleration * pow(time, 2)) / 2)
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJumpApex; // finalVelocity = initialVelocity + acceleration * time
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}
		
	public void SetDirectionalInput(Vector2 input){
		directionalInput = input;
	}

	void Update () {
		// Update Caleo data when not in Shooting Projectile Stance
		if (!fireStance.IsInShootingProjectileMode()) {
			
			float targetVelocityX = directionalInput.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

			SetAnimatorState ();

			int wallDirX = (controller.collisions.left) ? -1 : 1;
			bool wallSliding = WallSlidingLogic (directionalInput, wallDirX);
			JumpLogic (directionalInput, wallDirX, wallSliding);

			velocity.y += gravity * Time.deltaTime;

			controller.Move (velocity * Time.deltaTime, directionalInput, isJumpInputDownPressed);
		}
	}

	public FireStance GetFireStance(){
		return fireStance;
	}

	/**
	 * Set the proper Animator state in order to play the correct Animation
	 * */
	void SetAnimatorState () {
		if (directionalInput.x != 0) {
			playerAnimator.SetBool ("IsMoving", true);
			if (directionalInput.x > 0) {
				// Moving Right
				playerAnimator.transform.localScale = new Vector3 (1, 1, 1);
			}
			else {
				// Moving Left
				playerAnimator.transform.localScale = new Vector3 (-1, 1, 1);
			}
		}
		else {
			playerAnimator.SetBool ("IsMoving", false);
		}
		if (controller.collisions.above || controller.collisions.below) {
			playerAnimator.SetBool ("IsJumping", false);

			UpdateVelocityYOnSuddenTopBottomCollision ();
		}
	}

	void UpdateVelocityYOnSuddenTopBottomCollision (){
		if (controller.collisions.slidingDownMaxSlope) {
			velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
		}
		else {
			velocity.y = 0;
		}
	}

	public void OnJumpInputDown(bool isPressed) {
		isJumpInputDownPressed = isPressed;
	}

	public void OnJumpInputUp(bool isPressed){
		isJumpInputUpPressed = isPressed;
	}

	/** 
	 * Method that deals with Jump Logic 
	*/
	void JumpLogic (Vector2 input, int wallDirX, bool wallSliding){
		if (isJumpInputDownPressed && 
			(!controller.collisions.abovePassThroughPlatform || input.y != -1)) { // only jump if the user doesn't want to go down out of a platform

			playerAnimator.SetBool ("IsJumping", true);

			if (wallSliding) {

				if (wallDirX == input.x) {
					// If we are moving in same direction as wall that facing
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if (input.x == 0) {
					// no horizontal input: jump off the wall
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else {
					// horizontal input is opposite from wall: we do a leap
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}
			if (controller.collisions.below) {
				if (controller.collisions.slidingDownMaxSlope) {
					if(directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)){ // not jumping against MaxSlope
						velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
						velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
					}
				} else {
					velocity.y = maxJumpVelocity;
				}
			}
		}

		if (isJumpInputUpPressed) {
			if(velocity.y > minJumpVelocity)
				velocity.y = minJumpVelocity;
		}

	//	print ("vel y: " + velocity.y + " - " + minJumpVelocity);
	}

	public bool HasAGrip(){
		return controller.collisions.below && !controller.collisions.slidingDownMaxSlope;
	}

	/*
	 * Method that deals with Wall Sliding Logic. 
	 * Updates velocity Vector and deals with Stick/Unstick wall jumping leap
	 */
	bool WallSlidingLogic (Vector2 input, int wallDirX) {
		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && 
				!controller.collisions.below && velocity.y < 0) {
			wallSliding = true;
			if (velocity.y < -wallSlidingSpeedMax) {
				velocity.y = -wallSlidingSpeedMax;
			}
			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}
		}
		return wallSliding;
	}

	public void resetDirectionalInput(){
		directionalInput = new Vector2 (0, 0);
	}

	public void showPlayerRenderer(bool isToShow){
		if (isToShow && !playerRenderer.enabled)
			playerRenderer.enabled = true;
		
		if (!isToShow && playerRenderer.enabled)
			playerRenderer.enabled = false;	
	}

	public void updatePosition(Vector2 updatedPos){
		transform.position = updatedPos;
	}
}
