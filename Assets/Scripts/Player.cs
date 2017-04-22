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

	public Animator playerAnimator;

	/** Jump Logic Variables */
	public Vector2 directionalInput;
	public bool isJumpInputDownPressed;
	public bool isJumpInputUpPressed;

	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = - (2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2); // deltaMovement = initialVelocity * time + ((acceleration * pow(time, 2)) / 2)
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJumpApex; // finalVelocity = initialVelocity + acceleration * time
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		print ("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
	}
	
	void Update () {
		
		SetAnimatorState ();

		int wallDirX = (controller.collisions.left) ? -1 : 1;
		bool wallSliding = WallSlidingLogic (directionalInput, wallDirX);
		JumpLogic (directionalInput, wallDirX, wallSliding);

		CalculateVelocity ();

		controller.Move (velocity * Time.deltaTime, directionalInput, isJumpInputDownPressed);
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
			velocity.y = 0;
			playerAnimator.SetBool ("IsJumping", false);
		}
	}

	void CalculateVelocity (){
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	public void SetDirectionalInput(Vector2 input){
		directionalInput = input;
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
				velocity.y = maxJumpVelocity;
			}
		}
		if (isJumpInputUpPressed) {
			if(velocity.y > minJumpVelocity)
				velocity.y = minJumpVelocity;
		}

	//	print ("vel y: " + velocity.y + " - " + minJumpVelocity);
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
}
