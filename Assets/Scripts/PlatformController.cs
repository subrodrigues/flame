using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;
	public Vector3 moveVelocity;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D> ();

	List<PassengerController2DMovement> passengerController2DMovement;

	// Use this for initialization
	public override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateRaycastOrigins ();

		Vector3 velocity = moveVelocity * Time.deltaTime;

		CalculateController2DPassengerMovement (velocity);

		MoveController2DPassengers (true);
		transform.Translate (velocity);
		MoveController2DPassengers (false);
	}

	void MoveController2DPassengers(bool beforeMovePlatform){
		foreach(PassengerController2DMovement passenger in passengerController2DMovement){
			if (!passengerDictionary.ContainsKey (passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<Controller2D> ());
			}
			if (passenger.isToMoveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move (passenger.velocity, passenger.isStandingOnPlatform);
			}
		}
	}

	void CalculateController2DPassengerMovement(Vector3 velocity){
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerController2DMovement = new List<PassengerController2DMovement> ();

		float xDir = Mathf.Sin (velocity.x);
		float yDir = Mathf.Sin (velocity.y);

		// Vertical moving platform
		if (velocity.y != 0){
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = (yDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * yDir, rayLength, passengerMask);

				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);

						float pushX = (yDir == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * yDir;

						// if yDir == 1, raycast is UP. In this situation if we have a collision player is above platform. If yDir == -1, facing down and hit player, so player is below.
						// In this case we want the passenger to move first
						passengerController2DMovement.Add (new PassengerController2DMovement(hit.transform, new Vector3 (pushX, pushY), yDir == 1, true));
					}
				}
			}
		}

		// Horizontal moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;

			for (int i = 0; i < horizontalRayCount; i++) {
				Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * xDir, rayLength, passengerMask);

				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * xDir;
						float pushY = -skinWidth;

						// horizontal raycast hit, so is not on top
						// In this case we want the passenger to move first
						passengerController2DMovement.Add (new PassengerController2DMovement(hit.transform, new Vector3 (pushX, pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of platform moving horizontally or down
		if(yDir == -1 || velocity.y == 0 && velocity.x != 0){
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						// We know that passenger is on top
						// In this case we want the passenger to move first
						passengerController2DMovement.Add (new PassengerController2DMovement(hit.transform, new Vector3 (pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerController2DMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool isStandingOnPlatform;
		public bool isToMoveBeforePlatform;

		public PassengerController2DMovement(Transform transform, Vector3 velocity, bool isStandingOnPlatform, bool isToMoveBeforePlatform){
			this.transform = transform;
			this.velocity = velocity;
			this.isStandingOnPlatform = isStandingOnPlatform;
			this.isToMoveBeforePlatform = isToMoveBeforePlatform;
		}

	}
}
