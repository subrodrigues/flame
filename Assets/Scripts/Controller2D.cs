using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {
	
	public Vector2 playerInput;
	private bool isJumpPressed = false;

	float maxSlopeAngle = 75;

	public CollisionInfo collisions;

	// Use this for initialization
	public virtual void Awake () {
		collider = GetComponent<BoxCollider2D> ();	// Camera needs this to be initialized at Awake
	}

	public override void Start(){
		base.Start ();
		collisions.facingDir = 1;
	}

	public void Move (Vector2 deltaMove, bool standingOnPlatform = false){
		Move (deltaMove, new Vector2(), false, standingOnPlatform);
	}

	public void Move (Vector2 deltaMove, Vector2 input){
		Move (deltaMove, input, false);
	}

	public void Move (Vector2 deltaMove, Vector2 input, bool jumpPressed, bool standingOnPlatform = false){
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.oldVelocity = deltaMove;
		this.playerInput = input;
		this.isJumpPressed = jumpPressed;

		if (deltaMove.y < 0) {
			DescendSlope(ref deltaMove);
		}

		// Wall check
		if (deltaMove.x != 0) {
			collisions.facingDir = (int) Mathf.Sign(deltaMove.x);
		}

		HorizontalCollisions (ref deltaMove);
		if (deltaMove.y != 0) {
			VerticalCollisions (ref deltaMove);
		}

		transform.Translate (deltaMove);

		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	void ClimbSlope (ref Vector2 deltaMove, float slopeAngle){
		float moveDistance = Mathf.Abs (deltaMove.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (deltaMove.y <= climbVelocityY) {
			deltaMove.y = climbVelocityY;
			deltaMove.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (deltaMove.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope (ref Vector2 deltaMove){
		
		float xDir = Mathf.Sign (deltaMove.x);
		Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
		
		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
				if (Mathf.Sign(hit.normal.x) == xDir) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x)) {
						float moveDistance = Mathf.Abs(deltaMove.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						deltaMove.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (deltaMove.x);
						deltaMove.y -= descendVelocityY;
						
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void HorizontalCollisions(ref Vector2 deltaMove){
	//	float xDir = Mathf.Sign (deltaMove.x);
		// to wall jump
		float xDir = collisions.facingDir;

		float rayLength = Mathf.Abs (deltaMove.x) + skinWidth;

		// In order to detect the wall at wall jump, enlarge length collider
		if (Mathf.Abs (deltaMove.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * xDir, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * xDir, Color.red);

			if(hit){

				if(hit.distance == 0 || hit.collider.tag == "Pass Through"){
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				// If climbing
				if(i == 0 && slopeAngle <= maxSlopeAngle){
					if(collisions.descendingSlope){
						collisions.descendingSlope = false;
						deltaMove = collisions.oldVelocity;
					}

					float distanceToSlopStart = 0;

					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopStart = hit.distance - skinWidth;
						deltaMove.x -= distanceToSlopStart * xDir;
					}
					ClimbSlope(ref deltaMove, slopeAngle);
					print(slopeAngle);

					deltaMove.x += distanceToSlopStart * xDir;
				}
				if(!collisions.climbingSlope || slopeAngle > maxSlopeAngle){
					deltaMove.x = (hit.distance - skinWidth) * xDir;
					rayLength = hit.distance;

					if(collisions.climbingSlope){
						deltaMove.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMove.x);
					}

					collisions.left = xDir == -1;
					collisions.right = xDir == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector2 deltaMove){
		float yDir = Mathf.Sign (deltaMove.y);
		float rayLength = Mathf.Abs (deltaMove.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (yDir == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + deltaMove.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * yDir, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * yDir, Color.red);

			if(hit){
				if (hit.collider.tag == "Pass Through") {
					collisions.abovePassThroughPlatform = true;

					if (yDir == 1 || hit.distance == 0) { // going up
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1 && isJumpPressed) {
						collisions.abovePassThroughPlatform = false;
						collisions.fallingThroughPlatform = true; // used with jump down
						Invoke ("ResetFallingThroughPlatform", .5f);
						continue;
					}
				} else {
					collisions.abovePassThroughPlatform = false;
				}

				deltaMove.y = (hit.distance - skinWidth) * yDir;
				rayLength = hit.distance;

				if(collisions.climbingSlope){
					deltaMove.x = deltaMove.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(deltaMove.x);
				}

				collisions.below = yDir == -1;
				collisions.above = yDir == 1;
			}
		}

		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(deltaMove.x);
			rayLength = Mathf.Abs(deltaMove.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * deltaMove.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);
			
			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					deltaMove.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 oldVelocity;
		public int facingDir;
		public bool abovePassThroughPlatform;
		public bool fallingThroughPlatform;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
