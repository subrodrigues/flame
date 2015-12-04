using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;

	const float skinWidth = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;
	float maxClimbAngle = 60;
	float maxDescendAngle = 75;
	public Vector2 playerInput;
	private bool isJumpPressed = false;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D collider;
	RaycastOrigins raycastOrigins;

	public CollisionInfo collisions;

	// Use this for initialization
	public virtual void Awake () {
		collider = GetComponent<BoxCollider2D> ();
	}
	
	// Update is called once per frame
	public virtual void Start() {
		CalculateRaySpacing ();

		collisions.facingDir = 1;
	}

	public void Move (Vector3 velocity, Vector2 input){
		Move (velocity, input, false);
	}

	public void Move (Vector3 velocity, Vector2 input, bool jumpPressed){
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.oldVelocity = velocity;
		this.playerInput = input;
		this.isJumpPressed = jumpPressed;

		// Wall check
		if (velocity.x != 0) {
			collisions.facingDir = (int) Mathf.Sign(velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope(ref velocity);
		}

	//	if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
	//	}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);
	}

	void ClimbSlope (ref Vector3 velocity, float slopeAngle){
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope (ref Vector3 velocity){
		float xDir = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
		
		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign(hit.normal.x) == xDir) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;
						
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void HorizontalCollisions(ref Vector3 velocity){
	//	float xDir = Mathf.Sign (velocity.x);
		// to wall jump
		float xDir = collisions.facingDir;

		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		// In order to detect the wall at wall jump, enlarge length collider
		if (Mathf.Abs (velocity.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (xDir == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * xDir, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * xDir * rayLength, Color.red);

			if(hit){

				if(hit.collider.tag == "Pass Through"){
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				// If climbing
				if(i == 0 && slopeAngle <= maxClimbAngle){
					if(collisions.descendingSlope){
						collisions.descendingSlope = false;
						velocity = collisions.oldVelocity;
					}

					float distanceToSlopStart = 0;

					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopStart * xDir;
					}
					ClimbSlope(ref velocity, slopeAngle);
					print(slopeAngle);

					velocity.x += distanceToSlopStart * xDir;
				}
				if(!collisions.climbingSlope || slopeAngle > maxClimbAngle){
					velocity.x = (hit.distance - skinWidth) * xDir;
					rayLength = hit.distance;

					if(collisions.climbingSlope){
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collisions.left = xDir == -1;
					collisions.right = xDir == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity){
		float yDir = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (yDir == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * yDir, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * yDir * rayLength, Color.red);

			if(hit){
				if(hit.collider.tag == "Pass Through"){
					collisions.abovePassThroughPlatform = true; // used with jump down

					if (yDir == 1 || hit.distance == 0){ // going up
						continue;
					}
					if(playerInput.y == -1 && isJumpPressed){
						continue;
					}
				}
				else{
					collisions.abovePassThroughPlatform = false;
				}

				velocity.y = (hit.distance - skinWidth) * yDir;
				rayLength = hit.distance;

				if(collisions.climbingSlope){
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				collisions.below = yDir == -1;
				collisions.above = yDir == 1;
			}
		}

		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);
			
			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void UpdateRaycastOrigins(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);

	}

	void CalculateRaySpacing(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
		
	}

	struct RaycastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;

	}

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 oldVelocity;
		public int facingDir;
		public bool abovePassThroughPlatform;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			abovePassThroughPlatform = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
