using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public Vector2 wallJumpClimb, wallJumpOff, wallLeap;

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;

	public float timeToJumpApex = 0.4f; // how long to reach highest point

	public float wallSlidingSpeedMax = 3;
	public float wallStickTime = 0.25f; // seconds
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

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = - (2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2); // deltaMovement = initialVelocity * time + ((acceleration * pow(time, 2)) / 2)
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJumpApex; // finalVelocity = initialVelocity + acceleration * time
		minJumpHeight = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpVelocity);

		print ("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		// Player Animator 
		if (input.x != 0) {
			playerAnimator.SetBool ("IsMoving", true);

			if(input.x > 0){ // Moving Right
				playerAnimator.transform.localScale = new Vector3(1, 1, 1);
			}
			else{ // Moving Left
				playerAnimator.transform.localScale = new Vector3(-1, 1, 1);
			}
		} else {
			playerAnimator.SetBool ("IsMoving", false);
		}

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		bool wallSliding = WallSlidingLogic (input, wallDirX);
		JumpLogic (input, wallDirX, wallSliding);

		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, input, Input.GetButtonDown ("Jump"));
	}

	/*
	 * Method that deald with Jump Logic and updates velocity Vector
	 */
	void JumpLogic (Vector2 input, int wallDirX, bool wallSliding)
	{
		if (Input.GetButtonDown ("Jump") && 
		    (!controller.collisions.abovePassThroughPlatform || input.y != -1)) { // only jump if the user doesn't want to go down out of a platform
			if (wallSliding) {
				if (wallDirX == input.x) {
					// If we are moving in same direction as wall that facing
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else
					if (velocity.x == 0) {
					// off the wall
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else {
					// leap
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}
		if (Input.GetButtonUp ("Jump")) {
			if(velocity.y > minJumpVelocity)
				velocity.y = minJumpVelocity;
		}
	}

	/*
	 * Method that deald with Wall Sliding Logic. 
	 * Updates velocity Vector and deals with Leap and Stick/Unstick wall jumping
	 */
	bool WallSlidingLogic (Vector2 input, int wallDirX)
	{
		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
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
