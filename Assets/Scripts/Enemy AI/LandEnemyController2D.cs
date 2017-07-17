using UnityEngine;
using System.Collections;

public class LandEnemyController2D : RaycastController {
	
	public Vector2 aiInput;
	private bool isAttacking = false;

	public CollisionInfo collisions;
	public bool isPathBlocked;
	private float timeToBlockAgain = 0.0f;

	// Use this for initialization
	public virtual void Awake () {
		collider = GetComponent<BoxCollider2D> ();	// Camera needs this to be initialized at Awake
	}

	public override void Start(){
		base.Start ();
		collisions.facingDir = 1;
	}

	public bool IsPathBlocked(){
		return isPathBlocked;
	}

	public void SetPathBlocked(bool isPathBlocked){
		this.isPathBlocked = isPathBlocked;
	}

	public void Move (Vector2 deltaMove, Vector2 input){
		Move (deltaMove, input, false);
	}

	public void Move (Vector2 deltaMove, Vector2 input, bool isToJump){
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.oldVelocity = deltaMove;
		this.aiInput = input;

		// Wall check
		if (deltaMove.x != 0) {
			collisions.facingDir = (int) Mathf.Sign(deltaMove.x);
		}

		HorizontalCollisions (ref deltaMove);
		if (deltaMove.y != 0) {
			VerticalCollisions (ref deltaMove);
		}

		if(!IsPathBlocked())
			transform.Translate (deltaMove);
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

			if (hit) {
		//		Debug.Log ("D" + hit.distance);
				if (hit.distance == 0.0f || hit.collider.tag == "Pass Through") {
					continue;
				} else {
					SetPathBlocked (true);
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
				deltaMove.y = (hit.distance - skinWidth) * yDir;

				collisions.below = yDir == -1;
				collisions.above = yDir == 1;
			}
		}
	}
		
	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public int facingDir;
		public Vector2 oldVelocity;

		public void Reset(){
			above = below = false;
			left = right = false;
		}
	}
}
