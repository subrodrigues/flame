using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public Vector2 wallJumpClimb, wallJumpOff, wallLeap;

	public float jumpHeight = 3;
	public float timeToJumpApex = 0.4f; // how long to reach highest point

	public float wallSlidingSpeedMax = 3;

	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;
	float moveSpeed = 6;


	float gravity;
	float jumpVelocity;	
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = - (2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2); // deltaMovement = initialVelocity * time + ((acceleration * pow(time, 2)) / 2)
		jumpVelocity = Mathf.Abs (gravity) * timeToJumpApex; // finalVelocity = initialVelocity + acceleration * time
		print ("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
	}

	// TODO: WALL STICK. 9m18s
	// https://www.youtube.com/watch?v=46WNb1Aucyg
	// Update is called once per frame
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) &&
			!controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if(velocity.y < -wallSlidingSpeedMax){
				velocity.y = -wallSlidingSpeedMax;
			}
		}

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		if (Input.GetButtonDown ("Jump")) {
			if(wallSliding){
				if(wallDirX == input.x){ // If we are moving in same direction as wall that facing
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if(velocity.x == 0){ // off the wall
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else{ // leap
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}

			}
			if(controller.collisions.below){
				velocity.y = jumpVelocity;
			}
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, input);
	}
}
